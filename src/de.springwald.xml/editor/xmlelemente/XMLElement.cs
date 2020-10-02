#define klickbereicheRotAnzeigen // Sollen die klickbaren Bereiche rot angezeigt werden?

using de.springwald.xml.cursor;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.events;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Basic element for drawing XML editor elements
    /// </summary>
    /// <remarks>
    // (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    public abstract class XMLElement : IDisposable
    {
        private bool _disposed = false;

        /*private Color _backgroundColorNeutral_ = Color.White;
		private Color _backgroundColorSelected_ = Color.LightBlue;*/

        protected Point _cursorStrichPos;   // dort wird der Cursor in diesem Node gezeichnet, wenn es der aktuell  Node ist

        protected de.springwald.xml.editor.XMLEditor _xmlEditor;
        protected ArrayList _childElemente = new ArrayList();           // Die ChildElemente in diesem Steuerelement
        protected bool _wirdGeradeGezeichnet;                           // true=das Element befindet sich gerade in der Zeichnen-Phase

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
        public XMLElement(System.Xml.XmlNode xmlNode, de.springwald.xml.editor.XMLEditor xmlEditor) //, de.springwald.xml.XMLEditorPaintPos paintPos)
        {
            this.XMLNode = xmlNode;
            _xmlEditor = xmlEditor;

            _xmlEditor.CursorRoh.ChangedEvent.Add(this.Cursor_ChangedEvent);
            _xmlEditor.MouseDownEvent.Add(this._xmlEditor_MouseDownEvent);
            _xmlEditor.MouseUpEvent.Add(this._xmlEditor_MouseUpEvent);
            _xmlEditor.MouseDownMoveEvent.Add(this._xmlEditor_MouseDownMoveEvent);
            _xmlEditor.xmlElementeAufraeumenEvent += new EventHandler(_xmlEditor_xmlElementeAufraeumenEvent);
        }

        /// <summary>
        /// Zeichnet das XML-Element auf den Bildschirm
        /// </summary>
        public virtual async Task<PaintContext> Paint(PaintContext paintContext, IGraphics gfx)
        {
            if (this._disposed) return paintContext;
            if (this.XMLNode == null) return paintContext;
            if (this._xmlEditor == null) return paintContext;

            MausklickBereicheBufferLeeren();
            _cursorStrichPos = null; // new Point(paintContext.PaintPosX, paintContext.PaintPosY);

            // Alles zeichnen
            this._wirdGeradeGezeichnet = true;
            await NodeZeichnenStart(paintContext, gfx);
            await UnternodesZeichnen(paintContext, gfx);
            await NodeZeichnenAbschluss(paintContext, gfx);
            this._wirdGeradeGezeichnet = false;

#if klickbereicheRotAnzeigen
            KlickbereicheAnzeigen(paintContext, gfx);
#endif
            return paintContext;
        }

        /// <summary>
        /// Zeichnet die Grafik des aktuellen Nodes
        /// </summary>
        protected virtual async Task NodeZeichnenStart(PaintContext paintContext, IGraphics gfx)
        {
            await Task.CompletedTask; // to prevent warning because of empty async method
                                      // vermerken, wie hoch die Zeile bisher ist
                                      //this._hoeheAktuelleZeile = 0;
        }

        /// <summary>
        /// Aktualisiert alle Unternodes dieses Nodes
        /// </summary>
        /// <param name="nachDiesemNodeNeuZeichnenErzwingen">Alle Nodes nach diesem Childnode müssen
        /// noch gezeichnet werden. Das tritt zum Beispiel ein, wenn sich der Inhalt eines Childnodes
        /// geändert hat und nun alles folgende z.B. wegen Verschiebung neu gezeichnet werden muss.</param>
        protected virtual async Task UnternodesZeichnen(PaintContext paintContext, IGraphics gfx)
        {
            if (this.XMLNode is System.Xml.XmlText) // es handelt sich um einen Textnode 
            {
            }
            else
            {
                // es handelt sich um keinen Textnode
                XMLElement childElement;            // Das zu zeichnende XML-Child

                if (this.XMLNode == null)
                {
                    throw new ApplicationException("UnternodesZeichnen:XMLNode ist leer");
                }

                var childPaintContext = paintContext.Clone();
                childPaintContext.LimitLeft = paintContext.LimitLeft + _xmlEditor.Regelwerk.ChildEinrueckungX;

                // Alle Child-Controls anzeigen und ggf. vorher anlegen
                for (int childLauf = 0; childLauf < this.XMLNode.ChildNodes.Count; childLauf++)
                {
                    if (childLauf >= _childElemente.Count)
                    {   // Wenn noch nicht so viele ChildControls angelegt sind, wie
                        // es ChildXMLNodes gibt
                        childElement = this._xmlEditor.createElement(this.XMLNode.ChildNodes[childLauf]);
                        _childElemente.Add(childElement);
                    }
                    else
                    {   // es gibt schon ein Control an dieser Stelle
                        childElement = (XMLElement)_childElemente[childLauf];

                        if (childElement == null)
                        {
                            throw new ApplicationException($"UnternodesZeichnen:childElement ist leer: outerxml:{this.XMLNode.OuterXml} >> innerxml {this.XMLNode.InnerXml}");
                        }

                        // prüfen, ob es auch den selben XML-Node vertritt
                        if (childElement.XMLNode != this.XMLNode.ChildNodes[childLauf])
                        {   // Das ChildControl enthält nicht den selben ChildNode, also 
                            // löschen und neu machen
                            childElement.Dispose(); // altes Löschen
                            childElement = this._xmlEditor.createElement(this.XMLNode.ChildNodes[childLauf]);
                            _childElemente[childLauf] = childElement; // durch Neues ersetzen
                        }
                    }

                    // An dieser Stelle sollte im Objekt ChildControl die entsprechends
                    // Instanz des XMLElement-Controls für den aktuellen XMLChildNode stehen
                    switch (_xmlEditor.Regelwerk.DarstellungsArt(childElement.XMLNode))
                    {
                        case DarstellungsArten.EigeneZeile:

                            // Dieses Child-Element beginnt eine neue Zeile und wird dann in dieser gezeichnet

                            // Neue Zeile beginnen
                            childPaintContext.LimitLeft = paintContext.LimitLeft + _xmlEditor.Regelwerk.ChildEinrueckungX;
                            childPaintContext.PaintPosX = childPaintContext.LimitLeft;
                            childPaintContext.PaintPosY += _xmlEditor.Regelwerk.AbstandYZwischenZeilen + paintContext.HoeheAktZeile; // Zeilenumbruch
                            childPaintContext.HoeheAktZeile = 0; // noch kein Element in dieser Zeile, daher Hoehe 0
                                                                 // X-Cursor auf den Start der neuen Zeile setzen
                                                                 // Linie nach unten und dann nach rechts ins ChildElement
                            Pen myPen = new Pen(Color.Gray, 1);
                            myPen.DashStyle = Pen.DashStyles.Dash;

                            // Linie nach unten
                            myPen.StartCap = Pen.LineCap.SquareAnchor;
                            myPen.EndCap = Pen.LineCap.RoundAnchor;
                            gfx.AddJob(new JobDrawLine
                            {
                                Layer = paintContext.LayerTagBorder,
                                Batchable = true,
                                Pen = myPen,
                                X1 = paintContext.LimitLeft,
                                Y1 = paintContext.PaintPosY + this.LineHeight / 2,
                                X2 = paintContext.LimitLeft,
                                Y2 = childPaintContext.PaintPosY + childElement.LineHeight / 2
                            });

                            // Linie nach rechts mit Pfeil auf ChildElement
                            myPen.StartCap = Pen.LineCap.NoAnchor;
                            myPen.EndCap = Pen.LineCap.SquareAnchor; // Pfeil am Ende
                            gfx.AddJob(new JobDrawLine
                            {
                                Layer = paintContext.LayerTagBorder,
                                Batchable = true,
                                Pen = myPen,
                                X1 = paintContext.LimitLeft,
                                Y1 = childPaintContext.PaintPosY + childElement.LineHeight / 2,
                                X2 = childPaintContext.LimitLeft,
                                Y2 = childPaintContext.PaintPosY + childElement.LineHeight / 2
                            });

                            childPaintContext = await childElement.Paint(childPaintContext, gfx);
                            // paintContext.PaintPosX = context2.PaintPosX;
                            //paintContext.PaintPosY = context2.PaintPosY;
                            break;


                        case DarstellungsArten.Fliesselement:

                            // Dieses Child ist ein Fliesselement; es fügt sich in die selbe Zeile
                            // ein, wie das vorherige Element und beginnt keine neue Zeile, 
                            // es sei denn, die aktuelle Zeile ist bereits zu lang
                            if (childPaintContext.PaintPosX > paintContext.LimitRight) // Wenn die Zeile bereits zu voll ist
                            {
                                // in nächste Zeile
                                paintContext.PaintPosY += paintContext.HoeheAktZeile + _xmlEditor.Regelwerk.AbstandYZwischenZeilen;
                                paintContext.HoeheAktZeile = 0;
                                paintContext.PaintPosX = paintContext.ZeilenStartX;
                            }
                            else // es passt noch etwas in diese Zeile
                            {
                                // das Child rechts daneben setzen	
                            }

                            childPaintContext = await childElement.Paint(childPaintContext, gfx);
                            // paintContext.PaintPosX = context1.PaintPosX;
                            // paintContext.PaintPosY = context1.PaintPosY;
                            break;


                        default:
                            MessageBox.Show("undefiniert");
                            //ChildControl.Zeichnen();
                            break;
                    }

                    // Den Cursor in der aktuellen Zeile nach rechts verschieben
                    //paintContext.PaintPosX = childPaintContext.PaintPosX;
                    //paintContext.PaintPosY = childPaintContext.PaintPosY;

                    //// vermerken, wie hoch die Zeile ist
                    //paintContext.HoeheAktZeile = paintContext.HoeheAktZeile;
                    //paintContext.BisherMaxX = Math.Max(paintContext.BisherMaxX, paintContext.BisherMaxX);

                }

                // Sollten wir mehr ChildControls als XMLChildNodes haben, dann diese
                // am Ende der ChildControlListe löschen
                while (this.XMLNode.ChildNodes.Count < _childElemente.Count)
                {
                    childElement = (XMLElement)_childElemente[_childElemente.Count - 1];
                    _childElemente.Remove(_childElemente[_childElemente.Count - 1]);
                    childElement.Dispose();
                    _childElemente.TrimToSize();
                }

                paintContext.PaintPosX = childPaintContext.PaintPosX;
                paintContext.PaintPosY = childPaintContext.PaintPosY;
            }
        }

        /// <summary>
        /// Zeichnet den Abschluss des aktuellen Nodes (z.B. einen schließenden Haken)
        /// </summary>
        protected virtual async Task NodeZeichnenAbschluss(PaintContext paintContext, IGraphics gfx)
        {
            this.ZeichneCursorStrich(paintContext, gfx);
        }

        /// <summary>
        /// Zeichnet den senkrechten Cursorstrich
        /// </summary>
        protected virtual void ZeichneCursorStrich(PaintContext paintContext, IGraphics gfx)
        {
            if (this._cursorStrichPos == null) return;

            //if (!_xmlEditor.CursorRoh.IstEtwasSelektiert) // Wenn nichts selektiert ist
            //{
            //    if (this.XMLNode == this._xmlEditor.CursorOptimiert.StartPos.AktNode)  // Wenn dies überhaupt der aktuelle Node ist
            //    {
            //        if ((this._xmlEditor.CursorOptimiert.StartPos.PosAmNode != XMLCursorPositionen.CursorAufNodeSelbstVorderesTag) &&
            //        (this._xmlEditor.CursorOptimiert.StartPos.PosAmNode != XMLCursorPositionen.CursorAufNodeSelbstHinteresTag))// Wenn nicht ein ganzer Node markiert ist
            //        {
            if (this._xmlEditor.CursorBlinkOn) // Wenn der Cursor bei diesem Male gezeichnet werden soll
            {
                // Cursor-Strich zeichnen
                Pen newPen = new Pen(Color.Black, 2);
                var height = (int)(Math.Max(this._xmlEditor.EditorConfig.TextNodeFont.Height, this._xmlEditor.EditorConfig.NodeNameFont.Height) * 1.6);
                var margin = height / 5;
                gfx.AddJob(new JobDrawLine
                {
                    Batchable = true,
                    Layer = paintContext.LayerCursor,
                    Pen = newPen,
                    X1 = _cursorStrichPos.X,
                    Y1 = _cursorStrichPos.Y + margin,
                    X2 = _cursorStrichPos.X,
                    Y2 = _cursorStrichPos.Y + height - margin
                });
            }

            // merken, wo gerade der Cursor gezeichnet wird, damit dorthin gescrollt werden kann,
            // wenn der Cursor aus dem sichtbaren Bereich bewegt wird
            _xmlEditor.AktScrollingCursorPos = _cursorStrichPos;
            //        }
            //    }
            //}
        }

        /// <summary>
        /// zeichnet die per Maus klickbaren Bereiche
        /// </summary>
        private void KlickbereicheAnzeigen(PaintContext paintContext, IGraphics gfx)
        {
            var pen = new Pen(Color.Red, 1);
            foreach (Rectangle rechteck in this._klickBereiche)
            {
                gfx.AddJob(new JobDrawRectangle
                {
                    Layer = paintContext.LayerClickAreas,
                    Batchable = true,
                    Pen = pen,
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
            await _xmlEditor.CursorRoh.CursorPosSetzenDurchMausAktion(this.XMLNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, aktion);
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
                if (_xmlEditor.CursorRoh.StartPos.AktNode != this.XMLNode)
                {
                    return;
                }

                // Das Element neu Zeichnen

                //System.Drawing.Graphics g = this._xmlEditor.ZeichnungsSteuerelement.CreateGraphics();
                //this.UnPaint(g);	// Element wegradieren
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
                    _xmlEditor.CursorRoh.ChangedEvent.Remove(this.Cursor_ChangedEvent);
                    _xmlEditor.MouseDownEvent.Remove(this._xmlEditor_MouseDownEvent);
                    _xmlEditor.MouseUpEvent.Remove(this._xmlEditor_MouseUpEvent);
                    _xmlEditor.MouseDownMoveEvent.Remove(this._xmlEditor_MouseDownMoveEvent);
                    _xmlEditor.xmlElementeAufraeumenEvent -= new EventHandler(_xmlEditor_xmlElementeAufraeumenEvent);

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
