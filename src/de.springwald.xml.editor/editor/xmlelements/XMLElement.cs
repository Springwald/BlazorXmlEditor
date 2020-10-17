// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

#define XXklickbereicheRotAnzeigen // Sollen die klickbaren Bereiche rot angezeigt werden?

using de.springwald.xml.cursor;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Basic element for drawing XML editor elements
    /// </summary>
    public abstract class XMLElement : IDisposable
    {
        public enum PaintModes
        {
            ForcePaintNoUnPaintNeeded,
            ForcePaintAndUnpaintBefore,
            OnlyPaintWhenChanged
        }

        private bool _disposed = false;

        protected Point cursorPaintPos;  // there the cursor is drawn in this node, if it is the current node
        protected XMLEditor xmlEditor;
        protected EditorConfig Config { get; }
  
        /// <summary>
        /// The XMLNode to be displayed with this element
        /// </summary>
        public System.Xml.XmlNode XMLNode { get; }

        /// <param name="xmlNode">The XML-Node to be drawn</param>
        /// <param name="xmlEditor">The editor for which the node is to be drawn</param>
        public XMLElement(System.Xml.XmlNode xmlNode, XMLEditor xmlEditor)
        {
            this.XMLNode = xmlNode;
            this.xmlEditor = xmlEditor;
            this.Config = xmlEditor.EditorConfig;

            this.xmlEditor.EditorStatus.CursorRoh.ChangedEvent.Add(this.Cursor_ChangedEvent);
            this.xmlEditor.MouseHandler.MouseDownEvent.Add(this._xmlEditor_MouseDownEvent);
            this.xmlEditor.MouseHandler.MouseUpEvent.Add(this._xmlEditor_MouseUpEvent);
            this.xmlEditor.MouseHandler.MouseDownMoveEvent.Add(this._xmlEditor_MouseDownMoveEvent);
            this.xmlEditor.XmlElementeAufraeumenEvent += new EventHandler(_xmlEditor_xmlElementeAufraeumenEvent);
        }

        /// <summary>
        /// Draws the XML element on the screen
        /// </summary>
        public virtual async Task<PaintContext> Paint(PaintContext paintContext, XMLCursor cursor, IGraphics gfx, PaintModes paintMode)
        {
            if (this._disposed) return paintContext;
            if (this.XMLNode == null) return paintContext;
            if (this.xmlEditor == null) return paintContext;



            paintContext = await PaintInternal(paintContext, cursor, gfx, paintMode);
            if (this.cursorPaintPos != null) this.PaintCursor(gfx);

#if klickbereicheRotAnzeigen
            KlickbereicheAnzeigen(paintContext, gfx);
#endif
            return paintContext;
        }

        protected abstract Task<PaintContext> PaintInternal(PaintContext paintContext, XMLCursor cursor, IGraphics gfx, PaintModes paintMode);

        protected abstract void UnPaint(IGraphics gfx, PaintContext paintContext);

        protected abstract bool IsClickPosInsideNode(Point pos);

        /// <summary>
        /// Draws the vertical cursor line
        /// </summary>
        protected virtual void PaintCursor(IGraphics gfx)
        {
            if (this.cursorPaintPos == null) return;
            if (this.xmlEditor.CursorBlink.PaintCursor == false) return;

            var height = (int)(Math.Max(this.xmlEditor.EditorConfig.TextNodeFont.Height, this.xmlEditor.EditorConfig.NodeNameFont.Height) * 1.6);
            var margin = height / 5;
            gfx.AddJob(new JobDrawLine
            {
                Batchable = true,
                Layer = GfxJob.Layers.Cursor,
                Color = Color.Black,
                LineWidth = 2,
                X1 = cursorPaintPos.X,
                Y1 = cursorPaintPos.Y + margin,
                X2 = cursorPaintPos.X,
                Y2 = cursorPaintPos.Y + height - margin
            });

            // remember where the cursor is currently drawn, so that you can scroll there when the cursor is moved out of the visible area
            xmlEditor.AktScrollingCursorPos = cursorPaintPos;
        }

        /// <summary>
        /// draws the mouse clickable areas
        /// </summary>
        //private void KlickbereicheAnzeigen(IGraphics gfx)
        //{
        //    foreach (var rechteck in this._klickBereiche)
        //    {
        //        gfx.AddJob(new JobDrawRectangle
        //        {
        //            Layer = GfxJob.Layers.ClickAreas,
        //            Batchable = true,
        //            BorderColor = Color.Red,
        //            Rectangle = rechteck
        //        });
        //    }
        //}


        /// <summary>
        /// Der Editor hat darum gebeten, dass alle Elemente, welche nicht mehr verwendet werden, entladen werden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _xmlEditor_xmlElementeAufraeumenEvent(object sender, EventArgs e)
        {
            if (this.XMLNode == null)
            {
                Dispose();
                return;
            }

            if (this.XMLNode.ParentNode == null)
            {
                Dispose();
                return;
            }
        }

        protected void UnPaintRectangle(IGraphics gfx, Rectangle rectangle)
        {
            if (rectangle != null)
            {
                rectangle = new Rectangle(rectangle.X-1, rectangle.Y - 1, rectangle.Width + 2, rectangle.Height + 2);

                gfx.AddJob(new JobDrawRectangle
                {
                    Layer = GfxJob.Layers.ClearBackground,
                    Batchable = true,
                    // FillColor = Color.Red, // debug color
                    FillColor = this.xmlEditor.NativePlatform.ControlElement.BackColor,
                    Rectangle = rectangle
                }) ;
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn auf dieses Element geklickt wurde
        /// </summary>
        protected abstract Task WurdeAngeklickt(Point point, MausKlickAktionen aktion);

        /// <summary>
        /// Ein Mausklick ist eingegangen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task _xmlEditor_MouseDownEvent(MouseEventArgs e)
        {
            var point = new Point(e.X, e.Y);

            // Prüfen, ob der Mausklick überhaupt auf diesem Node geschehen ist
            if (this.IsClickPosInsideNode(point))
            {
                await WurdeAngeklickt(point, MausKlickAktionen.MouseDown);  // An Mausklick-Methode weitergeben
                return;
            }
        }

        /// <summary>
        /// Die Maus wurde von einem Mausklick wieder gelöst
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task _xmlEditor_MouseUpEvent(MouseEventArgs e)
        {
            Point point = new Point(e.X, e.Y);

            // Prüfen, ob der MausUpüberhaupt auf diesem Node geschehen ist
            if (this.IsClickPosInsideNode(point))
            {
                await WurdeAngeklickt(point, MausKlickAktionen.MouseUp);  // An MausUp-Methode weitergeben
                return;
            }
        }

        /// <summary>
        /// Die Maus wurde mit gedrückter Maustaste bewegt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task _xmlEditor_MouseDownMoveEvent(MouseEventArgs e)
        {
            Point point = new Point(e.X, e.Y);

            // Prüfen, ob der MausUpüberhaupt auf diesem Node geschehen ist
            if (this.IsClickPosInsideNode(point))
            {
                await WurdeAngeklickt(point, MausKlickAktionen.MouseDownMove);  // An MausUp-Methode weitergeben
                return;
            }
        }

        /// <summary>
        /// Der XML-Cursor hat sich geändert
        /// </summary>
        private async Task Cursor_ChangedEvent(EventArgs e)
        {
            if (this.XMLNode.ParentNode == null) // Wenn der betreffene Node gerade gelöscht wurde
            {   // Dann auch das XML-Anzeige-Objekt für den Node zerstören
                this.Dispose();
            }
            else
            {
                // Herausfinden, ob der Node dieses Elementes betroffen ist
                if (xmlEditor.EditorStatus.CursorRoh.StartPos.AktNode != this.XMLNode)
                {
                    return;
                }

                // Das Element neu Zeichnen

                //System.Drawing.Graphics g = this._xmlEditor.ZeichnungsSteuerelement.CreateGraphics();
                // this.UnPaint(g);	// Element wegradieren
                //this.Paint (false,new PaintEventArgs (g,this._xmlEditor.ZeichnungsSteuerelement.ClientRectangle)); // Neu zeichnen
            }
            await Task.CompletedTask; // to prevent warning because of empty async method
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object
            // from executing a second time.
            // GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing) // Dispose managed resources.
                {
                    // Von den Events abmelden
                    xmlEditor.EditorStatus.CursorRoh.ChangedEvent.Remove(this.Cursor_ChangedEvent);
                    xmlEditor.MouseHandler.MouseDownEvent.Remove(this._xmlEditor_MouseDownEvent);
                    xmlEditor.MouseHandler.MouseUpEvent.Remove(this._xmlEditor_MouseUpEvent);
                    xmlEditor.MouseHandler.MouseDownMoveEvent.Remove(this._xmlEditor_MouseDownMoveEvent);
                    xmlEditor.XmlElementeAufraeumenEvent -= new EventHandler(_xmlEditor_xmlElementeAufraeumenEvent);

                    // Referenzen lösen
                    this.xmlEditor = null;
                }
            }
            _disposed = true;
        }

    }
}
