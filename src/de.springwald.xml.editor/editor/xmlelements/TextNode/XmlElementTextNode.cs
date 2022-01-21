// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.cursor;
using de.springwald.xml.editor.cursor;
using de.springwald.xml.editor.helper;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.xmlelements.Caching;
using de.springwald.xml.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor.xmlelements.TextNode
{
    /// <summary>
    /// XML element for displaying a text node
    /// </summary>
    public partial class XmlElementTextNode : XmlElement
    {
        private class LastMouseUp
        {
            public DateTime Time { get; set; }
            public Point Pos { get; set; }
        }

        private LastMouseUp lastMouseUp;

        private class Selection
        {
            public int Start;
            public int Length;
        }

        protected Color colorText;
        protected Color colorBackground;

        protected int lastFontHeight = 0;
        protected double lastCalculatedFontWidth = 0;
        private LastPaintingDataText lastPaintData;
        private PaintContext lastPaintContextResult;

        private TextPart[] textParts;  // Buffer of the single, drawn lines. Each corresponds to a click area

        public XmlElementTextNode(System.Xml.XmlNode xmlNode, XmlEditor xmlEditor, EditorContext editorContext) : base(xmlNode, xmlEditor, editorContext)
        {
            this.SetColors();
        }

        protected virtual void SetColors()
        {
            this.colorText = this.Config.ColorText;
            this.colorBackground = this.Config.ColorBackground;
        }

        protected override async Task<PaintContext> PaintInternal(PaintContext paintContext, bool cursorBlinkOn, XmlCursor cursor, IGraphics gfx, PaintModes paintMode, int depth)
        {
            paintContext.PaintPosX += 3;

            var actualText = ToolboxXml.TextFromNodeCleaned(XmlNode);
            var selection = this.CalculateStartAndEndOfSelection(actualText, cursor);
            var actualPaintData = LastPaintingDataText.CalculateActualPaintData(paintContext, cursorBlinkOn, this.XmlNode, actualText, this.Config.FontTextNode.Height, cursor, selection.Start, selection.Length);

            var alreadyUnpainted = false;

            switch (paintMode)
            {
                case PaintModes.ForcePaintNoUnPaintNeeded:
                    alreadyUnpainted = true;
                    this.lastPaintData = null;
                    break;

                case PaintModes.ForcePaintAndUnpaintBefore:
                    alreadyUnpainted = true;
                    this.lastPaintData = null;
                    this.UnPaint(gfx);
                    break;

                case PaintModes.OnlyPaintWhenChanged:
                    if (!actualPaintData.Equals(lastPaintData)) this.lastPaintData = null;
                    break;
            }

            if (this.lastPaintData != null && this.lastPaintContextResult != null)
            {
                return lastPaintContextResult.Clone();
            }

            this.lastPaintData = actualPaintData;
            this.cursorPaintPos = null;

            if (lastFontHeight != this.Config.FontTextNode.Height)
            {
                lastFontHeight = this.Config.FontTextNode.Height;
                lastCalculatedFontWidth = await this.xmlEditor.NativePlatform.Gfx.MeasureDisplayStringWidthAsync("W", this.Config.FontTextNode);
            }

            paintContext.HeightActualRow = Math.Max(paintContext.HeightActualRow, this.Config.MinLineHeight);

            int marginY = (paintContext.HeightActualRow - this.Config.FontTextNode.Height) / 2;

            const int charMarginRight = 2;

            var textPartsRaw = TextSplitHelper.SplitText(
                text: actualText,
                invertStart: selection.Start,
                invertLength: selection.Length,
                maxLength: (int)((paintContext.LimitRight - paintContext.LimitLeft) / lastCalculatedFontWidth) - charMarginRight,
                maxLengthFirstLine: (int)((paintContext.LimitRight - paintContext.PaintPosX) / lastCalculatedFontWidth) - charMarginRight)
                .ToArray();

            var newTextParts = this.GetTextLinesFromTextParts(textPartsRaw, paintContext, cursorBlinkOn, cursor, lastCalculatedFontWidth).ToArray();

            // Now draw the content, if necessary wrap to several text parts and lines
            for (int i = 0; i < newTextParts.Length; i++)
            {
                var newPart = newTextParts[i];
                var oldPart = (this.textParts != null && i < this.textParts.Length) ? this.textParts[i] : null;
                if (alreadyUnpainted == false && newPart.Equals(oldPart))
                {
                    // no need to paint the text part again
                }
                else
                {
                    // draw the inverted background
                    if (!alreadyUnpainted && oldPart != null) gfx.UnPaintRectangle(oldPart.Rectangle);

                    if (newPart.Inverted || this.colorBackground != this.Config.ColorBackground)
                    {
                        gfx.AddJob(new JobDrawRectangle
                        {
                            Batchable = true,
                            Layer = GfxJob.Layers.TagBackground,
                            Rectangle = newPart.Rectangle,
                            FillColor = newPart.Inverted ? this.colorBackground.InvertedColor : this.colorBackground,
                        });
                    }

                    // draw the text
                    gfx.AddJob(new JobDrawString
                    {
                        Batchable = false,
                        Layer = GfxJob.Layers.Text,
                        Text = newPart.Text,
                        Color = newPart.Inverted ? this.colorText.InvertedColor : this.colorText,
                        X = newPart.Rectangle.X,
                        Y = newPart.Rectangle.Y + marginY,
                        Font = Config.FontTextNode
                    }); ;
                }
                paintContext.PaintPosY = newPart.Rectangle.Y;
                paintContext.PaintPosX = newPart.Rectangle.X + newPart.Rectangle.Width;
                paintContext.FoundMaxX = Math.Max(paintContext.FoundMaxX, paintContext.PaintPosX);
            }

            if (this.textParts != null) // unpaint old text parts out of new parts range
            {
                for (int i = newTextParts.Length; i < this.textParts.Length; i++)
                {
                    gfx.UnPaintRectangle(this.textParts[i].Rectangle);
                }
            }

            this.textParts = newTextParts;

            paintContext.PaintPosX += 2;

            this.lastPaintContextResult = paintContext.Clone();
            return paintContext.Clone();
        }

        internal override void UnPaint(IGraphics gfx)
        {
            foreach (var textPart in this.textParts)
            {
                gfx.UnPaintRectangle(textPart.Rectangle);
            }
        }

        private IEnumerable<TextPart> GetTextLinesFromTextParts(TextSplitHelper.TextPartRaw[] parts, PaintContext paintContext, bool cursorBlinkOn, XmlCursor cursor, double fontWidth)
        {
            paintContext.HeightActualRow = Math.Max(paintContext.HeightActualRow, this.Config.MinLineHeight);
            var x = paintContext.PaintPosX;
            var y = paintContext.PaintPosY;
            var actualLine = 0;
            var actualTextPartStartPos = 0;

            foreach (var part in parts)
            {
                var newLine = part.LineNo != actualLine;
                if (newLine)
                {
                    actualLine = part.LineNo;
                    y += paintContext.HeightActualRow;
                    x = paintContext.LimitLeft;
                }
                var width = (int)(part.Text.Length * fontWidth);
                var textPart = new TextPart
                {
                    Text = part.Text,
                    Inverted = part.Inverted,
                    Rectangle = new Rectangle(x, y, width, paintContext.HeightActualRow),
                    CursorPos = -1,
                    CursorBlink = cursorBlinkOn,
                };

                if (this.XmlNode == cursor.StartPos.ActualNode) // is the cursor inside the current text node
                {
                    // Check if the cursor is within this text part
                    var cursorPos = -1;
                    if (cursor.StartPos.ActualNode == this.XmlNode && !cursor.IsSomethingSelected)
                    {
                        switch (cursor.StartPos.PosOnNode)
                        {
                            case XmlCursorPositions.CursorInFrontOfNode:
                                if (part == parts.First()) cursorPos = 0;
                                break;
                            case XmlCursorPositions.CursorBehindTheNode:
                                if (part == parts.Last()) cursorPos = part.Text.Length;
                                break;
                            case XmlCursorPositions.CursorInsideTextNode:
                                if ((cursor.StartPos.PosInTextNode >= actualTextPartStartPos) && (cursor.StartPos.PosInTextNode <= actualTextPartStartPos + part.Text.Length))
                                {
                                    cursorPos = (int)(cursor.StartPos.PosInTextNode - actualTextPartStartPos);
                                }
                                break;
                        }
                    }
                    if (cursorPos != -1)
                    {
                        textPart.CursorPos = cursorPos;
                        this.cursorPaintPos = new Point(
                                textPart.Rectangle.X + (int)(textPart.CursorPos * lastCalculatedFontWidth),
                                textPart.Rectangle.Y
                                );
                    }
                }

                x += 1 + width;
                yield return textPart;
                actualTextPartStartPos += part.Text.Length;
            }
        }



        protected override async Task OnMouseAction(Point point, MouseClickActions action)
        {
            // Find out where the text was clicked
            int posInLine = 0;
            var lastUp = this.lastMouseUp;
            if (action == MouseClickActions.MouseUp)
            {
                this.lastMouseUp = null;
            }
            foreach (var part in textParts)
            {
                if (part.Rectangle.Contains(point))
                {
                    posInLine += Math.Min(part.Text.Length - 1, (int)((point.X - part.Rectangle.X) / Math.Max(1, this.lastCalculatedFontWidth) + 0.5));

                    // Check if it was doubleclick 
                    if (action == MouseClickActions.MouseUp)
                    {
                        this.lastMouseUp = new LastMouseUp
                        {
                            Time = DateTime.UtcNow,
                            Pos = point
                        };

                        if (lastUp != null && (DateTime.UtcNow - lastUp.Time) < TimeSpan.FromMilliseconds(500) && Math.Abs(point.X - lastUp.Pos.X) + Math.Abs(point.Y - lastUp.Pos.Y) < 10)
                        {
                            // Double-click -> select word under mouse
                            posInLine = Math.Min(posInLine, Math.Max(0, part.Text.Length - 1));
                            var startPos = posInLine;
                            while (startPos > 0 && IsWordPart(part.Text[startPos - 1])) startPos--;
                            var endPos = posInLine;
                            while (endPos < part.Text.Length && IsWordPart(part.Text[endPos])) endPos++;

                            await EditorState.CursorRaw.SetPositions(
                                startNode: this.XmlNode, posAtStartNode: XmlCursorPositions.CursorInsideTextNode, textPosInStartNode: startPos,
                                endNode: this.XmlNode, posAtEndNode: XmlCursorPositions.CursorInsideTextNode, textPosInEndNode: endPos,
                                throwChangedEventWhenValuesChanged: false
                                );
                            return;
                        }
                    }

                    await EditorState.CursorRaw.SetCursorByMouseAction(this.XmlNode, XmlCursorPositions.CursorInsideTextNode, posInLine, action);
                    return;
                }
                else
                {
                    posInLine += part.Text.Length;
                }
            }
        }

        private bool IsWordPart(char chr)
        {
            if (chr >= 'A' && chr <= 'Z') return true;
            if (chr >= 'a' && chr <= 'z') return true;
            if (chr >= '0' && chr <= '9') return true;

            switch (chr)
            {
                case '-':
                case '@':
                    return true;

                case ' ':
                case '.':
                case '!':
                case '?':
                    return false;

                default:
                    return false;
            }
        }

        private Selection CalculateStartAndEndOfSelection(string actualText, XmlCursor cursor)
        {
            var result = new Selection { Start = -1, Length = 0 };

            if (cursor.StartPos.ActualNode == this.XmlNode) // The start of the selection is on this node
            {
                switch (cursor.StartPos.PosOnNode)
                {
                    case XmlCursorPositions.CursorOnNodeStartTag: // The node itself is selected as start node
                    case XmlCursorPositions.CursorOnNodeEndTag:
                        throw new ArgumentOutOfRangeException($"{nameof(cursor.StartPos.PosOnNode)}:{cursor.StartPos.PosOnNode.ToString()} not possible on a text node");
                    // result.Start = 0;
                    // result.Length = actualText.Length;
                    // break;

                    case XmlCursorPositions.CursorBehindTheNode:
                    case XmlCursorPositions.CursorInsideTheEmptyNode:
                        // Since the cursor position arrives sorted, the EndPos can only lie behind the node
                        result.Start = -1;
                        result.Length = 0;
                        break;

                    case XmlCursorPositions.CursorInFrontOfNode:
                    case XmlCursorPositions.CursorInsideTextNode:

                        if (cursor.StartPos.PosOnNode == XmlCursorPositions.CursorInsideTextNode)
                        {
                            result.Start = Math.Max(0, cursor.StartPos.PosInTextNode); // inside text node
                        }
                        else
                        {
                            result.Start = 0; // in front of the node
                        }

                        if (cursor.EndPos.ActualNode == this.XmlNode) // If the end of the selection also is inside this node
                        {
                            switch (cursor.EndPos.PosOnNode)
                            {
                                case XmlCursorPositions.CursorOnNodeStartTag: // start node is in front of the node, end node behind: everything is selected
                                case XmlCursorPositions.CursorOnNodeEndTag:
                                case XmlCursorPositions.CursorBehindTheNode:
                                    result.Length = Math.Max(0, actualText.Length - result.Start);
                                    break;

                                case XmlCursorPositions.CursorInsideTheEmptyNode:
                                    result.Start = -1;
                                    result.Length = 0;
                                    break;

                                case XmlCursorPositions.CursorInsideTextNode:  // till the marker in the text 
                                    result.Length = Math.Max(0, cursor.EndPos.PosInTextNode - result.Start);
                                    break;

                                case XmlCursorPositions.CursorInFrontOfNode:
                                    result.Length = 0;
                                    break;

                                default:
                                    throw new ApplicationException("unknown cursor.EndPos.PosOnNode '" + cursor.EndPos.PosOnNode + "'B");
                            }
                        }
                        else // The end of the selection is not inside this node
                        {
                            if (cursor.EndPos.ActualNode.ParentNode == cursor.StartPos.ActualNode.ParentNode) // If start and end are different, but directly in the same parent
                            {
                                result.Length = Math.Max(0, actualText.Length - result.Start); // Select only the selected part
                            }
                            else // Start and end are different and have different parents
                            {
                                result.Start = 0;
                                result.Length = actualText.Length;   // select whole text node
                            }
                        }
                        break;

                    default:
                        throw new ApplicationException("unknown cursor.StartPos.PosOnNode '" + cursor.StartPos.PosOnNode + "'A");
                }
            }
            else // The start of the selection is not on this node
            {
                if (cursor.EndPos.ActualNode == this.XmlNode) // But the end of the selection is
                {
                    switch (cursor.EndPos.PosOnNode)
                    {
                        case XmlCursorPositions.CursorOnNodeStartTag:   // The node itself is selected as End-Node
                        case XmlCursorPositions.CursorOnNodeEndTag:     // start node is in front of the node, end node behind: everything is selected
                        case XmlCursorPositions.CursorBehindTheNode:
                            result.Start = 0;
                            result.Length = actualText.Length;
                            break;

                        case XmlCursorPositions.CursorInsideTheEmptyNode:
                            result.Start = -1;
                            result.Length = 0;
                            break;

                        case XmlCursorPositions.CursorInsideTextNode:   // Start node is in front of the node, end node in the middle, so select from front to middle
                            if (cursor.EndPos.ActualNode.ParentNode == cursor.StartPos.ActualNode.ParentNode) // If start and end are different, but directly in the same parent
                            {
                                result.Start = 0;
                                result.Length = Math.Max(0, cursor.EndPos.PosInTextNode); // Select only the selected front part
                            }
                            else // Start and end different and different parents
                            {
                                result.Start = 0;
                                result.Length = actualText.Length;   // select whole text node
                            }
                            break;

                        case XmlCursorPositions.CursorInFrontOfNode: // Start node is in front of the node, end node also
                            result.Start = -1;
                            result.Length = 0;
                            break;

                        default:
                            throw new ApplicationException("unknown cursor.EndPos.PosOnNode '" + cursor.EndPos.PosOnNode + "'X");
                    }
                }
                else // Neither the start nor the end of the selection lies exactly on this node
                {
                    if (XmlCursorSelectionHelper.IsThisNodeInsideSelection(EditorState.CursorOptimized, this.XmlNode))
                    {
                        result.Start = 0;
                        result.Length = actualText.Length;   // Select entire text node
                    }
                }
            }
            return result;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}