using de.springwald.xml.cursor;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;
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

       

        // Schriftart und Pinsel bereitstellen
        private static StringFormat _drawFormat;

        private static Font drawFontAttribute;
       // private static float _breiteProBuchstabeAttribute;
        //private static int _hoeheProBuchstabeAttribut;

        private static Font _drawFontNodeName;
        //private static int _hoeheProBuchstabeNodeName;

        private Rectangle _pfeilBereichLinks;       // Der Klickbereich des linken Pfeiles
        private Rectangle _pfeilBereichRechts;		// Der Klickbereich des rechten Pfeiles
        private Rectangle _tagBereichLinks;         // Der Klickbereich des linken Tags
        private Rectangle _tagBereichRechts;        // Der Klickbereich des rechten Tags

        private int _ankerEinzugY = 0; // soweit ist der Y-Mittelpunkt des NodesRahmens vom jeweiligen PosY enfernt

        private const int NameFontHeight = 36;
        private const int AttributeFontHeight = 12;

        private const int _rundung = 3;             // Diese Rundung hat der Rahmen
        private const int _pfeilLaenge = 7;         // so dick ist der zu zeichnende Pfeil
        private const int _pfeilDicke = 7;          // so lang ist der zu zeichnende Pfeil

        private const int innerMarginX = 5;
        private const int attributeMarginY = (NameFontHeight - AttributeFontHeight) / 2;


        public override int LineHeight { get; } = NameFontHeight;

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

            if (_drawFontNodeName == null)
            {
                // Das Format für die Schriftarten bereitstellen
                _drawFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
                _drawFormat.FormatFlags = _drawFormat.FormatFlags | StringFormatFlags.MeasureTrailingSpaces;
                _drawFormat.Trimming = StringTrimming.None;

                _drawFontNodeName = new Font("Arial", NameFontHeight, Font.GraphicsUnit.Point);
                drawFontAttribute = new Font("Courier New", AttributeFontHeight, Font.GraphicsUnit.Point);
                //_breiteProBuchstabeAttribute = await this._xmlEditor.NativePlatform.Gfx.MeasureDisplayStringWidthAsync("W", _drawFontAttribute, _drawFormat); 
            }

            // Falls der Cursor innherlb des leeren Nodes steht, dann den Cursor auch dahin zeichnen
            if (_xmlEditor.CursorOptimiert.StartPos.AktNode == this.XMLNode)
            {
                if (_xmlEditor.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorVorDemNode)
                {
                    // Position für Cursor-Strich vermerken
                    this._cursorStrichPos = new Point(paintContext.PaintPosX + 1, paintContext.PaintPosY);
                }
            }

            // Bestimmen, wie hoch der Anker am linken Node-Rand hängt
            _ankerEinzugY =  NameFontHeight / 2  ; // +  _randY;

            // ### Den Namen um den Node malen ###
            this.FarbenSetzen();

            // ### Den Namen des Nodes schreiben ###
            // Pinsel für Node-Name-Schrift bereitstellen
            var drawBrush = new SolidBrush(_farbeNodeNameSchrift);

            paintContext.PaintPosX += innerMarginX;  // Abstand vom Rahmen zur Node-Name-Schrift

            // Pre-calculate the width of the font
            int schriftBreite = (int)(await this._xmlEditor.NativePlatform.Gfx.MeasureDisplayStringWidthAsync(this.XMLNode.Name, _drawFontNodeName, _drawFormat));

            // Pre-calculate the width of the attribute string
            var attributeString = this.GetAttributeString();
            var attributeTextWidth = 10; // await this.GetAttributeTextWidth(attributeString, e);

            // RahmenBreite und Hoehe bestimmen
            var borderWidth = innerMarginX + schriftBreite + attributeTextWidth + innerMarginX; // (attributeTextWidth == 0 ? 0: innerMarginX + attributeTextWidth) + innerMarginX;
            var borderHeight = NameFontHeight;

            // Einen Rahmen um den Node und die Attribute zeichnen.
            await zeichneRahmenNachGroesse(startX, paintContext.PaintPosY, borderWidth, borderHeight, _rundung, _farbeRahmenHintergrund, _farbeRahmenRand, e);

            // Die Node-Namen-Schrift zeichnen
            await e.Graphics.DrawStringAsync(this.XMLNode.Name, _drawFontNodeName, drawBrush, paintContext.PaintPosX, paintContext.PaintPosY, _drawFormat);
            paintContext.PaintPosX += schriftBreite + innerMarginX;

            // Die Attribute zeichnen 
            await AttributeZeichnen(paintContext, attributeString, e);

            // Ein Pixel weiter nach rechts, weil wir sonst auf der Rahmenlinie zeichnen
            paintContext.PaintPosX++;

            // ### ggf. den weiterführenden Pfeil am Ende des Rahmens zeichnen ###
            if (_xmlEditor.Regelwerk.IstSchliessendesTagSichtbar(this.XMLNode))
            {
                // nach dem Noderahmen einen Pfeil nach rechts zeichnen
                // Pfeil nach rechts
                var brush = new SolidBrush(_farbePfeil);
                int x = paintContext.PaintPosX;
                int y = paintContext.PaintPosY + _ankerEinzugY;
                var point1 = new Point(x, y - _pfeilDicke);
                var point2 = new Point(x + _pfeilLaenge, y);
                var point3 = new Point(x, y + _pfeilDicke);
                Point[] points = { point1, point2, point3 };
                await e.Graphics.FillPolygonAsync(brush, points);  // Fill polygon to screen.

                // Den rechten Pfeilbereich merken
                _pfeilBereichLinks = new Rectangle(x, y - _pfeilDicke, _pfeilLaenge, _pfeilDicke * 2);
                paintContext.PaintPosX += _pfeilLaenge;
                paintContext.PaintPosX += _pfeilLaenge;  // Den Zeichnungscursor hinter den Pfeil setzen
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

            paintContext.HoeheAktZeile = System.Math.Max(paintContext.HoeheAktZeile, borderHeight); // Schauen, wie hoch die aktuelle Zeile ist

            // Merken, wo die Mausbereiche sind
            _tagBereichLinks = new Rectangle(paintContext.LimitLeft, startY, paintContext.PaintPosX - paintContext.LimitLeft, borderHeight);

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
                    int y = paintContext.PaintPosY + _ankerEinzugY;
                    Point point1 = new Point(x + _pfeilLaenge, y - _pfeilDicke);
                    Point point2 = new Point(x, y);
                    Point point3 = new Point(x + _pfeilLaenge, y + _pfeilDicke);
                    Point[] points = { point1, point2, point3 };
                    await e.Graphics.FillPolygonAsync(brush, points);  // Fill polygon to screen.

                    // Den rechten Pfeilbereich merken
                    _pfeilBereichRechts = new Rectangle(x, y - _pfeilDicke, _pfeilLaenge, _pfeilDicke * 2);
                    paintContext.PaintPosX += _pfeilLaenge; // Zeichnungscursor hinter den Pfeil setzen

                    int rahmenHoehe = NameFontHeight;

                    // Die Breite vorausberechnen
                    int schriftBreite = (int)(await e.Graphics.MeasureDisplayStringWidthAsync(this.XMLNode.Name, _drawFontNodeName, _drawFormat));

                    // ## RAHMEN für schließenden Node  zeichnen ###
                    await zeichneRahmenNachGroesse(paintContext.PaintPosX, paintContext.PaintPosY, schriftBreite + innerMarginX * 2, rahmenHoehe, _rundung, _farbeRahmenHintergrund, _farbeRahmenRand, e);
                    paintContext.PaintPosX += innerMarginX; // Abstand zwischen Rahmen und Schrift

                    // ## Name für schließenden Node zeichnen ###
                    // Pinsel bereitstellen
                    SolidBrush drawBrush = new SolidBrush(_farbeNodeNameSchrift);
                    // Den Namen des Nodes schreiben
                    await e.Graphics.DrawStringAsync(this.XMLNode.Name, _drawFontNodeName, drawBrush, paintContext.PaintPosX, paintContext.PaintPosY + innerMarginX, _drawFormat);
                    paintContext.PaintPosX += schriftBreite + innerMarginX; // Abstand zwischen Schrift und Rahmen

                    // Ein Pixel weiter nach rechts, weil wir sonst auf der Rahmenlinie zeichnen
                    // und der Cursor sonst auf dem Rahmen blinkt
                    paintContext.PaintPosX++;

                    // Merken, wo die Mausbereiche sind
                    _tagBereichRechts = new Rectangle(startX, paintContext.PaintPosY, paintContext.PaintPosX - startX, rahmenHoehe);
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


        /// <summary>
        /// Zeichnet den Bereich der Attribute 
        /// </summary>
        private async Task AttributeZeichnen(PaintContext paintContext, string attributeString, PaintEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(attributeString)) return;

            // Wenn Attribute an diesem XML-Node sind, dann anzeigen
            var attributeBreite = await this.GetAttributeTextWidth(attributeString, e);
            var attributeHoehe = AttributeFontHeight + 2 ;

            // einen Rahmen um die Attribute zeichnen
            await zeichneRahmenNachGroesse(paintContext.PaintPosX, paintContext.PaintPosY + attributeMarginY - 1, attributeBreite, attributeHoehe, 2, _farbeAttributeHintergrund, _farbeAttributeRand, e);

            // Pinsel bereitstellen
            SolidBrush drawBrush = new SolidBrush(_farbeAttributeSchrift);

            // Attribute zeichnen
            await e.Graphics.DrawStringAsync(attributeString.ToString(), drawFontAttribute, drawBrush, paintContext.PaintPosX + 1, paintContext.PaintPosY + attributeMarginY, _drawFormat); ;

            // Zeichencursor hinter die Attribute setzen
            paintContext.PaintPosX += attributeBreite + innerMarginX;
        }

        private async Task<int> GetAttributeTextWidth(string attributeString, PaintEventArgs e) {
            if (string.IsNullOrEmpty(attributeString)) return 0;
            return (int) await e.Graphics.MeasureDisplayStringWidthAsync(attributeString, drawFontAttribute, _drawFormat);
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
