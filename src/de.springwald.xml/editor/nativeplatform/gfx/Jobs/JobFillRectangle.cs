namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobFillRectangle : GfxJob
    {
        public override JobTypes JobType => JobTypes.FillRectangle;
        public Color Color { get; set; }
        public Rectangle Rectangle { get; set; }

        public override string SortKey => $"{this.Color}";
    }
}