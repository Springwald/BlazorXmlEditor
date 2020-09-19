using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Zeichnet einen Kommentar im Editor
    /// </summary>
    /// <remarks>
    /// (C)2005 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    class XMLElement_Kommentar : XMLElement_TextNode
    {
        public XMLElement_Kommentar(System.Xml.XmlNode xmlNode, de.springwald.xml.editor.XMLEditor xmlEditor) : base(xmlNode, xmlEditor)
        {
        }

        /// <summary>
        /// Vertauscht die Vorder- und Hintergrundfarben, um den Node selektiert darstellen zu können
        /// </summary>
        protected override void FarbenSetzen()
        {
            // Die Farben für "nicht invertiert" definieren
            _farbeHintergrund_ = Color.LightGray;
            _drawBrush_ = new SolidBrush(Color.Black);	// Schrift-Pinsel bereitstellen;

            // Die Farben für "invertiert" definieren
            _farbeHintergrundInvertiert_ = Color.Black;
            _drawBrushInvertiert_ = new SolidBrush(Color.Gray);	// Schrift-Pinsel bereitstellen;

            // Die Farben für schwach "invertiert" definieren
            _farbeHintergrundInvertiertOhneFokus_ = Color.Gray;
            _drawBrushInvertiertOhneFokus_ = new SolidBrush(Color.LightGray);	// Schrift-Pinsel bereitstellen;
        }


    }
}
