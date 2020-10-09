using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor.nativeplatform.gfxobs
{
    public class JobClear : GfxJob
    {
        public override JobTypes JobType => JobTypes.Clear;
        public Color FillColor { get; set; }

        public override string SortKey => $"{this.FillColor.AsHtml}";
    }
}