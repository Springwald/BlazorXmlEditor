// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class Color
    {
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }
        public byte A { get; private set; }
        public string AsHtml => ColorToHex(this);

        public static readonly Color Black = new Color(0, 0, 0);
        public static readonly Color Gray = new Color(100, 100, 100);
        public static readonly Color LightGray = new Color(200, 200, 200);
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Red = new Color(255, 0, 0);
        public static readonly Color Blue = new Color(0, 0, 255);
        public static readonly Color LightBlue = new Color(245, 245, 255);
        public static readonly Color DarkBlue = new Color(0, 0, 100);
        public static readonly Color Yellow = new Color(255, 255, 0);
        public static readonly Color Green = new Color(0, 255, 0);
        public static readonly Color LightGreen = new Color(200, 255, 200);
        public static readonly Color Transparent = new Color(0, 0, 0, 0);

        private Color(byte r, byte g, byte b) : this(r, g, b, 255) { }
        private Color(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        private Color invertedColor;

        public Color InvertedColor
        {
            get
            {
                if (this.invertedColor == null) this.invertedColor = new Color(r: (byte)(255 - this.R), g: (byte)(255 - this.G), b: (byte)(255 - this.B), a: this.A);
                return this.invertedColor;
            }
        }

        public static Color FromArgb(byte r, byte g, byte b)
        {
            return new Color(r, g, b);
        }

        public static string ColorToHex(Color actColor)
        // Translates a .NET Framework Color into a string containing the html hexadecimal
        // representation of a color. The string has a leading '#' character that is followed
        // by 6 hexadecimal digits.
        {
            if (actColor.A == 255) return $"#{actColor.R:X2}{actColor.G:X2}{actColor.B:X2}";
            return $"#{actColor.R:X2}{actColor.G:X2}{actColor.B:X2}{actColor.A:X2}";
        }

        public override bool Equals(object obj)
        {
            var col = (Color)obj;
            if (col == null) return false;
            return this.R == col.R && this.G == col.G && this.B == col.B && this.A == col.A;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                result = (result * 397) ^ R;
                result = (result * 397) ^ G;
                result = (result * 397) ^ B;
                result = (result * 397) ^ A;
                return result;
            }
        }
    }
}



