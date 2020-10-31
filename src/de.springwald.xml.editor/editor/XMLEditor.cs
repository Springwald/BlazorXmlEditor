// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.cursor;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.editor.nativeplatform.gfxobs;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    public partial class XmlEditor : IDisposable
    {
        private bool _disposed;
        internal CursorBlink CursorBlink { get; }
        internal MouseHandler MouseHandler { get; }
        internal KeyboardHandler KeyboardHandler { get; }

        private EditorContext editorContext;

        private EditorState EditorStatus => this.editorContext.EditorState;

        private EditorConfig EditorConfig => this.editorContext.EditorConfig;

        public INativePlatform NativePlatform => this.editorContext.NativePlatform;

        public int VirtualWidth { get; private set; }
        public int VirtualHeight { get; private set; }

        public XmlAsyncEvent<EventArgs> VirtualSizeChanged { get; } = new XmlAsyncEvent<EventArgs>();

        /// <summary>
        /// let all XML elements know that you are cleaning up
        /// </summary>
        public event EventHandler CleanUpXmlElementsEvent;

        /// <summary>
        /// Stellt einen XML-Editor bereit
        /// </summary>
        public XmlEditor(EditorContext editorContext)
        {
            this.editorContext = editorContext;
            this.NativePlatform.ControlElement.Enabled = false; // Bis zu einer Content-Zuweisung erstmal deaktiviert */

            this.editorContext.EditorState.CursorRaw.ChangedEvent.Add(this.CursorChangedEvent);
            this.editorContext.EditorState.ContentChangedEvent.Add(this.OnContentChanged);

            this.CursorBlink = new CursorBlink();
            this.CursorBlink.BlinkIntervalChanged.Add(this.CursorBlinkedEvent);

            this.MouseHandler = new MouseHandler(editorContext.NativePlatform);
            //   this.EditorActions = new EditorActions(nativePlatform, this.EditorStatus, this.regelwerk);
            this.KeyboardHandler = new KeyboardHandler(this.editorContext);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                this.CleanUpXmlElements();
                this.editorContext.EditorState.CursorRaw.ChangedEvent.Remove(this.CursorChangedEvent);
                this.editorContext.EditorState.ContentChangedEvent.Remove(this.OnContentChanged);

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
                    if (this.EditorStatus.RootElement.XmlNode != this.EditorStatus.RootNode)
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
                    this.EditorStatus.UndoHandler = new XmlUndoHandler(this.EditorStatus.RootNode);
                }

                this.NativePlatform.ControlElement.Enabled = true;
            }

            await this.EditorStatus.FireContentChangedEvent();
        }

        /// <summary>
        /// Provides an XML control element
        /// </summary>
        internal XmlElement CreateElement(System.Xml.XmlNode xmlNode)
        {
            return new ElementCreator(this, this.editorContext).CreatePaintElementForNode(xmlNode);
        }

        private bool sizeChangedSinceLastPaint = true;

        public void SizeHasChanged()
        {
            this.sizeChangedSinceLastPaint = true;
        }

        private bool virtualSizeChangedSinceLastPaint;

        public async Task Paint(int limitRight)
        {
            var paintMode = XmlElement.PaintModes.OnlyPaintWhenChanged;

            if (this.virtualSizeChangedSinceLastPaint)
            {
                await this.VirtualSizeChanged.Trigger(EventArgs.Empty);
            }

            if (this.sizeChangedSinceLastPaint)
            {
                this.NativePlatform.Gfx.AddJob(new JobClear { FillColor = this.EditorConfig.ColorBackground });
                this.sizeChangedSinceLastPaint = false;
                paintMode = XmlElement.PaintModes.ForcePaintNoUnPaintNeeded;
            }

            if (this.EditorStatus.RootElement != null)
            {
                var paintContext = new PaintContext
                {
                    LimitLeft = 0,
                    LimitRight = limitRight,
                    PaintPosX = 10,
                    PaintPosY = 10 ,
                    ZeilenStartX = 10 ,
                };

                var context1 = await this.EditorStatus.RootElement.Paint(paintContext.Clone(), this.EditorStatus.CursorOptimized, this.NativePlatform.Gfx, paintMode);
                var newVirtualWidth = context1.BisherMaxX + 50;
                var newVirtualHeight = context1.PaintPosY + 50;
                if (this.VirtualWidth != newVirtualWidth || this.VirtualHeight != newVirtualHeight)
                {
                    this.VirtualWidth = newVirtualWidth;
                    this.VirtualHeight = newVirtualHeight;
                    this.virtualSizeChangedSinceLastPaint = true;
                }
            }
            await this.NativePlatform.Gfx.PaintJobs(EditorConfig.ColorBackground);
            
        }

        public void FokusAufEingabeFormularSetzen()
        {
            /*if (this._zeichnungsSteuerelement != null)
            {
                this._zeichnungsSteuerelement.Focus();
            }*/
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
