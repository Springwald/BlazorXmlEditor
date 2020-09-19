using System;
using System.Collections.Generic;
using System.Text;


namespace de.springwald.xml.editor.textnode
{

    public class TextTeiler
    {
        #region SYSTEM

        #endregion

        #region PRIVATE ATTRIBUTES

        private List<TextTeil> _textTeile;    // Die entstandenen Textteile

        #endregion

        #region PUBLIC ATTRIBUTES

        /// <summary>
        /// Bei diesen Zeichen im Text wird umbrochen
        /// </summary>
        private char[] _zeichenZumUmbrechen { get; set; }


        public List<TextTeil> TextTeile
        {
            get { return _textTeile; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">Der von diesem Textteiler verteilte Text</param>
        /// <param name="invertiertStart">Ab diesen Zeichen ist der Text invertiert; -1, wenn nichts invertiert werden soll</param>
        /// <param name="invertiertEnde">Bis zu diesem Zeichen ist der Text invertiert; -1, wenn nichts invertiert werden soll</param>
        /// <param name="maxLaengeProZeile">So lang darf die erste Zeile sein (in Buchstaben)</param>
        /// <param name="bereitsLaengeDerZeile">So viele Buchstaben der ersten Zeile sind bereits vorher belegt worden und müssen
        /// bei der ersten Zeile von der MaxLaenge abgezogen werden</param>
        public TextTeiler(string text, int invertiertStart, int invertiertLaenge, int maxLaengeProZeile, int bereitsLaengeDerZeile, char[] zeichenZumUmbrechen)
        {
            _zeichenZumUmbrechen = zeichenZumUmbrechen;

            // Erst auf Zeilen verteilen
            List<TextTeil> geteilt = TextUmbrechenUndAufTeileVerteilen(text, bereitsLaengeDerZeile, maxLaengeProZeile);

            // Dann noch ggf invertieren
            _textTeile = TextTeileGgfInvertieren(geteilt, invertiertStart, invertiertLaenge);
        }

        #endregion

        #region PUBLIC METHODS

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Falls der Text an einigen Stellen invertiert ist und an anderen nicht, so werden die entsprechenden
        /// Textteile invertiert. Ist ein Textteil teilweise beides, wird er in mehrere Textteile gesplittet,
        /// die dann entweder invertiert oder nicht sind 
        /// </summary>
        private List<TextTeil> TextTeileGgfInvertieren(List<TextTeil> rohTeile, int invertiertStart, int invertiertLaenge)
        {
            if (invertiertLaenge == 0) return rohTeile; // Wenn nix markiert ist, dann alles 1:1 zurückgeben

            List<TextTeil> teile = new List<TextTeil>();

            int startDiesesTeiles = 0; // Wo im Gesamttext beginnt der nächste Teil

            foreach (TextTeil rohTeil in rohTeile)
            {

                bool zeilenUmbruchVorDiesemTeil = rohTeil.IstNeueZeile;

                if (invertiertStart > startDiesesTeiles + rohTeil.Text.Length)
                {
                    // Die Invertierung beginnt erst vor diesem Teil, also RohTeil 1:1 übernehmen
                    rohTeil.Invertiert = false;
                    teile.Add(rohTeil);
                }
                else  // Die Invertierung beginnt nicht erst hinter diesem Teil
                {
                    if (invertiertStart + invertiertLaenge < startDiesesTeiles)
                    {
                        // Die Invertierung liegt vor diesem Teil, also RohTeil 1:1 übernehmen
                        teile.Add(rohTeil);
                    }
                    else  // Die Invertierung betrifft diesen Teil
                    {
                        int invertiertLaengeInDiesemTeil = invertiertLaenge + invertiertStart - startDiesesTeiles;
                        invertiertLaengeInDiesemTeil = Math.Min(invertiertLaengeInDiesemTeil, rohTeil.Text.Length);
                        invertiertLaengeInDiesemTeil = Math.Max(invertiertLaengeInDiesemTeil, 0);

                        int anzahlVorher = 0;
                        if (invertiertStart > startDiesesTeiles) // Der Teil beginnt mit einem nicht-invertierten Teil
                        {
                            TextTeil vorher = new TextTeil();
                            vorher.Invertiert = false;
                            anzahlVorher = invertiertStart - startDiesesTeiles;
                            vorher.Text = rohTeil.Text.Substring(0, anzahlVorher);
                            vorher.IstNeueZeile = zeilenUmbruchVorDiesemTeil;
                            zeilenUmbruchVorDiesemTeil = false;
                            teile.Add(vorher);
                            invertiertLaengeInDiesemTeil -= anzahlVorher;
                        }

                        // den invertierten Bereich isolieren
                        int anzahlMitte = invertiertLaengeInDiesemTeil;
                        TextTeil mitte = new TextTeil();
                        mitte.Invertiert = true;
                        mitte.Text = rohTeil.Text.Substring(anzahlVorher, anzahlMitte);
                        mitte.IstNeueZeile = zeilenUmbruchVorDiesemTeil;
                        zeilenUmbruchVorDiesemTeil = false;
                        teile.Add(mitte);

                        int anzahlEnde = rohTeil.Text.Length - (anzahlVorher + anzahlMitte);
                        if (anzahlEnde > 0)
                        {
                            // Hinter der Invertierung folgt noch ein nicht-invertierter Teil
                            TextTeil ende = new TextTeil();
                            ende.Invertiert = false;
                            ende.Text = rohTeil.Text.Substring(rohTeil.Text.Length - anzahlEnde, anzahlEnde);
                            ende.IstNeueZeile = zeilenUmbruchVorDiesemTeil;
                            teile.Add(ende);
                        }
                    }
                }

                startDiesesTeiles += rohTeil.Text.Length; // Den Cursor zum Anfang des nächsten Teiles verschieben
            }

            return teile;
        }

        /*
             private void TextFuerInvertierungTeilen()
             {
                 if (_invertiertStart + _invertiertLaenge > _text.Length ) {
                     Debug.Assert(false, "InvertierungsEnde liegt hinter dem Ende des übergebenen Textes (TextFuerInvertierungTeilen)");
                     _invertiertLaenge = 0;
                 }

                 if (_invertiertLaenge == 0)
                 {
                     TextAufTextTeileVerteilen(_text, false); // nix invertiert 
                 }
                 else // es scheint etwas zu invertieren sein
                 {
                     if (_invertiertStart != 0) // Invertierung beginnt nicht ganz vorn, also noch den nicht-Invertierten Bereich vorher verarbeiten
                     {
                         // Vom Anfang bis zum Start der Invertierung 
                         TextAufTextTeileVerteilen(_text.Substring(0, _invertiertStart), false); 
                     }

                     // Den Invertierten Teil verarbeiten
                     TextAufTextTeileVerteilen(_text.Substring(_invertiertStart,_invertiertLaenge),true); 

                     // Wenn das Invertier-Ende nicht mit dem Ende des Textes zusammenfällt, dann den Rest normal übergeben
                     if (_invertiertStart + _invertiertLaenge < _text.Length )
                     {
                         TextAufTextTeileVerteilen(_text.Substring(_invertiertStart + _invertiertLaenge, _text.Length - (_invertiertStart + _invertiertLaenge)), false); 
                     }
                 }
             }
             */


        /// <summary>
        /// Verteilt den gesamten Text des Textnodes auf einzelne Textteile. Teilungsgründe können z.B.
        /// zu lange Texte sein (Zeilenumbruch) 
        /// Wenn besondere Umbruch-Zeichen angegeben sind, wird vorher noch danach umbrochen
        /// </summary>
        private List<TextTeil> TextUmbrechenUndAufTeileVerteilen(string text, int laengeAktZeile, int maxLaengeProZeile)
        {
            if (_zeichenZumUmbrechen != null)
            {
                // Es sind besondere Zeichen angegeben, mit welchen im Text ein direkter Umbruch erzwungen werden soll
                // das kann z.B. bei einem Script in AIML das { } oder ; sein
                char umbruchPlaceholder = '·';

                //text = text.Replace(umbruchPlaceholder,' ');
                foreach (char umbrecher in _zeichenZumUmbrechen)
                {
                    text = text.Replace(umbrecher.ToString(), string.Format("{0}{1}", umbrecher, umbruchPlaceholder));
                }
                bool erster = true;
                List<TextTeil> teile = new List<TextTeil>();

                string[] texteVorUmbrochen = text.Split(new char[] { umbruchPlaceholder }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string textTeil in texteVorUmbrochen)
                {
                    List<TextTeil> temp = TextAufTextTeileVerteilen(textTeil, laengeAktZeile, maxLaengeProZeile);
                    TextTeil letzterTeil = null;
                    foreach (TextTeil teil in temp)
                    {
                        letzterTeil = teil;
                        teile.Add(teil);
                    }
                    if (letzterTeil != null)
                    {
                        if (erster)
                        {
                            // der erste Teil ist keine neue Zeile
                            erster = false;
                        }
                        else
                        {
                            letzterTeil.IstNeueZeile = true;
                        }
                    }
                }
                return teile;
            }
            else
            {
                // Kein Umbrechen nach vorgegebenen Umbrechern
                return TextAufTextTeileVerteilen(text, laengeAktZeile, maxLaengeProZeile);
            }
        }

        /// <summary>
        /// Verteilt den gesamten Text des Textnodes auf einzelne Textteile. Teilungsgründe können z.B.
        /// zu lange Texte sein (Zeilenumbruch) 
        /// </summary>
        private List<TextTeil> TextAufTextTeileVerteilen(string text, int laengeAktZeile, int maxLaengeProZeile)
        {

            List<TextTeil> teile = new List<TextTeil>();

            StringBuilder aktText = new StringBuilder();
            bool aktIstNeueZeile = false;

            int bereitsVerbraucht = 0; // Bis zu welches Stelle des Textes ist bereits gearbeitet worden?

            int nextTrenner = text.IndexOf(' ', 0); // wo ist die erste Trennungsmöglichkeit

            while (bereitsVerbraucht < text.Length)
            {
                if (nextTrenner == -1)  // Kein Trenner mehr -  die Zeile besteht nur noch aus dem folgenden Wort
                {
                    string wort = text.Substring(bereitsVerbraucht, text.Length - bereitsVerbraucht);
                    aktText.Append(wort);   // alles in die aktuelle Zeile
                    laengeAktZeile += wort.Length;
                    bereitsVerbraucht = text.Length; // nix mehr zum Verteilen übrig
                }
                else
                {
                    nextTrenner++;

                    string wort = text.Substring(bereitsVerbraucht, nextTrenner - bereitsVerbraucht);

                    int restPlatzInZeile = maxLaengeProZeile - laengeAktZeile;

                    if (restPlatzInZeile <= wort.Length)  // Der nächste Trenner liegt hinter der gewünschten MaxLänge einer Zeile
                    {
                        if (bereitsVerbraucht == 0) // Geht zwar über die Zeile hinaus, ist aber offenbar nicht besser trennbar
                        {
                            // Kommt zähneknirschend dann doch noch in die akuelle Zeile hinein
                            aktText.Append(wort);
                            // Textteil abschließen
                            TextTeil neuerTeil = new TextTeil();
                            neuerTeil.Text = aktText.ToString();
                            neuerTeil.IstNeueZeile = aktIstNeueZeile;
                            neuerTeil.Invertiert = false;
                            teile.Add(neuerTeil);
                            // für neuen Textteil leeren und neue Zeile beginnen
                            aktText = new StringBuilder();
                            aktIstNeueZeile = true;
                            laengeAktZeile = 0;
                        }
                        else // für das nächste Wort eine neue Zeile beginnen
                        {
                            // Textteil abschließen
                            TextTeil neuerTeil = new TextTeil();
                            neuerTeil.Text = aktText.ToString();
                            neuerTeil.IstNeueZeile = aktIstNeueZeile;
                            neuerTeil.Invertiert = false;
                            teile.Add(neuerTeil);
                            // für neuen Textteil leeren
                            aktText = new StringBuilder();
                            // Das Worte als Start des neuen Textteiles einsetzen
                            aktText.Append(wort);
                            aktIstNeueZeile = true;
                            laengeAktZeile = wort.Length;
                        }
                    }
                    else // Das Wort passt noch prima in die akuelle Zeile
                    {
                        aktText.Append(wort);
                        laengeAktZeile += wort.Length;
                    }

                    // das nun eingesetzte Wort aus dem Rest-Text entfernen
                    bereitsVerbraucht = nextTrenner;

                    // wo ist die nächste Trennungsmöglichkeit
                    nextTrenner = text.IndexOf(' ', bereitsVerbraucht);
                }
            }

            // Wenn noch etwas im Zeilenbuffer steckt, dann hier noch einsetzen
            // Textteil abschließen
            if (aktText.Length != 0)
            {
                TextTeil neuerTeilRest = new TextTeil();
                neuerTeilRest.Text = aktText.ToString();
                neuerTeilRest.IstNeueZeile = aktIstNeueZeile;
                neuerTeilRest.Invertiert = false;
                teile.Add(neuerTeilRest);
            }

            return teile;
        }

        #endregion
    }
}
