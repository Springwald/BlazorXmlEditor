using System;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Der Teil des XMLEditors, welcher sich mit der Anzeige des Blink-Cursors
    /// beschäftigt
    /// </summary>
    /// <remarks>
    /// (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>

    public partial class XMLEditor
    {

        /// <summary>Merkt sich den Grafik-Status des XMLEditors ohne Cursor</summary>
        //private GraphicsState _statusOhneCursor;

        /// <summary>Der Timer, um den Cursor blinken zu lassen</summary>
        // private Timer _timerCursorBlink;

        private bool _cursorBlinkOn = true; // true=Cursor wird als nächstes als Strich gezeichnet, false=Cursor wird nicht gezeichnet


        /// <summary>
        /// Hat der XML-Editor den Fokus
        /// </summary>
        public bool HatFokus
        {
            get
            {
                if (this.NativePlatform.ControlElement == null)
                {
                    return false;
                }
                else
                {
                    return this.NativePlatform.ControlElement.Focused;
                }
            }
        }

        /// <summary>
        ///  true=Cursor wird als nächstes als Strich gezeichnet, false=Cursor wird nicht gezeichnet
        /// </summary>
        public bool CursorBlinkOn
        {
            get { return this._cursorBlinkOn; }
            set
            {
                if (this.NativePlatform.ControlElement != null)
                {
                    if (this.NativePlatform.ControlElement.Focused) // Zeichnungssteuerelement hat Fokus
                    {
                        _cursorBlinkOn = value; // Wunsch übernehmen
                    }
                    else // Zeichnunssteuerelemen hat nicht den Fokus
                    {
                        _cursorBlinkOn = false; // Wunsch ignorieren, immer kein Cursor
                    }
                }
                else
                {
                    _cursorBlinkOn = value;
                }
                this.NativePlatform.InputEvents.ResetBlinkInterval();
            }
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

        private void InitCursorBlink()
        {
            this.NativePlatform.InputEvents.BlinkInterval.Add(this._timerCursorBlink_Tick);
            //_timerCursorBlink = new Timer();
            //_timerCursorBlink.Enabled = _cursorBlinkOn;
            //_timerCursorBlink.Interval = 600;
            //_timerCursorBlink.Tick += new EventHandler(_timerCursorBlink_Tick);
        }

        /// <summary>
        /// Der Cursor soll einmal blinken
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task _timerCursorBlink_Tick( EventArgs e)
        {
            if (this.NativePlatform.ControlElement == null) return;
            if (this.CursorBlinkOn == false) return;

            if (HatFokus) // Fokus ist im Zeichnungselement
            {
                _cursorBlinkOn = (!_cursorBlinkOn);
                // await this.NativePlatform.ControlElement.Invalidated.Trigger(null);
            }
            else // Fokus ist nicht im Zeichnungssteuerelement
            {
                if (_cursorBlinkOn == true) // Muss noch ausgeschaltet werden?
                {
                    _cursorBlinkOn = false;
                   //  await this.NativePlatform.ControlElement.Invalidated.Trigger(null);
                }
            }
        }
    }
}
