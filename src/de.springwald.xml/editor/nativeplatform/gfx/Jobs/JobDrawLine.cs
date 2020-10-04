namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobDrawLine : GfxJob
    {
        public override JobTypes JobType => JobTypes.DrawLine;
        public Color Color { get; set; }
        public float LineWidth { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        public override string SortKey => $"{this.Color}";
    }
}
