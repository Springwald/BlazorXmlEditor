using de.springwald.xml.cursor;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Zusammenfassung für XMLElement_StandardNode.
    /// </summary>
    /// <remarks>
    /// (C)2005 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    public class XMLElement_StandardNode : XMLElement
    {
        private Color _farbeRahmenHintergrund;
        private Color _farbeRahmenRand;
        private Color _farbeNodeNameSchrift;
        private Color _farbePfeil;

        private Color _farbeAttributeHintergrund;
        private Color _farbeAttributeRand;
        private Color _farbeAttributeSchrift;

        private Rectangle _pfeilBereichLinks;       // Der Klickbereich des linken Pfeiles
        private Rectangle _pfeilBereichRechts;		// Der Klickbereich des rechten Pfeiles
        private Rectangle _tagBereichLinks;         // Der Klickbereich des linken Tags
        private Rectangle _tagBereichRechts;        // Der Klickbereich des rechten Tags

        private const int _rundung = 3;             // Diese Rundung hat der Rahmen
        private const int _pfeilLaenge = 7;         // so dick ist der zu zeichnende Pfeil
        private const int _pfeilDicke = 7;          // so lang ist der zu zeichnende Pfeil

        private int innerMarginX => (this._xmlEditor.EditorConfig.NodeNameFont.Height) / 1;

        private int innerMarginY => Math.Max(1, this._xmlEditor.EditorConfig.NodeNameFont.Height / 2);

        private int tagHeight => this._xmlEditor.EditorConfig.NodeNameFont.Height + innerMarginY * 2;

        private int attributeHeight => this._xmlEditor.EditorConfig.NodeAttributeFont.Height + 2;

        private int attributeMarginY => (tagHeight - attributeHeight) / 2;

        public override int LineHeight => tagHeight;

        /// <summary>
        /// Dort sollte der Ast des Baumes ankleben, wenn dieses Element in einem Ast des Parent gezeichnet werden soll
        /// </summary>
        /// <returns></returns>
        //protected override Point AnkerPos
        //{
        //    get { return new Point(_startX - 4, _startY + _ankerEinzugY); }
        //}

        public XMLElement_StandardNode(System.Xml.XmlNode xmlNode, de.springwald.xml.editor.XMLEditor xmlEditor) : base(xmlNode, xmlEditor)
        {
        }

        /// <summary>
        /// Zeichnet die Grafik des aktuellen Nodes
        /// </summary>
        protected override async Task NodeZeichnenStart(PaintContext paintContext, PaintEventArgs e)
        {
            var startX = paintContext.PaintPosX;
            var startY = paintContext.PaintPosY;

            // Falls der Cursor innherlb des leeren Nodes steht, dann den Cursor auch dahin zeichnen
            if (_xmlEditor.CursorOptimiert.StartPos.AktNode == this.XMLNode)
            {
                if (_xmlEditor.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorVorDemNode)
                {
                    // Position für Cursor-Strich vermerken
                    this._cursorStrichPos = new Point(paintContext.PaintPosX + 1, paintContext.PaintPosY);
                }
            }

            this.FarbenSetzen();

            // ### Den Namen des Nodes schreiben ###
            // Pinsel für Node-Name-Schrift bereitstellen
            var drawBrush = new SolidBrush(_farbeNodeNameSchrift);

            // Pre-calculate the width of the node name
            int nodeNameTextWidth = (int)(await this._xmlEditor.NativePlatform.Gfx.MeasureDisplayStringWidthAsync(this.XMLNode.Name, _xmlEditor.EditorConfig.NodeNameFont));

            // Pre-calculate the width of the attribute string
            var attributeString = this.GetAttributeString();
            var attributeTextWidth = await this.GetAttributeTextWidth(attributeString, e);

            // draw tag start border
            var borderWidth =
                innerMarginX // margin to left border
                + nodeNameTextWidth // node name
                + (attributeTextWidth == 0 ? 0 : innerMarginX + attributeTextWidth) // attributes
                + innerMarginX; // margin to right border

            await zeichneRahmenNachGroesse(startX, paintContext.PaintPosY, borderWidth, tagHeight, _rundung, _farbeRahmenHintergrund, _farbeRahmenRand, e);

            paintContext.PaintPosX += innerMarginX;  // margin to left border

            // draw node name
            await e.Graphics.DrawStringAsync(this.XMLNode.Name, _xmlEditor.EditorConfig.NodeNameFont, drawBrush, paintContext.PaintPosX, paintContext.PaintPosY + innerMarginY);
            paintContext.PaintPosX += nodeNameTextWidth + innerMarginX; 

            // draw the attributes
            await AttributeZeichnen(paintContext, attributeString, e);

            // standard distance + one pixel to the right, otherwise we draw on the frame line
            paintContext.PaintPosX += 1;

            // if necessary draw the continuing arrow at the end of the frame 
            if (_xmlEditor.Regelwerk.IstSchliessendesTagSichtbar(this.XMLNode))
            {
                var brush = new SolidBrush(_farbePfeil);
                int x = paintContext.PaintPosX;
                int y = paintContext.PaintPosY + tagHeight / 2;
                var point1 = new Point(x, y - _pfeilDicke);
                var point2 = new Point(x + _pfeilLaenge, y);
                var point3 = new Point(x, y + _pfeilDicke);
                Point[] points = { point1, point2, point3 };
                await e.Graphics.FillPolygonAsync(brush, points);  

                // Den rechten Pfeilbereich merken
                _pfeilBereichLinks = new Rectangle(x, y - _pfeilDicke, _pfeilLaenge, _pfeilDicke * 2);
                paintContext.PaintPosX += _pfeilLaenge;
            }
            else
            {
                _pfeilBereichLinks = new Rectangle(0, 0, 0, 0);
            }

            // Falls der Cursor innherlb des leeren Nodes steht, dann den Cursor auch dahin zeichnen
            if (_xmlEditor.CursorOptimiert.StartPos.AktNode == this.XMLNode)
            {
                if (_xmlEditor.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorInDemLeeremNode)
                {
                    // Position für Cursor-Strich vermerken
                    this._cursorStrichPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                }
            }

            paintContext.HoeheAktZeile = System.Math.Max(paintContext.HoeheAktZeile, tagHeight); // Schauen, wie hoch die aktuelle Zeile ist

            // Merken, wo die Mausbereiche sind
            _tagBereichLinks = new Rectangle(startX, startY, paintContext.PaintPosX - startX, tagHeight);

            this._klickBereiche = this._klickBereiche.Append(_tagBereichLinks).ToArray(); // original: this._klickBereiche.Add(_tagBereichLinks);

            // Wenn es kein schließendes Tag gibt, dann wird der Cursorstrich bei "hinter dem Node" direkt nach dem
            // ersten und einzigen Tag gezeichnet
            if (!_xmlEditor.Regelwerk.IstSchliessendesTagSichtbar(this.XMLNode))
            {
                // Falls der Cursor hinter dem Node stehen, dann den Cursor auch dahin zeichnen
                if (_xmlEditor.CursorOptimiert.StartPos.AktNode == this.XMLNode)
                {
                    if (_xmlEditor.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode)
                    {
                        // Position für Cursor-Strich vermerken
                        this._cursorStrichPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                    }
                }
            }

            paintContext.BisherMaxX = System.Math.Max(paintContext.BisherMaxX, paintContext.PaintPosX);
        }

        /// <summary>
        /// Zeichnet den Bereich der Attribute 
        /// </summary>
        private async Task AttributeZeichnen(PaintContext paintContext, string attributeString, PaintEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(attributeString)) return;

            // Wenn Attribute an diesem XML-Node sind, dann anzeigen
            var attributeBreite = await this.GetAttributeTextWidth(attributeString, e);

            // einen Rahmen um die Attribute zeichnen
            await zeichneRahmenNachGroesse(paintContext.PaintPosX, paintContext.PaintPosY + attributeMarginY, attributeBreite, attributeHeight, 2, _farbeAttributeHintergrund, _farbeAttributeRand, e);

            // Pinsel bereitstellen
            SolidBrush drawBrush = new SolidBrush(_farbeAttributeSchrift);

            // Attribute zeichnen
            var attributeInnerMarginY = (attributeHeight - this._xmlEditor.EditorConfig.NodeAttributeFont.Height) / 2;
            await e.Graphics.DrawStringAsync(attributeString.ToString(), this._xmlEditor.EditorConfig.NodeAttributeFont, drawBrush, paintContext.PaintPosX, paintContext.PaintPosY + attributeMarginY + attributeInnerMarginY); ;

            // Zeichencursor hinter die Attribute setzen
            paintContext.PaintPosX += attributeBreite + innerMarginX;  
        }

        protected override async Task NodeZeichnenAbschluss(PaintContext paintContext, PaintEventArgs e)
        {
            if (e != null) // wenn im Paint-Modus
            {
                // Falls der Cursor hinter dem letzten Child dieses Nodes steht, dann
                // den Cursor auch dahin zeichnen
                //if ((_xmlEditor.Cursor.AktNode == _xmlNode) && (_xmlEditor.Cursor.PosInNode == (int)XMLCursorPositionen.CursorHinterLetztemChild)) 
                //{
                //	e.Graphics.DrawLine (new Pen(Color.Black,1),_paintPos.PosX, _paintPos.PosY+2 ,_paintPos.PosX, _paintPos.PosY + 2 +_drawFontNodeName.Height);
                //}

                if (_xmlEditor.Regelwerk.IstSchliessendesTagSichtbar(this.XMLNode))
                {
                    int startX = paintContext.PaintPosX;

                    // vor dem Noderahmen einen Pfeil nach links zeichnen
                    // Pfeil nach links
                    SolidBrush brush = new SolidBrush(_farbePfeil);
                    int x = paintContext.PaintPosX;
                    int y = paintContext.PaintPosY + tagHeight / 2;
                    Point point1 = new Point(x + _pfeilLaenge, y - _pfeilDicke);
                    Point point2 = new Point(x, y);
                    Point point3 = new Point(x + _pfeilLaenge, y + _pfeilDicke);
                    Point[] points = { point1, point2, point3 };
                    await e.Graphics.FillPolygonAsync(brush, points);  // Fill polygon to screen.

                    // Den rechten Pfeilbereich merken
                    _pfeilBereichRechts = new Rectangle(x, y - _pfeilDicke, _pfeilLaenge, _pfeilDicke * 2);
                    paintContext.PaintPosX += _pfeilLaenge; // Zeichnungscursor hinter den Pfeil setzen

                    // Die Breite vorausberechnen
                    int schriftBreite = (int)(await e.Graphics.MeasureDisplayStringWidthAsync(this.XMLNode.Name, _xmlEditor.EditorConfig.NodeNameFont));

                    // ## RAHMEN für schließenden Node  zeichnen ###
                    await zeichneRahmenNachGroesse(paintContext.PaintPosX, paintContext.PaintPosY, schriftBreite + innerMarginX * 2, tagHeight, _rundung, _farbeRahmenHintergrund, _farbeRahmenRand, e);
                    paintContext.PaintPosX += innerMarginX; // Abstand zwischen Rahmen und Schrift

                    // ## Name für schließenden Node zeichnen ###
                    // Pinsel bereitstellen
                    SolidBrush drawBrush = new SolidBrush(_farbeNodeNameSchrift);
                    // Den Namen des Nodes schreiben
                    await e.Graphics.DrawStringAsync(this.XMLNode.Name, _xmlEditor.EditorConfig.NodeNameFont, drawBrush, paintContext.PaintPosX, paintContext.PaintPosY + innerMarginY);
                    paintContext.PaintPosX += schriftBreite + innerMarginX; // Abstand zwischen Schrift und Rahmen

                    // Ein Pixel weiter nach rechts, weil wir sonst auf der Rahmenlinie zeichnen
                    // und der Cursor sonst auf dem Rahmen blinkt
                    paintContext.PaintPosX++;

                    // Merken, wo die Mausbereiche sind
                    _tagBereichRechts = new Rectangle(startX, paintContext.PaintPosY, paintContext.PaintPosX - startX, tagHeight);
                    this._klickBereiche = this._klickBereiche.Append(_tagBereichRechts).ToArray(); // original:  _klickBereiche.Add(_tagBereichRechts);

                    // Falls der Cursor hinter dem Node stehen, dann den Cursor auch dahin zeichnen
                    if (_xmlEditor.CursorOptimiert.StartPos.AktNode == this.XMLNode)
                    {
                        if (_xmlEditor.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode)
                        {
                            // Position für Cursor-Strich vermerken
                            this._cursorStrichPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                        }
                    }
                    paintContext.BisherMaxX = System.Math.Max(paintContext.BisherMaxX, paintContext.PaintPosX);
                }
                else
                {
                    _pfeilBereichRechts = new Rectangle(0, 0, 0, 0);
                    _tagBereichRechts = new Rectangle(0, 0, 0, 0);
                }
            }
            await base.NodeZeichnenAbschluss(paintContext, e);
        }




        private async Task<int> GetAttributeTextWidth(string attributeString, PaintEventArgs e)
        {
            if (string.IsNullOrEmpty(attributeString)) return 0;
            return (int)await e.Graphics.MeasureDisplayStringWidthAsync(attributeString, this._xmlEditor.EditorConfig.NodeAttributeFont);
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
        private async Task zeichneRahmenNachKoordinaten(int x1, int y1, int x2, int y2, int rundung, Color fuellFarbe, Color rahmenFarbe, PaintEventArgs e)
        {
            // Rahmen definieren
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(x1 + rundung, y1, x2 - rundung, y1); //a
            gp.AddLine(x2, y1 + rundung, x2, y2 - rundung); //b
            gp.AddLine(x2 - rundung, y2, x1 + rundung, y2); //d
            gp.AddLine(x1, y2 - rundung, x1, y1 + rundung); //e
            gp.CloseFigure();

            // mit Farbe fuellen
            if (fuellFarbe != Color.Transparent)
            {
                SolidBrush newBrush = new SolidBrush(fuellFarbe);
                await e.Graphics.FillPathAsync(newBrush, gp);
            }

            // Rahmen zeichnen
            if (rahmenFarbe != Color.Transparent)
            {
                var pen = new Pen(color: rahmenFarbe, width: 1);
                pen.EndCap = Pen.LineCap.NoAnchor;
                pen.StartCap = Pen.LineCap.NoAnchor;
                await e.Graphics.DrawPathAsync(pen, gp);
            }
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
        private async Task zeichneRahmenNachGroesse(int x, int y, int breite, int hoehe, int rundung, Color fuellFarbe, Color rahmenFarbe, PaintEventArgs e)
        {
            await zeichneRahmenNachKoordinaten(x, y, x + breite, y + hoehe, rundung, fuellFarbe, rahmenFarbe, e);
        }

        /// <summary>
        /// Wird aufgerufen, wenn auf dieses Element geklickt wurde
        /// </summary>
        /// <param name="point"></param>
        protected override async Task WurdeAngeklickt(Point point, MausKlickAktionen aktion)
        {
            if (_pfeilBereichLinks.Contains(point)) // es wurde auf den rechten, schließenden Pfeil geklickt
            {
                if (this.XMLNode.ChildNodes.Count > 0) // Children vorhanden
                {
                    // vor das erste Child setzen
                    await _xmlEditor.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode.FirstChild, XMLCursorPositionen.CursorVorDemNode, aktion);
                    return;
                }
                else // Kein Child vorhanden
                {
                    // In den Node selbst setzen
                    await _xmlEditor.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorInDemLeeremNode, aktion);
                    return;
                }
            }

            if (_pfeilBereichRechts.Contains(point)) // er wurde auf den linken, öffnenden Pfeil geklickt
            {
                if (this.XMLNode.ChildNodes.Count > 0) // Children vorhanden
                {
                    // vor das erste Child setzen
                    await _xmlEditor.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode.LastChild, XMLCursorPositionen.CursorHinterDemNode, aktion);
                    return;
                }
                else // Kein Child vorhanden
                {
                    // In den Node selbst setzen
                    await _xmlEditor.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorInDemLeeremNode, aktion);
                    return;
                }
            }

            if (_tagBereichLinks.Contains(point)) // er wurde auf das linke Tag geklickt
            {
                await _xmlEditor.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, aktion);
                return;
            }

            if (_tagBereichRechts.Contains(point)) // er wurde auf das rechte Tag geklickt
            {
                await _xmlEditor.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstHinteresTag, aktion);
                return;
            }
            await base.WurdeAngeklickt(point, aktion); // Nicht auf Pfeil geklickt, dann Event weiterreichen an Base-Klasse
        }

        /// <summary>
        /// Vertauscht die Vorder- und Hintergrundfarben, um den Node selektiert darstellen zu können
        /// </summary>
        private void FarbenSetzen()
        {
            if (this._xmlEditor.CursorOptimiert.IstNodeInnerhalbDerSelektion(this.XMLNode))
            {
                // Selektiert
                _farbeRahmenHintergrund = _xmlEditor.Regelwerk.NodeFarbe(this.XMLNode, true);
                _farbeNodeNameSchrift = Color.White;
                _farbePfeil = Color.Black;
                _farbeAttributeHintergrund = Color.Transparent;
                _farbeAttributeSchrift = Color.White;
            }
            else
            {
                // nicht selektiert
                _farbeRahmenHintergrund = _xmlEditor.Regelwerk.NodeFarbe(this.XMLNode, false);
                _farbeNodeNameSchrift = Color.Black;
                _farbePfeil = Color.LightGray;
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
