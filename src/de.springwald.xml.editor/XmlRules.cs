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
        private DtdChecker dtdChecker;
        private DtdNodeEditCheck dtdNodeEditChecker;

        /// <summary>Die Gruppen, in welchen XML-Elemente gruppiert zum Einfügen vorgeschlagen werden können</summary>
        protected List<XmlElementGroup> elementGroups;

        /// <summary>
        /// Prüft Nodes und Attribute etc. innerhalb eines Dokumentes darauf hin, ob sie erlaubt sind
        /// </summary>
        public DtdChecker DtdChecker
        {
            get
            {
                if (dtdChecker == null) // Noch kein DTD-Prüfer instanziert
                {
                    if (this.Dtd == null) // Noch keine DTD zugewiesen
                    {
                        throw new ApplicationException("No DTD attached!");
                    }
                    dtdChecker = new DtdChecker(this.Dtd); // Neuen DTD-Prüfer für die DTD erzeugen
                }
                return dtdChecker;
            }
        }

        /// <summary>
        /// Wenn eine DTD zugewiesen ist, dann steht diese hier
        /// </summary>
        public Dtd Dtd { get; }

        /// <summary>
        /// Die Gruppen, in welchen XML-Elemente gruppiert zum Einfügen vorgeschlagen werden können
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
        /// Ermittelt die Farbe, in welcher dieser Node gezeichnet werden soll
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual Color NodeColor(System.Xml.XmlNode node)
        {
            return Color.LightBlue;
        }

        /// <summary>
        /// In welcher Art soll der übergebene Node gezeichnet werden?
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
        /// Wird der übergebene Node 2x gezeichnet, einmal mit > und einmal mit < ?
        /// </summary>
        public virtual bool HasEndTag(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode is System.Xml.XmlText) return false;

            var element = this.Dtd.DTDElementByNode_(xmlNode, false); // Das betroffene DTD-Element holen

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
        /// <param name="tagName">Der Name des Tags</param>
        /// <param name="cursorPos">Die zu prüfende Position</param>
        public bool IsThisTagAllowedAtThisPos(string tagName, XmlCursorPos targetPos)
        {
            // Die Liste der erlaubten Tags holen und schauen, ob darin das Tag vorkommt
            return this.AllowedInsertElements(targetPos, true, true).Contains(tagName);
        }

        /// <summary>
        /// Definiert, welche XML-Elemente an dieser Stelle eingefügt werden dürfen
        /// </summary>
        /// <param name="targetPos"></param>
        /// <param name="alsoListPpcData">wenn true, wird PCDATA mit als Node aufgeführt, sofern er erlaubt ist</param>
        /// <returns>Eine Auflistung der Nodenamen. Null bedeutet, es sind keine Elemente zugelassen.
        /// Ist der Inhalt "", dann ist das Element frei einzugeben </returns>
        public virtual string[] AllowedInsertElements(XmlCursorPos targetPos, bool alsoListPpcData, bool alsoListComments)
        {
#warning evtl. Optimierungs-TODO:
            // Wahrscheinlich (allein schon durch die Nutzung von IstDiesesTagAnDieserStelleErlaubt() etc.)
            // wird diese Liste oft hintereinander identisch neu erzeugt. Es macht daher Sinn, wenn der
            // das letzte Ergebnis hier ggf. gebuffert würde. Dabei sollte aber ausgeschlossen werden, dass
            // sich der XML-Inhalt in der Zwischenzeit geändert hat!

            if (targetPos.ActualNode == null) return new string[] { }; // Wenn nichts gewählt ist, ist auch nichts erlaubt

            if (this.Dtd == null) // Keine DTD hinterlegt
            {
                return new string[] { "" }; // Freie Eingabe erlaubt
            }
            else
            {
                if (dtdNodeEditChecker == null)
                {
                    dtdNodeEditChecker = new DtdNodeEditCheck(this.Dtd);
                }
                return dtdNodeEditChecker.AtThisPosAllowedTags(targetPos, alsoListPpcData, alsoListComments);
            }
            //string[] s = {"","node1","node2"}; // Freie Eingabe oder Node1 oder Node2 erlaubt
        }

        /// <summary>
        /// Konvertiert / Formatiert einen Text, welcher an eine bestimmte Stelle eingefügt werden soll
        /// so, wie es diese Stelle erfordert. In einer AIML-DTD kann dies z.B. bedeuten, dass der
        /// Text zum Einfügen in das PATTERN Tag auf Großbuchstaben umgestellt wird
        /// </summary>
        /// <param name="textToInsert"></param>
        /// <param name="insertWhere"></param>
        /// <returns></returns>
        /// <param name="replacementNode">Wenn statt des Textes ein Node eingefügt werden soll. Beispiel: Im
        /// AIML-Template wir * gedrückt, dann wird ein STAR-Tag eingefügt</param>
        public virtual string InsertTextTextPreProcessing(string textToInsert, XmlCursorPos insertWhere, out System.Xml.XmlNode replacementNode)
        {
            replacementNode = null;
            return textToInsert; // In der Standardform geht der Text immer durch
        }


    }
}
