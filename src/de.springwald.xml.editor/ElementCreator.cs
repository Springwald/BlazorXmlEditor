// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor;
using de.springwald.xml.editor.xmlelements.TextNode;

namespace de.springwald.xml
{
    class ElementCreator
    {
        private EditorContext editorContext;
        private XmlEditor xmlEditor;

        public ElementCreator(XmlEditor xmlEditor, EditorContext editorContext)
        {
            this.editorContext = editorContext;
            this.xmlEditor = xmlEditor;
        }
        /// <summary>
        /// Stellt das für den angegebenen Node optimale XML-Steuerelement bereit.
        /// Diese Methode kann in vererbten Regelwerken überschrieben werden, wenn
        /// spezielle Nodes eigene XML-Steuerelemente erfordern.
        /// In dieser Grundkonfiguration gibt die Methode erst einmal nur das Standard-Steuerelement zurück
        /// </summary>
        /// <param name="XMLNode">Der anzuzeigende Node</param>
        public XmlElement CreatePaintElementForNode(System.Xml.XmlNode xmlNode) //, de.springwald.xml.XMLEditorPaintPos paintPos ) 
        {
            if (xmlNode is System.Xml.XmlElement) return new XMLElement_StandardNode(xmlNode, this.xmlEditor, this.editorContext);
            if (xmlNode is System.Xml.XmlText) return new XMLElement_TextNode(xmlNode, this.xmlEditor, this.editorContext);
            if (xmlNode is System.Xml.XmlComment) return new XMLElement_Kommentar(xmlNode, this.xmlEditor, this.editorContext);
            return new XMLElement_StandardNode(xmlNode, this.xmlEditor, this.editorContext);
        }
    }
}
