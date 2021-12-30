// A platform independent tag-view-style graphical XML editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2021 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using Blazor.Extensions;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.editor.nativeplatform.events;
using de.springwald.xml.editor.nativeplatform.gfx;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorNativePlatform : INativePlatform
    {
        public IClipboard Clipboard { get; }

        public IInputEvents InputEvents { get; }

        public IGraphics Gfx { get; }

        public int DesiredMaxWidth { get; private set; }

        public BlazorNativePlatform(BECanvasComponent canvas, BlazorClipboard blazorClipboard)
        {
            this.Clipboard = blazorClipboard;
            this.InputEvents = new BlazorInputEvents();
            this.Gfx = new BlazorGfx(canvas);
        }

        public void LogError(string v)
        {
        }

        public async Task SetDesiredSize(int desiredMaxWidth)
        {
            this.DesiredMaxWidth =  desiredMaxWidth;
            await Task.CompletedTask;
        }
    }
}
