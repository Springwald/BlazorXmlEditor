using de.springwald.xml.editor.editor;
using de.springwald.xml.editor.editor.cursor;
using de.springwald.xml.editor.nativeplatform;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    public partial class XMLEditor : IDisposable
    {
        private bool _disposed;
        /// <summary>
        /// Die gewünschte Wunsch-Umbruch-Breite
        /// </summary>
        private int _wunschUmbruchXBuffer;

        internal CursorBlink CursorBlink { get; }
        internal MouseHandler MouseHandler { get; }
        internal KeyboardHandler KeyboardHandler { get; }
        public EditorActions EditorActions { get; }


        public IEditorConfig EditorConfig { get; }
        public INativePlatform NativePlatform { get; }
        public EditorStatus EditorStatus { get; }
        

        /// <summary>
        /// Stellt einen XML-Editor bereit
        /// </summary>
        /// <param name="regelwerk">Das Regelwerk zur Darstellung des XMLs</param>
        /// <param name="zeichnungsSteuerelement">Das Usercontrol, auf welchem der Editor gezeichnet werden soll</param>
        /// <param name="rootNode">Dies ist der oberste, zu bearbeitende Node. Höher darf nicht bearbeitet werden, selbst wenn im DOM Parents vorhanden sind</param>
        public XMLEditor(XMLRegelwerk regelwerk, INativePlatform nativePlatform, IEditorConfig editorConfig)
        {
            this.EditorConfig = editorConfig;
            this.NativePlatform = nativePlatform;
            this.NativePlatform.ControlElement.Enabled = false; // Bis zu einer Content-Zuweisung erstmal deaktiviert */

            this.EditorStatus = new EditorStatus(nativePlatform, regelwerk);

            this.EditorStatus.CursorRoh.ChangedEvent.Add(this.CursorChangedEvent);
            this.EditorStatus.ContentChangedEvent.Add(this.OnContentChanged);

            this.CursorBlink = new CursorBlink();
            this.CursorBlink.BlinkIntervalChanged.Add(this.CursorBlinkedEvent);

            this.MouseHandler = new MouseHandler(nativePlatform);
            this.KeyboardHandler = new KeyboardHandler(nativePlatform, this.EditorStatus, this.EditorActions);
            this.EditorActions = new EditorActions(nativePlatform, this.EditorStatus);

            // Events auf das Usercontrol ansetzen, auf welchem gezeichnet werden soll
            InitScrolling();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                this.xmlElementeAufraeumen();
                this.EditorStatus.CursorRoh.ChangedEvent.Remove(this.CursorChangedEvent);
                this.CursorBlink.Dispose();
                this.MouseHandler.Dispose();
                this.KeyboardHandler.Dispose();
                this.EditorStatus.Dispose();
                _disposed = true;
            }
        }


        public async Task SetRootNode(System.Xml.XmlNode value)
        {
            // await this.NativePlatform.Gfx.StartBatch();

            this.EditorStatus.RootNode = value;

            if (this.EditorStatus.RootNode == null)
            {
                if (this.EditorStatus.RootElement != null)
                {
                    this.EditorStatus.RootElement.Dispose();
                    this.EditorStatus.RootElement = null;
                }
                this.NativePlatform.ControlElement.Enabled = false;
            }
            else
            {
                // Das Root-Element bereitstellen
                // Wenn das aktuelle XML-Element nicht mehr dasselbe ist, dann
                // das bisherige zerstören, damit er danach neu erzeugt werden kann
                if (this.EditorStatus.RootElement != null)
                {
                    if (this.EditorStatus.RootElement.XMLNode != this.EditorStatus.RootNode)
                    {
                        this.EditorStatus.RootElement.Dispose();
                        this.EditorStatus.RootElement = null;
                    }
                }
                // Wenn kein XML-Element (noch) nicht instanziert ist, dann erzeugen
                if (this.EditorStatus.RootElement == null)
                {
                    this.EditorStatus.RootElement = createElement(this.EditorStatus.RootNode);
                }

                // einen passenden Undo-Handler bereitstellen
                if (this.EditorStatus.UndoHandler != null)
                {
                    if (this.EditorStatus.UndoHandler.RootNode != this.EditorStatus.RootNode)
                    {
                        this.EditorStatus.UndoHandler.Dispose();
                        this.EditorStatus.UndoHandler = null;
                    }
                }

                if (this.EditorStatus.UndoHandler == null)
                {
                    this.EditorStatus.UndoHandler = new XMLUndoHandler(this.EditorStatus.RootNode);
                }

                this.NativePlatform.ControlElement.Enabled = true;
            }

            // await this.NativePlatform.Gfx.EndBatch();

            // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
            await this.EditorStatus.FireContentChangedEvent();
        }

        private async Task OnContentChanged(EventArgs e)
        {
            // ContentChangedEvent?.Invoke(this, EventArgs.Empty);

            // Dem Zeichnungssteuerelement Bescheid sagen, dass es neu gezeichnet werden muss
            // if (this.NativePlatform.ControlElement != null) await this.NativePlatform.ControlElement.Invalidated.Trigger(EventArgs.Empty);

            var limitRight = this.NativePlatform.Gfx.Width;
            await this.Paint(limitRight: limitRight);

            // Nach einer Veränderung wird direkt der Cursor-Strich gezeichnet
            this.CursorBlink.Active = true;
            this.xmlElementeAufraeumen(); // Ggf. haben durch die Änderung XMLElemente Ihren Parent verloren etc.. Daher das Aufräumen anstoßen
        }

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
        async Task CursorBlinkedEvent(EventArgs e)
        {
            if (this.NativePlatform.ControlElement != null)
            {
                var limitRight = this.NativePlatform.Gfx.Width;
                await this.Paint(limitRight: limitRight);
            }
        }


        /// <summary>
        /// allen XML-Elementen Bescheid sagen, dass Sie sich aufräumen
        /// </summary>
        public event EventHandler xmlElementeAufraeumenEvent;
        protected virtual void xmlElementeAufraeumen()
        {
            if (xmlElementeAufraeumenEvent != null) xmlElementeAufraeumenEvent(this, EventArgs.Empty);
        }

 
        /// <summary>
        /// Der Cursor hat sich geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task CursorChangedEvent(EventArgs e)
        {
            // Nach einer Cursorbewegung wird der Cursor zunächst als Strich gezeichnet
            this.CursorBlink.ResetBlinkPhase();

            ScrollingNotwendig();
            if (this.NativePlatform.ControlElement != null)
            {
                var limitRight = this.NativePlatform.Gfx.Width;
                await this.Paint(limitRight: limitRight);
            }
        }

    }
}
