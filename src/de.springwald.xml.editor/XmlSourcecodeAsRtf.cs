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
        /// Event-Aufruf definieren, wenn ein Node des Doks geprüft wird (für Statusanzeige, damit man nicht denkt, das Pogramm hängt)
        /// </summary>
        public event EventHandler NodeWirdGeprueftEvent;

        /// <summary>
        /// Event-Aufruf definieren, wenn ein Node des Doks geprüft wird (für Statusanzeige, damit man nicht denkt, das Pogramm hängt)
        /// </summary>
        protected virtual void ActiveNodeIsBeeningChecked(EventArgs e)
        {
            this.NodeWirdGeprueftEvent?.Invoke(this, e);
        }

        private XmlRules rules;                // Das Regelwerk zur Beurteilung der Gültigkeit
        private bool showRowNumbers = true;     // Zeilennummern anzeigen

        private int rowNumber;                      // Der Counter für die jeweils aktuelle Zeilennummer des Quellcodes

        private System.Xml.XmlNode rootNode;               //  der Quellcode, den es zu untersuchen gilt

        private StringBuilder errorLogAsText;              // merkt sich das Fehlerprotokoll zum aktuell angezeigten Quellcode
        private StringBuilder sourcecodeAsRtf;             // der Ergebnis der Quellcodeumwandlung

        private string _rtf_Header = "{\\rtf1\\ansi\\deff0" + "\r\n" +                                  // Rtf-Header
            "{\\colortbl;\\red0\\green0\\blue0;" +                              // farbe1=schwarz
            "\\red255\\green0\\blue0;" +                                        // farbe2=rot
            "\\red200\\green200\\blue200;}";                                    // farbe3=grau

        private string _rtf_Footer = "\r\n}";

        private enum RtfColors { black, red, gray }; // Eine Aufzählung der verfügbaren Format-Schalter

        private string _rtf_LineBreak = "\\line" + "\r\n";
        private string _rtf_ColorBlack = "\\cf1" + "\r\n";
        private string _rtf_ColorRed = "\\cf2" + "\r\n";
        private string _rtf_ColorGray = "\\cf3" + "\r\n";

        private bool notRenderedJet = true;

        /// <summary>
        /// Das Regelwerk zur Beurteilung der Gültigkeit
        /// </summary>
        public XmlRules Rules
        {
            set { rules = value; }
        }

        /// <summary>
        /// der Quellcode, den es zu untersuchen gilt
        public System.Xml.XmlNode Rootnode
        {
            set
            {
                rootNode = value;
                notRenderedJet = true;
            }
        }

        /// <summary>
        /// Von außen Bescheid geben, dass neu zu berechnen ist
        /// </summary>
        public void SetChanged()
        {
            notRenderedJet = true;
        }

        /// <summary>
        /// Das Ergebnis direkt neu berechnen
        /// </summary>
        public void Render()
        {
            RenderInternal();
        }

        /// <summary>
        /// Liefert ein Fehlerprotokoll passend zum aktuell angezeigten Quellcode
        /// </summary>
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

        /// <summary>
        /// Liefert den Quellcode als RTF. 
        /// </summary>
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
        /// Den Quellcode + Fehlerprotokoll neu berechnen
        /// </summary>
        private void RenderInternal()
        {

            this.sourcecodeAsRtf = new StringBuilder();
            this.errorLogAsText = new StringBuilder();

            if (rules == null)
            {   // Kein Editor angegeben
                this.errorLogAsText.Append("no xml rules attached");
            }
            else
            {
                if (rootNode == null)  // Wenn kein Rootnode angegeben ist
                {
                    this.errorLogAsText.Append("no root node attached");
                }
                else // Es ist ein Rootnode angegeben
                {
                    bool nodeHasErrors = false;
                    this.rowNumber = 0;
                    sourcecodeAsRtf.Append(_rtf_Header);
                    sourcecodeAsRtf.AppendFormat("{0}\r\n", GetNodeAlsQuellText(rootNode, "", false, false, false, ref nodeHasErrors));
                    sourcecodeAsRtf.Append(_rtf_Footer);
                    notRenderedJet = false;
                }
            }
        }

        /// <summary>
        /// Zeichnet einen XML-Node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="indent">aktuelle Einrückung</param>
        /// <param name="needNewRow"></param>
        /// <param name="parentHasErrors">Wenn True, dann war bereits der Parent-Node so fehlerhaft, dass dieser Node nicht mehr weiter gegen DTD-Fehler geprüft werden muss</param>
        /// <returns></returns>
        private string GetNodeAlsQuellText(System.Xml.XmlNode node, string indent, bool needNewRow, bool parentHasErrors, bool posAlreadyCheckedAsOk, ref bool nodeHasError)
        {
            // Einer möglichen, externen Statusanzeige Bescheid geben, dass gerade ein Node geprüft wird
            ActiveNodeIsBeeningChecked(EventArgs.Empty);

            // Kommentare und Whitespace besonders behandeln
            if (node is System.Xml.XmlWhitespace)
            {
                return ""; // Whitespace wird nicht dargestellt
            }
            if (node is System.Xml.XmlComment) return String.Format("<!--{0}-->", node.InnerText);

            var quellcode = new StringBuilder();
            string indentPlus = "    ";
            string nodeColor;

            // Herausfinden, ob dieser Node ok ist
            string nodeErrorMessage;

            if (parentHasErrors)
            {
                //es war bereits der Parent-Node so fehlerhaft, dass dieser Node nicht mehr weiter gegen DTD-Fehler geprüft werden muss
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

            if (node is System.Xml.XmlText) // ein Textnode 
            {
                if (needNewRow) quellcode.Append(GetNewLine() + indent);

                quellcode.Append(nodeColor);

                StringBuilder nodetext = new StringBuilder(node.Value);
                nodetext.Replace("\t", " ");
                nodetext.Replace("\r\n", " ");
                nodetext.Replace("  ", " ");
                quellcode.Append(nodetext.ToString());
                nodetext = null;
            }
            else  // kein Textnode
            {

                // Den Node selbst zeichnen
                if (rules.HasEndTag(node))
                {

                    // Schauen, wie das Element gezeichnet werden soll
                    switch (rules.DisplayType(node))
                    {
                        case DisplayTypes.OwnRow:

                            quellcode.Append(GetNewLine());
                            quellcode.Append(nodeColor);
                            quellcode.Append(indent);

                            quellcode.AppendFormat("<{0}{1}>", node.Name, GetAttributesAsSourcecode(node.Attributes));

                            if (nodeErrorMessage != null)
                            {
                                this.errorLogAsText.Append(GetRowNumberString(this.rowNumber) + ": " + nodeErrorMessage + "\r\n");
                            }

                            // Children zeichnen
                            quellcode.Append(GetChildrenAsSourcecode(node.ChildNodes, indent + indentPlus, true, nodeHasError, false));

                            // Node schließen
                            quellcode.Append(GetNewLine());
                            quellcode.Append(nodeColor);
                            quellcode.Append(indent);
                            quellcode.AppendFormat("</{0}>", node.Name);

                            break;

                        case DisplayTypes.FloatingElement:

                            if (needNewRow) quellcode.Append(GetNewLine() + indent);

                            // Node öffnen
                            quellcode.AppendFormat("{0}<{1}{2}>", nodeColor, node.Name, GetAttributesAsSourcecode(node.Attributes));

                            // Children zeichnen
                            quellcode.Append(GetChildrenAsSourcecode(node.ChildNodes, indent + indentPlus, true, nodeHasError, false));

                            // Node schließen
                            quellcode.AppendFormat("{0}</{1}>", nodeColor, node.Name);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(rules.DisplayType) + ":" + rules.DisplayType(node).ToString());
                    }
                }
                else
                {
                    if (needNewRow) quellcode.Append(GetNewLine() + indent);

                    quellcode.Append(nodeColor);
                    quellcode.AppendFormat("<{0}{1}>", node.Name, GetAttributesAsSourcecode(node.Attributes));
                }
            }
            return quellcode.ToString();
        }

        /// <summary>
        /// Liefert den Umbruch für eine neue Zeile
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Macht aus der angegebenen Nummer einen links mit Nullen aufgefüllten String
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private string GetRowNumberString(int number)
        {
            var result = new StringBuilder(number.ToString(), 6);
            while (result.Length < 6) result.Insert(0, "0");
            return result.ToString();
        }

        /// <summary>
        /// Gibt RTF-Code für diese Farbe zurück
        /// </summary>
        /// <param name="color"></param>
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


        /// <summary>
        /// Die Childinhalte als String
        /// </summary>
        /// <param name="parentnode"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        private string GetChildrenAsSourcecode(System.Xml.XmlNodeList children, string indent, bool needNewLine, bool parentNodeAlreadyHasError, bool positionAlreadyAsOkChecked)
        {
            var sourcecode = new StringBuilder();
            bool parentOrSiblingAlreadyHasErrors = parentNodeAlreadyHasError;
            bool siblingHasErrors = false;
            foreach (System.Xml.XmlNode child in children)
            {
                sourcecode.Append(GetNodeAlsQuellText(child, indent, needNewLine, parentOrSiblingAlreadyHasErrors, positionAlreadyAsOkChecked, ref siblingHasErrors));
                // Wenn ein Geschwister kaputt ist, die folgenden Geschwister nicht mehr prüfen (Performance)
                if (siblingHasErrors)
                {
                    siblingHasErrors = true;
                    parentOrSiblingAlreadyHasErrors = true;
                }
                else
                {
                    positionAlreadyAsOkChecked = true; // Für die folgenden Geschwister merken, dass alles ok war
                }
                needNewLine = false; // musste nur beim ersten Child beachtet werden
            }
            return sourcecode.ToString();
        }

        /// <summary>
        /// Zeichnet die Attribute eines Nodes
        /// </summary>
        /// <param name="attributes"></param>
        private string GetAttributesAsSourcecode(System.Xml.XmlAttributeCollection attributes)
        {
            if (attributes == null) return string.Empty;
            var sourcecode = new StringBuilder();
            foreach (System.Xml.XmlAttribute attrib in attributes)
            {
                sourcecode.AppendFormat(" {0}=\"{1}\"", attrib.Name, attrib.Value);
            }
            return sourcecode.ToString();
        }
    }
}

