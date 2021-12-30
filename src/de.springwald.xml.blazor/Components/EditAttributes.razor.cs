// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor;
using de.springwald.xml.rules;
using de.springwald.xml.rules.dtd;
using Microsoft.AspNetCore.Components;

namespace de.springwald.xml.blazor.Components
{
    public partial class EditAttributes : ComponentBase, IDisposable
    {
        [Parameter]
        public EditorContext EditorContext { get; set; }

        [Parameter]
        public string Class { get; set; }

        private EditorState EditorState => this.EditorContext?.EditorState;

        // private string[] attributes;
        private System.Xml.XmlNode actualNode;
        private DtdElement actualNodeDtdElement;
        private string errorMessage;
        private System.Timers.Timer updateTimer;

        protected override Task OnInitializedAsync()
        {
            this.EditorState.CursorRaw.ChangedEvent.Add(this.CursorChanged);
            this.updateTimer = new System.Timers.Timer(300);
            this.updateTimer.Elapsed += this.UpdateTimer_Elapsed;
            this.updateTimer.Stop();
            return base.OnInitializedAsync();
        }

        public void Dispose()
        {
            this.updateTimer.Stop();
            this.updateTimer.Elapsed -= this.UpdateTimer_Elapsed;
            this.EditorState.CursorRaw.ChangedEvent.Remove(this.CursorChanged);
        }

        private async void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.updateTimer.Stop();
            await this.ShowAttributes();
        }

        protected async Task CursorChanged(EventArgs e)
        {
            this.updateTimer.Stop();
            this.updateTimer.Start();
            await Task.CompletedTask;
        }

        private async Task ShowAttributes()
        {
            if ((this.EditorState != null) && (this.EditorState.RootNode != null)) //  enough data to list attributes
            {
                if (this.EditorState.CursorRaw.StartPos.Equals(this.EditorState.CursorRaw.EndPos))  //  no area selected
                {
                    // Find out for which node the attributes should be displayed
                    switch (this.EditorState.CursorRaw.StartPos.PosOnNode)
                    {
                        case XmlCursorPos.XmlCursorPositions.CursorInsideTheEmptyNode:
                        case XmlCursorPos.XmlCursorPositions.CursorOnNodeEndTag:
                        case XmlCursorPos.XmlCursorPositions.CursorOnNodeStartTag:
                            // The node itself is selected
                            this.actualNode = this.EditorState.CursorRaw.StartPos.ActualNode;
                            break;

                        case XmlCursorPos.XmlCursorPositions.CursorInsideTextNode:
                            this.actualNode = this.EditorState.CursorRaw.StartPos.ActualNode;
                            break;

                        case XmlCursorPos.XmlCursorPositions.CursorBehindTheNode:
                        case XmlCursorPos.XmlCursorPositions.CursorInFrontOfNode:
                            // We are behind or in front of the node, so the attributes of the ParentNode are displayed
                            this.actualNode = this.EditorState.CursorRaw.StartPos.ActualNode?.ParentNode;
                            break;

                        default:
                            this.actualNode = null!;
                            break;
                    }

                    while (this.actualNode is System.Xml.XmlText) // its a text node
                    {
                        this.actualNode = this.actualNode.ParentNode;   //  display the attributes of the parent node
                    }

                    if (this.actualNode == null)
                    {
                        this.actualNodeDtdElement = null!;
                    }
                    else
                    {
                        this.actualNodeDtdElement = null!;

                        // Get the DTD definitions for the current node
                        try
                        {
                            this.actualNodeDtdElement = this.EditorContext.XmlRules.Dtd.DTDElementByName(this.actualNode.Name, true);
                        }
                        catch (Dtd.XMLUnknownElementException e)
                        {
                            this.errorMessage = $"unknown element '{e.ElementName}'";
                        }
                    }
                }
                StateHasChanged();
                await Task.CompletedTask;
            }
        }

        private async Task ValueSelected(string name, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                (this.actualNode as System.Xml.XmlElement).RemoveAttribute(name);
            }
            else
            {
                (this.actualNode as System.Xml.XmlElement).SetAttribute(name, value);
            }
            await this.EditorState.FireContentChangedEvent(needToSetFocusOnEditorWhenLost: false, forceFullRepaint: false);
        }
    }

}
