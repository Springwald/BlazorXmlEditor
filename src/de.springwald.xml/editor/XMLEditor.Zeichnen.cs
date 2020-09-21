using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{

    /// <summary>
    /// Die Zeichnungsfunktionen des XML-Editors
    /// </summary>
    /// <remarks>
    /// (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    public partial class XMLEditor
    {
        private XMLElement _rootElement;

        /// <summary>
        /// Muss in der überschriebenen OnPoint-Methode des Zeichnungssteuerelementes
        /// aufgerufen werden
        /// </summary>
        public async Task Paint(PaintEventArgs e)
        {
            if (_rootElement != null)  // Wenn das 
            {
                XMLEditorPaintPos paintPos = new XMLEditorPaintPos
                {
                    PosX = 10 + ZeichnungsOffsetX,
                    PosY = 10 + ZeichnungsOffsetY,
                    ZeilenStartX = 10 + ZeichnungsOffsetX,
                    ZeilenEndeX = WunschUmbruchX_ - ZeichnungsOffsetX
                };

                // XML-Anzeige vorberechnen
                _rootElement.PaintPos = paintPos.Clone();
                await _rootElement.Paint(XMLPaintArten.Vorberechnen, ZeichnungsOffsetX, ZeichnungsOffsetY, e);

                _virtuelleBreite = _rootElement.PaintPos.BisherMaxX + 50 - ZeichnungsOffsetX;
                _virtuelleHoehe = _rootElement.PaintPos.PosY + 50 - ZeichnungsOffsetY;

                // XML-Anzeige zeichnen
               // await this.NativePlatform.Gfx.ClearAsync(Color.White);
                _rootElement.PaintPos = paintPos;
               // await _rootElement.Paint(XMLPaintArten.AllesNeuZeichnenMitFehlerHighlighting, ZeichnungsOffsetX, ZeichnungsOffsetY, e);
            }
            await Task.CompletedTask;
        }

        public void FokusAufEingabeFormularSetzen()
        {
            /*if (this._zeichnungsSteuerelement != null)
            {
                this._zeichnungsSteuerelement.Focus();
            }*/
        }
    }
}
