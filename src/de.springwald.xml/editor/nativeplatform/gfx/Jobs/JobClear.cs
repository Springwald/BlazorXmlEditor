using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor.nativeplatform.gfxobs
{
    public class JobClear : GfxJob
    {
        public override JobTypes JobType => JobTypes.Clear;
        public Color Color { get; set; }
    }
}