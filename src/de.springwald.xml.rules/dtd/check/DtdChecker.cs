// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Text;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.rules.dtd
{
    /// <summary>
    /// Checks nodes and attributes etc. within a document to see if they are allowed
    /// </summary>
    public class DtdChecker
    {
        private readonly Dtd dtd;
        private DtdNodeEditCheck nodeChecker;
        private StringBuilder errorMessages;

        private DtdNodeEditCheck NodeChecker
        {
            get
            {
                if (this.nodeChecker == null)
                {
                    this.nodeChecker = new DtdNodeEditCheck(this.dtd);
                }
                return this.nodeChecker;
            }
        }

        public string ErrorMessages
        {
            get { return this.errorMessages.ToString(); }
        }

        /// <summary>
        /// Checks nodes and attributes etc. within a document to see if they are allowed
        /// </summary>
        /// <param name="dtd">The DTD to be checked against</param>
        public DtdChecker(Dtd dtd)
        {
            this.dtd = dtd;
            this.Reset();
        }

        public bool IsXmlAttributOk(System.Xml.XmlAttribute xmlAttribut)
        {
            this.Reset();
            return this.CheckAttribute(xmlAttribut);
        }

        /// <summary>
        /// Checks if the passed XML object is ok according to the given DTD. 
        /// </summary>
        /// <param name="posBereitsGeprueft">Is it already known for this node whether it is allowed to stand where it stands?</param>
        public bool IsXmlNodeOk(System.Xml.XmlNode xmlNode, bool posAlreadyCheckedAsOk)
        {
            this.Reset();
            if (posAlreadyCheckedAsOk)
            {
                return true;
            }
            else
            {
                return this.CheckNodePos(xmlNode);
            }
        }

        private bool CheckNodePos(System.Xml.XmlNode node)
        {
            // Comment is always ok
            //if (node is System.Xml.XmlComment) return true;
            // Whitespace is always ok
            if (node is System.Xml.XmlWhitespace) return true;

            if (dtd.IsDtdElementKnown(Dtd.GetElementNameFromNode(node)))// Das Element dieses Nodes ist in der DTD bekannt 
            {
                try
                {
                    if (this.NodeChecker.IsTheNodeAllowedAtThisPos(node))
                    {
                        return true;
                    }
                    else
                    {
                        // "Tag '{0}' hier nicht erlaubt: "
                        errorMessages.AppendFormat($"Tag '{node.Name}' not allowed here.");
                        var pos = new XmlCursorPos();
                        pos.SetPos(node, XmlCursorPositions.CursorOnNodeStartTag);
                        var allowedTags = this.NodeChecker.AtThisPosAllowedTags(pos, false, false); // what is allowed at this position?
                        if (allowedTags.Length > 0)
                        {
                            errorMessages.Append("At this position allowed:");
                            foreach (string tag in allowedTags)
                            {
                                errorMessages.AppendFormat("{0} ", tag);
                            }
                        }
                        else
                        {
                            errorMessages.Append("No tags are allowed at this point. Probably the parent tag is already invalid.");
                        }
                        return false;
                    }
                }
                catch (Dtd.XMLUnknownElementException e)
                {
                    errorMessages.AppendFormat($"unknown element '{e.ElementName}'");
                    return false;
                }
            }
            else // The element of this node is not known in the DTD
            {
                errorMessages.AppendFormat($"unknown element '{Dtd.GetElementNameFromNode(node)}'");
                return false;
            }
        }

        private bool CheckAttribute(System.Xml.XmlAttribute attribut)
        {
            return false;
        }

        private void Reset()
        {
            this.errorMessages = new StringBuilder();
        }
    }
}
