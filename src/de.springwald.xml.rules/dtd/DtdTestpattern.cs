// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Text;

namespace de.springwald.xml.rules.dtd
{
    /// <summary>
    /// An XML block of what it might look like after the intended change. This pattern is chased through the validator. All patterns, which were subsequently flagged as "confirmed", are permitted according to the DTD
    /// </summary>
    public class DtdTestpattern
    {
        private string parentElementName;	    // Dieses Element liegt über der zu testenden Cursor Pos (Zeichnung:C)

        private string compareStringForRegEx;

        private StringBuilder elementNameList;

        /// <summary>
        /// Das zum Test eingefügte Element. Ist es NULL, bedeutet das, dass statt Einfügen das Löschen geprüft wurde
        /// </summary>
        public string ElementName { get; }

        public string CompareStringForRegEx
        {
            get
            {
                if (compareStringForRegEx == null)
                {
                    elementNameList.Append("<");
                    compareStringForRegEx = elementNameList.ToString();
                }
                return compareStringForRegEx;
            }
        }

        /// <summary>
        /// Eine schriftliche Zusammenfassung dieses Musters
        /// </summary>
        public string Summary
        {
            get
            {
                var result = new StringBuilder();

                // Erfolgreich getestet?
                if (this.Success)
                {
                    result.Append("+ ");
                }
                else
                {
                    result.Append("- ");
                }

                // Der Name des ParentNodes
                result.Append(this.parentElementName);
                result.Append(" (");
                result.Append(CompareStringForRegEx);
                result.Append(")");

                // Was wurde getestet?
                if (this.ElementName == null)
                {
                    result.Append(" [getestet: löschen]");
                }
                else
                {
                    result.AppendFormat("[getestet: {0}]", this.ElementName);
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Ist das Muster erfolgreich anwendbar gewesen?
        /// </summary>
        public bool Success { get; set; }

        /// <param name="element">Das zum Test eingefügte Element. Ist es NULL, bedeutet das, dass statt Einfügen das Löschen geprüft wurde</param>
        /// <param name="parentElementName">Dieses Element liegt über der zu testenden Cursor Pos (Zeichnung:C)</param>
        public DtdTestpattern(string elementName, string parentElementName)
        {
            elementNameList = new StringBuilder();
            elementNameList.Append(">");

            this.ElementName = elementName;
            this.parentElementName = parentElementName;
            this.Success = false; // Bisher nicht bestätigt
        }

        public void AddElement(string elementName)
        {
            elementNameList.AppendFormat("-{0}", elementName);
        }
    }
}
