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
using de.springwald.xml.editor;
using de.springwald.xml.editor.cursor;
using de.springwald.xml.editor.nativeplatform;

namespace de.springwald.xml.editor
{
    public partial class XMLEditor : IDisposable
    {
        private bool _disposed;
        internal CursorBlink CursorBlink { get; }
        internal MouseHandler MouseHandler { get; }
        internal KeyboardHandler KeyboardHandler { get; }

       //  public EditorActions EditorActions { get; }

        private EditorContext editorContext;

        private EditorStatus EditorStatus => this.editorContext.EditorStatus;

        private EditorConfig EditorConfig => this.editorContext.EditorConfig;

        private XMLRegelwerk Regelwerk => this.editorContext.XmlRules;

        public INativePlatform NativePlatform => this.editorContext.NativePlatform;

        /// <summary>
        /// let all XML elements know that you are cleaning up
        /// </summary>
        public event EventHandler CleanUpXmlElementsEvent;

        /// <summary>
        /// Stellt einen XML-Editor bereit
        /// </summary>
        public XMLEditor(EditorContext editorContext)
        {
            this.editorContext = editorContext;
            this.NativePlatform.ControlElement.Enabled = false; // Bis zu einer Content-Zuweisung erstmal deaktiviert */

            this.editorContext.EditorStatus.CursorRoh.ChangedEvent.Add(this.CursorChangedEvent);
            this.editorContext.EditorStatus.ContentChangedEvent.Add(this.OnContentChanged);

            this.CursorBlink = new CursorBlink();
            this.CursorBlink.BlinkIntervalChanged.Add(this.CursorBlinkedEvent);

            this.MouseHandler = new MouseHandler(editorContext.NativePlatform);
          //   this.EditorActions = new EditorActions(nativePlatform, this.EditorStatus, this.regelwerk);
            this.KeyboardHandler = new KeyboardHandler(this.editorContext);

            InitScrolling();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                this.CleanUpXmlElements();
                this.editorContext.EditorStatus.CursorRoh.ChangedEvent.Remove(this.CursorChangedEvent);
                this.editorContext.EditorStatus.ContentChangedEvent.Remove(this.OnContentChanged);

                this.CursorBlink.Dispose();

                this.MouseHandler.Dispose();
                this.KeyboardHandler.Dispose();
                this.editorContext.Dispose();
                _disposed = true;
            }
        }

        public async Task SetRootNode(System.Xml.XmlNode value)
        {
            await this.EditorStatus.SetRootNode(value);

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
            return new ElementCreator(this,  this.editorContext).CreatePaintElementForNode(xmlNode);
        }

        private async Task OnContentChanged(EventArgs e)
        {
            var limitRight = this.NativePlatform.Gfx.Width;
            await this.Paint(limitRight: limitRight);
            this.CursorBlink.Active = true;  // After a change, the cursor line is drawn directly
            this.CleanUpXmlElements(); // XML elements may have lost their parent due to the change etc. Therefore trigger the cleanup
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
            // Nach einer Cursorbewegung wird der Cursor zunächst als Strich gezeichnet
            this.CursorBlink.ResetBlinkPhase();

            ScrollingNotwendig();
            if (this.NativePlatform.ControlElement != null)
            {
                var limitRight = this.NativePlatform.Gfx.Width;
                await this.Paint(limitRight: limitRight);
            }
        }

        private void CleanUpXmlElements()
        {
            this.CleanUpXmlElementsEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
