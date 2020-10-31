﻿// A platform indepentend tag-view-style graphical xml editor
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
    /// Provides information about the current character positions of the current XML line and the next continuous text element
    /// </summary>
    public class PaintContext
    {
        public bool CursorBlinkOn { get; set; }

        public int LimitLeft { get; set; }
        public int LimitRight { get; set; }

        public int PaintPosX { get; set; }
        public int PaintPosY { get; set; }

        public int RowStartX { get; set; }

        public int HeightActualRow { get; set; }

        public int FoundMaxX { get; set; }


        public PaintContext Clone()
        {
            return new PaintContext
            {
                CursorBlinkOn = this.CursorBlinkOn,
                LimitLeft = this.LimitLeft,
                LimitRight = this.LimitRight,
                PaintPosX = this.PaintPosX,
                PaintPosY = this.PaintPosY,
                RowStartX = this.RowStartX,
                HeightActualRow = this.HeightActualRow,
                FoundMaxX = this.FoundMaxX,
            };
        }
    }
}
