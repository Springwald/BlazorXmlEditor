using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobFillPolygon : GfxJob, IGfxJob
    {
        public SolidBrush Brush { get; set; }
        
        public  Point[] Points { get; set; }

        public async Task Paint(IGraphics gfx)
        {
            await gfx.FillPolygonAsync(this.Brush, this.Points);
        }
    }
}
