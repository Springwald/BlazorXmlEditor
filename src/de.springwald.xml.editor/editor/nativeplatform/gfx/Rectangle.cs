
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
    }
}
