using de.springwald.xml.events;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    public partial class XMLEditor
    {
        public XmlAsyncEvent<MouseEventArgs> MouseUpEvent = new XmlAsyncEvent<MouseEventArgs>();
        public XmlAsyncEvent<MouseEventArgs> MouseDownEvent = new XmlAsyncEvent<MouseEventArgs>();
        public XmlAsyncEvent<MouseEventArgs> MouseDownMoveEvent = new XmlAsyncEvent<MouseEventArgs>();

        private bool _mausIstGedrueckt = false; // Wird die Maustaste noch gehalten?

        private void MausEventsAnmelden()
        {
            this.NativePlatform.InputEvents.MouseDown.Add(this._zeichnungsSteuerelement_MouseDown);
            this.NativePlatform.InputEvents.MouseUp.Add(this._zeichnungsSteuerelement_MouseUp);
            this.NativePlatform.InputEvents.MouseMove.Add(this._zeichnungsSteuerelement_MouseMove);
        }

        /// <summary>
        /// In das Editor-Control wurde geklickt
        /// </summary>
        private async Task _zeichnungsSteuerelement_MouseDown(MouseEventArgs e)
        {
            _mausIstGedrueckt = true;
            await this.MouseDownEvent.Trigger(e);
        }

        /// <summary>
        /// Der Klick im Editor-Control wurde losgelassen
        /// </summary>
        async Task _zeichnungsSteuerelement_MouseUp(MouseEventArgs e)
        {
            _mausIstGedrueckt = false;
            await this.MouseUpEvent.Trigger(e);
        }

        /// <summary>
        /// Die Maus wird bewegt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task _zeichnungsSteuerelement_MouseMove(MouseEventArgs e)
        {
            if (_mausIstGedrueckt) await this.MouseDownMoveEvent.Trigger(e);
        }
    }
}
