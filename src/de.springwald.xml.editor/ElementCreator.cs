// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.editor.xmlelements.TextNode;

namespace de.springwald.xml
{
    class ElementCreator
    {
        private XMLRegelwerk regelwerk;
        private editor.XMLEditor xmlEditor;

        public ElementCreator(editor.XMLEditor xmlEditor, XMLRegelwerk regelwerk)
        {
            this.regelwerk = regelwerk;
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
        public editor.XMLElement CreatePaintElementForNode(System.Xml.XmlNode xmlNode) //, de.springwald.xml.XMLEditorPaintPos paintPos ) 
        {
            if (xmlNode is System.Xml.XmlElement)
            {
                //switch (xmlNode.Name)
                //{
                //    default: 
                return new editor.XMLElement_StandardNode(xmlNode, this.xmlEditor, regelwerk);
                //}
            }

            if (xmlNode is System.Xml.XmlText) return new XMLElement_TextNode(xmlNode, this.xmlEditor, this.regelwerk);

            if (xmlNode is System.Xml.XmlComment) return new XMLElement_Kommentar(xmlNode, this.xmlEditor, regelwerk);

            return new editor.XMLElement_StandardNode(xmlNode, this.xmlEditor, regelwerk);
        }
    }
}
