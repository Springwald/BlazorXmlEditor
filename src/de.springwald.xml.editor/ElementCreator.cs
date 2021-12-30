// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor;
using de.springwald.xml.editor.xmlelements.TextNode;

namespace de.springwald.xml
{
    internal class ElementCreator
    {
        private readonly EditorContext editorContext;
        private readonly XmlEditor xmlEditor;

        public ElementCreator(XmlEditor xmlEditor, EditorContext editorContext)
        {
            this.editorContext = editorContext;
            this.xmlEditor = xmlEditor;
        }
        /// <summary>
        /// Created the optimal XML control for the specified node.
        /// This method can be overridden in inherited rule sets if special nodes require their own XML controls.
        /// In this basic configuration, the method initially returns only the standard paint object
        /// </summary>
        public XmlElement CreatePaintElementForNode(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode is System.Xml.XmlElement) return new XmlElementStandardNode(xmlNode, this.xmlEditor, this.editorContext);
            if (xmlNode is System.Xml.XmlText) return new XmlElementTextNode(xmlNode, this.xmlEditor, this.editorContext);
            if (xmlNode is System.Xml.XmlComment) return new XmlElementComment(xmlNode, this.xmlEditor, this.editorContext);
            return new XmlElementStandardNode(xmlNode, this.xmlEditor, this.editorContext);
        }
    }
}
