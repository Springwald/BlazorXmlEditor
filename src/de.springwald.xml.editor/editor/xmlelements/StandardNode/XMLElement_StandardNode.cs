// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.cursor;
using de.springwald.xml.editor.editor.xmlelements;
using de.springwald.xml.editor.editor.xmlelements.StandardNode;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Draws a standard node for the editor
    /// </summary>
    public partial class XMLElement_StandardNode : XMLElement
    {
        private XmlNode node;
        private StandardNodeDimensionsAndColor nodeDimensions;
        private StandardNodeStartTagPainter StartTag;
        private StandardNodeEndTagPainter EndTag;
        //private int lastPaintNodeAbschlussX;
        //private int lastPaintNodeAbschlussY;
        //private PaintContext lastAfterStartNodePaintContext;
        //private PaintContext lastAfterClosingNodePaintContext;
        //private XMLCursor lastPaintCursor;

        protected List<XMLElement> childElements = new List<XMLElement>();   // Die ChildElemente in diesem Steuerelement

        public XMLElement_StandardNode(System.Xml.XmlNode xmlNode, XMLEditor xmlEditor) : base(xmlNode, xmlEditor)
        {
            var isClosingTagVisible = xmlEditor.EditorStatus.Regelwerk.IstSchliessendesTagSichtbar(xmlNode);
            var colorTagBackground = xmlEditor.EditorStatus.Regelwerk.NodeFarbe(this.XMLNode, selektiert: false);
            this.node = XMLNode;
            this.nodeDimensions = new StandardNodeDimensionsAndColor(xmlEditor.EditorConfig, colorTagBackground);
            this.StartTag = new StandardNodeStartTagPainter(this.xmlEditor, this.nodeDimensions, xmlNode, isClosingTagVisible);
            if (isClosingTagVisible)
            {
                this.EndTag = new StandardNodeEndTagPainter(this.xmlEditor, this.nodeDimensions, xmlNode);
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Alle Child-Elemente ebenfalls zerstören
            foreach (XMLElement element in this.childElements)
            {
                if (element != null) element.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override async Task<PaintContext> PaintInternal(PaintContext paintContext, XMLCursor cursor, IGraphics gfx, PaintModes paintMode)
        {
            this.nodeDimensions.Update();
            var isSelected = cursor.IstNodeInnerhalbDerSelektion(this.XMLNode);
            this.CreateChildElementsIfNeeded();

            Point newCursorPaintPos = null;

            // Falls der Cursor innherlb des leeren Nodes steht, dann den Cursor auch dahin zeichnen
            if (cursor.StartPos.AktNode == this.node)
            {
                if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorVorDemNode)
                {
                    // Position für Cursor-Strich vermerken
                    newCursorPaintPos = new Point(paintContext.PaintPosX + 1, paintContext.PaintPosY);
                }
            }

            paintContext = await this.StartTag.Paint(paintContext, cursor, isSelected, gfx);

            // If the cursor is inside the empty node, then draw the cursor there
            if (cursor.StartPos.AktNode == this.node)
            {
                if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorInDemLeeremNode)
                {
                    // set position for cursor line
                    newCursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                }
            }

            paintContext = await this.PaintSubNodes(paintContext, cursor, gfx, paintMode);

            if (this.EndTag != null)
            {
                paintContext = await this.EndTag.Paint(paintContext, cursor, isSelected, gfx);
            }

            // If the cursor is behind the node, then also draw the cursor there
            if (cursor.StartPos.AktNode == this.node)
            {
                if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode)
                {
                    this.cursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                }
            }

            this.cursorPaintPos = newCursorPaintPos;

            //var lastPaintStillUpToDate = this.LastPaintStillUpToDate(paintContext);
            //this.SaveLastPaintPosCacheAttributes(paintContext);

            //switch (paintMode)
            //{
            //    case PaintModes.ForcePaintNoUnPaintNeeded:
            //        lastPaintStillUpToDate = false;
            //        break;

            //    case PaintModes.ForcePaintAndUnpaintBefore:
            //        lastPaintStillUpToDate = false;
            //        this.UnPaint(gfx, paintContext);
            //        break;

            //    case PaintModes.OnlyPaintWhenChanged:
            //        if (lastPaintStillUpToDate == false) 
            //        {
            //            this.UnPaint(gfx, paintContext);
            //        }
            //        break;
            //}

            //if (lastPaintStillUpToDate && this.lastAfterStartNodePaintContext != null)
            //{
            //    paintContext = lastAfterStartNodePaintContext.Clone();
            //}
            //else
            //{
            //    paintContext = await this.StartTag.Paint(paintContext, cursor, isSelected, gfx);
            //    this.lastAfterStartNodePaintContext = paintContext.Clone();
            //}

            //paintContext = await this.PaintSubNodes(paintContext, cursor, gfx, paintMode);

            //if (this.EndTag != null)
            //{
            //    switch (paintMode)
            //    {
            //        case PaintModes.ForcePaintNoUnPaintNeeded:
            //            paintContext = await this.EndTag.Paint(paintContext, cursor, isSelected, gfx);
            //            break;

            //        case PaintModes.ForcePaintAndUnpaintBefore:
            //            this.EndTag.Unpaint(gfx);
            //            paintContext = await this.EndTag.Paint(paintContext, cursor, isSelected,gfx);
            //            break;

            //        case PaintModes.OnlyPaintWhenChanged:
            //            if (lastPaintNodeAbschlussX != paintContext.PaintPosX || lastPaintNodeAbschlussY != paintContext.PaintPosY)
            //            {
            //                this.EndTag.Unpaint(gfx);
            //                lastPaintNodeAbschlussX = paintContext.PaintPosX;
            //                lastPaintNodeAbschlussY = paintContext.PaintPosY;
            //                paintContext = await this.EndTag.Paint(paintContext,  cursor, isSelected, gfx);
            //                this.lastAfterClosingNodePaintContext = paintContext.Clone();
            //            }
            //            else
            //            {
            //                paintContext = this.lastAfterClosingNodePaintContext;
            //            }
            //            break;
            //    }
            //}

            //if (!cursor.Equals(this.lastPaintCursor)) {
            //    this.lastPaintCursor = cursor.Clone();
            //}

            return paintContext.Clone();
        }

        protected override bool IsClickPosInsideNode(Point pos)
        {
            return false;
        }


        protected override void UnPaint(IGraphics gfx, PaintContext paintContext)
        {
            this.StartTag.Unpaint(gfx);
            this.EndTag?.Unpaint(gfx);
        }


        protected async Task<PaintContext> PaintSubNodes(PaintContext paintContext, XMLCursor cursor, IGraphics gfx, PaintModes paintMode)
        {
            if (this.XMLNode == null)
            {
                throw new ApplicationException("UnternodesZeichnen:XMLNode ist leer");
            }

            var childPaintContext = paintContext.Clone();
            childPaintContext.LimitLeft = paintContext.LimitLeft + this.Config.ChildEinrueckungX;

            for (int childLauf = 0; childLauf < this.XMLNode.ChildNodes.Count; childLauf++)
            {
                // An dieser Stelle sollte im Objekt ChildControl die entsprechends
                // Instanz des XMLElement-Controls für den aktuellen XMLChildNode stehen
                var childElement = (XMLElement)childElements[childLauf];
                switch (xmlEditor.EditorStatus.Regelwerk.DarstellungsArt(childElement.XMLNode))
                {
                    case DarstellungsArten.EigeneZeile:

                        // Dieses Child-Element beginnt eine neue Zeile und wird dann in dieser gezeichnet

                        // Neue Zeile beginnen
                        childPaintContext.LimitLeft = paintContext.LimitLeft + this.Config.ChildEinrueckungX;
                        childPaintContext.PaintPosX = childPaintContext.LimitLeft;
                        childPaintContext.PaintPosY += this.Config.AbstandYZwischenZeilen + paintContext.HoeheAktZeile; // Zeilenumbruch
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
                            paintContext.PaintPosY += paintContext.HoeheAktZeile + this.Config.AbstandYZwischenZeilen;
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
        protected override async Task WurdeAngeklickt(Point point, MausKlickAktionen aktion)
        {
            if (this.StartTag.AreaTag?.Contains(point) == true) // er wurde auf das linke Tag geklickt
            {
                await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, aktion);
                return;
            }

            if (this.StartTag.AreaArrow?.Contains(point) == true) // er wurde auf den linken, öffnenden Pfeil geklickt
            {
                if (this.XMLNode.ChildNodes.Count > 0) // Children vorhanden
                {
                    // vor das erste Child setzen
                    await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode.LastChild, XMLCursorPositionen.CursorHinterDemNode, aktion);
                    return;
                }
                else // Kein Child vorhanden
                {
                    // In den Node selbst setzen
                    await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorInDemLeeremNode, aktion);
                    return;
                }
            }

            if (this.EndTag?.AreaTag?.Contains(point) == true) // er wurde auf das rechte Tag geklickt
            {
                await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstHinteresTag, aktion);
                return;
            }

            if (this.EndTag?.AreaArrow?.Contains(point) == true) // es wurde auf den rechten, schließenden Pfeil geklickt
            {
                if (this.XMLNode.ChildNodes.Count > 0) // Children vorhanden
                {
                    // vor das erste Child setzen
                    await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode.FirstChild, XMLCursorPositionen.CursorVorDemNode, aktion);
                    return;
                }
                else // Kein Child vorhanden
                {
                    // In den Node selbst setzen
                    await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorInDemLeeremNode, aktion);
                    return;
                }
            }

            // Nicht auf Pfeil geklickt, dann Event weiterreichen an Base-Klasse
            await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, aktion);
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
