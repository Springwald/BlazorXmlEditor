namespace de.springwald.xml
{
    class ElementCreator
    {
        private editor.XMLEditor xmlEditor;

        public ElementCreator(editor.XMLEditor xmlEditor)
        {
            this.xmlEditor = xmlEditor;
        }
        /// <summary>
        /// Stellt das für den angegebenen Node optimale XML-Steuerelement bereit.
        /// Diese Methode kann in vererbten Regelwerken überschrieben werden, wenn
        /// spezielle Nodes eigene XML-Steuerelemente erfordern.
        /// In dieser Grundkonfiguration gibt die Methode erst einmal nur das Standard-Steuerelement zurück
        /// </summary>
        /// <param name="XMLNode">Der anzuzeigende Node</param>
        /// <returns></returns>
        public editor.XMLElement createPaintElementForNode(System.Xml.XmlNode xmlNode) //, de.springwald.xml.XMLEditorPaintPos paintPos ) 
        {
            if (xmlNode is System.Xml.XmlElement)
            {
                //switch (xmlNode.Name)
                //{
                //    default: 
                return new editor.XMLElement_StandardNode(xmlNode, this.xmlEditor);
                //}
            }

            if (xmlNode is System.Xml.XmlText) return new editor.XMLElement_TextNode(xmlNode, this.xmlEditor);

            if (xmlNode is System.Xml.XmlComment) return new editor.XMLElement_Kommentar(xmlNode, this.xmlEditor);

            return new editor.XMLElement_StandardNode(xmlNode, this.xmlEditor);
        }
    }
}
