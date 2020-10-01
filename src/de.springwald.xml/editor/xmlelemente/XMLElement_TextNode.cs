using de.springwald.xml.cursor;
using de.springwald.xml.editor.helper;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// XML-Element zur Darstellung eines Textnodes
    /// </summary>
    /// <remarks>
    /// (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    public class XMLElement_TextNode : XMLElement
    {
        private static Font _drawFont;
        private static float _breiteProBuchstabe;
        private static int _hoeheProBuchstabe;
        private static StringFormat _drawFormat;

        protected Color _farbeHintergrund_;
        protected Color _farbeHintergrundInvertiert_;
        protected Color _farbeHintergrundInvertiertOhneFokus_;

        protected SolidBrush _drawBrush_;
        protected SolidBrush _drawBrushInvertiert_;
        protected SolidBrush _drawBrushInvertiertOhneFokus_;

        private TextSplitPart[] _textTeile;  // Buffer der einzelnen, gezeichneten Zeilen. Jeder entspricht einem Klickbereich

        //#region UnPaint-Werte
        //private string _lastPaintAktuellerInhalt; // der zuletzt in diesem Node gezeichnete Inhalt
        //#endregion

        /// <summary>
        /// Der offizielle Inhalt dieses textnodes
        /// </summary>
        private string AktuellerInhalt
        {
            get { return ToolboxXML.TextAusTextNodeBereinigt(XMLNode); }
        }

        private Color GetHintergrundFarbe(bool invertiert)
        {
            if (invertiert)
            {
                if (_xmlEditor.HatFokus)
                {
                    // Invertiert
                    return _farbeHintergrundInvertiert_;
                }
                else
                {
                    // geschwächt invertiert 
                    return _farbeHintergrundInvertiertOhneFokus_;
                }
            }
            else
            {
                // Hintergrund normal füllen
                return _farbeHintergrund_;
            }
        }

        private SolidBrush GetZeichenFarbe(bool invertiert)
        {
            if (invertiert)
            {
                if (_xmlEditor.HatFokus)
                {
                    // Invertiert
                    return _drawBrushInvertiert_;
                }
                else
                {
                    // geschwächt invertiert 
                    return _drawBrushInvertiertOhneFokus_;
                }
            }
            else
            {
                //  normal 
                return _drawBrush_;
            }
        }

        /// <summary>
        /// Bei diesen Zeichen im Text wird umbrochen
        /// </summary>
        public char[] ZeichenZumUmbrechen { get; set; }

        /// <summary>
        ///Bei diesen Zeichen im Text wird eingerückt 
        /// </summary>
        public char[] ZeichenZumEinruecken { get; set; }

        /// <summary>
        /// Bei diesen Zeichen wird im Text ein Einrücken rückgängig gemacht
        /// </summary>
        public char[] ZeichenZumAusruecken { get; set; }

        private const int FontHeight = 14;

        public override int LineHeight { get; } = FontHeight;

        public XMLElement_TextNode(System.Xml.XmlNode xmlNode, de.springwald.xml.editor.XMLEditor xmlEditor) : base(xmlNode, xmlEditor)
        {
            FarbenSetzen(); // Farben bereitstellen
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Zeichnet die Grafik des aktuellen Nodes
        /// </summary>
        protected override async Task NodeZeichnenStart(PaintContext paintContext, PaintEventArgs e)
        {
            if (_drawFont == null)
            {
                // Das Format für die Schriftarten bereitstellen
                _drawFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
                _drawFormat.FormatFlags = _drawFormat.FormatFlags | StringFormatFlags.MeasureTrailingSpaces;
                _drawFormat.Trimming = StringTrimming.None;

                // Die Schrift bereitstellen
                _drawFont = new Font("Courier New", FontHeight, Font.GraphicsUnit.Point);
                _breiteProBuchstabe = await this._xmlEditor.NativePlatform.Gfx.MeasureDisplayStringWidthAsync("o", _drawFont, _drawFormat);
                _hoeheProBuchstabe = _drawFont.Height;
            }

            int _randY = 2; // Abstand zum oberen Rand der Zeile
            int aktTextTeilStartPos = 0;
            int selektionStart = -1;
            int selektionLaenge = 0;

            StartUndEndeDerSelektionBestimmen(ref selektionStart, ref selektionLaenge);

            _textTeile = await new TextSplitHelper().SplitText(this._xmlEditor.NativePlatform.Gfx, AktuellerInhalt, selektionStart, selektionLaenge, paintContext, _drawFont, _drawFormat);

            if (selektionStart != -1) _ = _textTeile.Where(t => { t.Inverted = true; return true; });

            // Texthintergrund färben
            foreach (var teil in _textTeile)
            {
                // SolidBrush newBrush = new SolidBrush(GetHintergrundFarbe(teil.Invertiert));
                SolidBrush newBrush = new SolidBrush(Color.Red);

                // Hintergrund füllen
                await e.Graphics.FillRectangleAsync(newBrush, teil.Rectangle);
            }

            // Nun den Inhalt zeichnen, ggf. auf mehrere Textteile und Zeilen umbrochen
            foreach (var textTeil in _textTeile)
            {
                int schriftBreite = (int)(_breiteProBuchstabe * textTeil.Text.Length);

                // ggf. den Cursorstrich berechnen
                if (this.XMLNode == _xmlEditor.CursorOptimiert.StartPos.AktNode) // ist der Cursor im aktuellen Textnode
                {
                    if (_xmlEditor.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorInnerhalbDesTextNodes)
                    {
                        // Checken, ob der Cursor innerhalb dieses Textteiles liegt
                        if ((_xmlEditor.CursorOptimiert.StartPos.PosImTextnode >= aktTextTeilStartPos) && (_xmlEditor.CursorOptimiert.StartPos.PosImTextnode <= aktTextTeilStartPos + textTeil.Text.Length))
                        {
                            // Herausfinden, wieviel Pixel die Cursor-Position im Text liegt
                            int xCursorPos = paintContext.PaintPosX + (int)((_xmlEditor.CursorOptimiert.StartPos.PosImTextnode - aktTextTeilStartPos) * _breiteProBuchstabe);
                            xCursorPos = Math.Max(paintContext.PaintPosX, xCursorPos);

                            // Position für Cursor-Strich vermerken
                            this._cursorStrichPos = new Point(xCursorPos, paintContext.PaintPosY);
                        }
                    }
                }

                // Merken, wo im Text wir uns gerade befinden
                aktTextTeilStartPos += textTeil.Text.Length;

                // für die Klickbereiche merken, wohin dieser Textteil gezeichnet wird 
                this._klickBereiche = this._klickBereiche.Append(textTeil.Rectangle).ToArray(); // original:  this._klickBereiche.Add(textTeil.Rechteck);
                                                                                                // Die Schrift zeichnen
                await e.Graphics.DrawStringAsync(textTeil.Text, _drawFont, GetZeichenFarbe(textTeil.Inverted), textTeil.Rectangle.X, textTeil.Rectangle.Y + _randY, _drawFormat);

                paintContext.PaintPosX += textTeil.Rectangle.Width;
                paintContext.BisherMaxX = Math.Max(paintContext.BisherMaxX, paintContext.PaintPosX);
                paintContext.HoeheAktZeile = Math.Max(paintContext.HoeheAktZeile, _hoeheProBuchstabe + _randY + _randY);
            }


            // ggf. den Cursorstrich hinter dem Node berechnen
            if (this.XMLNode == _xmlEditor.CursorOptimiert.StartPos.AktNode)  // ist der Cursor im aktuellen Textnode
            {
                if (_xmlEditor.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode)
                {
                    this._cursorStrichPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                }
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn auf dieses Element geklickt wurde
        /// </summary>
        /// <param name="point"></param>
        protected override async Task WurdeAngeklickt(Point point, MausKlickAktionen aktion)
        {
            // Herausfinden, an welcher Position des Textes geklickt wurde
            int posInZeile = 0;
            foreach (var teil in _textTeile) // alle Textteile durchgehen
            {
                if (teil.Rectangle.Contains(point)) // Wenn der Klick in diesem Textteil ist
                {
                    posInZeile += Math.Min(teil.Text.Length - 1, (int)((point.X - teil.Rectangle.X) / _breiteProBuchstabe + 0.5));
                    break;
                }
                else // In diesem Textteil war der Klick nicht
                {
                    posInZeile += teil.Text.Length;
                }
            }
            await _xmlEditor.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, posInZeile, aktion);
        }

        /// <summary>
        /// Vertauscht die Vorder- und Hintergrundfarben, um den Node selektiert darstellen zu können
        /// </summary>
        protected virtual void FarbenSetzen()
        {
            // Die Farben für "nicht invertiert" definieren
            _farbeHintergrund_ = this._xmlEditor.NativePlatform.ControlElement.BackColor;
            _drawBrush_ = new SolidBrush(Color.Black);  // Schrift-Pinsel bereitstellen;

            // Die Farben für "invertiert" definieren
            _farbeHintergrundInvertiert_ = Color.DarkBlue;
            _drawBrushInvertiert_ = new SolidBrush(Color.White);    // Schrift-Pinsel bereitstellen;

            // Die Farben für schwach "invertiert" definieren
            _farbeHintergrundInvertiertOhneFokus_ = Color.Gray;
            _drawBrushInvertiertOhneFokus_ = new SolidBrush(Color.White);   // Schrift-Pinsel bereitstellen;
        }

        /// <summary>
        /// Findet heraus, in welche Zeile (bei umbrochenem Text) geklickt wurde
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private int LiegtInWelchemTextTeil(Point point)
        {
            for (int i = 0; i < _textTeile.Length; i++)
            {
                if (_textTeile[i].Rectangle.Contains(point))
                {
                    return i;
                }
            }
            throw (new ApplicationException("Punkt liegt in keiner der bekannten Zeilen (XMLElement_Textnode.LiegtInWelcherZeile)"));
        }

        /// <summary>
        /// Findet heraus, welche Bereiche des Textes invertiert dargestellt werden müssen
        /// </summary>
        /// <param name="selektionStart"></param>
        /// <param name="selektionEnde"></param>
        private void StartUndEndeDerSelektionBestimmen(ref int selektionStart, ref int selektionLaenge)
        {
            XMLCursor cursor = _xmlEditor.CursorOptimiert;

            if (cursor.StartPos.AktNode == this.XMLNode) // Der Start der Selektion liegt auf diesem Node
            {
                switch (cursor.StartPos.PosAmNode)
                {
                    case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag: // Das Node selbst ist als Startnode selektiert
                    case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                        selektionStart = 0;
                        selektionLaenge = AktuellerInhalt.Length;
                        break;

                    case XMLCursorPositionen.CursorHinterDemNode:
                    case XMLCursorPositionen.CursorInDemLeeremNode:
                        // Da die CursorPosition sortiert ankommt, kann die EndPos nur hintet dem Node liegen
                        selektionStart = -1;
                        selektionLaenge = 0;
                        break;

                    case XMLCursorPositionen.CursorVorDemNode:
                    case XMLCursorPositionen.CursorInnerhalbDesTextNodes:

                        if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorInnerhalbDesTextNodes)
                        {
                            selektionStart = Math.Max(0, cursor.StartPos.PosImTextnode); // im Textnode
                        }
                        else
                        {
                            selektionStart = 0; // Vor dem Node
                        }

                        if (cursor.EndPos.AktNode == this.XMLNode) // Wenn das Ende der Selektion auch in diesem Node liegt
                        {
                            switch (cursor.EndPos.PosAmNode)
                            {
                                case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag: // Startnode liegt vor dem Node, Endnode dahiner: alles ist selektiert
                                case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                                case XMLCursorPositionen.CursorHinterDemNode:
                                    selektionLaenge = Math.Max(0, AktuellerInhalt.Length - selektionStart);
                                    break;

                                case XMLCursorPositionen.CursorInDemLeeremNode:
                                    selektionStart = -1;
                                    selektionLaenge = 0;
                                    break;

                                case XMLCursorPositionen.CursorInnerhalbDesTextNodes:  // Bis zur Markierung im Text 
                                    selektionLaenge = Math.Max(0, cursor.EndPos.PosImTextnode - selektionStart);
                                    break;

                                case XMLCursorPositionen.CursorVorDemNode:
                                    selektionLaenge = 0;
                                    break;

                                default:
                                    throw new ApplicationException("Unbekannte XMLCursorPosition.EndPos.PosAmNode '" + cursor.EndPos.PosAmNode + "'B");
                            }
                        }
                        else // Das Ende der Selektion liegt nicht in diesem Node
                        {
                            if (cursor.EndPos.AktNode.ParentNode == cursor.StartPos.AktNode.ParentNode) // Wenn Start und Ende zwar verschieden, aber direkt im selben Parent stecken
                            {
                                selektionLaenge = Math.Max(0, AktuellerInhalt.Length - selektionStart); // Nur den selektierten Teil selektieren
                            }
                            else // Start und Ende unterschiedlich und unterschiedliche Parents
                            {
                                selektionStart = 0;
                                selektionLaenge = AktuellerInhalt.Length;   // Ganzen Textnode selektieren
                            }
                        }
                        break;

                    default:
                        throw new ApplicationException("Unbekannte XMLCursorPosition.StartPos.PosAmNode '" + cursor.StartPos.PosAmNode + "'A");
                }
            }
            else // Der Start der Selektion liegt nicht auf diesem Node
            {
                if (cursor.EndPos.AktNode == this.XMLNode) // Das Ende der Selektion aber schon
                {
                    switch (cursor.EndPos.PosAmNode)
                    {
                        case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag: // Der Node selbst ist als End-Node selektiert
                        case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag: // Startnode liegt vor dem Node, Endnode dahiner: alles ist selektiert
                        case XMLCursorPositionen.CursorHinterDemNode:
                            selektionStart = 0;
                            selektionLaenge = AktuellerInhalt.Length;
                            break;

                        case XMLCursorPositionen.CursorInDemLeeremNode:
                            selektionStart = -1;
                            selektionLaenge = 0;
                            break;

                        case XMLCursorPositionen.CursorInnerhalbDesTextNodes:// Startnode liegt vor dem Node, Endnode mitten drin, also von vorn bis zur Mitte selektieren
                            if (cursor.EndPos.AktNode.ParentNode == cursor.StartPos.AktNode.ParentNode) // Wenn Start und Ende zwar verschieden, aber direkt im selben Parent stecken
                            {
                                selektionStart = 0;
                                selektionLaenge = Math.Max(0, cursor.EndPos.PosImTextnode); // Nur den selektierten, vorderen Teil selektieren
                            }
                            else // Start und Ende unterschiedlich und unterschiedliche Parents
                            {
                                selektionStart = 0;
                                selektionLaenge = AktuellerInhalt.Length;   // Ganzen Textnode selektieren
                            }
                            break;

                        case XMLCursorPositionen.CursorVorDemNode: // Startnode liegt vor dem Node, Endnoch auch
                            selektionStart = -1;
                            selektionLaenge = 0;
                            break;

                        default:
                            throw new ApplicationException("Unbekannte XMLCursorPosition.EndPos.PosAmNode '" + cursor.EndPos.PosAmNode + "'X");
                    }
                }
                else // Weder der Start noch das Ende der Selektion liegen genau auf diesem Node
                {
                    if (_xmlEditor.CursorOptimiert.IstNodeInnerhalbDerSelektion(this.XMLNode))
                    {
                        selektionStart = 0;
                        selektionLaenge = AktuellerInhalt.Length;   // Ganzen Textnode selektieren
                    }
                }
            }
        }
    }
}