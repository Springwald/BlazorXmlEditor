using de.springwald.xml.cursor;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.textnode;
using de.springwald.xml.events;
using System;
using System.Collections.Generic;
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

        private List<TextTeil> _textTeile;  // Buffer der einzelnen, gezeichneten Zeilen. Jeder entspricht einem Klickbereich

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

        public override int LineHeight { get; }

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
                _drawFont = new Font("Courier New", 14, Font.GraphicsUnit.Point);
                _breiteProBuchstabe = await this._xmlEditor.NativePlatform.Gfx.MeasureDisplayStringWidthAsync("W", _drawFont, _drawFormat);
                _hoeheProBuchstabe = _drawFont.Height;
            }

            int _randY = 2; // Abstand zum oberen Rand der Zeile
            int aktTextTeilStartPos = 0;


            int selektionStart = -1;
            int selektionLaenge = 0;

            StartUndEndeDerSelektionBestimmen(ref selektionStart, ref selektionLaenge);

            // die Merke-Buffer der einzelnen Zeilen leeren
            int maxLaengeProZeile = (int)((paintContext.ZeilenEndeX - paintContext.ZeilenStartX) / _breiteProBuchstabe);
            int bereitsLaengeDerZeile = (int)(paintContext.PaintPosX / _breiteProBuchstabe);
            TextTeiler teiler =
                new TextTeiler(
                    AktuellerInhalt, selektionStart, selektionLaenge, maxLaengeProZeile, bereitsLaengeDerZeile,
                        ZeichenZumUmbrechen
                    );
            _textTeile = teiler.TextTeile;

            // Texthintergrund färben
            foreach (TextTeil teil in _textTeile)
            {
                SolidBrush newBrush = new SolidBrush(GetHintergrundFarbe(teil.Invertiert));
                // Hintergrund füllen
                await e.Graphics.FillRectangleAsync(newBrush, teil.Rechteck);
            }

            // Nun den Inhalt zeichnen, ggf. auf mehrere Textteile und Zeilen umbrochen
            foreach (TextTeil textTeil in _textTeile)
            {
                int schriftBreite = (int)(_breiteProBuchstabe * textTeil.Text.Length);

                if (textTeil.IstNeueZeile) // Dieser Textteil beginnt eine neue Zeile
                {

                    // Neue Zeile beginnen
                    paintContext.PaintPosY += _xmlEditor.Regelwerk.AbstandYZwischenZeilen + paintContext.HoeheAktZeile; // Zeilenumbruch
                    paintContext.PaintPosX = paintContext.ZeilenStartX;
                    paintContext.HoeheAktZeile = _hoeheProBuchstabe + _randY * 2;  // noch gar keine Höhe
                }

                // Nur Berechnen, nicht zeichnen
                textTeil.Rechteck = new Rectangle(paintContext.PaintPosX, paintContext.PaintPosY, (int)(_breiteProBuchstabe * textTeil.Text.Length), _hoeheProBuchstabe + _randY * 2);

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
                this._klickBereiche = this._klickBereiche.Append(textTeil.Rechteck).ToArray(); // original:  this._klickBereiche.Add(textTeil.Rechteck);
                                                                                               // Die Schrift zeichnen
                await e.Graphics.DrawStringAsync(textTeil.Text, _drawFont, GetZeichenFarbe(textTeil.Invertiert), textTeil.Rechteck.X, textTeil.Rechteck.Y + _randY, _drawFormat);

                paintContext.BisherMaxX = Math.Max(paintContext.BisherMaxX, textTeil.Rechteck.X + textTeil.Rechteck.Width);
                paintContext.HoeheAktZeile = Math.Max(paintContext.HoeheAktZeile, _hoeheProBuchstabe + _randY + _randY);
                paintContext.PaintPosX += schriftBreite;
                paintContext.PaintPosX += schriftBreite;
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
            foreach (TextTeil teil in _textTeile) // alle Textteile durchgehen
            {
                if (teil.Rechteck.Contains(point)) // Wenn der Klick in diesem Textteil ist
                {
                    posInZeile += Math.Min(teil.Text.Length - 1, (int)((point.X - teil.Rechteck.X) / _breiteProBuchstabe + 0.5));
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
            for (int i = 0; i < _textTeile.Count; i++)
            {
                if (_textTeile[i].Rechteck.Contains(point))
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