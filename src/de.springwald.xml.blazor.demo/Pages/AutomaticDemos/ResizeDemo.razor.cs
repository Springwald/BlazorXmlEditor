using de.springwald.xml.blazor.demo.DemoData;
using de.springwald.xml.editor;
using Microsoft.AspNetCore.Components;


namespace de.springwald.xml.blazor.demo.Pages.AutomaticDemos
{
    public partial class ResizeDemo : ComponentBase, IDisposable
    {
        private string documentContent = "<category>" +
            "<pattern>Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. </pattern>"            +
            "<template>Lorem ipsum Dolor sit amet. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.</template>" +
            "</category>";

        private Components.FpsCounter fpsCounter;
        private blazor.Components.XmlEditor xmlEditor;
        private EditorContext editorContext;
        private System.Timers.Timer resizeTimer;
        private System.Xml.XmlDocument xmlDocument;
        private int width;
        private int speed = 1;

        protected override async Task OnInitializedAsync()
        {
            var demoDtd = DemoDtd.LoadDemoDtd();
            this.editorContext = new EditorContext(BlazorEditorConfig.StandardConfig, new DemoXmlRules(demoDtd));
            this.xmlDocument = new System.Xml.XmlDocument();
            this.xmlDocument.LoadXml(this.documentContent);
            await base.OnInitializedAsync();
        }

        public void Dispose()
        {
            this.resizeTimer.Stop();
            this.resizeTimer.Elapsed -= TypingTimerEvent;
        }

        private async Task EditorIsReady()
        {
            await this.editorContext.EditorState.SetRootNode(xmlDocument.DocumentElement);
            this.InitTypingTimer();
        }

        private void InitTypingTimer()
        {
            this.resizeTimer = new System.Timers.Timer();
            this.resizeTimer.Elapsed += TypingTimerEvent;
            this.resizeTimer.Interval = 1;
            this.resizeTimer.Start();
        }

        private async void TypingTimerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            this.resizeTimer.Stop();

            this.width += this.speed;
            if (this.width > 1000) this.speed = -this.speed;
            if (this.width < 10) { this.width = 10; this.speed = -this.speed; }

            await this.xmlEditor.OuterResized(EventArgs.Empty);

            this.StateHasChanged();

            this.fpsCounter.Count();
            this.resizeTimer.Start();
        }
    }
}
