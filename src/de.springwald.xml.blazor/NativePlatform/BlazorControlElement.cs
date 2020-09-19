using Blazor.Extensions;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorControlElement : de.springwald.xml.editor.nativeplatform.IControlElement
    {
        private BECanvasComponent canvas;

        public BlazorControlElement(BECanvasComponent canvas)
        {
            this.canvas = canvas;
        }

        public int Width => (int)this.canvas.Width;

        public bool Enabled { get => true; set { } }

        public bool Focused => true;

        public Color BackColor => Color.White;

        public XmlAsyncEvent<EventArgs> Invalidated { get; } = new XmlAsyncEvent<EventArgs>();

    }
}
