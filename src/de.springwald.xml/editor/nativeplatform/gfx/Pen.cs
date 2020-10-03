namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class Pen
    {
         public Color Color { get; set; }
        public float Width { get; set; }

        public Pen(Color color, float width)
        {
            this.Color = color;
            this.Width = width;
        }
    }
}
