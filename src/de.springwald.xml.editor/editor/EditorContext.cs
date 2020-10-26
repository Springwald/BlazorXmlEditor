using de.springwald.xml.editor.actions;
using de.springwald.xml.editor.nativeplatform;
using System;

namespace de.springwald.xml.editor
{
    public class EditorContext : IDisposable
    {
        public EditorContext(EditorConfig editorConfig, XMLRegelwerk xmlRules)
        {
            this.EditorConfig = editorConfig;
            this.XmlRules = xmlRules;
            this.EditorStatus = new EditorStatus();
            this.Actions = new EditorActions(this);
        }

        public INativePlatform NativePlatform { get; set;  }

        public EditorConfig EditorConfig { get; }

        public EditorStatus EditorStatus { get; }

        public EditorActions Actions { get; }

        public XMLRegelwerk XmlRules { get;  }

        public void Dispose()
        {
            this.EditorStatus.Dispose();
        }
    }
}
