using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobDrawPolygon : GfxJob, IGfxJob
    {
        public Pen Pen { get; set; }

        public Point[] Points { get; set; }

        public async Task Paint(IGraphics gfx)
        {
            await gfx.DrawPolygonAsync(this.Pen, this.Points);
        }
    }
}
