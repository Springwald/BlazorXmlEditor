// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Threading.Tasks;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.events;

namespace de.springwald.xml.editor
{
    internal class MouseHandler: IDisposable
    {
        private INativePlatform nativePlatform;
        private bool mouseIsDown = false; // Is the mouse button still held down?

        public XmlAsyncEvent<MouseEventArgs> MouseUpEvent = new XmlAsyncEvent<MouseEventArgs>();
        public XmlAsyncEvent<MouseEventArgs> MouseDownEvent = new XmlAsyncEvent<MouseEventArgs>();
        public XmlAsyncEvent<MouseEventArgs> MouseDownMoveEvent = new XmlAsyncEvent<MouseEventArgs>();

        public MouseHandler(INativePlatform nativePlatform)
        {
            this.nativePlatform = nativePlatform;
            this.nativePlatform.InputEvents.MouseDown.Add(this.OnMouseDown);
            this.nativePlatform.InputEvents.MouseUp.Add(this.OnMouseUp);
            this.nativePlatform.InputEvents.MouseMove.Add(this.OnMouseMove);
        }

        public void Dispose()
        {
            this.nativePlatform.InputEvents.MouseDown.Remove(this.OnMouseDown);
            this.nativePlatform.InputEvents.MouseUp.Remove(this.OnMouseUp);
            this.nativePlatform.InputEvents.MouseMove.Remove(this.OnMouseMove);
        }

        private async Task OnMouseDown(MouseEventArgs e)
        {
            this.mouseIsDown = true;
            await this.MouseDownEvent.Trigger(e);
        }

        async Task OnMouseUp(MouseEventArgs e)
        {
            this.mouseIsDown = false;
            await this.MouseUpEvent.Trigger(e);
        }

        async Task OnMouseMove(MouseEventArgs e)
        {
            if (this.mouseIsDown) await this.MouseDownMoveEvent.Trigger(e);
        }
    }
}
