namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobFillPolygon : GfxJob
    {
        public override JobTypes JobType => JobTypes.FillPolygon;
        public Color Color { get; set; }
        public Point[] Points { get; set; }

        public override string SortKey => $"{this.Color}";
    }
}
