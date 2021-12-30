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
    public class Rectangle
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Rectangle(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public bool Contains(Point point)
        {
            return point.X >= this.X
                && point.Y >= this.Y
                && point.X < this.X + this.Width
                && point.Y < this.Y + this.Height;
        }

        public bool Equals(Rectangle second)
        {
            if (second == null) return false;
            return second.X == this.X
                && second.Y == this.Y
                && second.Height == this.Height
                && second.Width == this.Width;
        }
    }
}
