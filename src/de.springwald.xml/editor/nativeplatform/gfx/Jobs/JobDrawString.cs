using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobDrawString : GfxJob, IGfxJob
    {
        public int X { get; set; }
        public int Y { get; set; }

        public string Text { get; set; }

        public Font Font { get; set; }

        public Color Color  { get; set; }

        public async Task Paint(IGraphics gfx)
        {
            await gfx.DrawStringAsync(this.Text, this.Font, this.Color, this.X, this.Y);
        }
    }
}

