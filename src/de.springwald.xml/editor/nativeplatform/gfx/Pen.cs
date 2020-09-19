namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class Pen
    {
        public enum DashStyles
        {
            Solid,
            Dash
        }

        public enum LineCap
        {
            NoAnchor,
            RoundAnchor,
            SquareAnchor
        }

        public LineCap StartCap { get; set; }
        public LineCap EndCap { get; set; }
        public DashStyles DashStyle { get; set; }
        public Color Color { get; set; }
        public float Width { get; set; }

        public Pen(Color color, float width)
        {
            this.Color = color;
            this.Width = width;
        }
    }
}
