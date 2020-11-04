// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace de.springwald.xml.rules.dtd
{
    /// <summary>
    /// A single DTD element from a DTD
    /// </summary>
    public class DtdElement
    {
        private Regex _childrenRegExObjekt;         // Liefert ein RegEx-Objekt, mit welchem man Childfolgen darauf hin prüfen kann, ob sie für dieses Element gültig sind
        private string[] _alleElementNamenWelcheAlsDirektesChildZulaessigSind; // Diese DTD-Elemente dürfen innerhalb dieses Elementes vorkommen

        /// <summary>
        /// Der eindeutige Name dieses Elementes
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Die Child-Elemente dieses Elementes
        /// </summary>
        public DtdChildElements ChildElemente { get; set; }

        /// <summary>
        /// Diese DTD-Elemente dürfen innerhalb dieses Elementes vorkommen.
        /// </summary>
        public string[] AllChildNamesAllowedAsDirectChild
        {
            get
            {
                if (_alleElementNamenWelcheAlsDirektesChildZulaessigSind == null)
                {
                    _alleElementNamenWelcheAlsDirektesChildZulaessigSind = GetDTDElementeNamenAusChildElementen_(this.ChildElemente).Distinct().ToArray();
                }
                return _alleElementNamenWelcheAlsDirektesChildZulaessigSind;
            }
        }

        /// <summary>
        /// Die für dieses Element bekannten Attribute
        /// </summary>
        public List<DtdAttribute> Attribute { get; set; }

        /// <summary>
        /// Liefert ein RegEx-Objekt, mit welchem man Childfolgen darauf hin prüfen kann, ob sie für dieses
        /// Element gültig sind
        /// </summary>
        public Regex ChildrenRegExObjekt
        {
            get
            {
                if (_childrenRegExObjekt == null)
                {
                    StringBuilder ausdruck = new StringBuilder();
                    ausdruck.Append(">");
                    ausdruck.Append(this.ChildElemente.RegExAusdruck);
                    ausdruck.Append("<");
                    _childrenRegExObjekt = new Regex(ausdruck.ToString());// RegexOptions.Compiled);
                }
                return _childrenRegExObjekt;
            }
        }

        /// <summary>
        /// Liefert die Liste aller in den Children erwähnten Elemente
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        private IEnumerable<string> GetDTDElementeNamenAusChildElementen_(DtdChildElements children)
        {
            switch (children.ElementType)
            {
                case DtdChildElements.DtdChildElementTypes.SingleChild:
                    // Ist ein einzelnes ChildElement
                    yield return children.ElementName;
                    break;

                case DtdChildElements.DtdChildElementTypes.ChildList:
                    for (int i = 0; i < children.ChildrenCount; i++)
                    {
                        foreach (string childElementName in GetDTDElementeNamenAusChildElementen_(children.Child(i)))
                        {
                            yield return childElementName;
                        }
                    }
                    break;

                case DtdChildElements.DtdChildElementTypes.Empty:
                    break;

                default:
                    // "Unbekannte DTDChildElementArt {0}"
                    throw new ApplicationException($"Unbekannte DTDChildElementArt {children.ElementType}" );
            }

            yield return "#COMMENT";     // Das Kommentar-Tag hinzufügen, da dieses immer zulässig ist
        }


    }
}
