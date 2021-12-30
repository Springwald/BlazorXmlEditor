// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.editor.xmlelements.TextNode
{
    /// <summary>
    /// Draws a comment in the editor
    /// </summary>
    class XmlElementComment : XmlElementTextNode
    {
        public XmlElementComment(System.Xml.XmlNode xmlNode, XmlEditor xmlEditor, EditorContext editorContext) : base(xmlNode, xmlEditor, editorContext)
        {
        }

        protected override void SetColors()
        {
            base.SetColors();
            this.colorBackground = this.Config.ColorCommentTextBackground;
        }
    }
}
