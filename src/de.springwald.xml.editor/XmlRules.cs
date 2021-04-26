// A platform independent tag-view-style graphical xml editor
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
using de.springwald.xml.editor;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.rules;
using de.springwald.xml.rules.dtd;

namespace de.springwald.xml
{
    /// <summary>
    /// An XML element in the editor can assume these types of representation
    /// </summary>
	public enum DisplayTypes { FloatingElement = 1, OwnRow };

    public class XmlRules
    {
        private DtdChecker dtdChecker;
        private DtdNodeEditCheck dtdNodeEditChecker;
        protected List<XmlElementGroup> elementGroups;

        public DtdChecker DtdChecker
        {
            get
            {
                if (dtdChecker == null)
                {
                    if (this.Dtd == null)
                    {
                        throw new ApplicationException("No DTD attached!");
                    }
                    dtdChecker = new DtdChecker(this.Dtd);
                }
                return dtdChecker;
            }
        }

        public Dtd Dtd { get; }

        /// <summary>
        ///  The groups in which XML elements can be grouped and suggested for insertion
        /// </summary>
        public virtual List<XmlElementGroup> ElementGroups
        {
            get
            {
                if (elementGroups == null)
                {
                    elementGroups = new List<XmlElementGroup>();
                }
                return elementGroups;
            }
        }

        public XmlRules(Dtd dtd)
        {
            this.Dtd = dtd;
        }

        /// <summary>
        /// Returns the color in which this node should be drawn
        /// </summary>
        public virtual Color NodeColor(System.Xml.XmlNode node)
        {
            return Color.LightBlue;
        }

        /// <summary>
        /// In what way should the passed node be drawn?
        /// </summary>
        public virtual DisplayTypes DisplayType(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode is System.Xml.XmlText) return DisplayTypes.FloatingElement;
            if (xmlNode is System.Xml.XmlWhitespace) return DisplayTypes.FloatingElement;
            if (xmlNode is System.Xml.XmlComment) return DisplayTypes.OwnRow;
            if (HasEndTag(xmlNode)) return DisplayTypes.OwnRow;
            return DisplayTypes.FloatingElement;
        }

        /// <summary>
        /// Is the passed node drawn twice, once with > and once with < ?
        /// </summary>
        public virtual bool HasEndTag(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode is System.Xml.XmlText) return false;
            var element = this.Dtd.DTDElementByNode_(xmlNode, false);
            if (element == null) return true;
            if (element.AllChildNamesAllowedAsDirectChild.Length > 1) return true;// The element can have sub elements (> 1 instead of 0, because comment is always included)
            return false;  // The element cannot have any sub elements
        }

        /// <summary>
        /// Returns if the specified tag is allowed at this position
        /// </summary>
        public bool IsThisTagAllowedAtThisPos(string tagName, XmlCursorPos targetPos)
        {
            return this.AllowedInsertElements(targetPos, true, true).Contains(tagName);
        }

        /// <summary>
        /// Defines which XML elements may be inserted at this position
        /// </summary>
        /// <param name="alsoListPpcData">if true, PCDATA is also listed as a node, if it is allowed</param>
        /// <returns>A list of the node names. Zero means, no elements are allowed.
        /// If the content is "", then the element must be entered freely </returns>
        public virtual string[] AllowedInsertElements(XmlCursorPos targetPos, bool alsoListPpcData, bool alsoListComments)
        {
#warning evtl. Optimierungs-TODO:
            // Wahrscheinlich (allein schon durch die Nutzung von IstDiesesTagAnDieserStelleErlaubt() etc.)
            // wird diese Liste oft hintereinander identisch neu erzeugt. Es macht daher Sinn, wenn der
            // das letzte Ergebnis hier ggf. gebuffert würde. Dabei sollte aber ausgeschlossen werden, dass
            // sich der XML-Inhalt in der Zwischenzeit geändert hat!

            if (targetPos.ActualNode == null) return new string[] { }; //  If nothing is selected, nothing is allowed

            if (this.Dtd == null) return new string[] { string.Empty }; // Free input allowed
            if (dtdNodeEditChecker == null)
            {
                dtdNodeEditChecker = new DtdNodeEditCheck(this.Dtd);
            }
            return dtdNodeEditChecker.AtThisPosAllowedTags(targetPos, alsoListPpcData, alsoListComments);
        }

        /// <summary>
        /// Converts / formats text to be inserted in a specific location as required by that location. 
        /// In an AIML DTD, for example, this can mean that the text is converted to upper case for insertion into the PATTERN tag.
        /// </summary>
        /// <param name="replacementNode">If a node is to be inserted instead of the text. Example: In the AIML-Template we press *, then a STAR-Tag is inserted</param>
        public virtual string InsertTextTextPreProcessing(string textToInsert, XmlCursorPos insertWhere, out System.Xml.XmlNode replacementNode)
        {
            replacementNode = null;
            return textToInsert; // In the standard form the text always goes through
        }


    }
}
