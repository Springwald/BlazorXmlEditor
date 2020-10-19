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
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace de.springwald.xml.dtd
{
    /// <summary>
    /// A single DTD element from a DTD
    /// </summary>
    public class DTDElement
    {
        private Regex _childrenRegExObjekt;         // Liefert ein RegEx-Objekt, mit welchem man Childfolgen darauf hin prüfen kann, ob sie für dieses Element gültig sind
        private StringCollection _alleElementNamenWelcheAlsDirektesChildZulaessigSind; // Diese DTD-Elemente dürfen innerhalb dieses Elementes vorkommen

        /// <summary>
        /// Der eindeutige Name dieses Elementes
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Die Child-Elemente dieses Elementes
        /// </summary>
        public DTDChildElemente ChildElemente { get; set; }

        /// <summary>
        /// Diese DTD-Elemente dürfen innerhalb dieses Elementes vorkommen.
        /// </summary>
        public StringCollection AlleElementNamenWelcheAlsDirektesChildZulaessigSind
        {
            get
            {
                if (_alleElementNamenWelcheAlsDirektesChildZulaessigSind == null)
                {
                    _alleElementNamenWelcheAlsDirektesChildZulaessigSind = GetDTDElementeNamenAusChildElementen_(this.ChildElemente);
                    // Das Kommentar-Tag hinzufügen, da dieses immer zulässig ist
                    _alleElementNamenWelcheAlsDirektesChildZulaessigSind.Add("#COMMENT");
                }
                return _alleElementNamenWelcheAlsDirektesChildZulaessigSind;
            }
        }

        /// <summary>
        /// Die für dieses Element bekannten Attribute
        /// </summary>
        public List<DTDAttribut> Attribute { get; set; }

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
        /// Erzeugt ein DTDElement auf Basis des übergebenen DTD-Element-Quellcodes
        /// </summary>
        public DTDElement()
        {
        }



        /// <summary>
        /// Liefert die Liste aller in den Children erwähnten Elemente
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        private StringCollection GetDTDElementeNamenAusChildElementen_(DTDChildElemente children)
        {
            StringCollection liste = new StringCollection();

            switch (children.Art)
            {
                case DTDChildElemente.DTDChildElementArten.EinzelChild:
                    // Ist ein einzelnes ChildElement und noch nicht in der Liste: Hinzufügen
                    if (!liste.Contains(children.ElementName))
                    {
                        liste.Add(children.ElementName);
                    }
                    break;

                case DTDChildElemente.DTDChildElementArten.ChildListe:
                    for (int i = 0; i < children.AnzahlChildren; i++)
                    {
                        foreach (string childElementName in GetDTDElementeNamenAusChildElementen_(children.Child(i)))
                        {
                            if (!liste.Contains(childElementName))
                            {
                                liste.Add(childElementName);
                            }
                        }
                    }
                    break;

                case DTDChildElemente.DTDChildElementArten.Leer:
                    break;

                default:
                    // "Unbekannte DTDChildElementArt {0}"
                    throw new ApplicationException(String.Format(ResReader.Reader.GetString("UnbekannteDTDChildElementArt"), children.Art));
            }

            return liste;
        }


    }
}
