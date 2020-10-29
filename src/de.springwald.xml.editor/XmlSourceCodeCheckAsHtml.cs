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

        private XmlRules _xmlRules;                // Das Regelwerk zur Beurteilung der Gültigkeit
        private bool _showLineNumbers = true;     // Zeilennummern anzeigen

        private int _lineNumber;                      // Der Counter für die jeweils aktuelle Zeilennummer des Quellcodes

        private System.Xml.XmlNode _rootnode;               //  der Quellcode, den es zu untersuchen gilt

        private StringBuilder _errorsAsHtml;              // merkt sich das Fehlerprotokoll zum aktuell angezeigten Quellcode
        private StringBuilder _sourcecodeAsHtml;             // der Ergebnis der Quellcodeumwandlung

        private const string HtmlNewLine = "<br/>";

        private bool _notRenderedYet = true;

        /// <summary>
        /// Das Regelwerk zur Beurteilung der Gültigkeit
        /// </summary>
        public XmlRules XmlRules
        {
            set { _xmlRules = value; }
        }

        /// <summary>
        /// der Quellcode, den es zu untersuchen gilt
        public System.Xml.XmlNode Rootnode
        {
            set
            {
                _rootnode = value;
                _notRenderedYet = true;
            }
        }

        /// <summary>
        /// Von außen Bescheid geben, dass neu zu berechnen ist
        /// </summary>
        public void SetChanged()
        {
            _notRenderedYet = true;
        }

        /// <summary>
        /// Liefert ein Fehlerprotokoll passend zum aktuell angezeigten Quellcode
        /// </summary>
        public string ErrorsAsText
        {
            get
            {
                if (this._notRenderedYet)
                {
                    this.Render();
                }
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
                if (this._notRenderedYet)
                {
                    this.Render();
                }
                return this._sourcecodeAsHtml.ToString();
            }
        }

        /// <summary>
        /// Den Quellcode + Fehlerprotokoll neu berechnen
        /// </summary>
        public void Render()
        {
            this._sourcecodeAsHtml = new StringBuilder();
            this._errorsAsHtml = new StringBuilder();

            if (_xmlRules == null)
            {   // Kein Editor angegeben
                this._errorsAsHtml.Append("<li>No xml rules attached.</li>");
            }
            else
            {
                if (_rootnode == null)  // Wenn kein Rootnode angegeben ist
                {
                    this._errorsAsHtml.Append("<li>NochKeinRootNodeZugewiesen</li>");
                }
                else // Es ist ein Rootnode angegeben
                {
                    bool nodeFehlerhaft = false;
                    this._lineNumber = 0;
                    _sourcecodeAsHtml.Append(GetNodeAlsQuellText(_rootnode, einzug: "", newLineNeeded: false, parentWarFehlerhaft: false, posBereitsAlsOKGeprueft: false, ref nodeFehlerhaft));
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
        /// <param name="parentWarFehlerhaft">Wenn True, dann war bereits der Parent-Node so fehlerhaft, dass dieser Node nicht mehr weiter gegen DTD-Fehler geprüft werden muss</param>
        /// <returns></returns>
        private string GetNodeAlsQuellText(System.Xml.XmlNode node, string einzug, bool newLineNeeded, bool parentWarFehlerhaft, bool posBereitsAlsOKGeprueft, ref bool nodeFehlerhaft)
        {
            // Kommentare und Whitespace besonders behandeln
            if (node is XmlWhitespace)
            {
                return ""; // Whitespace wird nicht dargestellt
            }
            if (node is XmlComment) return String.Format("&lt;!--{0}--&gt;", node.InnerText);

            var quellcode = new StringBuilder();
            const string einzugplus = "&nbsp;&nbsp;&nbsp;&nbsp;";
            Colors nodeColor;

            // Herausfinden, ob dieser Node ok ist
            string nodeErrorMsg;

            if (parentWarFehlerhaft)
            {
                //es war bereits der Parent-Node so fehlerhaft, dass dieser Node nicht mehr weiter gegen DTD-Fehler geprüft werden muss
                nodeFehlerhaft = true;
                nodeErrorMsg = null; //"parent-Node bereits fehlerhaft";
                nodeColor = Colors.Undefined;
            }
            else
            {
                var pruefer = _xmlRules.DTDPruefer;
                if (pruefer.IsXmlNodeOk(node, posBereitsAlsOKGeprueft))
                {
                    nodeFehlerhaft = false;
                    nodeErrorMsg = null;
                    nodeColor = Colors.Black;
                }
                else
                {
                    nodeFehlerhaft = true;
                    nodeErrorMsg = pruefer.ErrorMessages;
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

            if (newLineNeeded || renderType == DisplayTypes.OwnRow) quellcode.Append(StartNewLine() + einzug);

            quellcode.Append($"<span style=\"{GetCss(nodeColor)}\">");  // start node color

            if (node is System.Xml.XmlText) // is text node 
            {
                var nodetext = new StringBuilder(node.Value);
                nodetext.Replace("\t", " ");
                nodetext.Replace("\r\n", " ");
                nodetext.Replace("  ", " ");
                quellcode.Append(System.Web.HttpUtility.HtmlEncode(nodetext.ToString()));
            }
            else  // not a text node
            {
                var closingTagVisible = _xmlRules.HasEndTag(node);

                quellcode.Append($"&lt;{node.Name}{GetAttributeAlsQuellText(node.Attributes)}{(closingTagVisible ? "" : "/")}&gt;");

                quellcode.Append(GetChildrenAlsQuellText(node.ChildNodes, einzug + einzugplus, true, nodeFehlerhaft, false)); // paint children

                if (closingTagVisible)
                {
                    quellcode.Append(StartNewLine() + einzug);
                    quellcode.Append($"&lt;/{node.Name}&gt;");
                }
            }

            quellcode.Append("</span>"); // end node color

            if (nodeErrorMsg != null)
            {
                //this._errorsAsHtml.Append($"<li><a href=\"#{this._lineNumber}\">{this._lineNumber:000000}: {System.Web.HttpUtility.HtmlEncode(nodeFehlerMsg)}</a></li>");
                this._errorsAsHtml.Append($"<li>{this._lineNumber:000000}: {System.Web.HttpUtility.HtmlEncode(nodeErrorMsg)}</li>");
            }

            return quellcode.ToString();
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

        /// <summary>
        /// Liefert den Umbruch für eine neue Zeile
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Die Childinhalte als String
        /// </summary>
        /// <param name="parentnode"></param>
        /// <param name="einzug"></param>
        /// <returns></returns>
        private string GetChildrenAlsQuellText(System.Xml.XmlNodeList children, string einzug, bool neueZeileNotwendig, bool parentNodeBereitsFehlerhaft, bool posBereitsAlsOKGeprueft)
        {
            var quellcode = new StringBuilder();
            bool parentOderGeschwisterBereitsFehlerhaft = parentNodeBereitsFehlerhaft;
            bool geschwisterDefekt = false;
            foreach (XmlNode child in children)
            {
                quellcode.Append(GetNodeAlsQuellText(child, einzug, neueZeileNotwendig, parentOderGeschwisterBereitsFehlerhaft, posBereitsAlsOKGeprueft, ref geschwisterDefekt));
                // Wenn ein Geschwister kaputt ist, die folgenden Geschwister nicht mehr prüfen (Performance)
                if (geschwisterDefekt)
                {
                    geschwisterDefekt = true;
                    parentOderGeschwisterBereitsFehlerhaft = true;
                }
                else
                {
                    posBereitsAlsOKGeprueft = true; // Für die folgenden Geschwister merken, dass alles ok war
                }
                neueZeileNotwendig = false; // musste nur beim ersten Child beachtet werden
            }
            return quellcode.ToString();
        }

        /// <summary>
        /// Zeichnet die Attribute eines Nodes
        /// </summary>
        /// <param name="attribute"></param>
        private string GetAttributeAlsQuellText(System.Xml.XmlAttributeCollection attribute)
        {
            if (attribute == null) return "";
            var quellcode = new StringBuilder();
            foreach (System.Xml.XmlAttribute attrib in attribute)
            {
                quellcode.AppendFormat(" {0}=\"{1}\"", attrib.Name, attrib.Value);
            }
            return quellcode.ToString();
        }
    }
}
