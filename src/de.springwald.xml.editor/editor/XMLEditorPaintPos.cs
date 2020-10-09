namespace de.springwald.xml.editor
{
    /// <summary>
    /// Stellt Informationen über die aktuellen Zeichenpositionen der aktuellen XML-Zeile und des nächsten
    /// Fließtext-Elementes bereit
    /// </summary>
    /// <remarks>
    // (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    public class XMLEditorPaintPos
    {

        /// <summary>
        /// Dorthin können die aktuellen Fließtext-Elemente umbrechen, wenn die Zeile voll ist
        /// </summary>
        public int ZeilenStartX { get; set; }

        /// <summary>
        /// Dort sollten die aktuellen Fließtext-Elemente umbrechen, sofern irgend möglich
        /// </summary>
        public int ZeilenEndeX { get; set; }

        /// <summary>
        /// Dort kann das nächste Fließtexte-Element auf der Horizontalen gezeichnet werden 
        /// </summary>
        public int PosX { get; set; }

        /// <summary>
        /// Dort kann das nächste Fließtexte-Element gezeichnet werden
        /// </summary>
        public int PosY { get; set; }

        /// <summary>
        /// So viel Platz benötigt die aktuelle Zeile  bereits in der Höhe. Der nächste Zeilenumbruch muss
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
