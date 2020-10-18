// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.editor.editor.xmlelements.TextNode
{
    /// <summary>
    /// Draws a comment in the editor
    /// </summary>
    class XMLElement_Kommentar : XMLElement_TextNode
    {
        public XMLElement_Kommentar(System.Xml.XmlNode xmlNode, XMLEditor xmlEditor) : base(xmlNode, xmlEditor)
        {
        }

        protected override void SetColors()
        {
            base.SetColors();
            this.colorBackground = this.config.ColorCommentTextBackground;
        }
    }
}
