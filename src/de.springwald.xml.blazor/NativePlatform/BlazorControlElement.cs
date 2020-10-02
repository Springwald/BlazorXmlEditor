
using de.springwald.xml.editor.nativeplatform.gfx;
using System;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorControlElement : de.springwald.xml.editor.nativeplatform.IControlElement
    {
        public BlazorControlElement()
        {
        }

        public bool Enabled { get => true; set { } }

        public bool Focused => true;

        public Color BackColor => Color.White;

        public XmlAsyncEvent<EventArgs> Invalidated { get; } = new XmlAsyncEvent<EventArgs>();

    }
}
