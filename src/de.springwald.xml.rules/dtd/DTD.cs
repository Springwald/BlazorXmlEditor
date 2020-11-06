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
        /// This exception is thrown if an element was queried which is not defined in the DTD
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
        /// The elements available in this DTD
        /// </summary>
        public DtdElement[] Elements { get; }

        /// <summary>
        /// The entities available in this DTD
        /// </summary>
        public DtdEntity[] Entities { get; }

        public Dtd(DtdElement[] elements, DtdEntity[] entities)
        {
            this.Elements = elements;
            this.Entities = entities;
            this.elementsByName = new Dictionary<string, DtdElement>();
        }

        /// <summary>
        /// Finds out if an element is known in this DTD
        /// </summary>
        public bool IsDtdElementKnown(string elementName)
        {
            return (DTDElementByNameIntern_(elementName, false) != null);
        }

        /// <summary>
        /// Finds the DTD element corresponding to the specified node
        /// </summary>
        public DtdElement DTDElementByNode_(System.Xml.XmlNode node, bool errorIfNotExists)
        {
            return DTDElementByNameIntern_(GetElementNameFromNode(node), errorIfNotExists);
        }

        /// <summary>
        /// Finds the DTD element corresponding to the specified name
        /// </summary>
        /// <param name="elementName"></param>
        public DtdElement DTDElementByName(string elementName, bool errorIfNotExists)
        {
            return DTDElementByNameIntern_(elementName, errorIfNotExists);
        }

        /// <summary>
        /// Determines the name of the specified node for the reference samples
        /// </summary>
        public static string GetElementNameFromNode(System.Xml.XmlNode node)
        {
            if (node == null) return string.Empty;
            if (node is System.Xml.XmlText) return "#PCDATA";
            if (node is System.Xml.XmlComment) return "#COMMENT";
            if (node is System.Xml.XmlWhitespace) return "#WHITESPACE";
            return node.Name;
        }

        /// <summary>
        /// Finds the DTD element corresponding to the specified name
        /// </summary>
        public DtdElement DTDElementByNameIntern_(string elementName, bool errorIfNotExists)
        {
            if (this.elementsByName.TryGetValue(elementName, out DtdElement elementInBuffer))
            {
                return elementInBuffer;
            }
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
            return null;
        }
    }
}
