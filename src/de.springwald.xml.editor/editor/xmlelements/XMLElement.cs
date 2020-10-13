// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

#define XXklickbereicheRotAnzeigen // Sollen die klickbaren Bereiche rot angezeigt werden?

using System;
using System.Collections;
using System.Threading.Tasks;
using de.springwald.xml.cursor;
using de.springwald.xml.editor.editor.xmlelemente;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Basic element for drawing XML editor elements
    /// </summary>
    public abstract class XMLElement : IDisposable
    {
        public enum PaintModes
        {
            ForcePaint,
            OnlyWhenChanged,
            OnlyWhenNotChanged
        }

        private bool _disposed = false;

        /*private Color _backgroundColorNeutral_ = Color.White;
		private Color _backgroundColorSelected_ = Color.LightBlue;*/

        protected Point _cursorStrichPos;   // dort wird der Cursor in diesem Node gezeichnet, wenn es der aktuell  Node ist

        protected XMLEditor _xmlEditor;
        protected ArrayList _childElemente = new ArrayList();           // Die ChildElemente in diesem Steuerelement

        protected Rectangle[] _klickBereiche = new Rectangle[] { }; // Die von diesem Element klickbaren Bereiche z.B. für Mausklicktests etc.

        /// <summary>
        /// Der mit diesem Element anzuzeigende XMLNode
        /// </summary>
        public System.Xml.XmlNode XMLNode { get; }

        //public de.springwald.xml.editor.XMLEditorPaintPos PaintPos { get; set; }

        public abstract int LineHeight { get; }

        /// <summary>
        /// Konstruktor des xmlElementes
        /// </summary>
        /// <param name="xmlNode">Der zu zeichnende XML-Node</param>
        /// <param name="xmlEditor">Der Editor, für welchen der Node gezeichnet werden soll</param>
        public XMLElement(System.Xml.XmlNode xmlNode, XMLEditor xmlEditor) //, de.springwald.xml.XMLEditorPaintPos paintPos)
        {
            this.XMLNode = xmlNode;
            _xmlEditor = xmlEditor;

            _xmlEditor.EditorStatus.CursorRoh.ChangedEvent.Add(this.Cursor_ChangedEvent);
            _xmlEditor.MouseHandler.MouseDownEvent.Add(this._xmlEditor_MouseDownEvent);
            _xmlEditor.MouseHandler.MouseUpEvent.Add(this._xmlEditor_MouseUpEvent);
            _xmlEditor.MouseHandler.MouseDownMoveEvent.Add(this._xmlEditor_MouseDownMoveEvent);
            _xmlEditor.XmlElementeAufraeumenEvent += new EventHandler(_xmlEditor_xmlElementeAufraeumenEvent);
        }
        protected abstract object PaintedValue { get; }
        protected abstract string PaintedAttributes { get; }
        protected XmlElementPaintCacheData lastPaintedData;

        /// <summary>
        /// Zeichnet das XML-Element auf den Bildschirm
        /// </summary>
        public virtual async Task<PaintContext> Paint(PaintContext paintContext, IGraphics gfx, PaintModes paintMode)
        {
            if (this._disposed) return paintContext;
            if (this.XMLNode == null) return paintContext;
            if (this._xmlEditor == null) return paintContext;

            var paintData = new XmlElementPaintCacheData
            {
                PaintPosX = paintContext.PaintPosX,
                PaintPosY = paintContext.PaintPosY,
                Attributes = this.PaintedAttributes,
                Value = this.PaintedValue
            };

            var toPaint = false;
            switch (paintMode)
            {
                case PaintModes.ForcePaint:
                    toPaint = true;
                    break;

                case PaintModes.OnlyWhenChanged:
                    toPaint = paintData.Changed(this.lastPaintedData);
                    break;

                case PaintModes.OnlyWhenNotChanged:
                    throw new NotImplementedException();
                    break;
            }

            _cursorStrichPos = null; // new Point(paintContext.PaintPosX, paintContext.PaintPosY);

            // Alles zeichnen
            if (toPaint)
            {
                this.UnPaint(gfx, paintContext);
                this.MausklickBereicheBufferLeeren();
            }
            
            paintContext = await PaintNodeContent(paintContext, gfx, paintMode, justCalculate: false);

#if klickbereicheRotAnzeigen
            KlickbereicheAnzeigen(paintContext, gfx);
#endif
            this.lastPaintedData = paintData;

            this.ZeichneCursorStrich(gfx);

            return paintContext;
        }

        protected abstract Task<PaintContext> PaintNodeContent(PaintContext paintContext, IGraphics gfx, PaintModes paintMode, bool justCalculate);

        private Color[] unPaintColors = new[] { Color.Blue, Color.DarkBlue, Color.Gray, Color.Red, Color.White };
        private int unPaintColor = 0;

        protected virtual void UnPaint(IGraphics gfx,PaintContext paintContext)
        {
            unPaintColor++;
            if (unPaintColor >= unPaintColors.Length) unPaintColor = 0;
            foreach (Rectangle rechteck in this._klickBereiche)
            {
                gfx.AddJob(new JobDrawRectangle
                {
                    Layer = GfxJob.Layers.ClearBackground,
                    Batchable = true,
                    FillColor = unPaintColors[unPaintColor],
                    Rectangle = rechteck
                });
            }
        }

        /// <summary>
        /// Zeichnet den senkrechten Cursorstrich
        /// </summary>
        protected virtual void ZeichneCursorStrich(IGraphics gfx)
        {
            if (this._cursorStrichPos == null) return;
            if (this._xmlEditor.CursorBlink.PaintCursor == false) return;

            // Cursor-Strich zeichnen
            var height = (int)(Math.Max(this._xmlEditor.EditorConfig.TextNodeFont.Height, this._xmlEditor.EditorConfig.NodeNameFont.Height) * 1.6);
            var margin = height / 5;
            gfx.AddJob(new JobDrawLine
            {
                Batchable = true,
                Layer = GfxJob.Layers.Cursor,
                Color = Color.Black,
                LineWidth = 2,
                X1 = _cursorStrichPos.X,
                Y1 = _cursorStrichPos.Y + margin,
                X2 = _cursorStrichPos.X,
                Y2 = _cursorStrichPos.Y + height - margin
            });

            // merken, wo gerade der Cursor gezeichnet wird, damit dorthin gescrollt werden kann,
            // wenn der Cursor aus dem sichtbaren Bereich bewegt wird
            _xmlEditor.AktScrollingCursorPos = _cursorStrichPos;
        }

        /// <summary>
        /// zeichnet die per Maus klickbaren Bereiche
        /// </summary>
        private void KlickbereicheAnzeigen(IGraphics gfx)
        {
            foreach (Rectangle rechteck in this._klickBereiche)
            {
                gfx.AddJob(new JobDrawRectangle
                {
                    Layer = GfxJob.Layers.ClickAreas,
                    Batchable = true,
                    BorderColor = Color.Red,
                    Rectangle = rechteck
                });
            }
        }

        /// <summary>
        /// leert den Buffer der Mausklick-flächen vor dem Berechnen zum neu-füllen
        /// </summary>
        private void MausklickBereicheBufferLeeren()
        {
            if (_klickBereiche.Length != 0) _klickBereiche = new Rectangle[] { };
        }

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


        /// <summary>
        /// Wird aufgerufen, wenn auf dieses Element geklickt wurde
        /// </summary>
        /// <param name="point"></param>
        protected virtual async Task WurdeAngeklickt(Point point, MausKlickAktionen aktion)
        {
            await _xmlEditor.EditorStatus.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, aktion);
            _xmlEditor.CursorBlink.ResetBlinkPhase();
        }

        /// <summary>
        /// Ein Mausklick ist eingegangen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task _xmlEditor_MouseDownEvent(MouseEventArgs e)
        {
            Point point = new Point(e.X, e.Y);

            // Prüfen, ob der Mausklick überhaupt auf diesem Node geschehen ist
            foreach (Rectangle rechteck in this._klickBereiche)
            {
                if (rechteck.Contains(point)) // Wenn der Klick in einem der Mausklickbereiche war
                {
                    await WurdeAngeklickt(point, MausKlickAktionen.MouseDown);  // An Mausklick-Methode weitergeben
                    return;
                }
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
            foreach (Rectangle rechteck in this._klickBereiche)
            {
                if (rechteck.Contains(point)) // Wenn der Up in einem der Mausklickbereiche war
                {
                    await WurdeAngeklickt(point, MausKlickAktionen.MouseUp);  // An MausUp-Methode weitergeben
                    return;
                }
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
            foreach (Rectangle rechteck in this._klickBereiche)
            {
                if (rechteck.Contains(point)) // Wenn der Up in einem der Mausklickbereiche war
                {
                    await WurdeAngeklickt(point, MausKlickAktionen.MouseDownMove);  // An MausUp-Methode weitergeben
                    return;
                }
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
                if (_xmlEditor.EditorStatus.CursorRoh.StartPos.AktNode != this.XMLNode)
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
                    _xmlEditor.EditorStatus.CursorRoh.ChangedEvent.Remove(this.Cursor_ChangedEvent);
                    _xmlEditor.MouseHandler.MouseDownEvent.Remove(this._xmlEditor_MouseDownEvent);
                    _xmlEditor.MouseHandler.MouseUpEvent.Remove(this._xmlEditor_MouseUpEvent);
                    _xmlEditor.MouseHandler.MouseDownMoveEvent.Remove(this._xmlEditor_MouseDownMoveEvent);
                    _xmlEditor.XmlElementeAufraeumenEvent -= new EventHandler(_xmlEditor_xmlElementeAufraeumenEvent);

                    // Alle Child-Elemente ebenfalls zerstören
                    foreach (XMLElement element in this._childElemente)
                    {
                        if (element != null) element.Dispose();
                    }

                    // Referenzen lösen
                    this._xmlEditor = null;
                }
            }
            _disposed = true;
        }

    }
}
