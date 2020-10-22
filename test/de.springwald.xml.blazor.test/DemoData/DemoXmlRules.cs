using de.springwald.xml.cursor;
using de.springwald.xml.editor;
using de.springwald.xml.editor.nativeplatform.gfx;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace de.springwald.xml.blazor.test.DemoData
{
    /// <summary>
    /// Die Regeln, wie die AIML-XML-Elemente im Zusammenhang stehen
    /// </summary>
    /// <remarks>
    /// (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    public class DemoXmlRules : de.springwald.xml.XMLRegelwerk
    {
        /// <summary>
        /// Die Gruppierungen, in welchen die XML-Elemente zum Einfügen angeboten werden
        /// </summary>
        public override List<XMLElementGruppe> ElementGruppen
        {
            get
            {
                if (_elementGruppen == null)
                {
                    _elementGruppen = new List<XMLElementGruppe>();

                    // Unwichtige Gruppen zuerst mal zusammenklappen?
                    const bool zusammengeklappt = true;

                    // Die Gruppe der Standard-Elemente
                    XMLElementGruppe standard = new XMLElementGruppe("standard", false);
                    standard.AddElementName("bot");
                    standard.AddElementName("get");
                    standard.AddElementName("li");
                    standard.AddElementName("pattern");
                    standard.AddElementName("random");
                    standard.AddElementName("set");
                    standard.AddElementName("srai");
                    standard.AddElementName("sr");
                    standard.AddElementName("star");
                    standard.AddElementName("template");
                    standard.AddElementName("that");
                    standard.AddElementName("thatstar");
                    standard.AddElementName("think");
                    _elementGruppen.Add(standard);

                    // Die Gruppe der Fortgeschrittenen-Elemente
                    XMLElementGruppe fortschritten = new XMLElementGruppe("advanced", zusammengeklappt);
                    fortschritten.AddElementName("condition");
                    fortschritten.AddElementName("formal");
                    fortschritten.AddElementName("gender");
                    fortschritten.AddElementName("input");
                    fortschritten.AddElementName("person");
                    fortschritten.AddElementName("person2");
                    fortschritten.AddElementName("sentence");
                    _elementGruppen.Add(fortschritten);

                    // Die Gruppe der HTML-Elemente
                    XMLElementGruppe html = new XMLElementGruppe("html", zusammengeklappt);
                    html.AddElementName("a");
                    html.AddElementName("applet");
                    html.AddElementName("br");
                    html.AddElementName("em");
                    html.AddElementName("img");
                    html.AddElementName("p");
                    html.AddElementName("table");
                    html.AddElementName("ul");
                    _elementGruppen.Add(html);

                    // Die Gruppe der besonderen GaitoBot-Elemente
                    XMLElementGruppe gaitobot = new XMLElementGruppe("GaitoBot", zusammengeklappt);
                    gaitobot.AddElementName("script");
                    _elementGruppen.Add(gaitobot);
                }
                return _elementGruppen;
            }
        }

        public DemoXmlRules(de.springwald.xml.dtd.DTD dtd) : base(dtd) { }

        /// <summary>
        /// Findet heraus, welche Farbe der Node haben soll
        /// </summary>
        /// <param name="node"></param>
        /// <param name="selektiert"></param>
        /// <returns></returns>
        public override Color NodeFarbe(XmlNode node, bool selektiert)
        {
            if (!selektiert) // Erstmal verändern wir nur die Farben der Nodes im unselektierten Zustand
            {
                switch (node.Name)
                {

                    case "condition":
                        return Color.FromArgb(150,
                                              221,
                                              220);

                    case "li":
                        switch (node.ParentNode.Name)
                        {
                            case "random":
                                return Color.FromArgb(255, 243, 187);
                            case "condition":
                                return Color.FromArgb(200, 250, 250);
                        }
                        break;

                    case "random":
                        return Color.FromArgb(255, 211, 80);

                    case "think":
                        return Color.FromArgb(200, 200, 200);

                }
            }

            return base.NodeFarbe(node, selektiert);
        }



        /// <summary>
        /// Wird der übergebene Node 2x gezeichnet, einmal mit > und einmal mit < ?
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public override bool IstSchliessendesTagSichtbar(XmlNode xmlNode)
        {
            switch (xmlNode.Name)
            {
                case "that": // Ein That im Template ist geschlossenes Fliesstext-Element, eines in der Category eine eigene, offene Zeile

                    if (xmlNode.ParentNode.Name == "template")  // that liegt im template node
                    {
                        return false;
                    }
                    else  // steckt nicht im Template-Tag
                    {
                        return true;
                    }

                default:
                    return base.IstSchliessendesTagSichtbar(xmlNode);
            }
        }

        /// <summary>
        /// In welcher Art soll der übergebene Node gezeichnet werden?
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public override DarstellungsArten DarstellungsArt(System.Xml.XmlNode xmlNode)
        {

            if (xmlNode is System.Xml.XmlElement)
            {
                switch (xmlNode.Name)
                {
                    case "a":
                    case "set":
                    case "bot":
                    case "formal":
                    case "gender":
                    case "person":
                    case "person2":
                        return DarstellungsArten.Fliesselement;

                    case "think": // Kommt ein think direkt nach dem Template vor, dann erhält es eine eigene Zeile, kommt es im Fließtext vor, dann nicht

                        if (xmlNode.ParentNode.Name == "template")  // think liegt im template node
                        {
                            if (xmlNode.PreviousSibling != null) // es gibt ein Element vor dem Think
                            {
                                if (xmlNode.PreviousSibling.Name == "think")
                                { // Zwei Thinks/Thats aufeinander? Um Rekursion zu vermeiden,
                                    // bei der beide Thinks sich gegenseitig als Sibling fragen,
                                    // wird hier der Umbruch zurückgegeben
                                    //return DarstellungsArten.EigeneZeile ;
                                }
                                else
                                {
                                    if (DarstellungsArt(xmlNode.PreviousSibling) == DarstellungsArten.Fliesselement)
                                    {
                                        // direkt vor dem Think liegt ein Fliesstextelement, also ist das Think auch eines
                                        return DarstellungsArten.Fliesselement;
                                    }
                                }
                            }

                            if (xmlNode.NextSibling != null) // es gibt ein Element nach dem Think
                            {
                                if (DarstellungsArt(xmlNode.NextSibling) == DarstellungsArten.Fliesselement)
                                {
                                    // direkt vor dem Think liegt ein Fliesstextelement, also ist das Think auch eines
                                    return DarstellungsArten.Fliesselement;
                                }
                            }

                            return DarstellungsArten.EigeneZeile;
                        }
                        else
                        {
                            return DarstellungsArten.Fliesselement;  // steckt nicht im Template-Tag
                        }


                    case "that": // Ein That im Template ist Fliestext-Element, eines in der Category eine eigene Zeile

                        if (xmlNode.ParentNode.Name == "template")  // that liegt im template node
                        {
                            return DarstellungsArten.Fliesselement;
                        }
                        else  // steckt nicht im Template-Tag
                        {
                            return DarstellungsArten.EigeneZeile;
                        }

                    default: return base.DarstellungsArt(xmlNode);
                }
            }
            return base.DarstellungsArt(xmlNode);
        }

        /// <summary>
        /// Konvertiert / Formatiert einen Text, welcher an eine bestimmte Stelle eingefügt werden soll
        /// so, wie es diese Stelle erfordert. In einer AIML-DTD kann dies z.B. bedeuten, dass der
        /// Text zum Einfügen in das PATTERN Tag auf Großbuchstaben umgestellt wird
        /// </summary>
        /// <param name="ersatzNode">Wenn statt des Textes ein Node eingefügt werden soll. Beispiel: Im
        /// AIML-Template wir * gedrückt, dann wird ein STAR-Tag eingefügt</param>
        public override string EinfuegeTextPreProcessing(string einfuegeText, XMLCursorPos woEinfuegen, out System.Xml.XmlNode ersatzNode)
        {
            XmlNode node;

            if (woEinfuegen.AktNode is XmlText)
            { // Pos ist ein Textnode
                // Node ist der Parent des Textnode
                node = woEinfuegen.AktNode.ParentNode;
            }
            else
            { // Pos ist kein Textnode
                // Die Pos selbst ist der Node
                node = woEinfuegen.AktNode;
            }

            string ausgabe;

            // An bestimmten Stellen beim drücken von * statt dessen SRAI verwenden
            if (einfuegeText == "*")
            {
                switch (node.Name)
                {
                    case "pattern":
                    case "that":
                    case "script":
                        // Hier ist der normale Stern erlaub
                        break;
                    default:
                        ersatzNode = woEinfuegen.AktNode.OwnerDocument.CreateElement("star");
                        return ""; // Den einfüge-Text leeren, da er ja schon als Star-Node zurück gegeben wurde
                }
            }

            // Ja nach Node verschiedene Eingaben zulassen / herausfiltern
            switch (node.Name)
            {

                case "srai":        // Im Srai-Tag immer Großbuchstaben und keine Sonderzeichen
                    ausgabe = einfuegeText;
                    ausgabe = ausgabe.Replace("*", ""); // Kein * im SRAI erlaubt
                    ausgabe = ausgabe.Replace("_", ""); // Kein _ im SRAI erlaubt
                    ersatzNode = null;
                    return ausgabe;

                case "pattern":     // Im Pattern-Tag immer Großbuchstaben und keine Sonderzeichen

                    StringBuilder sauber = new StringBuilder(einfuegeText); // einfuegeText.ToUpper());
                    // Bei der Eingabe Umlaute bereits ausschreiben
                    sauber.Replace("Ä", "AE");
                    sauber.Replace("Ö", "OE");
                    sauber.Replace("Ü", "UE");
                    sauber.Replace("ß", "SS");

#warning Hier optimieren

                    // convert to a char array
                    char[] tempArray = sauber.ToString().ToCharArray();

                    ArrayList verarbeiteteZeichen = new ArrayList();

                    // iterate through the char array 
                    for (int i = 0; i < tempArray.Length; i++)
                    {
                        if (((tempArray[i] == '*') || (tempArray[i] == '_')) && (node.Name == "pattern"))
                        {
                            verarbeiteteZeichen.Add((char)tempArray[i]); // * und _ sind nur in Pattern, nicht im SRAI erlaubt
                        }
                        else
                        {
                            // check its a valid character...
                            // valid in this case means:
                            // " "(space), "0-9", "a-z" and "A-Z"
                            if ((tempArray[i] > 64) & (tempArray[i] < 91) || // A-Z
                                (tempArray[i] > 96) & (tempArray[i] < 123) || // a-z
                                (tempArray[i] > 47) & (tempArray[i] < 58) || // 0-9
                                (tempArray[i] == 32))  // space
                            {
                                verarbeiteteZeichen.Add((char)tempArray[i]);
                            }
                        }
                    }

                    // turn the arraylist into a char array
                    char[] verarbeitet = new char[verarbeiteteZeichen.Count];

                    for (int i = 0; i < verarbeiteteZeichen.Count; i++)
                    {
                        verarbeitet[i] = (char)verarbeiteteZeichen[i];
                    }
                    ausgabe = new string(verarbeitet);

                    // et voila!
                    ersatzNode = null;
                    return ausgabe;

                default:
                    return base.EinfuegeTextPreProcessing(einfuegeText, woEinfuegen, out ersatzNode);
            }
        }

        /// <summary>
        /// Ermittelt, welche Elemente an dieser Stelle erlaubt sind
        /// </summary>
        /// <param name="zielPunkt"></param>
        /// <param name="pcDATAMitAuflisten"></param>
        /// <param name="kommentareMitAuflisten"></param>
        /// <returns></returns>
        public override string[] ErlaubteEinfuegeElemente_(XMLCursorPos zielPunkt, bool pcDATAMitAuflisten, bool kommentareMitAuflisten)
        {
            if (zielPunkt.AktNode != null)
            {
                if (zielPunkt.AktNode.Name.ToLower() == "category")
                {
                    // Anstelle des Category-Tags werden keine anderen Tags
                    // alternativ angeboten. Sonst würde man bei der Bearbeitung
                    // der Category auch die Tags META und TOPIC angezeigt bekommen,
                    // da diese laut DTD an dieser Stelle ja erlaubt wären
                    return new string[] { };
                }
            }

            return base.ErlaubteEinfuegeElemente_(zielPunkt, pcDATAMitAuflisten, kommentareMitAuflisten);
        }

        /* 
         * Noch nicht in GB2020 übernommen:
         * 
         * public override XMLElement createPaintElementForNode(XmlNode xmlNode, XMLEditor xmlEditor)
           {
               if (xmlNode is System.Xml.XmlText)
               {
                   if (xmlNode.ParentNode != null)
                   {
                       if (xmlNode.ParentNode.Name.ToLower() == "script") 
                       {
                           return new
                           de.springwald.xml.editor.XMLElement_TextNode(xmlNode, xmlEditor)
                           {
                                ZeichenZumUmbrechen = new char[] {'}','{',';' }
                           };
                       }
                   }
               }

               return base.createPaintElementForNode(xmlNode, xmlEditor);
           }*/
    }
}
