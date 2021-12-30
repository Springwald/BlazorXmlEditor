// A platform independent tag-view-style graphical xml editor
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
    public partial class AddElement : ComponentBase, IDisposable
    {
        private class Element
        {
            public string Title { get; set; }
            public XmlElementGroup Group { get; set; }
        }

        private EditorState EditorState => this.EditorContext.EditorState;
        private Element[] elements = new Element[] { };
        private XmlElementGroup[] groups = new XmlElementGroup[] { };
        private System.Timers.Timer updateTimer;

        [Parameter]
        public EditorContext EditorContext { get; set; }

        [Parameter]
        public string Class { get; set; }

        protected override Task OnInitializedAsync()
        {
            this.EditorState.CursorRaw.ChangedEvent.Add(this.CursorOrContentChanged);
            this.EditorState.ContentChangedEvent.Add(this.CursorOrContentChanged);
            this.updateTimer = new System.Timers.Timer(300);
            this.updateTimer.Elapsed += this.UpdateTimer_Elapsed;
            this.updateTimer.Stop();
            return base.OnInitializedAsync();
        }

        public void Dispose()
        {
            this.updateTimer.Stop();
            this.updateTimer.Elapsed -= this.UpdateTimer_Elapsed;
            this.EditorState.CursorRaw.ChangedEvent.Remove(this.CursorOrContentChanged);
            this.EditorState.ContentChangedEvent.Remove(this.CursorOrContentChanged);
        }

        protected async Task CursorOrContentChanged(EventArgs e)
        {
            this.updateTimer.Stop();
            this.updateTimer.Start();
            await Task.CompletedTask;
        }

        protected async Task ClickElement(string element)
        {
            await this.EditorContext.Actions.ActionInsertNewElementAtActCursorPos(element, EditorActions.SetUndoSnapshotOptions.Yes, setNewCursorPosBehindNewInsertedNode: false);
            StateHasChanged();
        }

        private async void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.updateTimer.Stop();
            await this.Show();
        }

        public async Task Show()
        {
            if ((this.EditorContext.EditorState.RootNode != null))  //  enough data to list elements to be inserted
            {
                try
                {
                    bool showCommentsToo = true;
                    var elementsRaw = this.EditorContext.XmlRules.AllowedInsertElements(this.EditorState.CursorOptimized.StartPos, false, showCommentsToo); // die Liste der erlaubten Tags holen
                    this.groups = this.EditorContext.XmlRules.ElementGroups.Append(null).ToArray();
                    this.elements = elementsRaw.Select(e => new Element { Title = e, Group = groups.Where(g => g != null && g.ContainsElement(e)).FirstOrDefault() }).ToArray();
                }
                catch (rules.dtd.Dtd.XMLUnknownElementException e)
                {
                    var error = $"unknown element '{e.ElementName}'";
                    //Debugger.GlobalDebugger.Protokolliere(String.Format("unknown element {0} in {1}->{2}", e.ElementName, this.Name, "Aktualisieren"));  //Eines der bezogenen Elemente ist in der DTD unbekannt
                    //lblFehler.Text = String.Format("unknown element '{0}'", e.ElementName);
                    //lblFehler.Visible = true;
                }
                this.StateHasChanged();
                await Task.CompletedTask;
            }
        }
    }
}
