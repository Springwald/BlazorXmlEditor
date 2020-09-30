using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Stellt Informationen über die aktuellen Zeichenpositionen der aktuellen XML-Zeile und des nächsten
    /// Fließtext-Elementes bereit
    /// </summary>
    /// <remarks>
    // (C)2020 Daniel Springwald, Bochum Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    public class PaintContext
    {
        public int LimitLeft { get; set; }
        public int LimitRight { get; set; }

        public int PaintPosX { get; set; }
        public int PaintPosY { get; set; }

        public int ZeilenStartX { get; set; }

        public int HoeheAktZeile { get; set; }

        public int ZeilenEndeX { get; set; }

        public int BisherMaxX { get; set; }


        public PaintContext Clone()
        {
            return new PaintContext
            {
                LimitLeft = this.LimitLeft,
                LimitRight = this.LimitRight,
                PaintPosX = this.PaintPosX,
                PaintPosY = this.PaintPosY,
                ZeilenStartX = this.ZeilenStartX,
                HoeheAktZeile = this.HoeheAktZeile,
                ZeilenEndeX = this.ZeilenEndeX,
                BisherMaxX = this.BisherMaxX,
            };
        }
    }
}
