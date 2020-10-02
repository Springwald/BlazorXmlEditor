using de.springwald.xml.cursor;
using de.springwald.xml.editor.nativeplatform;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Zusammenfassung für cXMLBearbeitung.
    /// </summary>
    /// <remarks>
    /// (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>

    public enum XMLPaintArten { Vorberechnen, AllesNeuZeichnenOhneFehlerHighlighting, AllesNeuZeichnenMitFehlerHighlighting };

    public partial class XMLEditor : IDisposable
    {
        public IEditorConfig EditorConfig { get; }
        public INativePlatform NativePlatform { get; }

        /// <summary>
        /// Der Inhalt des Editor-XMLs hat sich geändert
        /// </summary>
        public event EventHandler ContentChangedEvent;

        protected virtual async Task ContentChanged()
        {
            // ContentChangedEvent?.Invoke(this, EventArgs.Empty);

            // Dem Zeichnungssteuerelement Bescheid sagen, dass es neu gezeichnet werden muss
            // if (this.NativePlatform.ControlElement != null) await this.NativePlatform.ControlElement.Invalidated.Trigger(EventArgs.Empty);

            var limitRight = this.NativePlatform.ControlElement.Width;
            await this.Paint(new events.PaintEventArgs { Graphics = this.NativePlatform.Gfx }, limitRight: limitRight);

            // Nach einer Veränderung wird direkt der Cursor-Strich gezeichnet
            CursorBlinkOn = true;
            xmlElementeAufraeumen(); // Ggf. haben durch die Änderung XMLElemente Ihren Parent verloren etc.. Daher das Aufräumen anstoßen
        }

        private de.springwald.xml.cursor.XMLCursor _cursor;  // Dort befindet sich der der Cursor aktuell innerhalb des XML-Dokumentes
        private System.Xml.XmlNode _rootNode;				 // Dies ist der oberste, zu bearbeitende Node. Höher darf nicht bearbeitet werden, selbst wenn im DOM Parents vorhanden sind

        /// <summary>
        /// Die gewünschte Wunsch-Umbruch-Breite
        /// </summary>
        private int _wunschUmbruchXBuffer;

        /// <summary>
        /// Ist die aktuelle Datei schreibgeschützt?
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Die gewünschte Wunsch-Umbruch-Breite
        /// </summary>
        public int WunschUmbruchX_
        {
            get { return this.NativePlatform.ControlElement.Width - 100; }
        }

        /// <summary>
        /// Das Regelwerk, auf dessen Basis die XML-Bearbeitung geschieht
        /// </summary>
        public de.springwald.xml.XMLRegelwerk Regelwerk { get; set; }

        /// <summary>
        /// Dies ist die CursorPosition, optmimiert darauf, dass die StartPos auch vor der EndPos liegt
        /// </summary>
        public de.springwald.xml.cursor.XMLCursor CursorOptimiert
        {
            get
            {
                XMLCursor cursor = _cursor.Clone();
                cursor.SelektionOptimieren().Wait();
                return cursor;
            }
        }

        /// <summary>
        /// Dort befindet sich der der Cursor aktuell innerhalb des XML-Dokumentes
        /// </summary>
        public de.springwald.xml.cursor.XMLCursor CursorRoh
        {
            get { return _cursor; }
        }

        /// <summary>
        /// Dies ist der oberste, zu bearbeitende Node. Höher darf nicht bearbeitet werden, selbst wenn im DOM Parents vorhanden sind
        /// </summary>
        public System.Xml.XmlNode RootNode
        {
            get { return (_rootNode); }
        }


        public async Task SetRootNode(System.Xml.XmlNode value)
        {
            // await this.NativePlatform.Gfx.StartBatch();

            this._rootNode = value;

            if (_rootNode == null)
            {
                if (_rootElement != null)
                {
                    _rootElement.Dispose();
                    _rootElement = null;
                }
                this.NativePlatform.ControlElement.Enabled = false;
            }
            else
            {
                // Das Root-Element bereitstellen
                // Wenn das aktuelle XML-Element nicht mehr dasselbe ist, dann
                // das bisherige zerstören, damit er danach neu erzeugt werden kann
                if (_rootElement != null)
                {
                    if (_rootElement.XMLNode != _rootNode)
                    {
                        _rootElement.Dispose();
                        _rootElement = null;
                    }
                }
                // Wenn kein XML-Element (noch) nicht instanziert ist, dann erzeugen
                if (_rootElement == null)
                {
                    _rootElement = createElement(_rootNode);
                }

                // einen passenden Undo-Handler bereitstellen
                if (_undoHandler != null)
                {
                    if (_undoHandler.RootNode != _rootNode)
                    {
                        _undoHandler.Dispose();
                        _undoHandler = null;
                    }
                }

                if (_undoHandler == null)
                {
                    _undoHandler = new XMLUndoHandler(_rootNode);
                }

                this.NativePlatform.ControlElement.Enabled = true;
            }

            // await this.NativePlatform.Gfx.EndBatch();

            // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
            await ContentChanged();
        }

        /// <summary>
        /// Gibt an, ob der Rootnode selektiert ist 
        /// </summary>
        public bool IstRootNodeSelektiert
        {
            get
            {
                if (IstEtwasSelektiert) // Überhaupt was selektiert
                {
                    XMLCursorPos startpos = CursorOptimiert.StartPos;
                    if (startpos.AktNode == RootNode) // Der Rootnode ist im Cursor
                    {
                        switch (startpos.PosAmNode)
                        {
                            case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                            case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                                return true; // Das Rootnode ist selektiert
                        }
                    }
                }
                return false; // Der Rootnode ist nicht selektiert
            }
        }

        /// <summary>
        /// Stellt einen XML-Editor bereit
        /// </summary>
        /// <param name="regelwerk">Das Regelwerk zur Darstellung des XMLs</param>
        /// <param name="zeichnungsSteuerelement">Das Usercontrol, auf welchem der Editor gezeichnet werden soll</param>
        /// <param name="rootNode">Dies ist der oberste, zu bearbeitende Node. Höher darf nicht bearbeitet werden, selbst wenn im DOM Parents vorhanden sind</param>
        public XMLEditor(de.springwald.xml.XMLRegelwerk regelwerk, INativePlatform nativePlatform, IEditorConfig editorConfig)
        {
            this.EditorConfig = editorConfig;
            this.NativePlatform = nativePlatform;
            this.Regelwerk = regelwerk;

            this.NativePlatform.ControlElement.Enabled = false; // Bis zu einer Content-Zuweisung erstmal deaktiviert */

            // this.NativePlatform.ControlElement.Invalidated.Add(this.Invalidated);

            _cursor = new XMLCursor();
            _cursor.ChangedEvent.Add(this._cursor_ChangedEvent);

            // Events auf das Usercontrol ansetzen, auf welchem gezeichnet werden soll
            MausEventsAnmelden();
            TastaturEventsAnmelden();

            InitCursorBlink();
            InitScrolling();
        }

        //private async Task Invalidated(EventArgs data)
        //{
        //    var limitRight = this.NativePlatform.ControlElement.Width;
        //    await this.Paint(new events.PaintEventArgs { Graphics = this.NativePlatform.Gfx }, limitRight: limitRight);
        //}

        /// <summary>
        /// Stellt ein XML-Steuerelement-Element bereit
        /// </summary>
        /// <returns></returns>
        public virtual de.springwald.xml.editor.XMLElement createElement(System.Xml.XmlNode xmlNode)
        {
            de.springwald.xml.editor.XMLElement neuElement;

            neuElement = new ElementCreator(this).createPaintElementForNode(xmlNode);
            return neuElement;
        }

        /// <summary>
        /// Der Cursor hat sich geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task _cursor_ChangedEvent(EventArgs e)
        {
            ScrollingNotwendig();

            if (this.NativePlatform.ControlElement != null)
            {
                await this.ContentChanged();
                // await this.NativePlatform.ControlElement.Invalidated.Trigger(e);
            }

            // Nach einer Cursorbewegung wird der Cursor zunächst als Strich gezeichnet
            CursorBlinkOn = true;

        }

    }
}
