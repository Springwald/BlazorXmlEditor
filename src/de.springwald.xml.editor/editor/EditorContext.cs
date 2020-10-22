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
        }

        public INativePlatform NativePlatform { get; set;  }

        public EditorConfig EditorConfig { get; }

        public EditorStatus EditorStatus { get; } = new EditorStatus();

        public XmlAsyncEvent<EventArgs> EditorIsReady { get; } = new XmlAsyncEvent<EventArgs>();

        // public EditorActions Actions { get; }

        public XMLRegelwerk XmlRules { get;  }

        public void Dispose()
        {
            this.EditorStatus?.Dispose();
        }
    }
}
