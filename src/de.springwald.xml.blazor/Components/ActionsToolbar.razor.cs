﻿// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor;
using de.springwald.xml.editor.actions;
using Microsoft.AspNetCore.Components;

namespace de.springwald.xml.blazor.Components
{
    public partial class ActionsToolbar : ComponentBase, IDisposable
    {
        [Parameter]
        public EditorContext EditorContext
        {
            get { return this.editorContext; }
            set
            {
                this.editorContext = value;
                if (this.editorContext != null)
                {
                    this.editorContext.EditorState.CursorRaw.ChangedEvent.Add(this.ContentOrCursorChanged);
                    this.editorContext.EditorState.ContentChangedEvent.Add(this.ContentOrCursorChanged);
                }
            }
        }

        private enum ClickActions
        {
            Home,
            Undo,
            Copy,
            Paste,
            Cut,
            Delete
        }

        private EditorState EditorState => this.EditorContext?.EditorState;
        private EditorContext editorContext;

        private bool IsDisabled = true;
        private bool SomethingIsSelected;
        private string? UndoTitle;
        private bool InsertPossible;

        protected override async void OnInitialized()
        {
            await this.UpdateButtonStates();
            base.OnInitialized();
        }

        public void Dispose()
        {
            this.editorContext?.EditorState.CursorRaw.ChangedEvent.Remove(this.ContentOrCursorChanged);
            this.editorContext?.EditorState.ContentChangedEvent.Remove(this.ContentOrCursorChanged);
        }

        private async Task ContentOrCursorChanged(EventArgs e)
        {
            await this.UpdateButtonStates();
            await Task.CompletedTask;
        }

        private async Task Clicked(ClickActions action)
        {
            switch (action)
            {
                case ClickActions.Copy:
                    await this.EditorContext.Actions.ActionCopyToClipboard();
                    break;
                case ClickActions.Cut:
                    await this.EditorContext.Actions.AktionCutToClipboard(EditorActions.SetUndoSnapshotOptions.Yes);
                    break;
                case ClickActions.Delete:
                    await this.EditorContext.Actions.ActionDelete(EditorActions.SetUndoSnapshotOptions.Yes);
                    break;
                case ClickActions.Home:
                    await this.EditorContext.Actions.ActionCursorOnPos1();
                    break;
                case ClickActions.Paste:
                    await this.EditorContext.Actions.ActionPasteFromClipboard(EditorActions.SetUndoSnapshotOptions.Yes);
                    break;
                case ClickActions.Undo:
                    await this.EditorContext.EditorState.UnDo();
                    break;
            }
            await Task.CompletedTask;
        }

        private async Task UpdateButtonStates()
        {
            if (this.EditorState == null || this.EditorState.RootNode == null || this.EditorState.ReadOnly)
            {
                this.IsDisabled = true;
                this.UndoTitle = null;
                return;
            }
            else
            {
                this.IsDisabled = false;
            }

            this.UndoTitle = this.EditorState.UndoPossible ? this.EditorState.UndoStepName : null;

            this.InsertPossible = await this.EditorContext.NativePlatform.Clipboard.ContainsText() && this.EditorState.CursorRaw.StartPos.ActualNode != null;

            this.SomethingIsSelected = this.EditorState.IsSomethingSelected;
            this.StateHasChanged();
        }
    }
}
