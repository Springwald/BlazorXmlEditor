
using de.springwald.xml.editor.nativeplatform.events;
using de.springwald.xml.events;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorInputEvents : IInputEvents
    {
        public XmlAsyncEvent<events.MouseEventArgs> MouseDown { get; } = new XmlAsyncEvent<events.MouseEventArgs>();

        public XmlAsyncEvent<events.MouseEventArgs> MouseUp { get; } = new XmlAsyncEvent<events.MouseEventArgs>();

        public XmlAsyncEvent<events.MouseEventArgs> MouseMove { get; } = new XmlAsyncEvent<events.MouseEventArgs>();

        public XmlAsyncEvent<KeyEventArgs> KeyPress { get; } = new XmlAsyncEvent<KeyEventArgs>();

        public XmlAsyncEvent<KeyEventArgs> PreviewKey { get; } = new XmlAsyncEvent<KeyEventArgs>();
    }
}
