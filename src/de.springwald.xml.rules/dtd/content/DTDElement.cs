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
using System.Text.RegularExpressions;

namespace de.springwald.xml.rules.dtd
{
    /// <summary>
    /// A single DTD element from a DTD
    /// </summary>
    public class DtdElement
    {
        private Regex _childrenRegExObjekt;         // Returns a RegEx object which can be used to check if a sequence of images is valid for this element
        private string[] allChildNamesAllowedAsDirectChild; // These DTD elements may occur within this element

        /// <summary>
        /// The unique name of this element
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The child elements of this element
        /// </summary>
        public DtdChildElements ChildElements { get; set; }

        /// <summary>
        /// These DTD elements may occur within this element.
        /// </summary>
        public string[] AllChildNamesAllowedAsDirectChild
        {
            get
            {
                if (this.allChildNamesAllowedAsDirectChild == null)
                {
                    this.allChildNamesAllowedAsDirectChild = GetDtdElementNamesFromChildElements(this.ChildElements).Distinct().ToArray();
                }
                return this.allChildNamesAllowedAsDirectChild;
            }
        }

        /// <summary>
        /// The attributes known for this element
        /// </summary>
        public DtdAttribute[] Attributes { get; set; }

        /// <summary>
        /// Returns a RegEx object which can be used to check if a sequence of images is valid for this element
        /// </summary>
        public Regex ChildrenRegExObjekt
        {
            get
            {
                if (_childrenRegExObjekt == null)
                {
                    _childrenRegExObjekt = new Regex($">{this.ChildElements.RegExAusdruck}<");// RegexOptions.Compiled);
                }
                return _childrenRegExObjekt;
            }
        }

        /// <summary>
        /// Returns the list of all elements mentioned in the Children
        /// </summary>
        private IEnumerable<string> GetDtdElementNamesFromChildElements(DtdChildElements children)
        {
            switch (children.ElementType)
            {
                case DtdChildElements.DtdChildElementTypes.SingleChild:
                    // is a single ChildElement
                    yield return children.ElementName;
                    break;

                case DtdChildElements.DtdChildElementTypes.ChildList:
                    for (int i = 0; i < children.ChildrenCount; i++)
                    {
                        foreach (string childElementName in this.GetDtdElementNamesFromChildElements(children.Child(i)))
                        {
                            yield return childElementName;
                        }
                    }
                    break;

                case DtdChildElements.DtdChildElementTypes.Empty:
                    break;

                default:
                    throw new ApplicationException($"unknown DTDChildElementType {children.ElementType}" );
            }

            yield return "#COMMENT";     //Add the comment tag, as this is always allowed
        }


    }
}
