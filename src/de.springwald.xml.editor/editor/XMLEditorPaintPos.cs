// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Stellt Informationen �ber die aktuellen Zeichenpositionen der aktuellen XML-Zeile und des n�chsten
    /// Flie�text-Elementes bereit
    /// </summary>

    public class XMLEditorPaintPos
    {

        /// <summary>
        /// Dorthin k�nnen die aktuellen Flie�text-Elemente umbrechen, wenn die Zeile voll ist
        /// </summary>
        public int ZeilenStartX { get; set; }

        /// <summary>
        /// Dort sollten die aktuellen Flie�text-Elemente umbrechen, sofern irgend m�glich
        /// </summary>
        public int ZeilenEndeX { get; set; }

        /// <summary>
        /// Dort kann das n�chste Flie�texte-Element auf der Horizontalen gezeichnet werden 
        /// </summary>
        public int PosX { get; set; }

        /// <summary>
        /// Dort kann das n�chste Flie�texte-Element gezeichnet werden
        /// </summary>
        public int PosY { get; set; }

        /// <summary>
        /// So viel Platz ben�tigt die aktuelle Zeile  bereits in der H�he. Der n�chste Zeilenumbruch muss
        /// daher mindestens diesen Abstand zum oberen Rand der Zeile haben
        /// </summary>
        public int HoeheAktZeile { get; set; }

        /// <summary>
        /// Das bisher gefunden x-Maximum
        /// </summary>
        public int BisherMaxX { get; set; }


        public XMLEditorPaintPos()
        {
            ZeilenStartX = 0;
            ZeilenEndeX = 0;
            PosX = 0;
            PosY = 0;
            HoeheAktZeile = 0;
            BisherMaxX = PosX;
        }


        /// <summary>
		/// Erzeugt eine Inhaltsgleiche Kopie dieses PaintPos-Objektes
		/// </summary>
		public XMLEditorPaintPos Clone()
        {
            XMLEditorPaintPos paintPos = new XMLEditorPaintPos()
            {
                ZeilenStartX = ZeilenStartX,
                ZeilenEndeX = ZeilenEndeX,
                PosX = PosX,
                PosY = PosY,
                HoeheAktZeile = HoeheAktZeile,
                BisherMaxX = BisherMaxX
            };
            return paintPos;
        }

    }
}
