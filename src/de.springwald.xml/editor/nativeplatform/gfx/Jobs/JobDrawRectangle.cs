using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    class JobDrawRectangle : GfxJob, IGfxJob
    {
        public Pen Pen { get; set; }

        public Rectangle Rectangle { get; set; }

        public async Task Paint(IGraphics gfx)
        {
            await gfx.DrawRectangleAsync(this.Pen, this.Rectangle);
        }
    }
}
