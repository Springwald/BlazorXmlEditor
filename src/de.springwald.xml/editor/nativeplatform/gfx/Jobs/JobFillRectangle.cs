using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobFillRectangle : GfxJob, IGfxJob
    {
        public SolidBrush Brush { get; set; }

        public Rectangle Rectangle { get; set; }

        public async Task Paint(IGraphics gfx)
        {
            await gfx.FillRectangleAsync(this.Brush, this.Rectangle);
        }
    }
}