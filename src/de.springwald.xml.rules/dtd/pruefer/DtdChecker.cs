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
    /// Prüft Nodes und Attribute etc. innerhalb eines Dokumentes darauf hin, ob sie erlaubt sind
    /// </summary>
    public class DtdChecker
    {
        private DTD _dtd; // Die DTD, gegen die geprüft werden soll
        private DtdNodeEditCheck _nodeCheckerintern;
        private StringBuilder errorMessages;

        private DtdNodeEditCheck NodeChecker
        {
            get
            {
                if (this._nodeCheckerintern == null)
                {
                    this._nodeCheckerintern = new DtdNodeEditCheck(_dtd);
                }
                return this._nodeCheckerintern;
            }
        }

        public string ErrorMessages
        {
            get { return this.errorMessages.ToString(); }
        }

        /// <summary>
        /// Prüft Nodes und Attribute etc. innerhalb eines Dokumentes darauf hin, ob sie erlaubt sind
        /// </summary>
        /// <param name="dtd">Die DTD, gegen die geprüft werden soll</param>
        public DtdChecker(DTD dtd)
        {
            this._dtd = dtd;
            this.Reset();
        }

        /// <summary>
        /// Prüft, ob das übergebene XML-Objekt lt. der angegebenen DTD ok ist.
        /// </summary>
        /// <returns></returns>
        public bool IsXmlAttributOk(System.Xml.XmlAttribute xmlAttribut)
        {
            this.Reset();
            return this.CheckAttribute(xmlAttribut);
        }

        /// <summary>
        /// Prüft, ob das übergebene XML-Objekt lt. der angegebenen DTD ok ist.
        /// </summary>
        /// <param name="posBereitsGeprueft">Ist für diesen Node bereits bekannt, ob er dort stehen darf wo er steht?</param>
        /// <returns></returns>
        public bool IsXmlNodeOk(System.Xml.XmlNode xmlNode, bool posBereitsAlsOKBestaetigt)
        {
            this.Reset();
            if (posBereitsAlsOKBestaetigt)
            {
                return true;
            }
            else
            {
                return this.CheckNodePos(xmlNode);
            }
        }

        /// <summary>
        /// Prüft einen XML-Node gegen die DTD
        /// <param name="node"></param>
        /// <returns></returns>
        private bool CheckNodePos(System.Xml.XmlNode node)
        {
            // Kommentar ist immer ok
            //if (node is System.Xml.XmlComment) return true;
            // Whitespace ist immer ok
            if (node is System.Xml.XmlWhitespace) return true;

            if (_dtd.IstDTDElementBekannt(DTD.GetElementNameFromNode(node)))// Das Element dieses Nodes ist in der DTD bekannt 
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
                        errorMessages.AppendFormat($"Tag '{node.Name}' hier nicht erlaubt");
                        var pos = new XmlCursorPos();
                        pos.SetPos(node, XmlCursorPositions.CursorOnNodeStartTag);
                        var allowedTags = this.NodeChecker.AnDieserStelleErlaubteTags_(pos, false, false); // was ist an dieser Stelle erlaubt?
                        if (allowedTags.Length > 0)
                        {
                            // "An dieser Stelle erlaubte Tags: "
                            errorMessages.Append("An dieser Stelle erlaubte Tags:");
                            foreach (string tag in allowedTags)
                            {
                                errorMessages.AppendFormat("{0} ", tag);
                            }
                        }
                        else
                        {
                            //"An dieser Stelle sind keine Tags erlaubt. Wahrscheinlich ist das Parent-Tag bereits defekt."
                            errorMessages.Append("An dieser Stelle sind keine Tags erlaubt. Wahrscheinlich ist das Parent-Tag bereits defekt.");
                        }
                        return false;
                    }
                }
                catch (DTD.XMLUnknownElementException e)
                {
                    // "Unbekanntes Element '{0}'"
                    errorMessages.AppendFormat($"Unbekanntes Element '{e.ElementName}'");
                    return false;
                }
            }
            else // Das Element dieses Nodes ist in der DTD gar nicht bekannt
            {
                //  "Unbekanntes Element '{0}'"
                errorMessages.AppendFormat($"Unbekanntes Element '{DTD.GetElementNameFromNode(node)}'");
                return false;
            }
        }

        /// <summary>
        /// Prüft ein Attribut gegen die DTD
        /// </summary>
        /// <param name="attribut"></param>
        /// <returns></returns>
        private bool CheckAttribute(System.Xml.XmlAttribute attribut)
        {
            return false;
        }

        /// <summary>
        /// Setzt die Ergebnisse der letzten Prüfung auf Null zurück
        /// </summary>
        private void Reset()
        {
            errorMessages = new StringBuilder();
        }
    }
}
