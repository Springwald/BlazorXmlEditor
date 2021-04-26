// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.nativeplatform.gfxobs;
using static de.springwald.xml.editor.EditorState;

namespace de.springwald.xml.editor
{
    public partial class XmlEditor : IDisposable
    {
        private bool _disposed;

        internal MouseHandler MouseHandler { get; }
        internal KeyboardHandler KeyboardHandler { get; }

        private readonly EditorContext editorContext;

        private EditorState EditorState => this.editorContext.EditorState;

        private EditorConfig EditorConfig => this.editorContext.EditorConfig;

        public INativePlatform NativePlatform => this.editorContext.NativePlatform;

        public int VirtualWidth { get; private set; }
        public int VirtualHeight { get; private set; }

        public XmlAsyncEvent<EventArgs> VirtualSizeChanged { get; } = new XmlAsyncEvent<EventArgs>();

        /// <summary>
        /// let all XML elements know that you are cleaning up
        /// </summary>
        public event EventHandler CleanUpXmlElementsEvent;

        public XmlEditor(EditorContext editorContext)
        {
            this.editorContext = editorContext;
            this.editorContext.EditorState.CursorRaw.ChangedEvent.Add(this.CursorChangedEvent);
            this.editorContext.EditorState.ContentChangedEvent.Add(this.OnContentChanged);
            this.EditorState.RootNodeChanged.Add(this.OnRootNodeChanged);
            this.MouseHandler = new MouseHandler(editorContext.NativePlatform);
            this.KeyboardHandler = new KeyboardHandler(this.editorContext);
            this.EditorState.CursorBlink.BlinkIntervalChanged.Add(this.CursorBlinkedEvent);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                this.CleanUpXmlElements();
                this.editorContext.EditorState.CursorRaw.ChangedEvent.Remove(this.CursorChangedEvent);
                this.editorContext.EditorState.ContentChangedEvent.Remove(this.OnContentChanged);
                this.EditorState.RootNodeChanged.Remove(this.OnRootNodeChanged);
                this.MouseHandler.Dispose();
                this.KeyboardHandler.Dispose();
                this.editorContext.Dispose();
                _disposed = true;
            }
        }

        private async Task OnRootNodeChanged(System.Xml.XmlNode value)
        {
            if (this.EditorState.RootNode == null)
            {
                if (this.EditorState.RootElement != null)
                {
                    this.EditorState.RootElement.Dispose();
                    this.EditorState.RootElement = null;
                }
            }
            else
            {
                // Provide the Root Element
                // If the current XML element is no longer the same, destroy the previous one so that it can be recreated
                if (this.EditorState.RootElement != null)
                {
                    if (this.EditorState.RootElement.XmlNode != this.EditorState.RootNode)
                    {
                        this.EditorState.RootElement.Dispose();
                        this.EditorState.RootElement = null;
                    }
                }

                // If XML element is (yet) not instantiated, then create
                if (this.EditorState.RootElement == null)
                {
                    this.EditorState.RootElement = CreateElement(this.EditorState.RootNode);
                }

                // provide a suitable Undo-Handler
                if (this.EditorState.UndoHandler != null)
                {
                    if (this.EditorState.UndoHandler.RootNode != this.EditorState.RootNode)
                    {
                        this.EditorState.UndoHandler.Dispose();
                        this.EditorState.UndoHandler = null;
                    }
                }

                if (this.EditorState.UndoHandler == null)
                {
                    this.EditorState.UndoHandler = new XmlUndoHandler(this.EditorState.RootNode);
                }

                await this.EditorState.CursorRaw.SetPositions(null, rules.XmlCursorPos.XmlCursorPositions.CursorInFrontOfNode, 0, false);
            }

            bool debugUpdate = false;
            if (debugUpdate)
            {
                this.NativePlatform.Gfx.DeleteAllPaintJobs();
                this.NativePlatform.Gfx.AddJob(new JobClear());
                await this.NativePlatform.Gfx.PaintJobs(Color.Blue);
                Console.WriteLine("root node changed " + this.EditorState.RootNode?.InnerText + DateTime.Now.Ticks);
            }
            await this.EditorState.FireContentChangedEvent(needToSetFocusOnEditorWhenLost: false, forceFullRepaint: true);
        }

        /// <summary>
        /// Provides an XML control element
        /// </summary>
        internal XmlElement CreateElement(System.Xml.XmlNode xmlNode)
        {
            return new ElementCreator(this, this.editorContext).CreatePaintElementForNode(xmlNode);
        }

        public async Task CanvasSizeHasChanged()
        {
            var limitRight = this.NativePlatform.Gfx.Width;
            await this.Paint(limitRight: limitRight, forceRepaint: true, isCursorBlink: false);
        }


        protected async Task Paint(int limitRight, bool forceRepaint, bool isCursorBlink)
        {
            var gfx = this.NativePlatform.Gfx;
            gfx.DeleteAllPaintJobs();

            if (this.EditorState.RootElement == null)
            {
                gfx.AddJob(new JobClear());
                await gfx.PaintJobs(EditorConfig.ColorBackground);
            }
            else
            {
                var paintMode = XmlElement.PaintModes.OnlyPaintWhenChanged;

                if (forceRepaint)
                {
                    gfx.AddJob(new JobClear());
                    paintMode = XmlElement.PaintModes.ForcePaintNoUnPaintNeeded;
                }

                var paintContext = new PaintContext
                {
                    LimitLeft = 0,
                    LimitRight = limitRight - 20,
                    PaintPosX = 10,
                    PaintPosY = 10,
                    RowStartX = 10,
                };

                var paintResultContext = await this.EditorState.RootElement.Paint(paintContext.Clone(), EditorState.CursorBlink.PaintCursor, this.EditorState.CursorOptimized, gfx, paintMode, depth: 0);
                await gfx.PaintJobs(EditorConfig.ColorBackground);

                const int margin = 50;
                var newVirtualWidth = ((paintResultContext.FoundMaxX + margin) / margin) * margin;
                var newVirtualHeight = ((paintResultContext.PaintPosY + margin) / margin) * margin;
                if (this.VirtualWidth != newVirtualWidth || this.VirtualHeight != newVirtualHeight)
                {
                    this.VirtualWidth = newVirtualWidth;
                    this.VirtualHeight = newVirtualHeight;
                    await this.VirtualSizeChanged.Trigger(EventArgs.Empty);
                }
            }
        }

        private async void LateUpdatePaintTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var limitRight = this.NativePlatform.Gfx.Width;
            await this.Paint(limitRight: limitRight, forceRepaint: true, isCursorBlink: false);
        }

        private async Task OnContentChanged(ContentChangedEventArgs e)
        {
            var limitRight = this.NativePlatform.Gfx.Width;
            this.EditorState.CursorBlink.ResetBlinkPhase();  // After a change, the cursor line is drawn directly
            this.CleanUpXmlElements(); // XML elements may have lost their parent due to the change etc. Therefore trigger the cleanup
            await this.Paint(limitRight: limitRight, forceRepaint: e.ForceFullRepaint, isCursorBlink: false);
        }

        private async Task CursorBlinkedEvent(bool blinkOn)
        {
            var limitRight = this.NativePlatform.Gfx.Width;
            await this.Paint(limitRight: limitRight, forceRepaint: false, isCursorBlink: true);
        }

        private async Task CursorChangedEvent(EventArgs e)
        {
            // After a cursor movement, the cursor is first drawn as a line
            this.EditorState.CursorBlink.ResetBlinkPhase();
            var limitRight = this.NativePlatform.Gfx.Width;
            await this.Paint(limitRight: limitRight, forceRepaint: false, isCursorBlink: false);
        }

        private void CleanUpXmlElements()
        {
            this.CleanUpXmlElementsEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
