using de.springwald.xml.events;
using System;

namespace de.springwald.xml.editor.nativeplatform.events
{
    public interface IInputEvents
    {
        XmlAsyncEvent<MouseEventArgs> MouseDown { get; }
        XmlAsyncEvent<MouseEventArgs> MouseUp { get; }
        XmlAsyncEvent<MouseEventArgs> MouseMove { get; }

        XmlAsyncEvent<KeyPressEventArgs> KeyPress { get; }
        XmlAsyncEvent<PreviewKeyDownEventArgs> PreviewKey { get; }

        XmlAsyncEvent<EventArgs> Leave { get; }
        //XmlAsyncEvent<EventArgs> BlinkInterval { get; }

        //XmlAsyncEvent<EventArgs> BlinkDone { get; }

        /// <summary>
        /// reset blink timer to start
        /// </summary>
        //void ResetBlinkInterval();
    }
}
