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
using de.springwald.xml.editor;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.rules;
using de.springwald.xml.rules.dtd;

namespace de.springwald.xml
{

    /// <summary>
    /// Diese Arten der Darstellung kann ein XML-Element im Editor annehmen
    /// </summary>
	public enum DisplayTypes { FloatingElement = 1, OwnRow };

    /// <summary>
    /// Die Beschreibung der Regeln für Anlage und Abhängigkeiten der ucXMLElemente.
    /// </summary>
    public class XmlRules
    {
        private DtdChecker _dtdPruefer;
        private DtdNodeEditCheck _checker;

        /// <summary>Die Gruppen, in welchen XML-Elemente gruppiert zum Einfügen vorgeschlagen werden können</summary>
        protected List<XmlElementGroup> _elementGruppen;

        /// <summary>
        /// Prüft Nodes und Attribute etc. innerhalb eines Dokumentes darauf hin, ob sie erlaubt sind
        /// </summary>
        public DtdChecker DTDPruefer
        {
            get
            {
                if (_dtdPruefer == null) // Noch kein DTD-Prüfer instanziert
                {
                    if (this.DTD == null) // Noch keine DTD zugewiesen
                    {
                        throw new ApplicationException("No DTD attached!");
                    }
                    _dtdPruefer = new DtdChecker(this.DTD); // Neuen DTD-Prüfer für die DTD erzeugen
                }
                return _dtdPruefer;
            }
        }

        /// <summary>
        /// Wenn eine DTD zugewiesen ist, dann steht diese hier
        /// </summary>
        public DTD DTD { get; }

        /// <summary>
        /// Die Gruppen, in welchen XML-Elemente gruppiert zum Einfügen vorgeschlagen werden können
        /// </summary>
        public virtual List<XmlElementGroup> ElementGruppen
        {
            get
            {
                if (_elementGruppen == null)
                {
                    _elementGruppen = new List<XmlElementGroup>();
                }
                return _elementGruppen;
            }
        }

        public XmlRules(DTD dtd)
        {
            this.DTD = dtd;
        }

        /// <summary>
        /// Ermittelt die Farbe, in welcher dieser Node gezeichnet werden soll
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual Color NodeFarbe(System.Xml.XmlNode node, bool selektiert)
        {
            if (selektiert)
            {
                return Color.DarkBlue;
            }
            else
            {
                return Color.FromArgb(245, 245, 255);
            }
        }

        /// <summary>
        /// In welcher Art soll der übergebene Node gezeichnet werden?
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public virtual DisplayTypes DisplayType(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode is System.Xml.XmlText) return DisplayTypes.FloatingElement;
            if (xmlNode is System.Xml.XmlWhitespace) return DisplayTypes.FloatingElement;
            if (xmlNode is System.Xml.XmlComment) return DisplayTypes.OwnRow;
            if (HasEndTag(xmlNode))
            {
                return DisplayTypes.OwnRow;
            }
            else
            {
                return DisplayTypes.FloatingElement;
            }
        }

        /// <summary>
        /// Wird der übergebene Node 2x gezeichnet, einmal mit > und einmal mit < ?
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public virtual bool HasEndTag(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode is System.Xml.XmlText) return false;

            DTDElement element = this.DTD.DTDElementByNode_(xmlNode, false); // Das betroffene DTD-Element holen

            if (element != null)
            {
                if (element.AlleElementNamenWelcheAlsDirektesChildZulaessigSind.Length > 1) // Das Element kann Unterelement haben (> 1 statt 0, weil Kommentar ist immer dabei)
                {
                    return true;
                }
                else // Das Element kann keine Unterelemte haben
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Ermittelt, ob das angegebene Tag an dieser Stelle erlaubt ist
        /// </summary>
        /// <param name="tagname">Der Name des Tags</param>
        /// <param name="cursorPos">Die zu prüfende Position</param>
        /// <returns></returns>
        public bool IstDiesesTagAnDieserStelleErlaubt(string tagname, XmlCursorPos zielPunkt)
        {
            // Die Liste der erlaubten Tags holen und schauen, ob darin das Tag vorkommt
            return ErlaubteEinfuegeElemente_(zielPunkt, true, true).Contains(tagname);
        }

        /// <summary>
        /// Definiert, welche XML-Elemente an dieser Stelle eingefügt werden dürfen
        /// </summary>
        /// <param name="zielPunkt"></param>
        /// <param name="pcDATAMitAuflisten">wenn true, wird PCDATA mit als Node aufgeführt, sofern er erlaubt ist</param>
        /// <returns>Eine Auflistung der Nodenamen. Null bedeutet, es sind keine Elemente zugelassen.
        /// Ist der Inhalt "", dann ist das Element frei einzugeben </returns>
        public virtual string[] ErlaubteEinfuegeElemente_(XmlCursorPos zielPunkt, bool pcDATAMitAuflisten, bool kommentareMitAuflisten)
        {
#warning evtl. Optimierungs-TODO:
            // Wahrscheinlich (allein schon durch die Nutzung von IstDiesesTagAnDieserStelleErlaubt() etc.)
            // wird diese Liste oft hintereinander identisch neu erzeugt. Es macht daher Sinn, wenn der
            // das letzte Ergebnis hier ggf. gebuffert würde. Dabei sollte aber ausgeschlossen werden, dass
            // sich der XML-Inhalt in der Zwischenzeit geändert hat!

            if (zielPunkt.ActualNode == null) return new string[] { }; // Wenn nichts gewählt ist, ist auch nichts erlaubt

            if (this.DTD == null) // Keine DTD hinterlegt
            {
                return new string[] { "" }; // Freie Eingabe erlaubt
            }
            else
            {
                if (_checker == null)
                {
                    _checker = new DtdNodeEditCheck(this.DTD);
                }
                return _checker.AnDieserStelleErlaubteTags_(zielPunkt, pcDATAMitAuflisten, kommentareMitAuflisten);
            }

            //string[] s = {"","node1","node2"}; // Freie Eingabe oder Node1 oder Node2 erlaubt

        }


        /// <summary>
        /// Konvertiert / Formatiert einen Text, welcher an eine bestimmte Stelle eingefügt werden soll
        /// so, wie es diese Stelle erfordert. In einer AIML-DTD kann dies z.B. bedeuten, dass der
        /// Text zum Einfügen in das PATTERN Tag auf Großbuchstaben umgestellt wird
        /// </summary>
        /// <param name="einfuegeText"></param>
        /// <param name="woEinfuegen"></param>
        /// <returns></returns>
        /// <param name="ersatzNode">Wenn statt des Textes ein Node eingefügt werden soll. Beispiel: Im
        /// AIML-Template wir * gedrückt, dann wird ein STAR-Tag eingefügt</param>
        public virtual string EinfuegeTextPreProcessing(string einfuegeText, XmlCursorPos woEinfuegen, out System.Xml.XmlNode ersatzNode)
        {
            ersatzNode = null;
            return einfuegeText; // In der Standardform geht der Text immer durch
        }


    }
}
