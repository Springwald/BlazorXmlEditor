using de.springwald.xml.editor.actions;
using de.springwald.xml.editor.nativeplatform;
using System;

namespace de.springwald.xml.editor
{
    public class EditorContext : IDisposable
    {
        public EditorContext(EditorConfig editorConfig, XmlRules xmlRules)
        {
            this.EditorConfig = editorConfig;
            this.XmlRules = xmlRules;
            this.EditorState = new EditorStatus();
            this.Actions = new EditorActions(this);
        }

        public INativePlatform NativePlatform { get; set;  }

        public EditorConfig EditorConfig { get; }

        public EditorStatus EditorState { get; }

        public EditorActions Actions { get; }

        public XmlRules XmlRules { get;  }

        public void Dispose()
        {
            this.EditorState.Dispose();
        }
    }
}
