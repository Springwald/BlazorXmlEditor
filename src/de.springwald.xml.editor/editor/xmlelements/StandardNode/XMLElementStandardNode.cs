// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.cursor;
using de.springwald.xml.editor.cursor;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.xmlelements;
using de.springwald.xml.editor.xmlelements.StandardNode;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Draws a standard node for the editor
    /// </summary>
    public partial class XmlElementStandardNode : XmlElement
    {
        private readonly StandardNodeDimensionsAndColor nodeDimensions;
        private readonly StandardNodeStartTagPainter startTag;
        private readonly StandardNodeEndTagPainter endTag;
        protected List<XmlElement> childElements = new List<XmlElement>();

        public XmlElementStandardNode(XmlNode xmlNode, XmlEditor xmlEditor, EditorContext editorContext) : base(xmlNode, xmlEditor, editorContext)
        {
            var IsEndTagVisible = this.XmlRules.HasEndTag(xmlNode);
            var colorTagBackground = this.XmlRules.NodeColor(this.XmlNode);
            this.nodeDimensions = new StandardNodeDimensionsAndColor(editorContext.EditorConfig, colorTagBackground);
            this.startTag = new StandardNodeStartTagPainter(this.Config, this.nodeDimensions, xmlNode, IsEndTagVisible);
            if (IsEndTagVisible)
            {
                this.endTag = new StandardNodeEndTagPainter(this.Config, this.nodeDimensions, xmlNode, IsEndTagVisible);
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Destroy all child elements as well
            foreach (var child in this.childElements)
            {
                if (child != null) child.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override async Task<PaintContext> PaintInternal(PaintContext paintContext, bool cursorBlinkOn, XmlCursor cursor, IGraphics gfx, PaintModes paintMode, int depth)
        {
            this.nodeDimensions.Update();
            var isSelected = XmlCursorSelectionHelper.IsThisNodeInsideSelection(cursor, this.XmlNode);
            this.CreateChildElementsIfNeeded(gfx);

            Point newCursorPaintPos = null;

            bool alreadyUnpainted = false;

            switch (paintMode)
            {
                case PaintModes.ForcePaintNoUnPaintNeeded:
                    alreadyUnpainted = true;
                    break;
                case PaintModes.ForcePaintAndUnpaintBefore:
                    this.UnPaint(gfx);
                    alreadyUnpainted = true;
                    break;
                case PaintModes.OnlyPaintWhenChanged:
                    break;
            }

            // If the cursor is inside the empty node, then draw the cursor there
            if (cursor.StartPos.ActualNode == this.XmlNode)
            {
                if (cursor.StartPos.PosOnNode == XmlCursorPositions.CursorInFrontOfNode)
                {
                    // remember position for cursor line
                    newCursorPaintPos = new Point(paintContext.PaintPosX, paintContext.PaintPosY);
                }
            }

            var cursorIsOnThisNode = cursor.StartPos.ActualNode == this.XmlNode || cursor.EndPos.ActualNode == this.XmlNode;

            paintContext = await this.startTag.Paint(paintContext, cursorIsOnThisNode, cursorBlinkOn, alreadyUnpainted, isSelected, gfx);

            // If the cursor is inside the empty node, then draw the cursor there
            if (cursor.StartPos.ActualNode == this.XmlNode)
            {
                if (cursor.StartPos.PosOnNode == XmlCursorPositions.CursorInsideTheEmptyNode)
                {
                    // set position for cursor line
                    newCursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                }
            }

            paintContext = await this.PaintSubNodes(paintContext, cursorBlinkOn, cursor, gfx, paintMode, depth);

            if (this.endTag != null)
            {
                paintContext = await this.endTag.Paint(paintContext, cursorIsOnThisNode, cursorBlinkOn, alreadyUnpainted, isSelected, gfx);
            }

            // If the cursor is behind the node, then also draw the cursor there
            if (cursor.StartPos.ActualNode == this.XmlNode)
            {
                if (cursor.StartPos.PosOnNode == XmlCursorPositions.CursorBehindTheNode)
                {
                    this.cursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                }
            }
            this.cursorPaintPos = newCursorPaintPos;
            return paintContext.Clone();
        }

        internal override void UnPaint(IGraphics gfx)
        {
            this.startTag.Unpaint(gfx);
            this.endTag?.Unpaint(gfx);
        }

        protected async Task<PaintContext> PaintSubNodes(PaintContext paintContext, bool cursorBlinkOn, XmlCursor cursor, IGraphics gfx, PaintModes paintMode, int depth)
        {
            if (this.XmlNode == null) throw new ApplicationException("PaintSubNodes:xmlNode is null");

            var childPaintContext = paintContext.Clone();
            childPaintContext.LimitLeft = paintContext.LimitLeft + this.Config.ChildIndentX;

            for (int childLauf = 0; childLauf < this.XmlNode.ChildNodes.Count; childLauf++)
            {
                // At this point, the ChildControl object should contain the corresponding instance of the XMLElement control for the current XMLChildNode
                var childElement = (XmlElement)childElements[childLauf];
                var displayType = this.XmlRules.DisplayType(childElement.XmlNode);
                switch (displayType)
                {
                    case DisplayTypes.OwnRow:

                        // This child element starts a new row and is then drawn in this row

                        // start new row
                        childPaintContext.LimitLeft = paintContext.LimitLeft + this.Config.ChildIndentX;
                        childPaintContext.PaintPosX = childPaintContext.LimitLeft;
                        childPaintContext.PaintPosY += this.Config.SpaceYBetweenLines + paintContext.HeightActualRow; // line break
                        childPaintContext.HeightActualRow = 0; // no element in this line yet, therefore Height 0
                                                               // Set X-cursor to the start of the new line
                                                               // line down and then right into the ChildElement
                                                               // Line down
                        bool paintLines = false;
                        if (paintLines)
                        {
                            gfx.AddJob(new JobDrawLine
                            {
                                Layer = GfxJob.Layers.TagBorder,
                                Batchable = true,
                                Color = Color.LightGray,
                                X1 = paintContext.LimitLeft,
                                Y1 = paintContext.PaintPosY + this.Config.MinLineHeight / 2,
                                X2 = paintContext.LimitLeft,
                                Y2 = childPaintContext.PaintPosY + this.Config.MinLineHeight / 2
                            });

                            // Line to the right with arrow on ChildElement
                            gfx.AddJob(new JobDrawLine
                            {
                                Layer = GfxJob.Layers.TagBorder,
                                Batchable = true,
                                Color = Color.LightGray,
                                X1 = paintContext.LimitLeft,
                                Y1 = childPaintContext.PaintPosY + this.Config.MinLineHeight / 2,
                                X2 = childPaintContext.LimitLeft,
                                Y2 = childPaintContext.PaintPosY + this.Config.MinLineHeight / 2
                            });
                        }

                        childPaintContext = await childElement.Paint(childPaintContext, cursorBlinkOn, cursor, gfx, paintMode, depth + 1);
                        break;

                    case DisplayTypes.FloatingElement:
                        // This child is a floating element; it inserts itself into the same line as the previous element and does not start a new line unless the current line is already too long
                        if (childPaintContext.PaintPosX > paintContext.LimitRight) //  If the row is already too long
                        {
                            // to next row
                            paintContext.PaintPosY += paintContext.HeightActualRow + this.Config.SpaceYBetweenLines;
                            paintContext.HeightActualRow = 0;
                            paintContext.PaintPosX = paintContext.RowStartX;
                        }
                        else // fits into this line
                        {
                            // set the child to the right of it	
                        }
                        childPaintContext = await childElement.Paint(childPaintContext, cursorBlinkOn, cursor, gfx, paintMode, depth + 1);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(displayType) + ":" + displayType.ToString());
                }
                paintContext.FoundMaxX = childPaintContext.FoundMaxX;
                paintContext.PaintPosX = childPaintContext.PaintPosX;
                paintContext.PaintPosY = childPaintContext.PaintPosY;
            }

            // If we have more ChildControls than XMLChildNodes, then delete them at the end of the ChildControl list
            while (this.XmlNode.ChildNodes.Count < childElements.Count)
            {
                var deleteChildElement = childElements[childElements.Count - 1];
                deleteChildElement.UnPaint(gfx);
                childElements.Remove(childElements[childElements.Count - 1]);
                deleteChildElement.Dispose();
            }
            return paintContext;
        }

        protected override async Task OnMouseAction(Point point, MouseClickActions mouseAction)
        {
            if (this.startTag.AreaArrow?.Contains(point) == true) // clicked on the left, opening arrow
            {
                if (this.XmlNode.ChildNodes.Count > 0) // Children available
                {
                    // put in front of the first child
                    await EditorState.CursorRaw.SetCursorByMouseAction(this.XmlNode.FirstChild, XmlCursorPositions.CursorInFrontOfNode, mouseAction);
                    EditorState.CursorBlink.ResetBlinkPhase();
                    return;
                }
                else // No child available
                {
                    // Put into the node itself
                    await EditorState.CursorRaw.SetCursorByMouseAction(this.XmlNode, XmlCursorPositions.CursorInsideTheEmptyNode, mouseAction);
                    EditorState.CursorBlink.ResetBlinkPhase();
                    return;
                }
            }

            if (this.endTag?.AreaArrow?.Contains(point) == true) // the right, closing arrow was clicked
            {
                if (this.XmlNode.ChildNodes.Count > 0) // Children available
                {
                    //  put behind the last child
                    await EditorState.CursorRaw.SetCursorByMouseAction(this.XmlNode.LastChild, XmlCursorPositions.CursorBehindTheNode, mouseAction);
                    EditorState.CursorBlink.ResetBlinkPhase();
                    return;
                }
                else // No child available
                {
                    // Put into the node itself
                    await EditorState.CursorRaw.SetCursorByMouseAction(this.XmlNode, XmlCursorPositions.CursorInsideTheEmptyNode, mouseAction);
                    EditorState.CursorBlink.ResetBlinkPhase();
                    return;
                }
            }

            if (this.startTag.AreaTag?.Contains(point) == true) // clicked on the left tag
            {
                await EditorState.CursorRaw.SetCursorByMouseAction(this.XmlNode, XmlCursorPositions.CursorOnNodeStartTag, mouseAction);
                EditorState.CursorBlink.ResetBlinkPhase();
                return;
            }

            if (this.endTag?.AreaTag?.Contains(point) == true) // clicked on the right day
            {
                await EditorState.CursorRaw.SetCursorByMouseAction(this.XmlNode, XmlCursorPositions.CursorOnNodeEndTag, mouseAction);
                EditorState.CursorBlink.ResetBlinkPhase();
                return;
            }
        }

        private void CreateChildElementsIfNeeded(IGraphics gfx)
        {
            // Display all child controls and create them before if necessary
            for (int childLauf = 0; childLauf < this.XmlNode.ChildNodes.Count; childLauf++)
            {
                if (childLauf >= childElements.Count)
                {
                    // If not yet as many ChildControls are created as there are ChildXMLNodes
                    var childElement = this.xmlEditor.CreateElement(this.XmlNode.ChildNodes[childLauf]);
                    childElements.Add(childElement);
                }
                else
                {
                    // there is already a control at this point
                    var childElement = (XmlElement)childElements[childLauf];
                    if (childElement == null)
                    {
                        throw new ApplicationException($"CreateChildElementsIfNeeded:childElement is empty: outerxml:{this.XmlNode.OuterXml} >> innerxml {this.XmlNode.InnerXml}");
                    }

                    // check if it also represents the same XML node
                    if (childElement.XmlNode != this.XmlNode.ChildNodes[childLauf])
                    {
                        // The ChildControl does not contain the same ChildNode, so delete and redo
                        childElement.UnPaint(gfx);
                        childElement.Dispose(); // delete old
                        childElements[childLauf] = this.xmlEditor.CreateElement(this.XmlNode.ChildNodes[childLauf]);
                    }
                }
            }
        }


    }
}
