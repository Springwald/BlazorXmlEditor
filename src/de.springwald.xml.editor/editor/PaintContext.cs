// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Stellt Informationen über die aktuellen Zeichenpositionen der aktuellen XML-Zeile und des nächsten
    /// Fließtext-Elementes bereit
    /// </summary>

    public class PaintContext
    {
        public readonly int LayerClearBackground = 0;
        public readonly int LayerTagBackground = 1;
        public readonly int LayerAttributeBackground = 2;
        public readonly int LayerTagBorder = 3;
        public readonly int LayerClickAreas = 9;
        public readonly int LayerCursor = 10;
        public readonly int LayerText = 20;

        public int LimitLeft { get; set; }
        public int LimitRight { get; set; }

        public int PaintPosX { get; set; }
        public int PaintPosY { get; set; }

        public int ZeilenStartX { get; set; }

        public int HoeheAktZeile { get; set; }

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
                BisherMaxX = this.BisherMaxX,
            };
        }
    }
}
