namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobDrawPolygon : GfxJob
    {
        public override JobTypes JobType => JobTypes.DrawPolygon;

        public Color BorderColor { get; set; }
        public float BorderWidth { get; set; } = 1;
        public Color FillColor { get; set; }
        public Point[] Points { get; set; }

        public override string SortKey => $"{this.BorderColor}-{this.FillColor}";
    }
}
