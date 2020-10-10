// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Threading.Tasks;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.nativeplatform.gfxobs;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// The drawing functions of the XML editor
    /// </summary>
    public partial class XMLEditor
    {
        public async Task Paint(int limitRight)
        {
            this.NativePlatform.Gfx.AddJob(new JobClear { FillColor = Color.White });

            if (this.EditorStatus.RootElement != null)  
            {
                var paintContext = new PaintContext
                {
                    LimitLeft = 0,
                    LimitRight = limitRight,
                    PaintPosX = 10 + ZeichnungsOffsetX,
                    PaintPosY = 10 + ZeichnungsOffsetY,
                    ZeilenStartX = 10 + ZeichnungsOffsetX,
                };

                var context1 = await this.EditorStatus.RootElement.Paint(paintContext.Clone(), this.NativePlatform.Gfx);
                _virtuelleBreite = context1.BisherMaxX + 50 - ZeichnungsOffsetX;
                _virtuelleHoehe = context1.PaintPosY + 50 - ZeichnungsOffsetY;
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