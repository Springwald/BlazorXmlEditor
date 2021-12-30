// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

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
    public abstract class XmlElement : IDisposable
    {
        public enum PaintModes
        {
            ForcePaintNoUnPaintNeeded,
            ForcePaintAndUnpaintBefore,
            OnlyPaintWhenChanged
        }

        private bool disposed = false;

        protected Point cursorPaintPos;
        protected XmlEditor xmlEditor;

        private EditorContext editorContext;
        protected EditorConfig Config => this.editorContext.EditorConfig;
        protected XmlRules XmlRules => this.editorContext.XmlRules;
        protected EditorState EditorState => this.editorContext.EditorState;

        /// <summary>
        /// The XMLNode to be displayed with this element
        /// </summary>
        public System.Xml.XmlNode XmlNode { get; }

        public XmlElement(System.Xml.XmlNode xmlNode, XmlEditor xmlEditor, EditorContext editorContext)
        {
            this.editorContext = editorContext;
            this.XmlNode = xmlNode;
            this.xmlEditor = xmlEditor;
            this.EditorState.CursorRaw.ChangedEvent.Add(this.Cursor_ChangedEvent);
            this.xmlEditor.MouseHandler.MouseDownEvent.Add(this._xmlEditor_MouseDownEvent);
            this.xmlEditor.MouseHandler.MouseUpEvent.Add(this._xmlEditor_MouseUpEvent);
            this.xmlEditor.MouseHandler.MouseDownMoveEvent.Add(this._xmlEditor_MouseDownMoveEvent);
            this.xmlEditor.CleanUpXmlElementsEvent += new EventHandler(_xmlEditor_xmlElementsCleanUpEvent);
        }

        /// <summary>
        /// Draws the XML element on the screen
        /// </summary>
        public async Task<PaintContext> Paint(PaintContext paintContext, bool cursorBlinkOn, XmlCursor cursor, IGraphics gfx, PaintModes paintMode, int depth)
        {
            if (this.disposed) return paintContext;
            if (this.XmlNode == null) return paintContext;
            if (this.xmlEditor == null) return paintContext;

            paintContext = await PaintInternal(paintContext, cursorBlinkOn, cursor, gfx, paintMode, depth);
            if (this.cursorPaintPos != null && cursorBlinkOn) this.PaintCursor(gfx);
            return paintContext;
        }

        internal abstract void UnPaint(IGraphics gfx);

        protected abstract Task<PaintContext> PaintInternal(PaintContext paintContext, bool cursorBlinkOn, XmlCursor cursor, IGraphics gfx, PaintModes paintMode, int depth);

        /// <summary>
        /// Draws the vertical cursor line
        /// </summary>
        protected virtual void PaintCursor(IGraphics gfx)
        {
            if (this.cursorPaintPos == null) return;
            if (this.EditorState.CursorBlink.PaintCursor == false) return;

            var height = (int)(Math.Max(this.editorContext.EditorConfig.FontTextNode.Height, this.editorContext.EditorConfig.FontNodeName.Height) * 1.6);
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
        }

        /// <summary>
        /// The editor has asked to unload all elements that are no longer used
        /// </summary>
        void _xmlEditor_xmlElementsCleanUpEvent(object sender, EventArgs e)
        {
            if (this.XmlNode == null)
            {
                Dispose();
                return;
            }

            if (this.XmlNode.ParentNode == null)
            {
                Dispose();
                return;
            }
        }

        protected abstract Task OnMouseAction(Point point, MouseClickActions mouseAction);

        private async Task _xmlEditor_MouseDownEvent(MouseEventArgs e) => await OnMouseAction(new Point(e.X, e.Y), MouseClickActions.MouseDown);

        async Task _xmlEditor_MouseUpEvent(MouseEventArgs e) => await OnMouseAction(new Point(e.X, e.Y), MouseClickActions.MouseUp);

        async Task _xmlEditor_MouseDownMoveEvent(MouseEventArgs e) => await OnMouseAction(new Point(e.X, e.Y), MouseClickActions.MouseDownMove);

        private async Task Cursor_ChangedEvent(EventArgs e)
        {
            if (this.XmlNode.ParentNode == null) // If the node has just been deleted
            {
                // Then also destroy the XML display object for the node
                this.Dispose();
            }
            else
            {
                // Find out if the node of this element is affected
                if (this.editorContext.EditorState.CursorRaw.StartPos.ActualNode != this.XmlNode)
                {
                    return;
                }

                //  Redraw the element

                //System.Drawing.Graphics g = this._xmlEditor.ZeichnungsSteuerelement.CreateGraphics();
                // this.UnPaint(g);	// erase element
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
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing) // Dispose managed resources.
                {
                    this.UnPaint(this.xmlEditor.NativePlatform.Gfx);

                    // Unsubscribe from the events
                    editorContext.EditorState.CursorRaw.ChangedEvent.Remove(this.Cursor_ChangedEvent);
                    xmlEditor.MouseHandler.MouseDownEvent.Remove(this._xmlEditor_MouseDownEvent);
                    xmlEditor.MouseHandler.MouseUpEvent.Remove(this._xmlEditor_MouseUpEvent);
                    xmlEditor.MouseHandler.MouseDownMoveEvent.Remove(this._xmlEditor_MouseDownMoveEvent);
                    xmlEditor.CleanUpXmlElementsEvent -= new EventHandler(_xmlEditor_xmlElementsCleanUpEvent);

                    this.xmlEditor = null;
                }
            }
            disposed = true;
        }
    }
}
