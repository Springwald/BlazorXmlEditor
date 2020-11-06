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
using System.Xml;

namespace de.springwald.xml
{
    public class XmlSourceCodeCheckAsHtml
    {
        private enum Colors
        {
            Undefined,
            Black,
            Red,
            Gray
        };

        private XmlRules _xmlRules;
        private bool _showLineNumbers = true;
        private int _lineNumber;
        private XmlNode _rootnode;
        private StringBuilder _errorsAsHtml;
        private StringBuilder _sourcecodeAsHtml;
        private const string HtmlNewLine = "<br/>";
        private bool _notRenderedYet = true;

        public XmlRules XmlRules
        {
            set { _xmlRules = value; }
        }

        public System.Xml.XmlNode Rootnode
        {
            set
            {
                _rootnode = value;
                _notRenderedYet = true;
            }
        }

        public void SetChanged()
        {
            _notRenderedYet = true;
        }

        public string ErrorsAsText
        {
            get
            {
                if (this._notRenderedYet) this.Render();
                return this._errorsAsHtml.ToString();
            }
        }

        /// <summary>
        /// Liefert den Quellcode als RTF. 
        /// </summary>
        public string SourcecodeAsHtml
        {
            get
            {
                if (this._notRenderedYet) this.Render();
                return this._sourcecodeAsHtml.ToString();
            }
        }

        /// <summary>
        /// Recalculate the source code + error log
        /// </summary>
        public void Render()
        {
            this._sourcecodeAsHtml = new StringBuilder();
            this._errorsAsHtml = new StringBuilder();

            if (_xmlRules == null)
            {   
                this._errorsAsHtml.Append("<li>No xml rules attached.</li>");
            }
            else
            {
                if (_rootnode == null)  
                {
                    this._errorsAsHtml.Append("<li>no root node specified</li>");
                }
                else // A root node is specified
                {
                    bool nodeFehlerhaft = false;
                    this._lineNumber = 0;
                    _sourcecodeAsHtml.Append(GetNodeAlsQuellText(_rootnode, einzug: "", newLineNeeded: false, parentWasInvalid: false, positionAlreadyCheckedAsOk: false, ref nodeFehlerhaft));
                    _notRenderedYet = false;
                }
            }
        }

        /// <summary>
        /// Zeichnet einen XML-Node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="einzug">aktuelle Einrückung</param>
        /// <param name="newLineNeeded"></param>
        /// <param name="parentWasInvalid">Wenn True, dann war bereits der Parent-Node so fehlerhaft, dass dieser Node nicht mehr weiter gegen DTD-Fehler geprüft werden muss</param>
        /// <returns></returns>
        private string GetNodeAlsQuellText(System.Xml.XmlNode node, string einzug, bool newLineNeeded, bool parentWasInvalid, bool positionAlreadyCheckedAsOk, ref bool nodeInvalid)
        {
            if (node is XmlWhitespace)
            {
                return string.Empty; // dont paint Whitespace 
            }
            if (node is XmlComment) return $"&lt;!--{node.InnerText}--&gt;";

            var sourceCode = new StringBuilder();
            const string indentPlus = "&nbsp;&nbsp;&nbsp;&nbsp;";
            Colors nodeColor;

            string nodeErrorMsg;

            if (parentWasInvalid)
            {
                // the parent node was already so invalid that this node no longer needs to be checked against DTD errors
                nodeInvalid = true;
                nodeErrorMsg = null; //"parent-Node already invalid";
                nodeColor = Colors.Undefined;
            }
            else
            {
                var checker = _xmlRules.DtdChecker;
                if (checker.IsXmlNodeOk(node, positionAlreadyCheckedAsOk))
                {
                    nodeInvalid = false;
                    nodeErrorMsg = null;
                    nodeColor = Colors.Black;
                }
                else
                {
                    nodeInvalid = true;
                    nodeErrorMsg = checker.ErrorMessages;
                    nodeColor = Colors.Red;
                }
            }

            var renderType = _xmlRules.DisplayType(node);
            switch (renderType)
            {
                case DisplayTypes.FloatingElement:
                case DisplayTypes.OwnRow:
                    break;
                default:
                    throw new ApplicationException("unknown render type: " + _xmlRules.DisplayType(node));
            }

            if (newLineNeeded || renderType == DisplayTypes.OwnRow) sourceCode.Append(StartNewLine() + einzug);

            sourceCode.Append($"<span style=\"{GetCss(nodeColor)}\">");  // start node color

            if (node is XmlText) // is text node 
            {
                var nodetext = new StringBuilder(node.Value);
                nodetext.Replace("\t", " ");
                nodetext.Replace("\r\n", " ");
                nodetext.Replace("  ", " ");
                sourceCode.Append(System.Web.HttpUtility.HtmlEncode(nodetext.ToString()));
            }
            else  // not a text node
            {
                var closingTagVisible = _xmlRules.HasEndTag(node);

                sourceCode.Append($"&lt;{node.Name}{GetAttributeAsSourcecode(node.Attributes)}{(closingTagVisible ? "" : "/")}&gt;");
                sourceCode.Append(GetChildrenAsSourceCode(node.ChildNodes, einzug + indentPlus, true, nodeInvalid, false)); // paint children

                if (closingTagVisible)
                {
                    sourceCode.Append(StartNewLine() + einzug);
                    sourceCode.Append($"&lt;/{node.Name}&gt;");
                }
            }

            sourceCode.Append("</span>"); // end node color

            if (nodeErrorMsg != null)
            {
                this._errorsAsHtml.Append($"<li>{this._lineNumber:000000}: {System.Web.HttpUtility.HtmlEncode(nodeErrorMsg)}</li>");
            }

            return sourceCode.ToString();
        }

        private string GetCss(Colors color)
        {
            switch (color)
            {
                case Colors.Black:
                    return "color: black;";
                case Colors.Red:
                    return "color: red; font-weight: bold;";
                case Colors.Gray:
                    return "color: black;";
            }
            return "";
        }

        private string StartNewLine()
        {
            if (this._showLineNumbers)
            {
                _lineNumber++;
                return $"<a name=\"{this._lineNumber}\"></a>{HtmlNewLine}{_lineNumber:000000}: ";
            }
            else
            {
                return HtmlNewLine; ;
            }
        }

        /// <returns></returns>
        private string GetChildrenAsSourceCode(XmlNodeList children, string indent, bool needNewLine, bool parentNodeAlreadyInvaliod, bool positionAlreadyCheckedAsOk)
        {
            var sourceCode = new StringBuilder();
            bool parentOrSiblingAlreadyInvalid = parentNodeAlreadyInvaliod;
            bool siblingInvalid = false;
            foreach (XmlNode child in children)
            {
                sourceCode.Append(GetNodeAlsQuellText(child, indent, needNewLine, parentOrSiblingAlreadyInvalid, positionAlreadyCheckedAsOk, ref siblingInvalid));
              
                if (siblingInvalid)
                {
                    // If a sibling is broken, do not check the following siblings (performance)
                    siblingInvalid = true;
                    parentOrSiblingAlreadyInvalid = true;
                }
                else
                {
                    positionAlreadyCheckedAsOk = true; // For the following siblings notice that everything was ok
                }
                needNewLine = false; // had to be considered only with the first child
            }
            return sourceCode.ToString();
        }

        private string GetAttributeAsSourcecode(XmlAttributeCollection attribute)
        {
            if (attribute == null) return "";
            var sourceCode = new StringBuilder();
            foreach (XmlAttribute attrib in attribute)
            {
                sourceCode.Append($" {attrib.Name}=\"{attrib.Value}\"");
            }
            return sourceCode.ToString();
        }
    }
}
