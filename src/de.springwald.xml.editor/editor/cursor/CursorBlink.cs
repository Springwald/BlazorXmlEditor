// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;

namespace de.springwald.xml.editor.cursor
{
    internal class CursorBlink : IDisposable
    {
        private System.Timers.Timer blinkTimer;
        private bool active = true;

        public XmlAsyncEvent<bool> BlinkIntervalChanged = new XmlAsyncEvent<bool>();

        /// <summary>
        /// true = phase 1, paint cursor line; false = phase 2, dont paint cursor line
        /// </summary>
        public bool PaintCursor { get; private set; }

        public CursorBlink()
        {
            this.blinkTimer = new System.Timers.Timer();
            this.blinkTimer.Interval = 600;
            this.blinkTimer.Elapsed +=  BlinkTimer_Elapsed;
            this.blinkTimer.Start();
        }

        public void ResetBlinkPhase()
        {
            this.blinkTimer.Stop();
            if (this.active)
            {
                this.PaintCursor = true;
                this.blinkTimer.Start();
            }
            else
            {
                this.PaintCursor = false;
            }
        }

        public bool Active
        {
            get => active;
            set
            {
                this.active = value;
                this.ResetBlinkPhase();
            }
        }

        private async void BlinkTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.active)
            {
                this.PaintCursor = (!this.PaintCursor);
            }
            else
            {
                this.PaintCursor = false;
            }
            await this.BlinkIntervalChanged.Trigger(this.PaintCursor);
        }

        public void Dispose()
        {
            this.blinkTimer.Stop();
            this.blinkTimer.Elapsed -= BlinkTimer_Elapsed;
        }
    }
}
