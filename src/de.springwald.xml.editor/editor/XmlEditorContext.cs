namespace de.springwald.xml.editor
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
