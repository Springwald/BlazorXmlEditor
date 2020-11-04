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

namespace de.springwald.xml.rules.dtd
{
    public class Dtd
    {
        /// <summary>
        /// Diese Ausnahme wird geworfen, wenn ein Element erfragt wurde, welches in der DTD nicht definiert ist
        /// </summary>
        public class XMLUnknownElementException : Exception
        {
            public string ElementName { get; }

            public XMLUnknownElementException(string elementname)
            {
                this.ElementName = elementname;
            }
        }

        private Dictionary<string, DtdElement> elementsByName;

        /// <summary>
        /// Die in dieser DTD verfügbaren Elemente
        /// </summary>
        public List<DtdElement> Elements { get; }

        /// <summary>
        /// Die in dieser DTD verfügbaren Entities
        /// </summary>
        public List<DtdEntity> Entities { get; }

        public Dtd(List<DtdElement> elements, List<DtdEntity> entities)
        {
            this.Elements = elements;
            this.Entities = entities;
            this.elementsByName = new Dictionary<string, DtdElement>();
        }

        /// <summary>
        /// Findet heraus, ob ein Element in dieser DTD bekannt ist
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public bool IsDtdElementKnown(string elementName)
        {
            return (DTDElementByNameIntern_(elementName, false) != null);
        }

        /// <summary>
        /// Findet das dem angegebenen Node entsprechende DTD-Element
        /// </summary>
        /// <param name="elementName"></param>
        public DtdElement DTDElementByNode_(System.Xml.XmlNode node, bool errorIfNotExists)
        {
            return DTDElementByNameIntern_(GetElementNameFromNode(node), errorIfNotExists);
        }

        /// <summary>
        /// Findet das dem angegebenen Namen entsprechende DTD-Element
        /// </summary>
        /// <param name="elementName"></param>
        public DtdElement DTDElementByName(string elementName, bool errorIfNotExists)
        {
            return DTDElementByNameIntern_(elementName, errorIfNotExists);
        }

        /// <summary>
        /// Ermittelt für die Vergleichsmuster den Namen des angegebenen Nodes
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetElementNameFromNode(System.Xml.XmlNode node)
        {
            if (node == null) return "";

            //if (node.Name == "#text") 
            if (node is System.Xml.XmlText)
            {
                return "#PCDATA";
            }
            else
            {
                if (node is System.Xml.XmlComment)
                {
                    return "#COMMENT";
                }
                else
                {
                    if (node is System.Xml.XmlWhitespace)
                    {
                        return "#WHITESPACE";
                    }
                    else
                    {
                        return node.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Findet das dem angegebenen Namen entsprechende DTD-Element
        /// </summary>
        /// <param name="elementName"></param>
        public DtdElement DTDElementByNameIntern_(string elementName, bool errorIfNotExists)
        {
            if (this.elementsByName.TryGetValue(elementName, out DtdElement elementInBuffer))
            {
                return elementInBuffer;
            }
            else
            {
                foreach (var element in this.Elements)
                {
                    if (elementName == element.Name)
                    {
                        elementsByName.Add(elementName, element);
                        return element;
                    }
                }
                if (errorIfNotExists)
                {
                    // Das gesuchte DTD-Element mit diesem Namen existiert nicht in dieser DTD.
                    throw new XMLUnknownElementException(elementName);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
