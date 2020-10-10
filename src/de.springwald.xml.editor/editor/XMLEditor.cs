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
using de.springwald.xml.editor.editor;
using de.springwald.xml.editor.editor.cursor;
using de.springwald.xml.editor.nativeplatform;

namespace de.springwald.xml.editor
{
    public partial class XMLEditor : IDisposable
    {
        private bool _disposed;

        internal CursorBlink CursorBlink { get; }
        internal MouseHandler MouseHandler { get; }
        internal KeyboardHandler KeyboardHandler { get; }

        public EditorActions EditorActions { get; }
        public IEditorConfig EditorConfig { get; }
        public INativePlatform NativePlatform { get; }
        public EditorStatus EditorStatus { get; }

        /// <summary>
        /// let all XML elements know that you are cleaning up
        /// </summary>
        public event EventHandler XmlElementeAufraeumenEvent;

        /// <summary>
        /// Stellt einen XML-Editor bereit
        /// </summary>
        /// <param name="regelwerk">Das Regelwerk zur Darstellung des XMLs</param>
        /// <param name="zeichnungsSteuerelement">Das Usercontrol, auf welchem der Editor gezeichnet werden soll</param>
        /// <param name="rootNode">Dies ist der oberste, zu bearbeitende Node. H�her darf nicht bearbeitet werden, selbst wenn im DOM Parents vorhanden sind</param>
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
                // Provide the Root Element
                // If the current XML element is no longer the same, destroy the previous one so that it can be recreated
                if (this.EditorStatus.RootElement != null)
                {
                    if (this.EditorStatus.RootElement.XMLNode != this.EditorStatus.RootNode)
                    {
                        this.EditorStatus.RootElement.Dispose();
                        this.EditorStatus.RootElement = null;
                    }
                }
                // If XML element is (yet) not instantiated, then create
                if (this.EditorStatus.RootElement == null)
                {
                    this.EditorStatus.RootElement = CreateElement(this.EditorStatus.RootNode);
                }

                // provide a suitable Undo-Handler
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

            await this.EditorStatus.FireContentChangedEvent();
        }

        /// <summary>
        /// Provides an XML control element
        /// </summary>
        internal XMLElement CreateElement(System.Xml.XmlNode xmlNode)
        {
            return new ElementCreator(this).createPaintElementForNode(xmlNode);
        }

        private async Task OnContentChanged(EventArgs e)
        {
            var limitRight = this.NativePlatform.Gfx.Width;
            await this.Paint(limitRight: limitRight);
            this.CursorBlink.Active = true;  // After a change, the cursor line is drawn directly
            this.xmlElementeAufraeumen(); // XML elements may have lost their parent due to the change etc. Therefore trigger the cleanup
        }

        private async Task CursorBlinkedEvent(EventArgs e)
        {
            if (this.NativePlatform.ControlElement != null)
            {
                var limitRight = this.NativePlatform.Gfx.Width;
                await this.Paint(limitRight: limitRight);
            }
        }

        private async Task CursorChangedEvent(EventArgs e)
        {
            // Nach einer Cursorbewegung wird der Cursor zun�chst als Strich gezeichnet
            this.CursorBlink.ResetBlinkPhase();

            ScrollingNotwendig();
            if (this.NativePlatform.ControlElement != null)
            {
                var limitRight = this.NativePlatform.Gfx.Width;
                await this.Paint(limitRight: limitRight);
            }
        }

        private void xmlElementeAufraeumen()
        {
            this.XmlElementeAufraeumenEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}