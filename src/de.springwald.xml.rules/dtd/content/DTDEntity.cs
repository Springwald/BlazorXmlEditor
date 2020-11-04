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
    public class DtdEntity
    {
        public string Name { get; set; }

        public string Content { get; set; }

        /// <summary>
        /// Is a % - entity, which contains only a string to be replaced and does not remain under its name as to be inserted?
        /// </summary>
        public bool IsReplacementEntity { get; set; }
    }
}
