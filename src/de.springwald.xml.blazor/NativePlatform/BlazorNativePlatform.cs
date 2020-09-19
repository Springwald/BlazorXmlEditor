using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.editor.nativeplatform.events;
using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.blazor.NativePlatform { 
    public class BlazorNativePlatform : INativePlatform
    {
        public IClipboard Clipboard { get; }

        public IControlElement ControlElement { get; }

        public IInputEvents InputEvents { get; }

        public IFocus Focus { get; }

        public IGraphics Gfx { get; }


        public BlazorNativePlatform(BECanvasComponent canvas, Canvas2DContext context)
        {
            this.Clipboard = new BlazorClipboard();
            this.ControlElement = new BlazorControlElement(canvas);
            this.InputEvents = new BlazorInputEvents();
            this.Focus = new BlazorFocus();
            this.Gfx = new BlazorGfx(context);
        }

        public void ProtokolliereFehler(string v)
        {

        }
    }
}
