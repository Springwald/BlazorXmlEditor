namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobDrawPolygon : GfxJob
    {
        public override JobTypes JobType => JobTypes.DrawPolygon;
        public Pen Pen { get; set; }
        public Point[] Points { get; set; }

        public override string SortKey => $"{this.Pen.Color}";
    }
}
