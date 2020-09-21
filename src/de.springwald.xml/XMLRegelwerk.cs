using de.springwald.xml.cursor;
using de.springwald.xml.dtd;
using de.springwald.xml.dtd.pruefer;
using de.springwald.xml.editor;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Linq;

namespace de.springwald.xml
{

    /// <summary>
    /// Diese Arten der Darstellung kann ein XML-Element im Editor annehmen
    /// </summary>
	public enum DarstellungsArten { Fliesselement = 1, EigeneZeile };

    /// <summary>
    /// Die Beschreibung der Regeln f�r Anlage und Abh�ngigkeiten der ucXMLElemente.
    /// </summary>
    /// <remarks>
    /// Sch�n w�re es, wenn hier sp�ter einmal ein DTD-Import m�glich ist
    /// </remarks>
    /// <remarks>
    /// (C)2005 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>

    public class XMLRegelwerk
    {

        private DTD _dtd;                   // Wenn eine DTD zugewiesen ist, dann steht diese hier
        private DTDPruefer _dtdPruefer;
        private de.springwald.xml.dtd.DTDNodeEditCheck _checker;

        /// <summary>Die Gruppen, in welchen XML-Elemente gruppiert zum Einf�gen vorgeschlagen werden k�nnen</summary>
        protected XMLElementGruppenListe _elementGruppen;

        /// <summary>
        /// Pr�ft Nodes und Attribute etc. innerhalb eines Dokumentes darauf hin, ob sie erlaubt sind
        /// </summary>
        public de.springwald.xml.dtd.pruefer.DTDPruefer DTDPruefer
        {
            get
            {
                if (_dtdPruefer == null) // Noch kein DTD-Pr�fer instanziert
                {
                    if (_dtd == null) // Noch keine DTD zugewiesen
                    {
                        throw new ApplicationException("No DTD attached!");
                    }
                    _dtdPruefer = new DTDPruefer(_dtd); // Neuen DTD-Pr�fer f�r die DTD erzeugen
                }
                return _dtdPruefer;
            }
        }

        /// <summary>
        /// Wenn eine DTD zugewiesen ist, dann steht diese hier
        /// </summary>
        public de.springwald.xml.dtd.DTD DTD
        {
            get { return _dtd; }
        }

        /// <summary>
        ///  Soweit wird ein Child-Element in einer neuen Zeile einger�ckt
        /// </summary>
        public virtual int ChildEinrueckungX
        {
            get { return 20; }
        }

        /// <summary>
        /// Der Abstand zwischen zwei Zeilen
        /// </summary>
        public virtual int AbstandYZwischenZeilen
        {
            get { return 5; }
        }

        /// <summary>
        /// Der Abstand zwischen zwei Fliesstextelementen
        /// </summary>
        public virtual int AbstandFliessElementeX
        {
            get { return 0; }
        }

        /// <summary>
        /// Die Gruppen, in welchen XML-Elemente gruppiert zum Einf�gen vorgeschlagen werden k�nnen
        /// </summary>
        public virtual XMLElementGruppenListe ElementGruppen
        {
            get
            {
                if (_elementGruppen == null)
                {
                    _elementGruppen = new XMLElementGruppenListe();
                }
                return _elementGruppen;
            }
        }



        public XMLRegelwerk(de.springwald.xml.dtd.DTD dtd)
        {
            _dtd = dtd;
        }

        public XMLRegelwerk()
        {
            _dtd = null;
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
        /// In welcher Art soll der �bergebene Node gezeichnet werden?
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public virtual DarstellungsArten DarstellungsArt(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode is System.Xml.XmlText) return DarstellungsArten.Fliesselement;
            if (xmlNode is System.Xml.XmlWhitespace) return DarstellungsArten.Fliesselement;
            if (xmlNode is System.Xml.XmlComment) return DarstellungsArten.EigeneZeile;

            if (IstSchliessendesTagSichtbar(xmlNode))
            {
                return DarstellungsArten.EigeneZeile;
            }
            else
            {
                return DarstellungsArten.Fliesselement;
            }
        }

        /// <summary>
        /// Wird der �bergebene Node 2x gezeichnet, einmal mit > und einmal mit < ?
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public virtual bool IstSchliessendesTagSichtbar(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode is System.Xml.XmlText) return false;

            DTDElement element = _dtd.DTDElementByNode_(xmlNode, false); // Das betroffene DTD-Element holen

            if (element != null)
            {
                if (element.AlleElementNamenWelcheAlsDirektesChildZulaessigSind.Count > 1) // Das Element kann Unterelement haben (> 1 statt 0, weil Kommentar ist immer dabei)
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
        /// <param name="cursorPos">Die zu pr�fende Position</param>
        /// <returns></returns>
        public bool IstDiesesTagAnDieserStelleErlaubt(string tagname, XMLCursorPos zielPunkt)
        {
            // Die Liste der erlaubten Tags holen und schauen, ob darin das Tag vorkommt
            return (from e in ErlaubteEinfuegeElemente_(zielPunkt, true, true)
                    where e == tagname
                    select e).Count() > 0;
        }


        /// <summary>
        /// Definiert, welche XML-Elemente an dieser Stelle eingef�gt werden d�rfen
        /// </summary>
        /// <param name="zielPunkt"></param>
        /// <param name="pcDATAMitAuflisten">wenn true, wird PCDATA mit als Node aufgef�hrt, sofern er erlaubt ist</param>
        /// <returns>Eine Auflistung der Nodenamen. Null bedeutet, es sind keine Elemente zugelassen.
        /// Ist der Inhalt "", dann ist das Element frei einzugeben </returns>
        public virtual string[] ErlaubteEinfuegeElemente_(XMLCursorPos zielPunkt, bool pcDATAMitAuflisten, bool kommentareMitAuflisten)
        {

#warning evtl. Optimierungs-TODO:
            // Wahrscheinlich (allein schon durch die Nutzung von IstDiesesTagAnDieserStelleErlaubt() etc.)
            // wird diese Liste oft hintereinander identisch neu erzeugt. Es macht daher Sinn, wenn der
            // das letzte Ergebnis hier ggf. gebuffert w�rde. Dabei sollte aber ausgeschlossen werden, dass
            // sich der XML-Inhalt in der Zwischenzeit ge�ndert hat!

            if (zielPunkt.AktNode == null) return new string[] { }; // Wenn nichts gew�hlt ist, ist auch nichts erlaubt

            if (this._dtd == null) // Keine DTD hinterlegt
            {
                return new string[] { "" }; // Freie Eingabe erlaubt
            }
            else
            {
                if (_checker == null)
                {
                    _checker = new DTDNodeEditCheck(_dtd);
                }
                return _checker.AnDieserStelleErlaubteTags_(zielPunkt, pcDATAMitAuflisten, kommentareMitAuflisten);
            }

            //string[] s = {"","node1","node2"}; // Freie Eingabe oder Node1 oder Node2 erlaubt

        }





        /// <summary>
        /// Konvertiert / Formatiert einen Text, welcher an eine bestimmte Stelle eingef�gt werden soll
        /// so, wie es diese Stelle erfordert. In einer AIML-DTD kann dies z.B. bedeuten, dass der
        /// Text zum Einf�gen in das PATTERN Tag auf Gro�buchstaben umgestellt wird
        /// </summary>
        /// <param name="einfuegeText"></param>
        /// <param name="woEinfuegen"></param>
        /// <returns></returns>
        /// <param name="ersatzNode">Wenn statt des Textes ein Node eingef�gt werden soll. Beispiel: Im
        /// AIML-Template wir * gedr�ckt, dann wird ein STAR-Tag eingef�gt</param>
        public virtual string EinfuegeTextPreProcessing(string einfuegeText, XMLCursorPos woEinfuegen, out System.Xml.XmlNode ersatzNode)
        {
            ersatzNode = null;
            return einfuegeText; // In der Standardform geht der Text immer durch
        }


    }
}