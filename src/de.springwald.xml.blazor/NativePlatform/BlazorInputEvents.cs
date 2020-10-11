

using System;
using System.Timers;
using Microsoft.AspNetCore.Components.Web;

using de.springwald.xml.events;
using de.springwald.xml.editor.nativeplatform.events;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorInputEvents : IInputEvents, IDisposable
    {
        private Timer timer;

        public BlazorInputEvents()
        {
            this.SetUpTimer();
        }

        public XmlAsyncEvent<events.MouseEventArgs> MouseDown { get; } = new XmlAsyncEvent<events.MouseEventArgs>();

        public XmlAsyncEvent<events.MouseEventArgs> MouseUp { get; } = new XmlAsyncEvent<events.MouseEventArgs>();

        public XmlAsyncEvent<events.MouseEventArgs> MouseMove { get; } = new XmlAsyncEvent<events.MouseEventArgs>();

        public XmlAsyncEvent<KeyEventArgs> KeyPress { get; } = new XmlAsyncEvent<KeyEventArgs>();

        public XmlAsyncEvent<KeyEventArgs> PreviewKey { get; } = new XmlAsyncEvent<KeyEventArgs>();

        public XmlAsyncEvent<EventArgs> Leave { get; } = new XmlAsyncEvent<EventArgs>();

        public XmlAsyncEvent<EventArgs> BlinkInterval { get; } = new XmlAsyncEvent<EventArgs>();

        public XmlAsyncEvent<EventArgs> BlinkDone { get; } = new XmlAsyncEvent<EventArgs>();


        //async void EventOnKeyUp(KeyboardEventArgs e)
        //{
        //    await this.nativePlattform.InputEvents.KeyPress.Trigger(new de.springwald.xml.events.KeyPressEventArgs { KeyChar = e.Key });
        //}

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
