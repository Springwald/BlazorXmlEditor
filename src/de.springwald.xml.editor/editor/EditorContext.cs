using de.springwald.xml.editor.actions;
using de.springwald.xml.editor.nativeplatform;
using System;

namespace de.springwald.xml.editor
{
    public class EditorContext : IDisposable
    {
        public EditorContext(EditorConfig editorConfig)
        {
            this.EditorConfig = editorConfig;
            this.EditorState = new EditorState();
            this.Actions = new EditorActions(this);
        }

        public EditorContext(EditorConfig editorConfig, XmlRules xmlRules):this(editorConfig)
        {
            this.XmlRules = xmlRules;
        }

        public INativePlatform NativePlatform { get; set;  }

        public EditorConfig EditorConfig { get; }

        public EditorState EditorState { get; }

        public EditorActions Actions { get; }

        public XmlRules XmlRules { get; set; }

        public void Dispose()
        {
            this.EditorState.Dispose();
        }
    }
}
