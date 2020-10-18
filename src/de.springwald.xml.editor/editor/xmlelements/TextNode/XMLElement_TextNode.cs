// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.cursor;
using de.springwald.xml.editor.editor.xmlelements.Caching;
using de.springwald.xml.editor.helper;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace de.springwald.xml.editor.editor.xmlelements.TextNode
{
    /// <summary>
    /// XML element for displaying a text node
    /// </summary>
    public partial class XMLElement_TextNode : XMLElement
    {
        protected Color colorText;
        protected Color colorBackground;

        protected int lastFontHeight = 0;
        protected double lastCalculatedFontWidth = 0;

        private TextLine[] textParts;  // Buffer of the single, drawn lines. Each corresponds to a click area

        /// <summary>
        /// Der offizielle Inhalt dieses textnodes
        /// </summary>
        private string AktuellerInhalt => ToolboxXML.TextAusTextNodeBereinigt(XMLNode);

        public XMLElement_TextNode(System.Xml.XmlNode xmlNode, XMLEditor xmlEditor) : base(xmlNode, xmlEditor)
        {
            this.SetColors();
        }

        protected virtual void SetColors()
        {
            this.colorText = this.Config.ColorText;
            this.colorBackground = this.Config.ColorBackground;
        }

        protected override bool IsClickPosInsideNode(Point pos)
        {
            if (this.textParts == null) return false;
            return this.textParts.Where(t => t.Rectangle.Contains(pos)).Any();
        }

        protected override async Task<PaintContext> PaintInternal(PaintContext paintContext, XMLCursor cursor, IGraphics gfx, PaintModes paintMode)
        {
            this.StartUndEndeDerSelektionBestimmen(out int selektionStart, out int selektionLaenge, cursor);
            var actualPaintData = CalculateActualPaintData(paintContext, null, selektionStart, selektionLaenge);

            switch (paintMode)
            {
                case PaintModes.ForcePaintNoUnPaintNeeded:
                    this.lastPaintData = null;
                    break;

                case PaintModes.ForcePaintAndUnpaintBefore:
                    this.lastPaintData = null;
                    this.UnPaint(gfx, paintContext);
                    break;

                case PaintModes.OnlyPaintWhenChanged:
                    if (!actualPaintData.Equals(lastPaintData)) {
                        this.lastPaintData = null;
                        this.UnPaint(gfx, paintContext);
                    }
                    break;
            }

            if (this.lastPaintData != null && this.lastPaintContextResult != null)
            {
                return this.lastPaintContextResult.Clone();
            }
            else
            {
                this.lastPaintData = actualPaintData;

                if (lastFontHeight != this.xmlEditor.EditorConfig.FontTextNode.Height)
                {
                    lastFontHeight = this.xmlEditor.EditorConfig.FontTextNode.Height;
                    lastCalculatedFontWidth = await this.xmlEditor.NativePlatform.Gfx.MeasureDisplayStringWidthAsync("W", this.xmlEditor.EditorConfig.FontTextNode);
                }
                paintContext.HoeheAktZeile = Math.Max(paintContext.HoeheAktZeile, this.xmlEditor.EditorConfig.MinLineHeight);

                int marginY = (paintContext.HoeheAktZeile - this.xmlEditor.EditorConfig.FontTextNode.Height) / 2;

                // ggf. den Cursorstrich vor dem Node berechnen
                if (this.XMLNode == cursor.StartPos.AktNode)  // ist der Cursor im aktuellen Textnode
                {
                    if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorVorDemNode)
                    {
                        this.cursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY);
                    }
                }

                const int charMarginRight = 2;

                var textPartsRaw = TextSplitHelper.SplitText(
                    text: AktuellerInhalt,
                    invertStart: selektionStart,
                    invertLength: selektionLaenge,
                    maxLength: (int)((paintContext.LimitRight - paintContext.LimitLeft) / lastCalculatedFontWidth) - charMarginRight,
                    maxLengthFirstLine: (int)((paintContext.LimitRight - paintContext.PaintPosX) / lastCalculatedFontWidth) - charMarginRight)
                    .ToArray();

                this.textParts = this.GetTextLinesFromTextParts(textPartsRaw, paintContext, lastFontHeight, lastCalculatedFontWidth).ToArray();

                // Nun den Inhalt zeichnen, ggf. auf mehrere Textteile und Zeilen umbrochen
                int actualTextPartStartPos = 0;
                foreach (var part in this.textParts)
                {
                    // ggf. den Cursorstrich berechnen
                    if (this.XMLNode == cursor.StartPos.AktNode) // ist der Cursor im aktuellen Textnode
                    {
                        if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorInnerhalbDesTextNodes)
                        {
                            // Checken, ob der Cursor innerhalb dieses Textteiles liegt
                            if ((cursor.StartPos.PosImTextnode >= actualTextPartStartPos) && (cursor.StartPos.PosImTextnode <= actualTextPartStartPos + part.Text.Length))
                            {
                                this.cursorPaintPos = new Point(
                                    part.Rectangle.X + (int)((cursor.StartPos.PosImTextnode - actualTextPartStartPos) * lastCalculatedFontWidth),
                                    part.Rectangle.Y
                                    );
                            }
                        }
                    }

                    // Merken, wo im Text wir uns gerade befinden
                    actualTextPartStartPos += part.Text.Length;

                    // draw the inverted background
                    if (part.Inverted)
                    {
                        gfx.AddJob(new JobDrawRectangle
                        {
                            Batchable = true,
                            Layer = GfxJob.Layers.TagBackground,
                            Rectangle = part.Rectangle,
                            FillColor = part.Inverted ? this.colorBackground.InvertedColor : this.colorBackground,
                        });
                    }

                    // draw the text
                    gfx.AddJob(new JobDrawString
                    {
                        Batchable = false,
                        Layer = GfxJob.Layers.Text,
                        Text = part.Text,
                        Color = part.Inverted ? this.colorText.InvertedColor : this.colorText,
                        X = part.Rectangle.X,
                        Y = part.Rectangle.Y + marginY,
                        Font = xmlEditor.EditorConfig.FontTextNode
                    }); ;
                    paintContext.PaintPosY = part.Rectangle.Y;
                    paintContext.PaintPosX = part.Rectangle.X + part.Rectangle.Width;
                    paintContext.BisherMaxX = Math.Max(paintContext.BisherMaxX, paintContext.PaintPosX);
                }

                // ggf. den Cursorstrich hinter dem Node berechnen
                if (this.XMLNode == cursor.StartPos.AktNode)  // ist der Cursor im aktuellen Textnode
                {
                    if (cursor.StartPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode)
                    {
                        this.cursorPaintPos = new Point(paintContext.PaintPosX - 1, paintContext.PaintPosY + marginY);
                    }
                }
                this.lastPaintContextResult = paintContext.Clone();
                return paintContext.Clone();
            }
        }

        protected override void UnPaint(IGraphics gfx, PaintContext paintContext)
        {
            foreach (var textPart in this.textParts)
            {
                gfx.UnPaintRectangle(textPart.Rectangle);
            }
        }

        private IEnumerable<TextLine> GetTextLinesFromTextParts(TextSplitHelper.TextPart[] parts, PaintContext paintContext, int fontHeight, double fontWidth)
        {
            paintContext.HoeheAktZeile = Math.Max(paintContext.HoeheAktZeile, this.Config.MinLineHeight);
            var x = paintContext.PaintPosX;
            var y = paintContext.PaintPosY;
            var actualLine = 0;

            foreach (var part in parts)
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
            await xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, posInLine, action);
        }

        /// <summary>
        /// Findet heraus, welche Bereiche des Textes invertiert dargestellt werden müssen
        /// </summary>
        /// <param name="selectionStart"></param>
        /// <param name="selektionEnde"></param>
        private void StartUndEndeDerSelektionBestimmen(out int selectionStart, out int selectionEnd, XMLCursor cursor)
        {
            selectionStart = -1;
            selectionEnd = 0;

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
                    if (xmlEditor.EditorStatus.CursorOptimiert.IstNodeInnerhalbDerSelektion(this.XMLNode))
                    {
                        selectionStart = 0;
                        selectionEnd = AktuellerInhalt.Length;   // Ganzen Textnode selektieren
                    }
                }
            }
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}