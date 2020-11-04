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
        public enum MandatoryTypes { Mandatory, Optional, Constant };

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Is the attribute mandatory?
        /// </summary>
        public MandatoryTypes Mandatory { get; set; } = MandatoryTypes.Optional;

        /// <summary>
        /// The values that are allowed for this attribute. NULL=No default, everything is allowed
        /// </summary>
        public string[] AllowedValues { get; set; } = new string[] { };

        public string StandardValue { get; set; } = string.Empty;

        public string Type { set; get; }
    }
}
