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

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Draws a standard node for the editor
    /// </summary>
    public partial class XMLElement_StandardNode : XMLElement
    {
        private StandardNodeDimensionsAndColor nodeDimensions;
        private StandardNodeStartTagPainter startTag;
        private StandardNodeEndTagPainter endTag;

        protected List<XMLElement> childElements = new List<XMLElement>();   // Die ChildElemente in diesem Steuerelement

        public XMLElement_StandardNode(XmlNode xmlNode, XMLEditor xmlEditor, EditorContext editorContext) : base(xmlNode, xmlEditor, editorContext)
        {
            var isClosingTagVisible = this.Regelwerk.IstSchliessendesTagSichtbar(xmlNode);
            var colorTagBackground = this.Regelwerk.NodeFarbe(this.XMLNode, selektiert: false);
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

        protected override async Task<PaintContext> PaintInternal(PaintContext paintContext, XMLCursor cursor, IGraphics gfx, PaintModes paintMode)
        {
            this.nodeDimensions.Update();
            var isSelected = cursor.IstNodeInnerhalbDerSelektion(this.XMLNode);
            this.CreateChildElementsIfNeeded();

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
            if (cursor.StartPos.AktNode == this.XMLNode)
            {
                if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorVorDemNode)
                {
                    // Position für Cursor-Strich vermerken
                    newCursorPaintPos = new Point(paintContext.PaintPosX + 1, paintContext.PaintPosY);
                }
            }

            paintContext = await this.startTag.Paint(paintContext, alreadyUnpainted,  isSelected, gfx);

            // If the cursor is inside the empty node, then draw the cursor there
            if (cursor.StartPos.AktNode == this.XMLNode)
            {
                if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorInDemLeeremNode)
                {
                    // set position for cursor line
                    newCursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                }
            }

            paintContext = await this.PaintSubNodes(paintContext, cursor, gfx, paintMode);

            if (this.endTag != null)
            {
                paintContext = await this.endTag.Paint(paintContext, alreadyUnpainted, isSelected, gfx);
            }

            // If the cursor is behind the node, then also draw the cursor there
            if (cursor.StartPos.AktNode == this.XMLNode)
            {
                if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode)
                {
                    this.cursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                }
            }

            this.cursorPaintPos = newCursorPaintPos;

            return paintContext.Clone();
        }

        protected override bool IsClickPosInsideNode(Point pos)
        {
            return false;
        }


        protected override void UnPaint(IGraphics gfx)
        {
            this.startTag.Unpaint(gfx);
            this.endTag?.Unpaint(gfx);
        }


        protected async Task<PaintContext> PaintSubNodes(PaintContext paintContext, XMLCursor cursor, IGraphics gfx, PaintModes paintMode)
        {
            if (this.XMLNode == null)
            {
                throw new ApplicationException("UnternodesZeichnen:XMLNode ist leer");
            }

            var childPaintContext = paintContext.Clone();
            childPaintContext.LimitLeft = paintContext.LimitLeft + this.Config.ChildIndentX;

            for (int childLauf = 0; childLauf < this.XMLNode.ChildNodes.Count; childLauf++)
            {
                // An dieser Stelle sollte im Objekt ChildControl die entsprechends
                // Instanz des XMLElement-Controls für den aktuellen XMLChildNode stehen
                var childElement = (XMLElement)childElements[childLauf];
                switch (this.Regelwerk.DarstellungsArt(childElement.XMLNode))
                {
                    case DarstellungsArten.EigeneZeile:

                        // Dieses Child-Element beginnt eine neue Zeile und wird dann in dieser gezeichnet

                        // Neue Zeile beginnen
                        childPaintContext.LimitLeft = paintContext.LimitLeft + this.Config.ChildIndentX;
                        childPaintContext.PaintPosX = childPaintContext.LimitLeft;
                        childPaintContext.PaintPosY += this.Config.SpaceYBetweenLines + paintContext.HoeheAktZeile; // Zeilenumbruch
                        childPaintContext.HoeheAktZeile = 0; // noch kein Element in dieser Zeile, daher Hoehe 0
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

                        childPaintContext = await childElement.Paint(childPaintContext, cursor, gfx, paintMode);
                        break;

                    case DarstellungsArten.Fliesselement:
                        // Dieses Child ist ein Fliesselement; es fügt sich in die selbe Zeile
                        // ein, wie das vorherige Element und beginnt keine neue Zeile, 
                        // es sei denn, die aktuelle Zeile ist bereits zu lang
                        if (childPaintContext.PaintPosX > paintContext.LimitRight) // Wenn die Zeile bereits zu voll ist
                        {
                            // in nächste Zeile
                            paintContext.PaintPosY += paintContext.HoeheAktZeile + this.Config.SpaceYBetweenLines;
                            paintContext.HoeheAktZeile = 0;
                            paintContext.PaintPosX = paintContext.ZeilenStartX;
                        }
                        else // es passt noch etwas in diese Zeile
                        {
                            // das Child rechts daneben setzen	
                        }

                        childPaintContext = await childElement.Paint(childPaintContext, cursor, gfx, paintMode);
                        break;


                    default:
                        MessageBox.Show("undefiniert");
                        break;
                }
                paintContext.PaintPosX = childPaintContext.PaintPosX;
                paintContext.PaintPosY = childPaintContext.PaintPosY;
            }

            // Sollten wir mehr ChildControls als XMLChildNodes haben, dann diese
            // am Ende der ChildControlListe löschen
            while (this.XMLNode.ChildNodes.Count < childElements.Count)
            {
                var deleteChildElement = (XMLElement)childElements[childElements.Count - 1];
                childElements.Remove(childElements[childElements.Count - 1]);
                deleteChildElement.Dispose();
            }
            return paintContext;
        }


        /// <summary>
        /// Wird aufgerufen, wenn auf dieses Element geklickt wurde
        /// </summary>
        /// <param name="point"></param>
        protected override async Task WurdeAngeklickt(Point point, MausKlickAktionen mouseAction)
        {
            if (this.startTag.AreaTag?.Contains(point) == true) // er wurde auf das linke Tag geklickt
            {
                await EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, mouseAction);
                return;
            }

            if (this.startTag.AreaArrow?.Contains(point) == true) // er wurde auf den linken, öffnenden Pfeil geklickt
            {
                if (this.XMLNode.ChildNodes.Count > 0) // Children vorhanden
                {
                    // vor das erste Child setzen
                    await EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode.LastChild, XMLCursorPositionen.CursorHinterDemNode, mouseAction);
                    return;
                }
                else // Kein Child vorhanden
                {
                    // In den Node selbst setzen
                    await EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorInDemLeeremNode, mouseAction);
                    return;
                }
            }

            if (this.endTag?.AreaTag?.Contains(point) == true) // er wurde auf das rechte Tag geklickt
            {
                await EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstHinteresTag, mouseAction);
                return;
            }

            if (this.endTag?.AreaArrow?.Contains(point) == true) // es wurde auf den rechten, schließenden Pfeil geklickt
            {
                if (this.XMLNode.ChildNodes.Count > 0) // Children vorhanden
                {
                    // vor das erste Child setzen
                    await EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode.FirstChild, XMLCursorPositionen.CursorVorDemNode, mouseAction);
                    return;
                }
                else // Kein Child vorhanden
                {
                    // In den Node selbst setzen
                    await EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorInDemLeeremNode, mouseAction);
                    return;
                }
            }

            // Nicht auf Pfeil geklickt, dann Event weiterreichen an Base-Klasse
            await EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, mouseAction);
            xmlEditor.CursorBlink.ResetBlinkPhase();
        }

        private void CreateChildElementsIfNeeded()
        {
            // Alle Child-Controls anzeigen und ggf. vorher anlegen
            for (int childLauf = 0; childLauf < this.XMLNode.ChildNodes.Count; childLauf++)
            {
                if (childLauf >= childElements.Count)
                {   // Wenn noch nicht so viele ChildControls angelegt sind, wie
                    // es ChildXMLNodes gibt
                    var childElement = this.xmlEditor.CreateElement(this.XMLNode.ChildNodes[childLauf]);
                    childElements.Add(childElement);
                }
                else
                {   // es gibt schon ein Control an dieser Stelle
                    var childElement = (XMLElement)childElements[childLauf];

                    if (childElement == null)
                    {
                        throw new ApplicationException($"UnternodesZeichnen:childElement ist leer: outerxml:{this.XMLNode.OuterXml} >> innerxml {this.XMLNode.InnerXml}");
                    }

                    // prüfen, ob es auch den selben XML-Node vertritt
                    if (childElement.XMLNode != this.XMLNode.ChildNodes[childLauf])
                    {   // Das ChildControl enthält nicht den selben ChildNode, also 
                        // löschen und neu machen
                        childElement.Dispose(); // altes Löschen
                        childElements[childLauf] = this.xmlEditor.CreateElement(this.XMLNode.ChildNodes[childLauf]);
                    }
                }
            }
        }


    }
}
