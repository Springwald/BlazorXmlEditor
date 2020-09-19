using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml
{
    public class XMLQuellcodeAlsRTF
    {
        /// <summary>
        /// Event-Aufruf definieren, wenn ein Node des Doks geprüft wird (für Statusanzeige, damit man nicht denkt, das Pogramm hängt)
        /// </summary>
        public event System.EventHandler NodeWirdGeprueftEvent;

        /// <summary>
        /// Event-Aufruf definieren, wenn ein Node des Doks geprüft wird (für Statusanzeige, damit man nicht denkt, das Pogramm hängt)
        /// </summary>
        protected virtual void ActivateNodeWirdGeprueft(EventArgs e)
        {
            if (NodeWirdGeprueftEvent != null)
            {
                NodeWirdGeprueftEvent(this, e);
            }
        }

        private XMLRegelwerk _regelwerk;                // Das Regelwerk zur Beurteilung der Gültigkeit
        private bool _zeilenNummernAnzeigen = true;     // Zeilennummern anzeigen

        private int _zeilenNummer;                      // Der Counter für die jeweils aktuelle Zeilennummer des Quellcodes

        private System.Xml.XmlNode _rootnode;               //  der Quellcode, den es zu untersuchen gilt

        private StringBuilder _fehlerProtokollAlsText;              // merkt sich das Fehlerprotokoll zum aktuell angezeigten Quellcode
        private StringBuilder _quellcodeAlsRTF;             // der Ergebnis der Quellcodeumwandlung


        private string _rtf_Header = "{\\rtf1\\ansi\\deff0" + "\r\n" +                                  // Rtf-Header
            "{\\colortbl;\\red0\\green0\\blue0;" +                              // farbe1=schwarz
            "\\red255\\green0\\blue0;" +                                        // farbe2=rot
            "\\red200\\green200\\blue200;}";                                    // farbe3=grau

        private string _rtf_Footer = "\r\n}";

        private enum RtfFarben { schwarz, rot, grau }; // Eine Aufzählung der verfügbaren Format-Schalter

        private string _rtf_Umbruch = "\\line" + "\r\n";
        private string _rtf_FarbeSchwarz = "\\cf1" + "\r\n";
        private string _rtf_FarbeRot = "\\cf2" + "\r\n";
        private string _rtf_FarbeGrau = "\\cf3" + "\r\n";

        private bool _nochNichtGerendert = true;

        /// <summary>
        /// Das Regelwerk zur Beurteilung der Gültigkeit
        /// </summary>
        public XMLRegelwerk Regelwerk
        {
            set { _regelwerk = value; }
        }

        /// <summary>
        /// der Quellcode, den es zu untersuchen gilt
        public System.Xml.XmlNode Rootnode
        {
            set
            {
                _rootnode = value;
                _nochNichtGerendert = true;
            }
        }

        /// <summary>
        /// Von außen Bescheid geben, dass neu zu berechnen ist
        /// </summary>
        public void SetChanged()
        {
            _nochNichtGerendert = true;
        }

        /// <summary>
        /// Das Ergebnis direkt neu berechnen
        /// </summary>
        public void Rendern()
        {
            RenderIntern();
        }

        /// <summary>
        /// Liefert ein Fehlerprotokoll passend zum aktuell angezeigten Quellcode
        /// </summary>
        public string FehlerProtokollAlsText
        {
            get
            {
                if (this._nochNichtGerendert)
                {
                    this.RenderIntern();
                }
                return this._fehlerProtokollAlsText.ToString();
            }
        }

        /// <summary>
        /// Liefert den Quellcode als RTF. 
        /// </summary>
        public string QuellCodeAlsRTF
        {
            get
            {
                if (this._nochNichtGerendert)
                {
                    this.RenderIntern();
                }
                return this._quellcodeAlsRTF.ToString();
            }
        }

        public XMLQuellcodeAlsRTF()
        {
        }


        /// <summary>
        /// Den Quellcode + Fehlerprotokoll neu berechnen
        /// </summary>
        private void RenderIntern()
        {

            this._quellcodeAlsRTF = new StringBuilder();
            this._fehlerProtokollAlsText = new StringBuilder();

            if (_regelwerk == null)
            {   // Kein Editor angegeben
                this._fehlerProtokollAlsText.Append("Noch kein Regelwerk-Objekt zugewiesen");
            }
            else
            {
                if (_rootnode == null)  // Wenn kein Rootnode angegeben ist
                {
                    this._fehlerProtokollAlsText.Append("NochKeinRootNodeZugewiesen");
                }
                else // Es ist ein Rootnode angegeben
                {
                    bool nodeFehlerhaft = false;
                    this._zeilenNummer = 0;

                    _quellcodeAlsRTF.Append(_rtf_Header);
                    _quellcodeAlsRTF.AppendFormat("{0}\r\n", GetNodeAlsQuellText(_rootnode, "", false, false, false, ref nodeFehlerhaft));
                    _quellcodeAlsRTF.Append(_rtf_Footer);

                    _nochNichtGerendert = false;

                }
            }
        }

        /// <summary>
        /// Zeichnet einen XML-Node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="einzug">aktuelle Einrückung</param>
        /// <param name="neueZeileNotwendig"></param>
        /// <param name="parentWarFehlerhaft">Wenn True, dann war bereits der Parent-Node so fehlerhaft, dass dieser Node nicht mehr weiter gegen DTD-Fehler geprüft werden muss</param>
        /// <returns></returns>
        private string GetNodeAlsQuellText(System.Xml.XmlNode node, string einzug, bool neueZeileNotwendig, bool parentWarFehlerhaft, bool posBereitsAlsOKGeprueft, ref bool nodeFehlerhaft)
        {
            // Einer möglichen, externen Statusanzeige Bescheid geben, dass gerade ein Node geprüft wird
            ActivateNodeWirdGeprueft(EventArgs.Empty);

            // Kommentare und Whitespace besonders behandeln
            if (node is System.Xml.XmlWhitespace)
            {
                return ""; // Whitespace wird nicht dargestellt
            }
            if (node is System.Xml.XmlComment) return String.Format("<!--{0}-->", node.InnerText);

            StringBuilder quellcode = new StringBuilder();
            string einzugplus = "    ";
            string nodeFarbe;

            // Herausfinden, ob dieser Node ok ist
            string nodeFehlerMsg;

            if (parentWarFehlerhaft)
            {
                //es war bereits der Parent-Node so fehlerhaft, dass dieser Node nicht mehr weiter gegen DTD-Fehler geprüft werden muss
                nodeFehlerhaft = true;
                nodeFehlerMsg = null; //"parent-Node bereits fehlerhaft";
                nodeFarbe = ""; //RTFFarbe(RtfFarben.schwarz); 
                                //nodeFarbe = RTFFarbe(RtfFarben.rot); 
            }
            else
            {
                de.springwald.xml.dtd.pruefer.DTDPruefer pruefer = _regelwerk.DTDPruefer;
                if (pruefer.IstXmlNodeOk(node, posBereitsAlsOKGeprueft))
                {
                    nodeFehlerhaft = false;
                    nodeFehlerMsg = null;
                    nodeFarbe = RTFFarbe(RtfFarben.schwarz);
                }
                else
                {
                    nodeFehlerhaft = true;
                    nodeFehlerMsg = pruefer.Fehlermeldungen;
                    nodeFarbe = RTFFarbe(RtfFarben.rot);
                }
            }

            if (node is System.Xml.XmlText) // ein Textnode 
            {
                if (neueZeileNotwendig) quellcode.Append(GetNeueZeile() + einzug);

                quellcode.Append(nodeFarbe);

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
                if (_regelwerk.IstSchliessendesTagSichtbar(node))
                {

                    // Schauen, wie das Element gezeichnet werden soll
                    switch (_regelwerk.DarstellungsArt(node))
                    {
                        case DarstellungsArten.EigeneZeile:

                            quellcode.Append(GetNeueZeile());
                            quellcode.Append(nodeFarbe);
                            quellcode.Append(einzug);

                            quellcode.AppendFormat("<{0}{1}>", node.Name, GetAttributeAlsQuellText(node.Attributes));

                            if (nodeFehlerMsg != null)
                            {
                                this._fehlerProtokollAlsText.Append(GetZeilenNummerString(this._zeilenNummer) + ": " + nodeFehlerMsg + "\r\n");
                            }

                            // Children zeichnen
                            quellcode.Append(GetChildrenAlsQuellText(node.ChildNodes, einzug + einzugplus, true, nodeFehlerhaft, false));

                            // Node schließen
                            quellcode.Append(GetNeueZeile());
                            quellcode.Append(nodeFarbe);
                            quellcode.Append(einzug);
                            quellcode.AppendFormat("</{0}>", node.Name);

                            break;

                        case DarstellungsArten.Fliesselement:

                            if (neueZeileNotwendig) quellcode.Append(GetNeueZeile() + einzug);

                            // Node öffnen
                            quellcode.AppendFormat("{0}<{1}{2}>", nodeFarbe, node.Name, GetAttributeAlsQuellText(node.Attributes));

                            // Children zeichnen
                            quellcode.Append(GetChildrenAlsQuellText(node.ChildNodes, einzug + einzugplus, true, nodeFehlerhaft, false));

                            // Node schließen
                            quellcode.AppendFormat("{0}</{1}>", nodeFarbe, node.Name);
                            break;

                        default:
                            throw new ApplicationException("Unbekannte Darstellungsart " + _regelwerk.DarstellungsArt(node));
                    }
                }
                else
                {
                    if (neueZeileNotwendig) quellcode.Append(GetNeueZeile() + einzug);

                    quellcode.Append(nodeFarbe);
                    quellcode.AppendFormat("<{0}{1}>", node.Name, GetAttributeAlsQuellText(node.Attributes));
                }
            }
            return quellcode.ToString();
        }

        /// <summary>
        /// Liefert den Umbruch für eine neue Zeile
        /// </summary>
        /// <returns></returns>
        private string GetNeueZeile()
        {
            if (this._zeilenNummernAnzeigen)
            {
                _zeilenNummer++;
                //return this._rtf_Umbruch + GetZeilenNummerString(_zeilenNummer) + ": ";
                StringBuilder rueckgabe = new StringBuilder(_rtf_Umbruch);
                rueckgabe.Append(RTFFarbe(RtfFarben.schwarz));
                rueckgabe.AppendFormat("{0}: ", GetZeilenNummerString(_zeilenNummer));
                return rueckgabe.ToString();
            }
            else
            {
                return this._rtf_Umbruch;
            }
        }

        /// <summary>
        /// Macht aus der angegebenen Nummer einen links mit Nullen aufgefüllten String
        /// </summary>
        /// <param name="nummer"></param>
        /// <returns></returns>
        private string GetZeilenNummerString(int nummer)
        {
            StringBuilder ausgabe = new StringBuilder(nummer.ToString(), 6);
            while (ausgabe.Length < 6) ausgabe.Insert(0, "0");
            return ausgabe.ToString();
        }

        /// <summary>
        /// Gibt RTF-Code für diese Farbe zurück
        /// </summary>
        /// <param name="farbe"></param>
        private string RTFFarbe(RtfFarben farbe)
        {
            switch (farbe)
            {
                case RtfFarben.schwarz: return this._rtf_FarbeSchwarz;
                case RtfFarben.rot: return this._rtf_FarbeRot;
                case RtfFarben.grau: return this._rtf_FarbeGrau;
                default:
                    throw new ApplicationException("Unbekannt Farbe '" + farbe + "'");
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
            StringBuilder quellcode = new StringBuilder();
            bool parentOderGeschwisterBereitsFehlerhaft = parentNodeBereitsFehlerhaft;
            bool geschwisterDefekt = false;
            foreach (System.Xml.XmlNode child in children)
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
            if (attribute == null)
            {
                return "";
            }
            else
            {
                StringBuilder quellcode = new StringBuilder();
                foreach (System.Xml.XmlAttribute attrib in attribute)
                {
                    quellcode.AppendFormat(" {0}=\"{1}\"", attrib.Name, attrib.Value);
                }
                return quellcode.ToString();
            }
        }
    }
}

