// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Draws a comment in the editor
    /// </summary>
    class XMLElement_Kommentar : XMLElement_TextNode
    {
        public XMLElement_Kommentar(System.Xml.XmlNode xmlNode, XMLEditor xmlEditor) : base(xmlNode, xmlEditor)
        {
        }

        /// <summary>
        /// Switches the foreground and background colors to display the selected node
        /// </summary>
        protected override void FarbenSetzen()
        {
            // Define the colors for "not inverted"
            _farbeHintergrund_ = Color.LightGray;
            _drawBrush_ = Color.Black;

            // Define the colors for "inverted"
            _farbeHintergrundInvertiert_ = Color.Black;
            _drawBrushInvertiert_ = Color.Gray;

            // Define the colors for "weak inverted"
            _farbeHintergrundInvertiertOhneFokus_ = Color.Gray;
            _drawBrushInvertiertOhneFokus_ = Color.LightGray;	
        }
    }
}
