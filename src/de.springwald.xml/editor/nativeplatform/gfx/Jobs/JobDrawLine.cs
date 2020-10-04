using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobDrawLine : GfxJob, IGfxJob
    {
        public Pen Pen { get; set; }

        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }

        public async Task Paint(IGraphics gfx)
        {
            await gfx.DrawLineAsync(this.Pen, X1, Y1, X2, Y2);
        }
    }
}
