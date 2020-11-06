// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Text;
using de.springwald.xml.rules.dtd;

namespace de.springwald.xml
{
    public class XmlSourcecodeAsRtf
    {
        /// <summary>
        /// define event call when a node of the Dock is checked (for status display, so you don't think the program hangs)
        /// </summary>
        public event EventHandler CheckingNodeEvent;

        /// <summary>
        /// define event call when a node of the Dock is checked (for status display, so you don't think the program hangs)
        /// </summary>
        protected virtual void ActiveNodeIsBeeningChecked(EventArgs e)
        {
            this.CheckingNodeEvent?.Invoke(this, e);
        }

        private XmlRules rules;
        private bool showRowNumbers = true;

        private int rowNumber;                          // The counter for the current line number of the source code

        private System.Xml.XmlNode rootNode;

        private StringBuilder errorLogAsText;
        private StringBuilder sourcecodeAsRtf;

        private string _rtf_Header = "{\\rtf1\\ansi\\deff0" + "\r\n" +                                  // Rtf-Header
            "{\\colortbl;\\red0\\green0\\blue0;" +                              // farbe1=schwarz
            "\\red255\\green0\\blue0;" +                                        // farbe2=rot
            "\\red200\\green200\\blue200;}";                                    // farbe3=grau

        private string _rtf_Footer = "\r\n}";

        private enum RtfColors { black, red, gray }; // enumeration of the available format switches

        private string _rtf_LineBreak = "\\line" + "\r\n";
        private string _rtf_ColorBlack = "\\cf1" + "\r\n";
        private string _rtf_ColorRed = "\\cf2" + "\r\n";
        private string _rtf_ColorGray = "\\cf3" + "\r\n";

        private bool notRenderedJet = true;

        public XmlRules Rules
        {
            set { rules = value; }
        }

        public System.Xml.XmlNode Rootnode
        {
            set
            {
                rootNode = value;
                notRenderedJet = true;
            }
        }

        public void SetChanged()
        {
            notRenderedJet = true;
        }

        public void Render()
        {
            RenderInternal();
        }

        public string ErrorLogAsText
        {
            get
            {
                if (this.notRenderedJet)
                {
                    this.RenderInternal();
                }
                return this.errorLogAsText.ToString();
            }
        }

        public string SourcecodeAsRtf
        {
            get
            {
                if (this.notRenderedJet)
                {
                    this.RenderInternal();
                }
                return this.sourcecodeAsRtf.ToString();
            }
        }

        public XmlSourcecodeAsRtf()
        {
        }


        /// <summary>
        /// Recalculate the source code + error log
        /// </summary>
        private void RenderInternal()
        {
            this.sourcecodeAsRtf = new StringBuilder();
            this.errorLogAsText = new StringBuilder();

            if (rules == null)
            {
                this.errorLogAsText.Append("no xml rules attached");
            }
            else
            {
                if (rootNode == null)
                {
                    this.errorLogAsText.Append("no root node attached");
                }
                else
                {
                    bool nodeHasErrors = false;
                    this.rowNumber = 0;
                    sourcecodeAsRtf.Append(_rtf_Header);
                    sourcecodeAsRtf.AppendFormat("{0}\r\n", GetNodeAsSourceText(rootNode, "", false, false, false, ref nodeHasErrors));
                    sourcecodeAsRtf.Append(_rtf_Footer);
                    notRenderedJet = false;
                }
            }
        }

        /// <summary>
        /// draw a xml node
        /// </summary>
        /// <param name="parentHasErrors">If True, then already the parent node was so faulty that this node does not need to be checked against DTD errors</param>
        /// <returns></returns>
        private string GetNodeAsSourceText(System.Xml.XmlNode node, string indent, bool needNewRow, bool parentHasErrors, bool posAlreadyCheckedAsOk, ref bool nodeHasError)
        {
            // Notify a possible external status display that a node is being checked
            ActiveNodeIsBeeningChecked(EventArgs.Empty);

            // handle comments and whitepace special
            if (node is System.Xml.XmlWhitespace)
            {
                return string.Empty; // dont show Whitespace 
            }
            if (node is System.Xml.XmlComment) return $"<!--{node.InnerText}-->";

            var quellcode = new StringBuilder();
            string indentPlus = "    ";
            string nodeColor;

            string nodeErrorMessage;

            if (parentHasErrors)
            {
                //
                nodeHasError = true;
                nodeErrorMessage = null; //"parent-Node bereits fehlerhaft";
                nodeColor = ""; //RTFFarbe(RtfFarben.schwarz); 
                                //nodeFarbe = RTFFarbe(RtfFarben.rot); 
            }
            else
            {
                var checker = rules.DtdChecker;
                if (checker.IsXmlNodeOk(node, posAlreadyCheckedAsOk))
                {
                    nodeHasError = false;
                    nodeErrorMessage = null;
                    nodeColor = RtfColor(RtfColors.black);
                }
                else
                {
                    nodeHasError = true;
                    nodeErrorMessage = checker.ErrorMessages;
                    nodeColor = RtfColor(RtfColors.red);
                }
            }

            if (node is System.Xml.XmlText)
            {
                if (needNewRow) quellcode.Append($"{GetNewLine()}{indent}");
                quellcode.Append(nodeColor);
                StringBuilder nodetext = new StringBuilder(node.Value);
                nodetext.Replace("\t", " ");
                nodetext.Replace("\r\n", " ");
                nodetext.Replace("  ", " ");
                quellcode.Append(nodetext.ToString());
                nodetext = null;
            }
            else  // not a  text node
            {

                // draw the node itself
                if (rules.HasEndTag(node))
                {
                    switch (rules.DisplayType(node))
                    {
                        case DisplayTypes.OwnRow:

                            quellcode.Append(GetNewLine());
                            quellcode.Append(nodeColor);
                            quellcode.Append(indent);
                            quellcode.Append("<{node.Name}{GetAttributesAsSourcecode(node.Attributes)}>");

                            if (nodeErrorMessage != null)
                            {
                                this.errorLogAsText.Append($"{GetRowNumberString(this.rowNumber)}: {nodeErrorMessage}\r\n");
                            }

                            // draw children
                            quellcode.Append(GetChildrenAsSourcecode(node.ChildNodes, indent + indentPlus, true, nodeHasError, false));

                            // close node
                            quellcode.Append(GetNewLine());
                            quellcode.Append(nodeColor);
                            quellcode.Append(indent);
                            quellcode.Append($"</{node.Name}>");

                            break;

                        case DisplayTypes.FloatingElement:

                            if (needNewRow) quellcode.Append(GetNewLine() + indent);

                            // open node
                            quellcode.Append($"{nodeColor}<{ node.Name}{GetAttributesAsSourcecode(node.Attributes)}>");

                            // draw children
                            quellcode.Append(GetChildrenAsSourcecode(node.ChildNodes, indent + indentPlus, true, nodeHasError, false));

                            // close node
                            quellcode.Append($"{nodeColor}</{node.Name}>");
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(rules.DisplayType) + ":" + rules.DisplayType(node).ToString());
                    }
                }
                else
                {
                    if (needNewRow) quellcode.Append(GetNewLine() + indent);

                    quellcode.Append(nodeColor);
                    quellcode.Append($"<{node.Name}{GetAttributesAsSourcecode(node.Attributes)}>");
                }
            }
            return quellcode.ToString();
        }

        private string GetNewLine()
        {
            if (this.showRowNumbers)
            {
                rowNumber++;
                //return this._rtf_Umbruch + GetZeilenNummerString(_zeilenNummer) + ": ";
                var result = new StringBuilder(_rtf_LineBreak);
                result.Append(RtfColor(RtfColors.black));
                result.AppendFormat("{0}: ", GetRowNumberString(rowNumber));
                return result.ToString();
            }
            else
            {
                return this._rtf_LineBreak;
            }
        }

        private string GetRowNumberString(int number)
        {
            var result = new StringBuilder(number.ToString(), 6);
            while (result.Length < 6) result.Insert(0, "0");
            return result.ToString();
        }

        private string RtfColor(RtfColors color)
        {
            switch (color)
            {
                case RtfColors.black: return this._rtf_ColorBlack;
                case RtfColors.red: return this._rtf_ColorRed;
                case RtfColors.gray: return this._rtf_ColorGray;
                default:
                    throw new ArgumentOutOfRangeException("unhandled color '" + color + "'");
            }
        }


        private string GetChildrenAsSourcecode(System.Xml.XmlNodeList children, string indent, bool needNewLine, bool parentNodeAlreadyHasError, bool positionAlreadyAsOkChecked)
        {
            var sourcecode = new StringBuilder();
            bool parentOrSiblingAlreadyHasErrors = parentNodeAlreadyHasError;
            bool siblingHasErrors = false;
            foreach (System.Xml.XmlNode child in children)
            {
                sourcecode.Append(GetNodeAsSourceText(child, indent, needNewLine, parentOrSiblingAlreadyHasErrors, positionAlreadyAsOkChecked, ref siblingHasErrors));
                //  If a sibling is invalid, do not check the following siblings (performance)
                if (siblingHasErrors)
                {
                    siblingHasErrors = true;
                    parentOrSiblingAlreadyHasErrors = true;
                }
                else
                {
                    positionAlreadyAsOkChecked = true; // For the following siblings notice that everything was ok
                }
                needNewLine = false; // had to be considered only with the first child
            }
            return sourcecode.ToString();
        }

        private string GetAttributesAsSourcecode(System.Xml.XmlAttributeCollection attributes)
        {
            if (attributes == null) return string.Empty;
            var sourcecode = new StringBuilder();
            foreach (System.Xml.XmlAttribute attrib in attributes)
            {
                sourcecode.Append($" {attrib.Name}=\"{ attrib.Value}\"");
            }
            return sourcecode.ToString();
        }
    }
}

