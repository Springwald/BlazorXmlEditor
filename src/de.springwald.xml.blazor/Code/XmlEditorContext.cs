using de.springwald.xml.editor.editor;

namespace de.springwald.xml.blazor
{
    public class XmlEditorContext
    {
        //private XmlNode rootNode;

        public EditorStatus EditorStatus { get; } = new EditorStatus();

        public XMLRegelwerk XmlRules { get; set; }

        //public XmlNode RootNode
        //{
        //    get => this.rootNode;
        //}



    }
}
