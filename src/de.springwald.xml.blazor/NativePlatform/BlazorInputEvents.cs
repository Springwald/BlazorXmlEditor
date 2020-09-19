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

        public AsyncEvent<MouseEventArgs> MouseDown { get; } = new AsyncEvent<MouseEventArgs>();

        public AsyncEvent<MouseEventArgs> MouseUp { get; } = new AsyncEvent<MouseEventArgs>();

        public AsyncEvent<MouseEventArgs> MouseMove { get; } = new AsyncEvent<MouseEventArgs>();

        public AsyncEvent<KeyPressEventArgs> KeyPress { get; } = new AsyncEvent<KeyPressEventArgs>();

        public AsyncEvent<PreviewKeyDownEventArgs> PreviewKey { get; } = new AsyncEvent<PreviewKeyDownEventArgs>();

        public AsyncEvent<EventArgs> Leave { get; } = new AsyncEvent<EventArgs>();

        public AsyncEvent<EventArgs> BlinkInterval { get; } = new AsyncEvent<EventArgs>();

        public AsyncEvent<EventArgs> BlinkDone { get; } = new AsyncEvent<EventArgs>();

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
