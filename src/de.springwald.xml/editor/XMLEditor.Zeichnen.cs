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
        public async Task Paint(int limitRight)
        {
            await this.NativePlatform.Gfx.ClearAsync(Color.White);

            if (_rootElement != null)  // Wenn das 
            {
                var paintContext = new PaintContext
                {
                    LimitLeft = 0,
                    LimitRight = limitRight,
                    PaintPosX = 10 + ZeichnungsOffsetX,
                    PaintPosY = 10 + ZeichnungsOffsetY,
                    ZeilenStartX = 10 + ZeichnungsOffsetX,
                };

                // XML-Anzeige vorberechnen
                var context1 =  await _rootElement.Paint(paintContext.Clone() , this.NativePlatform.Gfx);
                _virtuelleBreite = context1.BisherMaxX + 50 - ZeichnungsOffsetX;
                _virtuelleHoehe = context1.PaintPosY + 50 - ZeichnungsOffsetY;

                //// XML-Anzeige zeichnen
                //await this.NativePlatform.Gfx.ClearAsync(Color.White);
                //_rootElement.PaintPos = paintPos;
                //await _rootElement.Paint(paintContext, XMLPaintArten.AllesNeuZeichnenMitFehlerHighlighting, e);
            }

            await this.NativePlatform.Gfx.PaintJobs();
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
