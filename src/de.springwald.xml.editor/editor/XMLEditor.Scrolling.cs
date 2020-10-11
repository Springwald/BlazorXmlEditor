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
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;

namespace de.springwald.xml.editor
{
    public partial class XMLEditor
    {

        private int _virtuelleBreite;
        private int _virtuelleHoehe;


        /// <summary>
        /// Die Aktuelle Position, an welcher der Cursor zur Zeit steckt. Wird z.B. genutzt um sicher zu stellen,
        /// dass das Scrolling des Fensters immer korrekt ist
        /// </summary>
        private Point _aktScrollingCursorPos_;

        //private VScrollBar _vScrollBar;
        //private HScrollBar _hScrollBar;

        /// <summary>
        /// Dort soll zu Zeichnen begonnen werden
        /// </summary>
        private int ZeichnungsOffsetY
        {
            get
            {
                /*if (_vScrollBar.Visible)
                {
                    return -_vScrollBar.Value;
                }
                else*/
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Dort soll zu Zeichnen begonnen werden
        /// </summary>
        private int ZeichnungsOffsetX
        {
            get
            {
              /*  if (_hScrollBar.Visible)
                {
                    return -_hScrollBar.Value;
                }
                else*/
                {
                    return 0;
                }
            }
        }
 
        /// <summary>
        /// Die Aktuelle Position, an welcher der Cursor zur Zeit steckt. Wird z.B. genutzt um sicher zu stellen,
        /// dass das Scrolling des Fensters immer korrekt ist
        /// </summary>
        public Point AktScrollingCursorPos
        {
            set { _aktScrollingCursorPos_ = value; }
            get { return _aktScrollingCursorPos_; }
        }


        /// <summary>
        /// Macht alles dafür bereit, dass im Zeichnunssteuerelement gescrollt werden kann
        /// </summary>
        private void InitScrolling()
        {
            if (this.NativePlatform.ControlElement != null)
            {
                // Dem Zeichnunssteuereleme Scrollbars einsetzen
                //_vScrollBar = new VScrollBar();
                //_hScrollBar = new HScrollBar();
                //_zeichnungsSteuerelement.Controls.Add(_vScrollBar);
                //_zeichnungsSteuerelement.Controls.Add(_hScrollBar);
                //_vScrollBar.ValueChanged += new EventHandler(_vScrollBar_ValueChanged);
                //_hScrollBar.ValueChanged += new EventHandler(_hScrollBar_ValueChanged);

                // Wenn das Zeichnunssteuerelemente verändert wird, dann auch die Scrollbars neu anlegen

                /*
                _zeichnungsSteuerelement.Resize += new EventHandler(_zeichnungsSteuerelement_Resize);
                _zeichnungsSteuerelement_Resize(null, null);

                _zeichnungsSteuerelement.MouseWheel += new MouseEventHandler(_zeichnungsSteuerelement_MouseWheel);

                */


            }
        }

        /// <summary>
        /// Sagt Bescheid, dass gescrollt werden muss
        /// </summary>
        void ScrollingNotwendig()
        {
            DoTheScrollIntern();
        }

        void DoTheScrollIntern()
        {
            ScrollbarsAnzeigen();
            CursorInSichtbarenBereichScrollen();
        }

        async Task _hScrollBar_ValueChanged(object sender, EventArgs e)
        {
         //   await this.NativePlatform.ControlElement.Invalidated.Trigger(EventArgs.Empty);
        }

        async Task _vScrollBar_ValueChanged(object sender, EventArgs e)
        {
           // await this.NativePlatform.ControlElement.Invalidated.Trigger(EventArgs.Empty);
        }


        /// <summary>
        ///  Wenn mit einer Scrollmaus im Editor-Control gescrollt wurde, dann auch scrollen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _zeichnungsSteuerelement_MouseWheel(object sender, MouseEventArgs e)
        {
           /*  if (_vScrollBar.Visible)
            {
                _vScrollBar.Value = Math.Max(0, Math.Min(_vScrollBar.Maximum - _vScrollBar.LargeChange, Math.Max(0, _vScrollBar.Value - e.Delta)));
            }*/
        }

        /// <summary>
        /// Zeichnet die Scrollbars und deren Position
        /// </summary>
        private void ScrollbarsAnzeigen()
        {
          /* // Die Vertikale Scrollbar einstellen
            if (_vScrollBar.Height > _virtuelleHoehe + 20)
            {
                // Keine vertikale Scrollbar notwendig
                _vScrollBar.Value = 0;
                _vScrollBar.Visible = false;
            }
            else
            {
                _vScrollBar.Visible = true;
                _vScrollBar.Maximum = _virtuelleHoehe;
                _vScrollBar.LargeChange = _zeichnungsSteuerelement.Height;
            }

            // Die horizontale Scrollbar einstellen
            if (_hScrollBar.Width > _virtuelleBreite + 20)
            {
                // Keine horizontale Scrollbar notwendig
                _hScrollBar.Value = 0;
                _hScrollBar.Visible = false;
            }
            else
            {
                _hScrollBar.Visible = true;
                _hScrollBar.Maximum = _virtuelleBreite;
                _hScrollBar.LargeChange = _zeichnungsSteuerelement.Width;
            }

            // Größe und Position der Scrollbars bestimmen
            _vScrollBar.Top = 0;
            _vScrollBar.Left = _zeichnungsSteuerelement.Width - _vScrollBar.Width;
            if (_hScrollBar.Visible)
            {
                _vScrollBar.Height = _zeichnungsSteuerelement.Height - _hScrollBar.Height;
            }
            else
            {
                _vScrollBar.Height = _zeichnungsSteuerelement.Height;
            }
            _hScrollBar.Left = 0;
            _hScrollBar.Top = _zeichnungsSteuerelement.Height - _hScrollBar.Height;
            if (_vScrollBar.Visible)
            {
                _hScrollBar.Width = _zeichnungsSteuerelement.Width - _vScrollBar.Width;
            }
            else
            {
                _hScrollBar.Width = _zeichnungsSteuerelement.Width;
            }*/
        }


        /// <summary>
        /// Wenn die aktuelle Position des Cursors nicht im sichtbaren Bereich ist, dann hineinscrollem
        /// </summary>
        public void CursorInSichtbarenBereichScrollen()
        {
            /*
            if (_hScrollBar.Visible)
            {

                int wieWeitNachLinksRaus = AktScrollingCursorPos.X;
                if (wieWeitNachLinksRaus < 0)
                {
                    // Zu weit nach unten gescrollt, also nach oben scrollen
                    _hScrollBar.Value = Math.Max(0, _hScrollBar.Value + wieWeitNachLinksRaus);
                }
                else
                {
                    int wieWeitNachRechtsRaus = AktScrollingCursorPos.X + -_hScrollBar.Width;
                    if (wieWeitNachRechtsRaus > 0)
                    {
                        // Zu weit nach oben gescrollt, also nach unten scrollen
                        _hScrollBar.Value =
                            Math.Max(0,
                            Math.Min(
                                _virtuelleBreite - _hScrollBar.LargeChange,
                                _hScrollBar.Value + wieWeitNachRechtsRaus)
                                );
                    }
                }
            }


            if (_vScrollBar.Visible)
            {
                int wieWeitNachObenRaus = AktScrollingCursorPos.Y;
                if (wieWeitNachObenRaus < 0)
                {
                    // Zu weit nach unten gescrollt, also nach oben scrollen
                    _vScrollBar.Value = Math.Max(0, _vScrollBar.Value + wieWeitNachObenRaus);
                }
                else
                {
                    const int cursorHoehe = 20;
                    int wieWeitNachUntenRaus = AktScrollingCursorPos.Y + cursorHoehe + -_vScrollBar.Height;
                    if (wieWeitNachUntenRaus > 0)
                    {
                        // Zu weit nach oben gescrollt, also nach unten scrollen
                        _vScrollBar.Value =
                            Math.Max(0,
                            Math.Min(
                                _virtuelleHoehe - _vScrollBar.LargeChange,
                                _vScrollBar.Value + wieWeitNachUntenRaus)
                                );

                    }
                }
            }*/
        }

        /// <summary>
        /// Wenn das Zeichnunssteuerelemente verändert wird, dann auch die Scrollbars neu anlegen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _zeichnungsSteuerelement_Resize(object sender, EventArgs e)
        {
            ScrollbarsAnzeigen();
        }

    }
}
