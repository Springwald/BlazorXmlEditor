//#define klickbereicheRotAnzeigen // Sollen die klickbaren Bereiche rot angezeigt werden?

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

        protected XMLEditorPaintPos _merkeStartPaintPos;

        protected Point _cursorStrichPos;   // dort wird der Cursor in diesem Node gezeichnet, wenn es der aktuell  Node ist

        protected int _startX = 0;      // dort wurde dieses Element zu zeichnen begonnen
        protected int _startY = 0;      // dort wurde dieses Element zu zeichnen begonnen

        protected de.springwald.xml.editor.XMLEditor _xmlEditor;
        protected ArrayList _childElemente = new ArrayList();           // Die ChildElemente in diesem Steuerelement
        protected bool _wirdGeradeGezeichnet;                           // true=das Element befindet sich gerade in der Zeichnen-Phase

        protected Rectangle[] _klickBereiche = new Rectangle[] { }; // Die von diesem Element klickbaren Bereiche z.B. f�r Mausklicktests etc.

        /// <summary>
        /// Der mit diesem Element anzuzeigende XMLNode
        /// </summary>
        public System.Xml.XmlNode XMLNode { get;  }

        public de.springwald.xml.editor.XMLEditorPaintPos PaintPos { get; set; }

        /// <summary>
        /// Dort sollte der Ast des Baumes ankleben, wenn dieses Element in einem Ast des Parent gezeichnet werden soll
        /// </summary>
        /// <returns></returns>
        protected virtual Point AnkerPos
        {
            get { return new Point(_startX, _startY); }
        }

        /// <summary>
        /// Konstruktor des xmlElementes
        /// </summary>
        /// <param name="xmlNode">Der zu zeichnende XML-Node</param>
        /// <param name="xmlEditor">Der Editor, f�r welchen der Node gezeichnet werden soll</param>
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
        public virtual async Task Paint(int marginLeft, int paintPosX, int paintPosY, XMLPaintArten paintArt, PaintEventArgs e)
        {
            if (this._disposed) return;
            if (this.XMLNode == null) return;
            if (this._xmlEditor == null) return;

            if (paintArt == XMLPaintArten.Vorberechnen)
            {
                _merkeStartPaintPos = this.PaintPos.Clone(); // Paintpos sichern
            }
            else
            {
                this.PaintPos = _merkeStartPaintPos.Clone(); // PaintPos wiederherstellen
            }

            // Startposition merken
            _startX = this.PaintPos.PosX;
            _startY = this.PaintPos.PosY;

            if (paintArt == XMLPaintArten.Vorberechnen)
            {
                MausklickBereicheBufferLeeren();
                _cursorStrichPos = new Point(_startX, _startY);
            }

            // Alles zeichnen
            this._wirdGeradeGezeichnet = true;
            await NodeZeichnenStart(marginLeft, paintPosX,  paintPosY, paintArt, e);
            await UnternodesZeichnen(marginLeft, paintPosX,  paintPosY, paintArt, e);
            await NodeZeichnenAbschluss(marginLeft, paintPosX,  paintPosY, paintArt, e);
            this._wirdGeradeGezeichnet = false;

#if klickbereicheRotAnzeigen
			if (paintArt != XMLPaintArten.Vorberechnen) 
			{
				KlickbereicheAnzeigen(e);
			}
#endif
        }

        /// <summary>
        /// Zeichnet die Grafik des aktuellen Nodes
        /// </summary>
        protected virtual async Task NodeZeichnenStart(int marginLeft, int paintPosX, int PaintPosY, XMLPaintArten paintArt,  PaintEventArgs e)
        {
            await Task.CompletedTask; // to prevent warning because of empty async method
            // vermerken, wie hoch die Zeile bisher ist
            //this._hoeheAktuelleZeile = 0;
        }

        /// <summary>
        /// Aktualisiert alle Unternodes dieses Nodes
        /// </summary>
        /// <param name="nachDiesemNodeNeuZeichnenErzwingen">Alle Nodes nach diesem Childnode m�ssen
        /// noch gezeichnet werden. Das tritt zum Beispiel ein, wenn sich der Inhalt eines Childnodes
        /// ge�ndert hat und nun alles folgende z.B. wegen Verschiebung neu gezeichnet werden muss.</param>
        protected virtual async Task UnternodesZeichnen(int marginLeft, int paintPosX, int paintPosY, XMLPaintArten paintArt,  PaintEventArgs e)
        {
            if (this.XMLNode is System.Xml.XmlText) // es handelt sich um einen Textnode 
            {
            }
            else
            { // es handelt sich um keinen Textnode

                XMLElement childElement;            // Das zu zeichnende XML-Child
                XMLEditorPaintPos childZeichenPos;  // Dort soll das Child gezeichnet werden

                if (this.XMLNode == null)
                {
                    throw new ApplicationException("UnternodesZeichnen:XMLNode ist leer");
                }

                this.PaintPos.PosX += this._xmlEditor.Regelwerk.AbstandFliessElementeX;

                switch (_xmlEditor.Regelwerk.DarstellungsArt(this.XMLNode))
                {
                    case DarstellungsArten.Fliesselement:
                        break;

                    case DarstellungsArten.EigeneZeile:
                        // Den Zeilenstart f�r den Fliesselement-Umbruch auf den Start dieses
                        // Elementes verlegen

                        //_paintPos.ZeilenStartX = _startX ;  // Beginnend mit dem ersten Child
                        this.PaintPos.ZeilenStartX = this.PaintPos.PosX;    // Beginnen mit dem Element selbst
                        break;
                }

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
                            throw new ApplicationException(String.Format("UnternodesZeichnen:childElement ist leer: PaintArt:{0} outerxml:{1} >> innerxml {2}", paintArt, this.XMLNode.OuterXml, this.XMLNode.InnerXml));
                        }

                        // pr�fen, ob es auch den selben XML-Node vertritt
                        if (childElement.XMLNode != this.XMLNode.ChildNodes[childLauf])
                        {   // Das ChildControl enth�lt nicht den selben ChildNode, also 
                            // l�schen und neu machen
                            if (paintArt == XMLPaintArten.Vorberechnen)
                            {
                                childElement.Dispose(); // altes L�schen
                                childElement = this._xmlEditor.createElement(this.XMLNode.ChildNodes[childLauf]);
                                _childElemente[childLauf] = childElement; // durch Neues ersetzen
                            }
                        }
                    }

                    //this.ZeichenPosY+=myXMLRegelwerk.ZeilenAbstandY+this.myUeberlaengeYAktuelleZeile ; // Zeilenumbruch
                    //this.myUeberlaengeYAktuelleZeile = 0; // die aktuelle Zeile hat noch keinen

                    // An dieser Stelle sollte im Objekt ChildControl die entsprechends
                    // Instanz des XMLElement-Controls f�r den aktuellen XMLChildNode stehen
                    switch (_xmlEditor.Regelwerk.DarstellungsArt(childElement.XMLNode))
                    {
                        case DarstellungsArten.Fliesselement:

                            // Dieses Child ist ein Fliesselement; es f�gt sich in die selbe Zeile
                            // ein, wie das vorherige Element und beginnt keine neue Zeile, 
                            // es sei denn, die aktuelle Zeile ist bereits zu lang

#warning Hier noch vorausschauend berechnen, d.h. die wahrscheinliche L�nge des ChildElementes beim Rechnen bereits anh�ngen
                            if (this.PaintPos.PosX > this.PaintPos.ZeilenEndeX) // Wenn die Zeile bereits zu voll ist
                            {
                                // in n�chste Zeile
                                this.PaintPos.PosY += this.PaintPos.HoeheAktZeile + _xmlEditor.Regelwerk.AbstandYZwischenZeilen;
                                this.PaintPos.HoeheAktZeile = 0;
                                this.PaintPos.PosX = this.PaintPos.ZeilenStartX;
                            }
                            else // es passt noch etwas in diese Zeile
                            {
                                // das Child rechts daneben setzen	
                            }


                            childZeichenPos = new XMLEditorPaintPos()
                            {
                                ZeilenStartX = this.PaintPos.ZeilenStartX,
                                ZeilenEndeX = this.PaintPos.ZeilenEndeX,
                                PosX = this.PaintPos.PosX,
                                PosY = this.PaintPos.PosY,
                                HoeheAktZeile = this.PaintPos.HoeheAktZeile
                            };
                            childElement.PaintPos = childZeichenPos;
                            await childElement.Paint(marginLeft, paintPosX,  paintPosY, paintArt, e);
                            break;

                        case DarstellungsArten.EigeneZeile:

                            // Dieses Child-Element beginnt eine neue Zeile und 
                            // wird dann in dieser gezeichnet

                            // Neue Zeile beginnen
                            this.PaintPos.PosY += _xmlEditor.Regelwerk.AbstandYZwischenZeilen + this.PaintPos.HoeheAktZeile; // Zeilenumbruch
                            this.PaintPos.HoeheAktZeile = 0; // noch kein Element in dieser Zeile, daher Hoehe 0
                                                              // X-Cursor auf den Start der neuen Zeile setzen
                            this.PaintPos.PosX = _startX + _xmlEditor.Regelwerk.ChildEinrueckungX;

                            // das Child rechts daneben setzen
                            childZeichenPos = new XMLEditorPaintPos()
                            {
                                ZeilenStartX = this.PaintPos.ZeilenStartX,
                                ZeilenEndeX = this.PaintPos.ZeilenEndeX,
                                PosX = this.PaintPos.PosX,
                                PosY = this.PaintPos.PosY,
                                HoeheAktZeile = this.PaintPos.HoeheAktZeile
                            };

                            if (paintArt != XMLPaintArten.Vorberechnen)
                            {
                                // Linie nach unten und dann nach rechts ins ChildElement

                                Pen myPen = new Pen(Color.Gray, 1);
                                myPen.DashStyle = Pen.DashStyles.Dash;

                                // Linie nach unten
                                myPen.StartCap = Pen.LineCap.SquareAnchor;
                                myPen.EndCap = Pen.LineCap.NoAnchor;
                                //e.Graphics.DrawLine(myPen, _startX,_startY, _startX , childElement.AnkerPos.Y); 
                                await e.Graphics.DrawLineAsync(myPen, AnkerPos.X, AnkerPos.Y, AnkerPos.X, childElement.AnkerPos.Y);

                                // Linie nach rechts mit Pfeil auf ChildElement
                                myPen.StartCap = Pen.LineCap.NoAnchor;
                                myPen.EndCap = Pen.LineCap.SquareAnchor; // Pfeil am Ende
                                                                         //e.Graphics.DrawLine(myPen, _startX ,  childElement.AnkerPos.Y, childElement.AnkerPos.X, childElement.AnkerPos.Y); 
                                await e.Graphics.DrawLineAsync(myPen, AnkerPos.X, childElement.AnkerPos.Y, childElement.AnkerPos.X, childElement.AnkerPos.Y);


                            }

                            childElement.PaintPos = childZeichenPos;
                            await childElement.Paint(marginLeft, paintPosX, paintPosY, paintArt,  e);

                            break;

                        default:
                            MessageBox.Show("undefiniert");
                            //ChildControl.Zeichnen();
                            break;
                    }

                    // Den Cursor in der aktuellen Zeile nach rechts verschieben
                    this.PaintPos.PosX = childElement.PaintPos.PosX;
                    this.PaintPos.PosY = childElement.PaintPos.PosY;

                    // vermerken, wie hoch die Zeile ist
                    this.PaintPos.HoeheAktZeile = childElement.PaintPos.HoeheAktZeile;
                    this.PaintPos.BisherMaxX = Math.Max(this.PaintPos.BisherMaxX, childElement.PaintPos.BisherMaxX);

                }

                // Sollten wir mehr ChildControls als XMLChildNodes haben, dann diese
                // am Ende der ChildControlListe l�schen
                while (this.XMLNode.ChildNodes.Count < _childElemente.Count)
                {
                    childElement = (XMLElement)_childElemente[_childElemente.Count - 1];
                    _childElemente.Remove(_childElemente[_childElemente.Count - 1]);
                    childElement.Dispose();
                    _childElemente.TrimToSize();
                }
            }

        }

        /// <summary>
        /// Zeichnet den Abschluss des aktuellen Nodes (z.B. einen schlie�enden Haken)
        /// </summary>
        protected virtual async Task NodeZeichnenAbschluss(int marginLeft, int paintPosX, int paintPosY, XMLPaintArten paintArt,  PaintEventArgs e)
        {
            await ZeichneCursorStrich(e);
        }

        /// <summary>
        /// Zeichnet den senkrechten Cursorstrich
        /// </summary>
        protected virtual async Task ZeichneCursorStrich(PaintEventArgs e)
        {
            if (!_xmlEditor.CursorRoh.IstEtwasSelektiert) // Wenn nichts selektiert ist
            {
                if (this.XMLNode == this._xmlEditor.CursorOptimiert.StartPos.AktNode)  // Wenn dies �berhaupt der aktuelle Node ist
                {
                    if ((this._xmlEditor.CursorOptimiert.StartPos.PosAmNode != XMLCursorPositionen.CursorAufNodeSelbstVorderesTag) &&
                    (this._xmlEditor.CursorOptimiert.StartPos.PosAmNode != XMLCursorPositionen.CursorAufNodeSelbstHinteresTag))// Wenn nicht ein ganzer Node markiert ist
                    {
                        if (this._xmlEditor.CursorBlinkOn) // Wenn der Cursor bei diesem Male gezeichnet werden soll
                        {
                            // Cursor-Strich zeichnen
                            Pen newPen = new Pen(Color.Black, 2);
                            await e.Graphics.DrawLineAsync(newPen, _cursorStrichPos.X, _cursorStrichPos.Y + 1, _cursorStrichPos.X, _cursorStrichPos.Y + 20);
                        }

                        // merken, wo gerade der Cursor gezeichnet wird, damit dorthin gescrollt werden kann,
                        // wenn der Cursor aus dem sichtbaren Bereich bewegt wird
                        _xmlEditor.AktScrollingCursorPos = _cursorStrichPos;
                    }
                }
            }
        }

        /// <summary>
        /// zeichnet die per Maus klickbaren Bereiche
        /// </summary>
        private async Task KlickbereicheAnzeigen(PaintEventArgs e)
        {
            Pen newPen = new Pen(Color.Red, 1);
            foreach (Rectangle rechteck in this._klickBereiche)
            {
                await e.Graphics.DrawRectangleAsync(newPen, rechteck);
            }
        }

        /// <summary>
        /// leert den Buffer der Mausklick-fl�chen vor dem Berechnen zum neu-f�llen
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

            // Pr�fen, ob der Mausklick �berhaupt auf diesem Node geschehen ist
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
        /// Die Maus wurde von einem Mausklick wieder gel�st
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task _xmlEditor_MouseUpEvent(MouseEventArgs e)
        {
            Point point = new Point(e.X, e.Y);

            // Pr�fen, ob der MausUp�berhaupt auf diesem Node geschehen ist
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
        /// Die Maus wurde mit gedr�ckter Maustaste bewegt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task _xmlEditor_MouseDownMoveEvent(MouseEventArgs e)
        {
            Point point = new Point(e.X, e.Y);

            // Pr�fen, ob der MausUp�berhaupt auf diesem Node geschehen ist
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
        /// Der XML-Cursor hat sich ge�ndert
        /// </summary>
        private async Task Cursor_ChangedEvent(EventArgs e)
        {
            if (this.XMLNode.ParentNode == null) // Wenn der betreffene Node gerade gel�scht wurde
            {   // Dann auch das XML-Anzeige-Objekt f�r den Node zerst�ren
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

                    // Alle Child-Elemente ebenfalls zerst�ren
                    foreach (XMLElement element in this._childElemente)
                    {
                        if (element != null) element.Dispose();
                    }

                    // Referenzen l�sen
                    this.PaintPos = null;
                    this._xmlEditor = null;
                }
            }
            _disposed = true;
        }

    }
}
