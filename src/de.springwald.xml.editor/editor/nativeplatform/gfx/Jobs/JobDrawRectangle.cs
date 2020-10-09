namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobDrawRectangle : GfxJob
    {
        public override JobTypes JobType => JobTypes.DrawRectangle;

        public Color FillColor { get; set; }

        public Color BorderColor { get; set; }
        public float BorderWidth { get; set; } = 1;

        public Rectangle Rectangle { get; set; }

        public override string SortKey => $"{this.BorderColor}-{this.FillColor}";
    }
}