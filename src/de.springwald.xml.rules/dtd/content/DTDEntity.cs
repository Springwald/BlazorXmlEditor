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
    /// A single DTD element from a DTD
    /// </summary>
    public class DTDEntity
    {
        /// <summary>
        /// Der eindeutige Name  dieser Entity
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Der Inhalt dieser Entity
        /// </summary>
        public string Inhalt { get; set; }

        /// <summary>
        /// Ist eine eine % - Entity, d.h.enthält nur einen zu ersetzenden String und bleibt nicht unter ihrem Namen als einzufügen bestehen?
        /// </summary>
        public bool IstErsetzungsEntity { get; set; }
    }
}
