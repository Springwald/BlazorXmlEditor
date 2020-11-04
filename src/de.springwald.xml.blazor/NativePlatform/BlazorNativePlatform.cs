using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.editor.nativeplatform.events;
using de.springwald.xml.editor.nativeplatform.gfx;
//using Excubo.Blazor.Canvas;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorNativePlatform : INativePlatform
    {
        public IClipboard Clipboard { get; }

        public IInputEvents InputEvents { get; }

        public IGraphics Gfx { get; }

#if Use2

        public BlazorNativePlatform(Canvas canvas)
        {
            this.Clipboard = new BlazorClipboard();
            this.ControlElement = new BlazorControlElement(canvas);
            this.InputEvents = new BlazorInputEvents();
            this.Focus = new BlazorFocus();
            this.Gfx = new BlazorGfx2(canvas);
        }

#else
        public BlazorNativePlatform(BECanvasComponent canvas, BlazorClipboard blazorClipboard)
        {
            this.Clipboard = blazorClipboard;
            this.InputEvents = new BlazorInputEvents();
            this.Gfx = new BlazorGfx(canvas);
        }

#endif 

        public void LogError(string v)
        {

        }

        public async Task SetSize(int width, int height)
        {
            await this.Gfx.SetSize(width, height);
        }
    }
}
