// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.cursor;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Draws a standard node for the editor
    /// </summary>
    public class XMLElement_StandardNode : XMLElement
    {
        private Color _farbeRahmenHintergrund;
        private Color _farbeRahmenRand;
        private Color _farbeNodeNameSchrift;

        private Color _farbeAttributeHintergrund;
        private Color _farbeAttributeRand;
        private Color _farbeAttributeSchrift;

        private Rectangle areaArrowClosingTag;       // Der Klickbereich des linken Pfeiles
        private Rectangle areaArrowStartTag;		// Der Klickbereich des rechten Pfeiles
        private Rectangle areaClosingTag;         // Der Klickbereich des linken Tags
        private Rectangle areaStartTag;        // Der Klickbereich des rechten Tags

        protected List<XMLElement> childElements = new List<XMLElement>();   // Die ChildElemente in diesem Steuerelement

        private const int _rundung = 3;             // Diese Rundung hat der Rahmen

        private int innerMarginX => (this.Config.NodeNameFont.Height) / 2;

        private int attributeMarginY => (this.Config.TagHeight - attributeHeight - attributeInnerMarginY) / 2;

        private int attributeInnerMarginY => Math.Max(1, (this.Config.NodeNameFont.Height - this.Config.NodeAttributeFont.Height) / 2);
        private int attributeHeight => this.Config.NodeAttributeFont.Height + attributeInnerMarginY * 2;

        private int lastPaintNodeAbschlussX;
        private int lastPaintNodeAbschlussY;

        public XMLElement_StandardNode(System.Xml.XmlNode xmlNode, XMLEditor xmlEditor) : base(xmlNode, xmlEditor)
        {
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

        protected override async Task<PaintContext> PaintInternal(PaintContext paintContext, IGraphics gfx, PaintModes paintMode)
        {
            paintContext = await this.NodeZeichnenStart(paintContext, gfx);
            paintContext = await this.PaintSubNodes(paintContext, gfx, paintMode);

            if (xmlEditor.EditorStatus.Regelwerk.IstSchliessendesTagSichtbar(this.XMLNode))
            {
                switch (paintMode)
                {
                    case PaintModes.ForcePaintNoUnPaintNeeded:
                        paintContext = await this.NodeZeichnenAbschluss(paintContext, gfx);
                        break;

                    case PaintModes.ForcePaintAndUnpaintBefore:
                        this.UnPaintNodeAbschluss();
                        paintContext = await this.NodeZeichnenAbschluss(paintContext, gfx);
                        break;

                    case PaintModes.OnlyPaintWhenChanged:
                        if (lastPaintNodeAbschlussX != paintContext.PaintPosX || lastPaintNodeAbschlussY != paintContext.PaintPosY)
                        {
                            this.UnPaintNodeAbschluss();
                            lastPaintNodeAbschlussX = paintContext.PaintPosX;
                            lastPaintNodeAbschlussY = paintContext.PaintPosY;
                            paintContext = await this.NodeZeichnenAbschluss(paintContext, gfx);
                        }
                        break;
                }
            }
            
            return paintContext.Clone();
        }

        protected override bool IsClickPosInsideNode(Point pos)
        {
            return false;
        }

        protected override bool LastPaintStillUpToDate(PaintContext paintContext)
        {
            return false;
        }

        protected override void UnPaint(IGraphics gfx, PaintContext paintContext)
        {
            this.UnPaintNodeStart();
            if (xmlEditor.EditorStatus.Regelwerk.IstSchliessendesTagSichtbar(this.XMLNode))
            {
                this.UnPaintNodeAbschluss();
            }
        }

        private void UnPaintNodeAbschluss()
        {
        }

        private void UnPaintNodeStart()
        {
        }

        protected async Task<PaintContext> PaintSubNodes(PaintContext paintContext, IGraphics gfx, PaintModes paintMode)
        {
            if (this.XMLNode == null)
            {
                throw new ApplicationException("UnternodesZeichnen:XMLNode ist leer");
            }

            this.CreateChildElementsIfNeeded();

            var childPaintContext = paintContext.Clone();
            childPaintContext.LimitLeft+= xmlEditor.EditorStatus.Regelwerk.ChildEinrueckungX;

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
                        childPaintContext.LimitLeft = paintContext.LimitLeft + xmlEditor.EditorStatus.Regelwerk.ChildEinrueckungX;
                        childPaintContext.PaintPosX = childPaintContext.LimitLeft;
                        childPaintContext.PaintPosY += xmlEditor.EditorStatus.Regelwerk.AbstandYZwischenZeilen + paintContext.HoeheAktZeile; // Zeilenumbruch
                        childPaintContext.HoeheAktZeile = 0; // noch kein Element in dieser Zeile, daher Hoehe 0
                                                             // X-Cursor auf den Start der neuen Zeile setzen
                                                             // Linie nach unten und dann nach rechts ins ChildElement
                                                             // Linie nach unten
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

                        childPaintContext = await childElement.Paint(childPaintContext, gfx, paintMode);
                        break;


                    case DarstellungsArten.Fliesselement:
                        // Dieses Child ist ein Fliesselement; es fügt sich in die selbe Zeile
                        // ein, wie das vorherige Element und beginnt keine neue Zeile, 
                        // es sei denn, die aktuelle Zeile ist bereits zu lang
                        if (childPaintContext.PaintPosX > paintContext.LimitRight) // Wenn die Zeile bereits zu voll ist
                        {
                            // in nächste Zeile
                            paintContext.PaintPosY += paintContext.HoeheAktZeile + xmlEditor.EditorStatus.Regelwerk.AbstandYZwischenZeilen;
                            paintContext.HoeheAktZeile = 0;
                            paintContext.PaintPosX = paintContext.ZeilenStartX;
                        }
                        else // es passt noch etwas in diese Zeile
                        {
                            // das Child rechts daneben setzen	
                        }

                        childPaintContext = await childElement.Paint(childPaintContext, gfx, paintMode);
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

        /// <summary>
        /// Zeichnet die Grafik des aktuellen Nodes
        /// </summary>
        protected async Task<PaintContext> NodeZeichnenStart(PaintContext paintContext, IGraphics gfx)
        {
            var startX = paintContext.PaintPosX;
            var startY = paintContext.PaintPosY;

            // Falls der Cursor innherlb des leeren Nodes steht, dann den Cursor auch dahin zeichnen
            if (xmlEditor.EditorStatus.CursorOptimiert.StartPos.AktNode == this.XMLNode)
            {
                if (xmlEditor.EditorStatus.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorVorDemNode)
                {
                    // Position für Cursor-Strich vermerken
                    this.cursorPaintPos = new Point(paintContext.PaintPosX + 1, paintContext.PaintPosY);
                }
            }

            this.DefineColors();

            // ### Write the name of the node ###

            // Pre-calculate the width of the node name
            int nodeNameTextWidth = (int)(await this.xmlEditor.NativePlatform.Gfx.MeasureDisplayStringWidthAsync(this.XMLNode.Name, xmlEditor.EditorConfig.NodeNameFont));

            // Pre-calculate the width of the attribute string
            var attributeString = this.GetAttributeString();
            var attributeTextWidth = await this.GetAttributeTextWidth(attributeString, gfx);

            // draw tag start border
            var borderWidth =
                innerMarginX // margin to left border
                + nodeNameTextWidth // node name
                + (attributeTextWidth == 0 ? 0 : innerMarginX + attributeTextWidth) // attributes
                + innerMarginX; // margin to right border

            zeichneRahmenNachGroesse(GfxJob.Layers.TagBackground, startX, paintContext.PaintPosY, borderWidth, this.Config.TagHeight, _rundung, _farbeRahmenHintergrund, _farbeRahmenRand, gfx);

            paintContext.PaintPosX += innerMarginX;  // margin to left border

            // draw node name
            gfx.AddJob(new JobDrawString
            {
                Batchable = false,
                Layer = GfxJob.Layers.Text,
                Text = this.XMLNode.Name,
                Color = _farbeNodeNameSchrift,
                X = paintContext.PaintPosX,
                Y = paintContext.PaintPosY + this.Config.InnerMarginY,
                Font = xmlEditor.EditorConfig.NodeNameFont
            });

            paintContext.PaintPosX += nodeNameTextWidth + innerMarginX;

            // draw the attributes
            await AttributeZeichnen(paintContext, attributeString, gfx);

            // standard distance + one pixel to the right, otherwise we draw on the frame line
            paintContext.PaintPosX += 1;

            // if necessary draw the continuing arrow at the end of the frame 
            if (xmlEditor.EditorStatus.Regelwerk.IstSchliessendesTagSichtbar(this.XMLNode))
            {
                var point1 = new Point(paintContext.PaintPosX, paintContext.PaintPosY + this.Config.InnerMarginY);
                var point2 = new Point(paintContext.PaintPosX + innerMarginX, paintContext.PaintPosY + this.Config.TagHeight / 2);
                var point3 = new Point(paintContext.PaintPosX, paintContext.PaintPosY + this.Config.TagHeight - this.Config.InnerMarginY);
                gfx.AddJob(new JobDrawPolygon
                {
                    Batchable = true,
                    Layer = GfxJob.Layers.TagBorder,
                    FillColor = _farbeRahmenRand,
                    Points = new[] { point1, point2, point3 }
                });

                // Remember the right arrow area
                areaArrowClosingTag = new Rectangle(paintContext.PaintPosX, paintContext.PaintPosY, innerMarginX, this.Config.TagHeight);
                paintContext.PaintPosX += innerMarginX;
            }
            else
            {
                areaArrowClosingTag = new Rectangle(0, 0, 0, 0);
            }

            // If the cursor is inside the empty node, then draw the cursor there
            if (xmlEditor.EditorStatus.CursorOptimiert.StartPos.AktNode == this.XMLNode)
            {
                if (xmlEditor.EditorStatus.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorInDemLeeremNode)
                {
                    // set position for cursor line
                    this.cursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                }
            }

            paintContext.HoeheAktZeile = System.Math.Max(paintContext.HoeheAktZeile, this.Config.TagHeight); // See how high the current line is

            // Remember where the mouse areas are
            areaClosingTag = new Rectangle(startX, startY, paintContext.PaintPosX - startX, this.Config.TagHeight);

            // this._klickBereiche = this._klickBereiche.Append(_tagBereichLinks).ToArray();

            // If there is no closing tag, the cursor line at "behind the node" is drawn directly after the first and only tag
            if (!xmlEditor.EditorStatus.Regelwerk.IstSchliessendesTagSichtbar(this.XMLNode))
            {
                // If the cursor is behind the node, then also draw the cursor there
                if (xmlEditor.EditorStatus.CursorOptimiert.StartPos.AktNode == this.XMLNode)
                {
                    if (xmlEditor.EditorStatus.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode)
                    {
                        // set position for cursor line
                        this.cursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                    }
                }
            }
            paintContext.BisherMaxX = System.Math.Max(paintContext.BisherMaxX, paintContext.PaintPosX);

            return paintContext;
        }

        /// <summary>
        /// Draws the attributes 
        /// </summary>
        private async Task AttributeZeichnen(PaintContext paintContext, string attributeString, IGraphics gfx)
        {
            if (string.IsNullOrWhiteSpace(attributeString)) return;

            var attributeBreite = await this.GetAttributeTextWidth(attributeString, gfx);

            // draw a frame around the attributes
            zeichneRahmenNachGroesse(GfxJob.Layers.AttributeBackground,
                paintContext.PaintPosX, paintContext.PaintPosY + attributeMarginY,
                attributeBreite, attributeHeight, 2,
              _farbeAttributeHintergrund, _farbeAttributeRand, gfx);

            // Draw attributes
            gfx.AddJob(new JobDrawString
            {
                Batchable = false,
                Layer = GfxJob.Layers.Text,
                Text = attributeString.ToString(),
                Color = _farbeAttributeSchrift,
                X = paintContext.PaintPosX,
                Y = paintContext.PaintPosY + attributeMarginY + attributeInnerMarginY + 1,
                Font = xmlEditor.EditorConfig.NodeAttributeFont
            });

            // Set character cursor behind the attributes
            paintContext.PaintPosX += attributeBreite + innerMarginX;
        }

        protected async Task<PaintContext> NodeZeichnenAbschluss(PaintContext paintContext, IGraphics gfx)
        {
            // Falls der Cursor hinter dem letzten Child dieses Nodes steht, dann
            // den Cursor auch dahin zeichnen
            //if ((_xmlEditor.Cursor.AktNode == _xmlNode) && (_xmlEditor.Cursor.PosInNode == (int)XMLCursorPositionen.CursorHinterLetztemChild)) 
            //{
            //	e.Graphics.DrawLine (new Pen(Color.Black,1),_paintPos.PosX, _paintPos.PosY+2 ,_paintPos.PosX, _paintPos.PosY + 2 +_drawFontNodeName.Height);
            //}

            if (xmlEditor.EditorStatus.Regelwerk.IstSchliessendesTagSichtbar(this.XMLNode))
            {
                // Die Breite vorausberechnen
                int schriftBreite = (int)(await gfx.MeasureDisplayStringWidthAsync(this.XMLNode.Name, xmlEditor.EditorConfig.NodeNameFont));

                var esteemedWidth = schriftBreite + innerMarginX * 3;
                if (paintContext.PaintPosX + esteemedWidth > paintContext.LimitRight && paintContext.PaintPosX != paintContext.LimitLeft)
                {
                    paintContext.PaintPosX = paintContext.LimitLeft + xmlEditor.EditorStatus.Regelwerk.ChildEinrueckungX;
                    paintContext.PaintPosY += paintContext.HoeheAktZeile;
                }

                int startX = paintContext.PaintPosX;

                // vor dem Noderahmen einen Pfeil nach links zeichnen
                // Pfeil nach links
                Point point1 = new Point(paintContext.PaintPosX + innerMarginX, paintContext.PaintPosY + this.Config.InnerMarginY);
                Point point2 = new Point(paintContext.PaintPosX + innerMarginX, paintContext.PaintPosY + this.Config.TagHeight - this.Config.InnerMarginY);
                Point point3 = new Point(paintContext.PaintPosX, paintContext.PaintPosY + this.Config.TagHeight / 2);
                gfx.AddJob(new JobDrawPolygon
                {
                    Batchable = true,
                    Layer = GfxJob.Layers.TagBorder,
                    FillColor = _farbeRahmenRand,
                    Points = new[] { point1, point2, point3 }
                });

                // Den rechten Pfeilbereich merken
                areaArrowStartTag = new Rectangle(paintContext.PaintPosX, paintContext.PaintPosY, innerMarginX, this.Config.TagHeight);
                paintContext.PaintPosX += innerMarginX + 1; // Zeichnungscursor hinter den Pfeil setzen

                // ## RAHMEN für schließenden Node  zeichnen ###
                zeichneRahmenNachGroesse(GfxJob.Layers.TagBackground, paintContext.PaintPosX, paintContext.PaintPosY, schriftBreite + innerMarginX * 2, this.Config.TagHeight, _rundung, _farbeRahmenHintergrund, _farbeRahmenRand, gfx);
                paintContext.PaintPosX += innerMarginX; // Abstand zwischen Rahmen und Schrift

                // ## Name für schließenden Node zeichnen ###
                gfx.AddJob(new JobDrawString
                {
                    Batchable = false,
                    Layer = GfxJob.Layers.Text,
                    Text = this.XMLNode.Name,
                    Color = _farbeNodeNameSchrift,
                    X = paintContext.PaintPosX,
                    Y = paintContext.PaintPosY + this.Config.InnerMarginY,
                    Font = xmlEditor.EditorConfig.NodeNameFont
                });

                paintContext.PaintPosX += schriftBreite + innerMarginX; // Distance between font and frame

                // One pixel to the right, because otherwise we draw on the frame line and the cursor flashes on the frame
                paintContext.PaintPosX++;

                // Remember where the mouse areas are
                areaStartTag = new Rectangle(startX, paintContext.PaintPosY, paintContext.PaintPosX - startX, this.Config.TagHeight);
                //this._klickBereiche = this._klickBereiche.Append(_tagBereichRechts).ToArray(); // original:  _klickBereiche.Add(_tagBereichRechts);

                // If the cursor is behind the node, then also draw the cursor there
                if (xmlEditor.EditorStatus.CursorOptimiert.StartPos.AktNode == this.XMLNode)
                {
                    if (xmlEditor.EditorStatus.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode)
                    {
                        this.cursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                    }
                }
                paintContext.BisherMaxX = System.Math.Max(paintContext.BisherMaxX, paintContext.PaintPosX);
            }
            else
            {
                areaArrowStartTag = new Rectangle(0, 0, 0, 0);
                areaStartTag = new Rectangle(0, 0, 0, 0);
            }
            return paintContext;
        }
        private async Task<int> GetAttributeTextWidth(string attributeString, IGraphics gfx)
        {
            if (string.IsNullOrEmpty(attributeString)) return 0;
            return (int)await gfx.MeasureDisplayStringWidthAsync(attributeString, this.xmlEditor.EditorConfig.NodeAttributeFont);
        }

        private string GetAttributeString()
        {
            System.Xml.XmlAttributeCollection attribute = this.XMLNode.Attributes; // Attribs auf Kurznamen umlegen
            if (attribute == null) return null;
            if (attribute.Count == 0) return null;

            StringBuilder attributeString = new StringBuilder();
            for (int attribLauf = 0; attribLauf < attribute.Count; attribLauf++)
            {
                attributeString.AppendFormat(" {0}=\"{1}\"", attribute[attribLauf].Name, attribute[attribLauf].Value);
            }
            return attributeString.ToString();
        }

        /// <summary>
        /// Zeichnet einen Rahmen unter Angabe der Punkte oben/links und unten/rechts
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="fuellFarbe"></param>
        /// <param name="rahmenFarbe"></param>
        private void zeichneRahmenNachKoordinaten(GfxJob.Layers layer, int x1, int y1, int x2, int y2, int rundung, Color fuellFarbe, Color rahmenFarbe, IGraphics gfx)
        {
            Point[] points = new[] {
                new Point(x1 + rundung, y1),
                new Point(x2 - rundung, y1),
                new Point(x2, y1 + rundung),
                new Point(x2, y2 - rundung),
                new Point(x2 - rundung, y2),
                new Point(x1 + rundung, y2),
                new Point(x1, y2 - rundung),
                new Point(x1, y1 + rundung)};

            // Rahmen zeichnen
            gfx.AddJob(new JobDrawPolygon
            {
                Batchable = true,
                Layer = layer,
                FillColor = fuellFarbe == Color.Transparent ? null : fuellFarbe,
                BorderColor = rahmenFarbe == Color.Transparent ? null : rahmenFarbe,
                BorderWidth = 1,
                Points = points
            });
        }

        /// <summary>
        /// Zeichnet einen Rahmen unter Angabe des Punktes oben/links und der Größe
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="breite"></param>
        /// <param name="hoehe"></param>
        /// <param name="fuellFarbe"></param>
        /// <param name="rahmenFarbe"></param>
        /// <param name="e"></param>
        private void zeichneRahmenNachGroesse(GfxJob.Layers layer, int x, int y, int breite, int hoehe, int rundung, Color fuellFarbe, Color rahmenFarbe, IGraphics gfx)
        {
            this.zeichneRahmenNachKoordinaten(layer, x, y, x + breite, y + hoehe, rundung, fuellFarbe, rahmenFarbe, gfx);
        }

        /// <summary>
        /// Wird aufgerufen, wenn auf dieses Element geklickt wurde
        /// </summary>
        /// <param name="point"></param>
        protected override async Task WurdeAngeklickt(Point point, MausKlickAktionen aktion)
        {
            if (areaArrowClosingTag.Contains(point)) // es wurde auf den rechten, schließenden Pfeil geklickt
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

            if (areaArrowStartTag.Contains(point)) // er wurde auf den linken, öffnenden Pfeil geklickt
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

            if (areaClosingTag.Contains(point)) // er wurde auf das linke Tag geklickt
            {
                await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, aktion);
                return;
            }

            if (areaStartTag.Contains(point)) // er wurde auf das rechte Tag geklickt
            {
                await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstHinteresTag, aktion);
                return;
            }

            // Nicht auf Pfeil geklickt, dann Event weiterreichen an Base-Klasse
            await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, aktion);
            xmlEditor.CursorBlink.ResetBlinkPhase();
        }

        /// <summary>
        /// Vertauscht die Vorder- und Hintergrundfarben, um den Node selektiert darstellen zu können
        /// </summary>
        private void DefineColors()
        {
            if (this.xmlEditor.EditorStatus.CursorOptimiert.IstNodeInnerhalbDerSelektion(this.XMLNode))
            {
                // Selektiert
                _farbeRahmenHintergrund = xmlEditor.EditorStatus.Regelwerk.NodeFarbe(this.XMLNode, true);
                _farbeNodeNameSchrift = Color.White;
                _farbeAttributeHintergrund = Color.Transparent;
                _farbeAttributeSchrift = Color.White;
            }
            else
            {
                // nicht selektiert
                _farbeRahmenHintergrund = xmlEditor.EditorStatus.Regelwerk.NodeFarbe(this.XMLNode, false);
                _farbeNodeNameSchrift = Color.Black;
                _farbeAttributeHintergrund = Color.White;// Color.FromArgb(225, 225, 225);
                _farbeAttributeSchrift = Color.Black;
            }
            _farbeAttributeRand = Color.FromArgb(225, 225, 225);
            _farbeRahmenRand = Color.FromArgb(100, 100, 150);

            //if (paintArt == XMLPaintArten.AllesNeuZeichnenMitFehlerHighlighting)
            //{
            //    // Wenn der Node laut DTD defekt ist, dann Farben überschreiben
            //    if (!this._xmlEditor.Regelwerk.DTDPruefer.IstXmlNodeOk(this.XMLNode, false))
            //    {
            //        _farbeNodeNameSchrift = Color.Red;
            //        _farbePfeil = Color.Red;
            //        _farbeRahmenRand = Color.Red;
            //    }
            //}
        }


    }
}
