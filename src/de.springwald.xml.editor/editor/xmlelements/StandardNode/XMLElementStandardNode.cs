// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using de.springwald.xml.cursor;
using de.springwald.xml.editor.xmlelements;
using de.springwald.xml.editor.xmlelements.StandardNode;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;
using de.springwald.xml.editor.cursor;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Draws a standard node for the editor
    /// </summary>
    public partial class XMLElementStandardNode : XmlElement
    {
        private StandardNodeDimensionsAndColor nodeDimensions;
        private StandardNodeStartTagPainter startTag;
        private StandardNodeEndTagPainter endTag;

        protected List<XmlElement> childElements = new List<XmlElement>();   // Die ChildElemente in diesem Steuerelement

        public XMLElementStandardNode(XmlNode xmlNode, XmlEditor xmlEditor, EditorContext editorContext) : base(xmlNode, xmlEditor, editorContext)
        {
            var isClosingTagVisible = this.XmlRules.HasEndTag(xmlNode);
            var colorTagBackground = this.XmlRules.NodeFarbe(this.XmlNode, selektiert: false);
            this.nodeDimensions = new StandardNodeDimensionsAndColor(editorContext.EditorConfig, colorTagBackground);
            this.startTag = new StandardNodeStartTagPainter(this.Config, this.nodeDimensions, xmlNode, isClosingTagVisible);
            if (isClosingTagVisible)
            {
                this.endTag = new StandardNodeEndTagPainter(this.Config, this.nodeDimensions, xmlNode);
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Alle Child-Elemente ebenfalls zerstören
            foreach (var child in this.childElements)
            {
                if (child != null) child.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override async Task<PaintContext> PaintInternal(PaintContext paintContext, bool cursorBlinkOn, XmlCursor cursor, IGraphics gfx, PaintModes paintMode, int depth)
        {
            this.nodeDimensions.Update();
            var isSelected = XmlCursorSelectionHelper.IstNodeInnerhalbDerSelektion(cursor, this.XmlNode);
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

            // Falls der Cursor innherlb des leeren Nodes steht, dann den Cursor auch dahin zeichnen
            if (cursor.StartPos.ActualNode == this.XmlNode)
            {
                if (cursor.StartPos.PosOnNode == XmlCursorPositions.CursorInFrontOfNode)
                {
                    // Position für Cursor-Strich vermerken
                    newCursorPaintPos = new Point(paintContext.PaintPosX + 1, paintContext.PaintPosY);
                }
            }

            paintContext = await this.startTag.Paint(paintContext, cursorBlinkOn, alreadyUnpainted,  isSelected, gfx);

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
                paintContext = await this.endTag.Paint(paintContext, cursorBlinkOn, alreadyUnpainted, isSelected, gfx);
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
            if (this.XmlNode == null)
            {
                throw new ApplicationException("UnternodesZeichnen:XMLNode ist leer");
            }

            var childPaintContext = paintContext.Clone();
            childPaintContext.LimitLeft = paintContext.LimitLeft + this.Config.ChildIndentX;

            for (int childLauf = 0; childLauf < this.XmlNode.ChildNodes.Count; childLauf++)
            {
                // An dieser Stelle sollte im Objekt ChildControl die entsprechends
                // Instanz des XMLElement-Controls für den aktuellen XMLChildNode stehen
                var childElement = (XmlElement)childElements[childLauf];
                var displayType = this.XmlRules.DisplayType(childElement.XmlNode);
                switch (displayType)
                {
                    case DisplayTypes.OwnRow:

                        // Dieses Child-Element beginnt eine neue Zeile und wird dann in dieser gezeichnet

                        // Neue Zeile beginnen
                        childPaintContext.LimitLeft = paintContext.LimitLeft + this.Config.ChildIndentX;
                        childPaintContext.PaintPosX = childPaintContext.LimitLeft;
                        childPaintContext.PaintPosY += this.Config.SpaceYBetweenLines + paintContext.HeightActualRow; // Zeilenumbruch
                        childPaintContext.HeightActualRow = 0; // noch kein Element in dieser Zeile, daher Hoehe 0
                                                             // X-Cursor auf den Start der neuen Zeile setzen
                                                             // Linie nach unten und dann nach rechts ins ChildElement
                                                             // Linie nach unten
                        const bool paintLines = false;

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

                            // Linie nach rechts mit Pfeil auf ChildElement
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

                        childPaintContext = await childElement.Paint(childPaintContext, cursorBlinkOn, cursor, gfx, paintMode, depth+1);
                        break;

                    case DisplayTypes.FloatingElement:
                        // Dieses Child ist ein Fliesselement; es fügt sich in die selbe Zeile
                        // ein, wie das vorherige Element und beginnt keine neue Zeile, 
                        // es sei denn, die aktuelle Zeile ist bereits zu lang
                        if (childPaintContext.PaintPosX > paintContext.LimitRight) // Wenn die Zeile bereits zu voll ist
                        {
                            // in nächste Zeile
                            paintContext.PaintPosY += paintContext.HeightActualRow + this.Config.SpaceYBetweenLines;
                            paintContext.HeightActualRow = 0;
                            paintContext.PaintPosX = paintContext.RowStartX;
                        }
                        else // es passt noch etwas in diese Zeile
                        {
                            // das Child rechts daneben setzen	
                        }

                        childPaintContext = await childElement.Paint(childPaintContext, cursorBlinkOn, cursor, gfx, paintMode, depth+1);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(displayType) + ":" + displayType.ToString());
                }
                paintContext.PaintPosX = childPaintContext.PaintPosX;
                paintContext.PaintPosY = childPaintContext.PaintPosY;
            }

            // Sollten wir mehr ChildControls als XMLChildNodes haben, dann diese
            // am Ende der ChildControlListe löschen
            while (this.XmlNode.ChildNodes.Count < childElements.Count)
            {
                var deleteChildElement = childElements[childElements.Count - 1];
                deleteChildElement.UnPaint(gfx);
                childElements.Remove(childElements[childElements.Count - 1]);
                deleteChildElement.Dispose();
            }
            return paintContext;
        }


        /// <summary>
        /// Wird aufgerufen, wenn auf dieses Element geklickt wurde
        /// </summary>
        /// <param name="point"></param>
        protected override async Task OnMouseAction(Point point, MouseClickActions mouseAction)
        {
            if (this.startTag.AreaTag?.Contains(point) == true) // er wurde auf das linke Tag geklickt
            {
                await EditorState.CursorRaw.CursorPosSetzenDurchMausAktion(this.XmlNode, XmlCursorPositions.CursorOnNodeStartTag, mouseAction);
                EditorState.CursorBlink.ResetBlinkPhase();
                return;
            }

            if (this.startTag.AreaArrow?.Contains(point) == true) // er wurde auf den linken, öffnenden Pfeil geklickt
            {
                if (this.XmlNode.ChildNodes.Count > 0) // Children vorhanden
                {
                    // vor das erste Child setzen
                    await EditorState.CursorRaw.CursorPosSetzenDurchMausAktion(this.XmlNode.LastChild, XmlCursorPositions.CursorBehindTheNode, mouseAction);
                    EditorState.CursorBlink.ResetBlinkPhase();
                    return;
                }
                else // Kein Child vorhanden
                {
                    // In den Node selbst setzen
                    await EditorState.CursorRaw.CursorPosSetzenDurchMausAktion(this.XmlNode, XmlCursorPositions.CursorInsideTheEmptyNode, mouseAction);
                    EditorState.CursorBlink.ResetBlinkPhase();
                    return;
                }
            }

            if (this.endTag?.AreaTag?.Contains(point) == true) // er wurde auf das rechte Tag geklickt
            {
                await EditorState.CursorRaw.CursorPosSetzenDurchMausAktion(this.XmlNode, XmlCursorPositions.CursorOnNodeEndTag, mouseAction);
                EditorState.CursorBlink.ResetBlinkPhase();
                return;
            }

            if (this.endTag?.AreaArrow?.Contains(point) == true) // es wurde auf den rechten, schließenden Pfeil geklickt
            {
                if (this.XmlNode.ChildNodes.Count > 0) // Children vorhanden
                {
                    // vor das erste Child setzen
                    await EditorState.CursorRaw.CursorPosSetzenDurchMausAktion(this.XmlNode.FirstChild, XmlCursorPositions.CursorInFrontOfNode, mouseAction);
                    EditorState.CursorBlink.ResetBlinkPhase();
                    return;
                }
                else // Kein Child vorhanden
                {
                    // In den Node selbst setzen
                    await EditorState.CursorRaw.CursorPosSetzenDurchMausAktion(this.XmlNode, XmlCursorPositions.CursorInsideTheEmptyNode, mouseAction);
                    EditorState.CursorBlink.ResetBlinkPhase();
                    return;
                }
            }

            // Nicht auf Pfeil geklickt, dann Event weiterreichen an Base-Klasse
            //await EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, mouseAction);
            //xmlEditor.CursorBlink.ResetBlinkPhase();
        }

        private void CreateChildElementsIfNeeded(IGraphics gfx)
        {
            // Alle Child-Controls anzeigen und ggf. vorher anlegen
            for (int childLauf = 0; childLauf < this.XmlNode.ChildNodes.Count; childLauf++)
            {
                if (childLauf >= childElements.Count)
                {   // Wenn noch nicht so viele ChildControls angelegt sind, wie
                    // es ChildXMLNodes gibt
                    var childElement = this.xmlEditor.CreateElement(this.XmlNode.ChildNodes[childLauf]);
                    childElements.Add(childElement);
                }
                else
                {   // es gibt schon ein Control an dieser Stelle
                    var childElement = (XmlElement)childElements[childLauf];

                    if (childElement == null)
                    {
                        throw new ApplicationException($"UnternodesZeichnen:childElement ist leer: outerxml:{this.XmlNode.OuterXml} >> innerxml {this.XmlNode.InnerXml}");
                    }

                    // prüfen, ob es auch den selben XML-Node vertritt
                    if (childElement.XmlNode != this.XmlNode.ChildNodes[childLauf])
                    {   // Das ChildControl enthält nicht den selben ChildNode, also 
                        // löschen und neu machen
                        childElement.UnPaint(gfx);
                        childElement.Dispose(); // altes Löschen
                        childElements[childLauf] = this.xmlEditor.CreateElement(this.XmlNode.ChildNodes[childLauf]);
                    }
                }
            }
        }


    }
}
