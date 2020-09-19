using de.springwald.xml.events;
using System;
using System.Timers;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorInputEvents : de.springwald.xml.editor.nativeplatform.events.IInputEvents, IDisposable
    {
        private Timer timer;

        public BlazorInputEvents()
        {
            this.SetUpTimer();
        }

        public XmlAsyncEvent<MouseEventArgs> MouseDown { get; } = new XmlAsyncEvent<MouseEventArgs>();

        public XmlAsyncEvent<MouseEventArgs> MouseUp { get; } = new XmlAsyncEvent<MouseEventArgs>();

        public XmlAsyncEvent<MouseEventArgs> MouseMove { get; } = new XmlAsyncEvent<MouseEventArgs>();

        public XmlAsyncEvent<KeyPressEventArgs> KeyPress { get; } = new XmlAsyncEvent<KeyPressEventArgs>();

        public XmlAsyncEvent<PreviewKeyDownEventArgs> PreviewKey { get; } = new XmlAsyncEvent<PreviewKeyDownEventArgs>();

        public XmlAsyncEvent<EventArgs> Leave { get; } = new XmlAsyncEvent<EventArgs>();

        public XmlAsyncEvent<EventArgs> BlinkInterval { get; } = new XmlAsyncEvent<EventArgs>();

        public XmlAsyncEvent<EventArgs> BlinkDone { get; } = new XmlAsyncEvent<EventArgs>();

        public void Dispose()
        {
            this.timer.Stop();
        }

        public void ResetBlinkInterval()
        {
            this.timer.Stop();
            this.timer.Start();
        }

        private void SetUpTimer()
        {
            this.timer = new Timer();
            this.timer.Interval = 600;
            this.timer.Elapsed += async (sender, e) =>
            {
                await this.BlinkInterval.Trigger(e);
                await this.BlinkDone.Trigger(e);
            };
            this.timer.Start();
        }
    }
}
