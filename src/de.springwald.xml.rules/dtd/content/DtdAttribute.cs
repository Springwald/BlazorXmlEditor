// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.rules.dtd
{
    /// <summary>
    /// A single DTD attribute from a DTD
    /// </summary>
    public class DtdAttribute
    {
        public enum PflichtArten { Pflicht, Optional, Konstante };

        /// <summary>
        /// Der Name des Attributes
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Ist das Attribut Pflicht?
        /// </summary>
        public PflichtArten Pflicht { get; set; } = PflichtArten.Optional;

        /// <summary>
        /// Die Werte, welche für dieses Attribut erlaubt sind. NULL=Keine Vorgabe, alles ist erlaubt
        /// </summary>
        public string[] ErlaubteWerte { get; set; } = new string[] { };

        /// <summary>
        /// Der Vorgabewert
        /// </summary>
        public string StandardWert { get; set; } = string.Empty;

        /// <summary>
        /// Der Typ des Attributes
        /// </summary>
        public string Typ { set; get; }
    }
}
