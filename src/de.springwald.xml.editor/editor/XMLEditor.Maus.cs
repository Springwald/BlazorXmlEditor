//// A platform indepentend tag-view-style graphical xml editor
//// https://github.com/Springwald/BlazorXmlEditor
////
//// (C) 2020 Daniel Springwald, Bochum Germany
//// Springwald Software  -   www.springwald.de
//// daniel@springwald.de -  +49 234 298 788 46
//// All rights reserved
//// Licensed under MIT License

//using de.springwald.xml.events;
//using System.Threading.Tasks;

//namespace de.springwald.xml.editor
//{
//    public partial class XMLEditor
//    {
//        public XmlAsyncEvent<MouseEventArgs> MouseUpEvent = new XmlAsyncEvent<MouseEventArgs>();
//        public XmlAsyncEvent<MouseEventArgs> MouseDownEvent = new XmlAsyncEvent<MouseEventArgs>();
//        public XmlAsyncEvent<MouseEventArgs> MouseDownMoveEvent = new XmlAsyncEvent<MouseEventArgs>();

//        private bool mouseIsDown = false; // Wird die Maustaste noch gehalten?

//        private void MausEventsAnmelden()
//        {
//            this.NativePlatform.InputEvents.MouseDown.Add(this.OnMouseDown);
//            this.NativePlatform.InputEvents.MouseUp.Add(this.OnMouseUp);
//            this.NativePlatform.InputEvents.MouseMove.Add(this.OnMouseMove);
//        }

//        /// <summary>
//        /// The editor control was clicked
//        /// </summary>
//        private async Task OnMouseDown(MouseEventArgs e)
//        {
//            this.mouseIsDown = true;
//            await this.MouseDownEvent.Trigger(e);
//        }

//        /// <summary>
//        /// The click in the editor control was released
//        /// </summary>
//        async Task OnMouseUp(MouseEventArgs e)
//        {
//            this.mouseIsDown = false;
//            await this.MouseUpEvent.Trigger(e);
//        }

//        /// <summary>
//        /// The mouse is moving
//        /// </summary>
//        async Task OnMouseMove(MouseEventArgs e)
//        {
//            if (this.mouseIsDown) await this.MouseDownMoveEvent.Trigger(e);
//        }
//    }
//}
