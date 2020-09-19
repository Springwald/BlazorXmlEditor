namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class SolidBrush
    {
        public Color Color { get; protected set; }

        public SolidBrush(Color color)
        {
            this.Color = color;
        }
    }
}
