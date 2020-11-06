// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.cursor;
using de.springwald.xml.editor.xmlelements.Caching;
using de.springwald.xml.editor.helper;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using de.springwald.xml.editor.cursor;
using static de.springwald.xml.rules.XmlCursorPos;
using System.Runtime.InteropServices;

namespace de.springwald.xml.editor.xmlelements.TextNode
{
    /// <summary>
    /// XML element for displaying a text node
    /// </summary>
    public partial class XmlElementTextNode : XmlElement
    {

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

            // Nun den Inhalt zeichnen, ggf. auf mehrere Textteile und Zeilen umbrochen
            for (int i = 0; i < newTextParts.Length; i++)
            {
                var newPart = newTextParts[i];
                var oldPart = (this.textParts != null && i < this.textParts.Length) ? this.textParts[i] : null;
                if (alreadyUnpainted == false && newPart.Equals(oldPart))
                {
                    // no need to paint the textpart again
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

            if (this.textParts != null) // unpaint old textparts out of new parts range
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

                if (this.XmlNode == cursor.StartPos.ActualNode) // ist der Cursor im aktuellen Textnode
                {
                    // Checken, ob der Cursor innerhalb dieses Textteiles liegt
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

        /// <summary>
        /// Wird aufgerufen, wenn auf dieses Element geklickt wurde
        /// </summary>
        protected override async Task OnMouseAction(Point point, MouseClickActions action)
        {
            // Herausfinden, an welcher Position des Textes geklickt wurde
            int posInLine = 0;
            foreach (var part in textParts) // alle Textteile durchgehen
            {
                if (part.Rectangle.Contains(point)) // Wenn der Klick in diesem Textteil ist
                {
                    posInLine += Math.Min(part.Text.Length - 1, (int)((point.X - part.Rectangle.X) / Math.Max(1, this.lastCalculatedFontWidth) + 0.5));
                    await EditorState.CursorRaw.SetCursorByMouseAction(this.XmlNode, XmlCursorPositions.CursorInsideTextNode, posInLine, action);
                    return;
                }
                else // In diesem Textteil war der Klick nicht
                {
                    posInLine += part.Text.Length;
                }
            }
        }

        /// <summary>
        /// Findet heraus, welche Bereiche des Textes invertiert dargestellt werden müssen
        /// </summary>
        /// <param name="selectionStart"></param>
        /// <param name="selektionEnde"></param>
        private Selection CalculateStartAndEndOfSelection(string actualText, XmlCursor cursor)
        {
            var result = new Selection { Start = -1, Length = 0 };

            if (cursor.StartPos.ActualNode == this.XmlNode) // Der Start der Selektion liegt auf diesem Node
            {
                switch (cursor.StartPos.PosOnNode)
                {
                    case XmlCursorPositions.CursorOnNodeStartTag: // Das Node selbst ist als Startnode selektiert
                    case XmlCursorPositions.CursorOnNodeEndTag:
                        throw new ArgumentOutOfRangeException(nameof(cursor.StartPos.PosOnNode) + ":" + cursor.StartPos.PosOnNode.ToString() + " not possible on a text node");
                        result.Start = 0;
                        result.Length = actualText.Length;
                        break;

                    case XmlCursorPositions.CursorBehindTheNode:
                    case XmlCursorPositions.CursorInsideTheEmptyNode:
                        // Da die CursorPosition sortiert ankommt, kann die EndPos nur hintet dem Node liegen
                        result.Start = -1;
                        result.Length = 0;
                        break;

                    case XmlCursorPositions.CursorInFrontOfNode:
                    case XmlCursorPositions.CursorInsideTextNode:

                        if (cursor.StartPos.PosOnNode == XmlCursorPositions.CursorInsideTextNode)
                        {
                            result.Start = Math.Max(0, cursor.StartPos.PosInTextNode); // im Textnode
                        }
                        else
                        {
                            result.Start = 0; // Vor dem Node
                        }

                        if (cursor.EndPos.ActualNode == this.XmlNode) // Wenn das Ende der Selektion auch in diesem Node liegt
                        {
                            switch (cursor.EndPos.PosOnNode)
                            {
                                case XmlCursorPositions.CursorOnNodeStartTag: // Startnode liegt vor dem Node, Endnode dahiner: alles ist selektiert
                                case XmlCursorPositions.CursorOnNodeEndTag:
                                case XmlCursorPositions.CursorBehindTheNode:
                                    result.Length = Math.Max(0, actualText.Length - result.Start);
                                    break;

                                case XmlCursorPositions.CursorInsideTheEmptyNode:
                                    result.Start = -1;
                                    result.Length = 0;
                                    break;

                                case XmlCursorPositions.CursorInsideTextNode:  // Bis zur Markierung im Text 
                                    result.Length = Math.Max(0, cursor.EndPos.PosInTextNode - result.Start);
                                    break;

                                case XmlCursorPositions.CursorInFrontOfNode:
                                    result.Length = 0;
                                    break;

                                default:
                                    throw new ApplicationException("unknown cursor.EndPos.PosOnNode '" + cursor.EndPos.PosOnNode + "'B");
                            }
                        }
                        else // Das Ende der Selektion liegt nicht in diesem Node
                        {
                            if (cursor.EndPos.ActualNode.ParentNode == cursor.StartPos.ActualNode.ParentNode) // Wenn Start und Ende zwar verschieden, aber direkt im selben Parent stecken
                            {
                                result.Length = Math.Max(0, actualText.Length - result.Start); // Nur den selektierten Teil selektieren
                            }
                            else // Start und Ende unterschiedlich und unterschiedliche Parents
                            {
                                result.Start = 0;
                                result.Length = actualText.Length;   // Ganzen Textnode selektieren
                            }
                        }
                        break;

                    default:
                        throw new ApplicationException("unknown cursor.StartPos.PosOnNode '" + cursor.StartPos.PosOnNode + "'A");
                }
            }
            else // Der Start der Selektion liegt nicht auf diesem Node
            {
                if (cursor.EndPos.ActualNode == this.XmlNode) // Das Ende der Selektion aber schon
                {
                    switch (cursor.EndPos.PosOnNode)
                    {
                        case XmlCursorPositions.CursorOnNodeStartTag: // Der Node selbst ist als End-Node selektiert
                        case XmlCursorPositions.CursorOnNodeEndTag: // Startnode liegt vor dem Node, Endnode dahiner: alles ist selektiert
                        case XmlCursorPositions.CursorBehindTheNode:
                            result.Start = 0;
                            result.Length = actualText.Length;
                            break;

                        case XmlCursorPositions.CursorInsideTheEmptyNode:
                            result.Start = -1;
                            result.Length = 0;
                            break;

                        case XmlCursorPositions.CursorInsideTextNode:// Startnode liegt vor dem Node, Endnode mitten drin, also von vorn bis zur Mitte selektieren
                            if (cursor.EndPos.ActualNode.ParentNode == cursor.StartPos.ActualNode.ParentNode) // Wenn Start und Ende zwar verschieden, aber direkt im selben Parent stecken
                            {
                                result.Start = 0;
                                result.Length = Math.Max(0, cursor.EndPos.PosInTextNode); // Nur den selektierten, vorderen Teil selektieren
                            }
                            else // Start und Ende unterschiedlich und unterschiedliche Parents
                            {
                                result.Start = 0;
                                result.Length = actualText.Length;   // Ganzen Textnode selektieren
                            }
                            break;

                        case XmlCursorPositions.CursorInFrontOfNode: // Startnode liegt vor dem Node, Endnoch auch
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