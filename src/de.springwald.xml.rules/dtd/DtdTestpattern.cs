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
        private string _parentElementName;	    // Dieses Element liegt über der zu testenden Cursor Pos (Zeichnung:C)

        private string _vergleichsStringFuerRegEx;

        private StringBuilder _elementNamenListe;

        /// <summary>
        /// Das zum Test eingefügte Element. Ist es NULL, bedeutet das, dass statt Einfügen das Löschen geprüft wurde
        /// </summary>
        public string ElementName { get; }

        public string VergleichStringFuerRegEx
        {
            get
            {
                if (_vergleichsStringFuerRegEx == null)
                {
                    _elementNamenListe.Append("<");
                    _vergleichsStringFuerRegEx = _elementNamenListe.ToString();
                }
                return _vergleichsStringFuerRegEx;
            }
        }

        /// <summary>
        /// Eine schriftliche Zusammenfassung dieses Musters
        /// </summary>
        public string Zusammenfassung
        {
            get
            {
                StringBuilder ergebnis = new StringBuilder();

                // Erfolgreich getestet?
                if (this.Erfolgreich)
                {
                    ergebnis.Append("+ ");
                }
                else
                {
                    ergebnis.Append("- ");
                }

                // Der Name des ParentNodes
                ergebnis.Append(this._parentElementName);
                ergebnis.Append(" (");
                ergebnis.Append(VergleichStringFuerRegEx);
                ergebnis.Append(")");

                // Was wurde getestet?
                if (this.ElementName == null)
                {
                    ergebnis.Append(" [getestet: löschen]");
                }
                else
                {
                    ergebnis.AppendFormat("[getestet: {0}]", this.ElementName);
                }

                return ergebnis.ToString();
            }
        }

        /// <summary>
        /// Ist das Muster erfolgreich anwendbar gewesen?
        /// </summary>
        public bool Erfolgreich { get; set; }

        /// <param name="element">Das zum Test eingefügte Element. Ist es NULL, bedeutet das, dass statt Einfügen das Löschen geprüft wurde</param>
        /// <param name="parentElementName">Dieses Element liegt über der zu testenden Cursor Pos (Zeichnung:C)</param>
        public DtdTestpattern(string elementName, string parentElementName)
        {
            _elementNamenListe = new StringBuilder();
            _elementNamenListe.Append(">");

            this.ElementName = elementName;
            this._parentElementName = parentElementName;
            this.Erfolgreich = false; // Bisher nicht bestätigt
        }

        public void AddElement(string elementName)
        {
            _elementNamenListe.AppendFormat("-{0}", elementName);
        }
    }
}
