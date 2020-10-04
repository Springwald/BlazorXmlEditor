namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobDrawRectangle : GfxJob
    {
        public override JobTypes JobType => JobTypes.DrawRectangle;
        public Pen Pen { get; set; }
        public Rectangle Rectangle { get; set; }
    }
}