// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.cursor;
using de.springwald.xml.editor.helper;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// XML element for displaying a text node
    /// </summary>
    public class XMLElement_TextNode : XMLElement
    {
        private class TextLine
        {
            public string Text { get; set; }
            public Rectangle Rectangle { get; set; }
            public bool Inverted { get; set; }
        }

        protected int lastFontHeight = 0;
        protected float lastCalculatedFontWidth = 0;

        protected Color _farbeHintergrund_;
        protected Color _farbeHintergrundInvertiert_;
        protected Color _farbeHintergrundInvertiertOhneFokus_;

        protected Color _drawBrush_;
        protected Color _drawBrushInvertiert_;
        protected Color _drawBrushInvertiertOhneFokus_;

        private TextLine[] textParts;  // Buffer of the single, drawn lines. Each corresponds to a click area

        //#region UnPaint-Werte
        //private string _lastPaintAktuellerInhalt; // der zuletzt in diesem Node gezeichnete Inhalt
        //#endregion

        /// <summary>
        /// Der offizielle Inhalt dieses textnodes
        /// </summary>
        private string AktuellerInhalt =>  ToolboxXML.TextAusTextNodeBereinigt(XMLNode); 

        private Color GetHintergrundFarbe(bool invertiert)
        {
            if (invertiert)
            {
                if (_xmlEditor.EditorStatus.HasFocus)
                {
                    // Inverted
                    return _farbeHintergrundInvertiert_;
                }
                else
                {
                    // weak inverted  
                    return _farbeHintergrundInvertiertOhneFokus_;
                }
            }
            else
            {
                // Fill background normally
                return _farbeHintergrund_;
            }
        }

        private Color GetZeichenFarbe(bool inverted)
        {
            if (inverted)
            {
                if (_xmlEditor.EditorStatus.HasFocus)
                {
                    // Inverted
                    return _drawBrushInvertiert_;
                }
                else
                {
                    // weak inverted  
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

        public override int LineHeight => this._xmlEditor.EditorConfig.TextNodeFont.Height;

        public XMLElement_TextNode(System.Xml.XmlNode xmlNode, XMLEditor xmlEditor) : base(xmlNode, xmlEditor)
        {
            FarbenSetzen(); // Farben bereitstellen
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        ///  Draws the graphic of the current node
        /// </summary>
        protected override async Task NodeZeichnenStart(PaintContext paintContext, IGraphics gfx)
        {
            if (lastFontHeight != this._xmlEditor.EditorConfig.TextNodeFont.Height)
            {
                lastFontHeight = this._xmlEditor.EditorConfig.TextNodeFont.Height;
                lastCalculatedFontWidth = await this._xmlEditor.NativePlatform.Gfx.MeasureDisplayStringWidthAsync("W", this._xmlEditor.EditorConfig.TextNodeFont);
            }
            paintContext.HoeheAktZeile = Math.Max(paintContext.HoeheAktZeile, this._xmlEditor.EditorConfig.TextNodeFont.Height);

            int marginY = (paintContext.HoeheAktZeile - this._xmlEditor.EditorConfig.TextNodeFont.Height) / 2;

            // ggf. den Cursorstrich vor dem Node berechnen
            if (this.XMLNode == _xmlEditor.EditorStatus.CursorOptimiert.StartPos.AktNode)  // ist der Cursor im aktuellen Textnode
            {
                if (_xmlEditor.EditorStatus.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorVorDemNode)
                {
                    this._cursorStrichPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                }
            }

            this.StartUndEndeDerSelektionBestimmen(out int selektionStart, out int selektionLaenge);

            const int charMarginRight = 2;

            var textPartsRaw = TextSplitHelper.SplitText(
                text: AktuellerInhalt,
                invertStart: selektionStart,
                invertLength: selektionLaenge,
                maxLength: (int)((paintContext.LimitRight - paintContext.LimitLeft) / lastCalculatedFontWidth) - charMarginRight,
                maxLengthFirstLine: (int)((paintContext.LimitRight - paintContext.PaintPosX) / lastCalculatedFontWidth) - charMarginRight)
                .ToArray();

            textParts = this.GetTextLinesFromTextParts(textPartsRaw, paintContext, lastFontHeight, lastCalculatedFontWidth).ToArray();

            // Texthintergrund färben
            foreach (var part in textParts)
            {
                // Hintergrund füllen
                gfx.AddJob(new JobDrawRectangle
                {
                    Layer = paintContext.LayerTagBackground,
                    Batchable = true,
                    FillColor = GetHintergrundFarbe(part.Inverted),
                    Rectangle = part.Rectangle
                });
            }

            // Nun den Inhalt zeichnen, ggf. auf mehrere Textteile und Zeilen umbrochen
            int actualTextPartStartPos = 0;
            foreach (var part in textParts)
            {
                // ggf. den Cursorstrich berechnen
                if (this.XMLNode == _xmlEditor.EditorStatus.CursorOptimiert.StartPos.AktNode) // ist der Cursor im aktuellen Textnode
                {
                    if (_xmlEditor.EditorStatus.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorInnerhalbDesTextNodes)
                    {
                        // Checken, ob der Cursor innerhalb dieses Textteiles liegt
                        if ((_xmlEditor.EditorStatus.CursorOptimiert.StartPos.PosImTextnode >= actualTextPartStartPos) && (_xmlEditor.EditorStatus.CursorOptimiert.StartPos.PosImTextnode <= actualTextPartStartPos + part.Text.Length))
                        {
                            this._cursorStrichPos = new Point(
                                part.Rectangle.X + (int)((_xmlEditor.EditorStatus.CursorOptimiert.StartPos.PosImTextnode - actualTextPartStartPos) * lastCalculatedFontWidth),
                                part.Rectangle.Y
                                );
                        }
                    }
                }

                // Merken, wo im Text wir uns gerade befinden
                actualTextPartStartPos += part.Text.Length;

                // für die Klickbereiche merken, wohin dieser Textteil gezeichnet wird 
                this._klickBereiche = this._klickBereiche.Append(new Rectangle(part.Rectangle.X, part.Rectangle.Y, part.Rectangle.Width, paintContext.HoeheAktZeile)).ToArray(); // original:  this._klickBereiche.Add(textTeil.Rechteck);

                // draw the text
                gfx.AddJob(new JobDrawString
                {
                    Batchable = false,
                    Layer = paintContext.LayerText,
                    Text = part.Text,
                    Color = GetZeichenFarbe(part.Inverted),
                    X = part.Rectangle.X,
                    Y = part.Rectangle.Y + marginY,
                    Font = _xmlEditor.EditorConfig.TextNodeFont
                });
                paintContext.PaintPosY = part.Rectangle.Y;
                paintContext.PaintPosX = part.Rectangle.X + part.Rectangle.Width;
                paintContext.BisherMaxX = Math.Max(paintContext.BisherMaxX, paintContext.PaintPosX);
            }

            // ggf. den Cursorstrich hinter dem Node berechnen
            if (this.XMLNode == _xmlEditor.EditorStatus.CursorOptimiert.StartPos.AktNode)  // ist der Cursor im aktuellen Textnode
            {
                if (_xmlEditor.EditorStatus.CursorOptimiert.StartPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode)
                {
                    this._cursorStrichPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY + marginY);
                }
            }
        }

        private IEnumerable<TextLine> GetTextLinesFromTextParts(TextSplitHelper.TextPart[] parts, PaintContext paintContext, int fontHeight, float fontWidth)
        {
            paintContext.HoeheAktZeile = Math.Max(paintContext.HoeheAktZeile, fontHeight);
            var x = paintContext.PaintPosX;
            var y = paintContext.PaintPosY;
            var actualLine = 0;

            foreach(var part in parts)
            {
                var newLine = part.LineNo != actualLine;
                if (newLine)
                {
                    actualLine = part.LineNo;
                    y += paintContext.HoeheAktZeile;
                    x = paintContext.LimitLeft;
                }
                var width = (int)(part.Text.Length * fontWidth);
                yield return new TextLine
                {
                    Text = part.Text,
                    Inverted = part.Inverted,
                    Rectangle = new Rectangle(x, y, width, paintContext.HoeheAktZeile)
                };
                x += 1 + width;
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn auf dieses Element geklickt wurde
        /// </summary>
        protected override async Task WurdeAngeklickt(Point point, MausKlickAktionen action)
        {
            // Herausfinden, an welcher Position des Textes geklickt wurde
            int posInLine = 0;
            foreach (var part in textParts) // alle Textteile durchgehen
            {
                if (part.Rectangle.Contains(point)) // Wenn der Klick in diesem Textteil ist
                {
                    posInLine += Math.Min(part.Text.Length - 1, (int)((point.X - part.Rectangle.X) / Math.Max(1, this.lastCalculatedFontWidth) + 0.5));
                    break;
                }
                else // In diesem Textteil war der Klick nicht
                {
                    posInLine += part.Text.Length;
                }
            }
            await _xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, posInLine, action);
        }

        /// <summary>
        /// Vertauscht die Vorder- und Hintergrundfarben, um den Node selektiert darstellen zu können
        /// </summary>
        protected virtual void FarbenSetzen()
        {
            // Die Farben für "nicht invertiert" definieren
            _farbeHintergrund_ = this._xmlEditor.NativePlatform.ControlElement.BackColor;
            _drawBrush_ = Color.Black;  // Schrift-Pinsel bereitstellen;

            // Die Farben für "invertiert" definieren
            _farbeHintergrundInvertiert_ = Color.DarkBlue;
            _drawBrushInvertiert_ = Color.White;    // Schrift-Pinsel bereitstellen;

            // Die Farben für schwach "invertiert" definieren
            _farbeHintergrundInvertiertOhneFokus_ = Color.Gray;
            _drawBrushInvertiertOhneFokus_ = Color.White;   // Schrift-Pinsel bereitstellen;
        }

        /// <summary>
        /// Findet heraus, welche Bereiche des Textes invertiert dargestellt werden müssen
        /// </summary>
        /// <param name="selectionStart"></param>
        /// <param name="selektionEnde"></param>
        private void StartUndEndeDerSelektionBestimmen(out int selectionStart, out int selectionEnd)
        {
            selectionStart = -1;
            selectionEnd = 0;

            var cursor = _xmlEditor.EditorStatus.CursorOptimiert;

            if (cursor.StartPos.AktNode == this.XMLNode) // Der Start der Selektion liegt auf diesem Node
            {
                switch (cursor.StartPos.PosAmNode)
                {
                    case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag: // Das Node selbst ist als Startnode selektiert
                    case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                        selectionStart = 0;
                        selectionEnd = AktuellerInhalt.Length;
                        break;

                    case XMLCursorPositionen.CursorHinterDemNode:
                    case XMLCursorPositionen.CursorInDemLeeremNode:
                        // Da die CursorPosition sortiert ankommt, kann die EndPos nur hintet dem Node liegen
                        selectionStart = -1;
                        selectionEnd = 0;
                        break;

                    case XMLCursorPositionen.CursorVorDemNode:
                    case XMLCursorPositionen.CursorInnerhalbDesTextNodes:

                        if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorInnerhalbDesTextNodes)
                        {
                            selectionStart = Math.Max(0, cursor.StartPos.PosImTextnode); // im Textnode
                        }
                        else
                        {
                            selectionStart = 0; // Vor dem Node
                        }

                        if (cursor.EndPos.AktNode == this.XMLNode) // Wenn das Ende der Selektion auch in diesem Node liegt
                        {
                            switch (cursor.EndPos.PosAmNode)
                            {
                                case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag: // Startnode liegt vor dem Node, Endnode dahiner: alles ist selektiert
                                case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                                case XMLCursorPositionen.CursorHinterDemNode:
                                    selectionEnd = Math.Max(0, AktuellerInhalt.Length - selectionStart);
                                    break;

                                case XMLCursorPositionen.CursorInDemLeeremNode:
                                    selectionStart = -1;
                                    selectionEnd = 0;
                                    break;

                                case XMLCursorPositionen.CursorInnerhalbDesTextNodes:  // Bis zur Markierung im Text 
                                    selectionEnd = Math.Max(0, cursor.EndPos.PosImTextnode - selectionStart);
                                    break;

                                case XMLCursorPositionen.CursorVorDemNode:
                                    selectionEnd = 0;
                                    break;

                                default:
                                    throw new ApplicationException("Unbekannte XMLCursorPosition.EndPos.PosAmNode '" + cursor.EndPos.PosAmNode + "'B");
                            }
                        }
                        else // Das Ende der Selektion liegt nicht in diesem Node
                        {
                            if (cursor.EndPos.AktNode.ParentNode == cursor.StartPos.AktNode.ParentNode) // Wenn Start und Ende zwar verschieden, aber direkt im selben Parent stecken
                            {
                                selectionEnd = Math.Max(0, AktuellerInhalt.Length - selectionStart); // Nur den selektierten Teil selektieren
                            }
                            else // Start und Ende unterschiedlich und unterschiedliche Parents
                            {
                                selectionStart = 0;
                                selectionEnd = AktuellerInhalt.Length;   // Ganzen Textnode selektieren
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
                            selectionStart = 0;
                            selectionEnd = AktuellerInhalt.Length;
                            break;

                        case XMLCursorPositionen.CursorInDemLeeremNode:
                            selectionStart = -1;
                            selectionEnd = 0;
                            break;

                        case XMLCursorPositionen.CursorInnerhalbDesTextNodes:// Startnode liegt vor dem Node, Endnode mitten drin, also von vorn bis zur Mitte selektieren
                            if (cursor.EndPos.AktNode.ParentNode == cursor.StartPos.AktNode.ParentNode) // Wenn Start und Ende zwar verschieden, aber direkt im selben Parent stecken
                            {
                                selectionStart = 0;
                                selectionEnd = Math.Max(0, cursor.EndPos.PosImTextnode); // Nur den selektierten, vorderen Teil selektieren
                            }
                            else // Start und Ende unterschiedlich und unterschiedliche Parents
                            {
                                selectionStart = 0;
                                selectionEnd = AktuellerInhalt.Length;   // Ganzen Textnode selektieren
                            }
                            break;

                        case XMLCursorPositionen.CursorVorDemNode: // Startnode liegt vor dem Node, Endnoch auch
                            selectionStart = -1;
                            selectionEnd = 0;
                            break;

                        default:
                            throw new ApplicationException("Unbekannte XMLCursorPosition.EndPos.PosAmNode '" + cursor.EndPos.PosAmNode + "'X");
                    }
                }
                else // Weder der Start noch das Ende der Selektion liegen genau auf diesem Node
                {
                    if (_xmlEditor.EditorStatus.CursorOptimiert.IstNodeInnerhalbDerSelektion(this.XMLNode))
                    {
                        selectionStart = 0;
                        selectionEnd = AktuellerInhalt.Length;   // Ganzen Textnode selektieren
                    }
                }
            }
        }
    }
}