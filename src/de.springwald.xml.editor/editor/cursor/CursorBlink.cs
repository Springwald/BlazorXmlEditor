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

namespace de.springwald.xml.editor.editor.cursor
{
    internal class CursorBlink : IDisposable
    {
        private System.Timers.Timer blinkTimer;
        private bool active = true;

        public XmlAsyncEvent<EventArgs> BlinkIntervalChanged = new XmlAsyncEvent<EventArgs>();

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

        /// <summary>
        /// is the cursor blinking?
        /// </summary>
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
             var remember = this.PaintCursor;
            if (this.active)
            {
                this.PaintCursor = (!this.PaintCursor);
            }
            else
            {
                this.PaintCursor = false;
            }
            if (remember != this.PaintCursor) await this.BlinkIntervalChanged.Trigger(EventArgs.Empty);
        }

        ///// <summary>
        ///// Der Cursor soll einmal blinken
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //async Task _timerCursorBlink_Tick(EventArgs e)
        //{
        //    if (this.active == false)
        //    {
        //        this.PaintCursor = false;
        //    }
        //    else
        //    {
        //        this.PaintCursor = (!this.PaintCursor);
        //    }

        //    //if (this.HasFocus) // Fokus ist im Zeichnungselement
        //    //{
        //    //    this.BlinkPhaseVisible = (!this.BlinkPhaseVisible);
        //    //    // await this.NativePlatform.ControlElement.Invalidated.Trigger(null);
        //    //}
        //    //else // Fokus ist nicht im Zeichnungssteuerelement
        //    //{
        //    //    if (this.BlinkPhaseVisible == true) // Muss noch ausgeschaltet werden?
        //    //    {
        //    //        this.BlinkPhaseVisible = false;
        //    //        //  await this.NativePlatform.ControlElement.Invalidated.Trigger(null);
        //    //    }
        //    //}
        //    await Task.CompletedTask;
        //}

        public void Dispose()
        {
            this.blinkTimer.Stop();
            this.blinkTimer.Elapsed -= BlinkTimer_Elapsed;
        }

        /*

        /// <summary>
        /// Zeichnet den Cursor auf den Screen und sichert vorher den Screen
        /// ohne Cursor
        /// </summary>
        public void CursorZeichnen()
        {
            if (_statusOhneCursor != null)
            {
#warning Hier noch evtl. speicherprobleme checken
                // Noch ein alter Status drin. Kann das zu Speicherproblemen führen?
            }

            // Den Zustand ohne Cursor speichern
            _statusOhneCursor = _zeichnungsSteuerelement.CreateGraphics().Save();
        }

        /// <summary>
        /// Entfernt den Cursor, indem der Screen ohne Cursor wieder hergestellt wird
        /// </summary>
        public void CursorAusblenden()
        {
            // Zustand ohne Cursor wieder herstellen
            _zeichnungsSteuerelement.CreateGraphics().Restore(_statusOhneCursor);
            _statusOhneCursor = null;
        }*/

        //private void InitCursorBlink()
        //{
        //    this.NativePlatform.InputEvents.BlinkInterval.Add(this._timerCursorBlink_Tick);
        //    //_timerCursorBlink = new Timer();
        //    //_timerCursorBlink.Enabled = _cursorBlinkOn;
        //    //_timerCursorBlink.Interval = 600;
        //    //_timerCursorBlink.Tick += new EventHandler(_timerCursorBlink_Tick);
        //}


    }
}
