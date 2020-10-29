using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor.nativeplatform.gfx.Jobs
{
    public class JobUnpaintRectangle : GfxJob
    {
        public Rectangle Rectangle { get; set; }

        public override JobTypes JobType => JobTypes.UnPaintRectangle;

        public override string SortKey => $"";
    }
}
