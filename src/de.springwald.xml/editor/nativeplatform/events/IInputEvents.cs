using de.springwald.xml.events;
using System;

namespace de.springwald.xml.editor.nativeplatform.events
{
    public interface IInputEvents
    {
        AsyncEvent<MouseEventArgs> MouseDown { get; }
        AsyncEvent<MouseEventArgs> MouseUp { get; }
        AsyncEvent<MouseEventArgs> MouseMove { get; }

        AsyncEvent<KeyPressEventArgs> KeyPress { get; }
        AsyncEvent<PreviewKeyDownEventArgs> PreviewKey { get; }

        AsyncEvent<EventArgs> Leave { get; }

        AsyncEvent<EventArgs> BlinkInterval { get; }

        AsyncEvent<EventArgs> BlinkDone { get; }

        /// <summary>
        /// reset blink timer to start
        /// </summary>
        void ResetBlinkInterval();
    }
}
