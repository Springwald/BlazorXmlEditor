// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using Blazor.Extensions;
using CurrieTechnologies.Razor.Clipboard;
using de.springwald.xml.blazor.Code;
using de.springwald.xml.blazor.NativePlatform;
using de.springwald.xml.editor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace de.springwald.xml.blazor.Components
{
    public partial class XmlEditor : ComponentBase, IDisposable
    {
        public class BoundingClientRect
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public double Top { get; set; }
            public double Right { get; set; }
            public double Bottom { get; set; }
            public double Left { get; set; }
        }

        protected int canvasWidth = 10;
        protected int canvasHeight = 10;

        private bool showContextMenu;

        protected BECanvasComponent _canvasReference;
        protected ElementReference _xmlEditorBoxDivReference;
        protected ElementReference _canvasOuterDivReference;
        private de.springwald.xml.editor.XmlEditor editor;

        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ClipboardService clipboardService { get; set; }

        [Parameter]
        public EditorContext EditorContext { get; set; }

        [Parameter]
        public string Style { get; set; }

        [Parameter]
        public string Class { get; set; }

        [Parameter] public EventCallback OnReady { get; set; }

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await this.Init();
                await JSRuntime.InvokeVoidAsync("browserResize.registerResizeCallback");
                BrowserResize.OnResize.Add(this.OuterResized);
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task Init()
        {
            if (this.EditorContext == null) throw new ArgumentNullException(nameof(this.EditorContext));
            this.EditorContext.NativePlatform = new BlazorNativePlatform(_canvasReference, new BlazorClipboard(this.clipboardService));
            this.editor = new editor.XmlEditor(this.EditorContext);
            this.EditorContext.EditorState.RootNodeChanged.Add(this.RootNodeChanged);
            this.EditorContext.EditorState.ContentChangedEvent.Add(this.ContentChanged);
            this.editor.VirtualSizeChanged.Add(this.VirtualSizeChanged);
            await OnReady.InvokeAsync(EventArgs.Empty);
        }

        public void Dispose()
        {
            BrowserResize.OnResize.Remove(this.OuterResized);
            this.EditorContext?.EditorState.RootNodeChanged.Remove(this.RootNodeChanged);
            this.editor.VirtualSizeChanged.Remove(this.VirtualSizeChanged);
            this.editor.Dispose();
        }

        private async Task ContentChanged(EditorState.ContentChangedEventArgs e)
        {
            if (e.NeedToSetFocusOnEditorWhenLost)
            {
                await Task.Delay(400);
                await JSRuntime.InvokeVoidAsync("XmlEditorFocusElement", _canvasOuterDivReference);
            }
        }

        private async Task RootNodeChanged(System.Xml.XmlNode rootNode)
        {
            if (rootNode == null) return;
            await this.OuterResized(EventArgs.Empty);
        }

        const int PreventHorizontalScrollBarTolerance = 20;

        private async Task VirtualSizeChanged(EventArgs e)
        {
            var changed = false;

            if (Math.Abs((this.editor.VirtualWidth + PreventHorizontalScrollBarTolerance) - this.canvasWidth) >= PreventHorizontalScrollBarTolerance)
            {
                changed = true;
                this.canvasWidth = this.editor.VirtualWidth + PreventHorizontalScrollBarTolerance;
            }

            if (Math.Abs((this.editor.VirtualHeight + PreventHorizontalScrollBarTolerance) - this.canvasHeight) >= PreventHorizontalScrollBarTolerance)
            {
                changed = true;
                this.canvasHeight = this.editor.VirtualHeight + PreventHorizontalScrollBarTolerance;
            }

            if (changed)
            {
                this.StateHasChanged();
                await this.editor.CanvasSizeHasChanged();
            }
        }

        public async Task OuterResized(EventArgs e)
        {
            var size = await JSRuntime.InvokeAsync<BoundingClientRect>("XmlEditorGetBoundingClientRect", new object[] { this._xmlEditorBoxDivReference });
            if (size == null || size.Width < 50) return;
            var outerWidth = (int)size.Width;
            var desiredMaxSize = outerWidth - (PreventHorizontalScrollBarTolerance + 5);
            await this.EditorContext.NativePlatform.SetDesiredSize(desiredMaxWidth: desiredMaxSize);
            await this.editor.CanvasSizeHasChanged();

        }

        #region key events

        public async void EventFocusIn(FocusEventArgs e)
        {
            this.EditorContext.EditorState.HasFocus = true;
            await Task.CompletedTask;
        }

        public async void EventFocusOut(FocusEventArgs e)
        {
            this.EditorContext.EditorState.HasFocus = false;
            await Task.CompletedTask;
        }

        public async void EventOnKeyDown(KeyboardEventArgs e)
        {
            if (this.showContextMenu)
            {
                if (e.Key == "Escape" || e.Key == "ContextMenu") this.showContextMenu = false;
                return;
            }
            if (e.Key == "ContextMenu")
            {
                this.showContextMenu = true;
                return;
            }

            var args = KeyEventTranslation.Translate(e);
            if (args != null)
            {
                await this.EditorContext.NativePlatform.InputEvents.PreviewKey.Trigger(args);
            }
        }

        #endregion

        #region context menu

        async void MenuClickInsert(MouseEventArgs e)
        {
            this.showContextMenu = false;
            await this.EditorContext.NativePlatform.InputEvents.PreviewKey.Trigger(new events.KeyEventArgs { CtrlKey = true, Key = events.Keys.V });
        }

        async void MenuClickCut(MouseEventArgs e)
        {
            this.showContextMenu = false;
            await this.EditorContext.NativePlatform.InputEvents.PreviewKey.Trigger(new events.KeyEventArgs { CtrlKey = true, Key = events.Keys.X });
        }

        async void MenuClickCopy(MouseEventArgs e)
        {
            this.showContextMenu = false;
            await this.EditorContext.NativePlatform.InputEvents.PreviewKey.Trigger(new events.KeyEventArgs { CtrlKey = true, Key = events.Keys.C });
        }

        async void EventClickOutsideContextMenu(MouseEventArgs e)
        {
            this.showContextMenu = false;
            this.StateHasChanged();
            await Task.CompletedTask;
        }

        #endregion

        #region mouse events

        async void EventOnMouseDown(MouseEventArgs e)
        {
            if (e.Button != 0) return;

            var x = (int)e.OffsetX;
            var y = (int)e.OffsetY;

            await this.EditorContext.NativePlatform.InputEvents.MouseDown.Trigger(new de.springwald.xml.events.MouseEventArgs
            {
                X = x,
                Y = y
            });
        }

        async void EventOnMouseMove(MouseEventArgs e)
        {
            var x = (int)e.OffsetX;
            var y = (int)e.OffsetY;

            await this.EditorContext.NativePlatform.InputEvents.MouseMove.Trigger(new de.springwald.xml.events.MouseEventArgs
            {
                X = x,
                Y = y
            });
        }

        async void EventOnMouseUp(MouseEventArgs e)
        {
            if (e.Button != 0) return;

            var x = (int)e.OffsetX;
            var y = (int)e.OffsetY;

            await this.EditorContext.NativePlatform.InputEvents.MouseUp.Trigger(new de.springwald.xml.events.MouseEventArgs
            {
                X = x,
                Y = y
            });
        }

        void HandleRightClick(MouseEventArgs args)
        {
            if (args.Button == 2) this.showContextMenu = true;
        }

        #endregion

    }

}
