namespace de.springwald.xml.dtd
{

    /// <summary>
    /// Ein einzelnes DTD-Attribut aus einer DTD
    /// </summary>
    /// <remarks>
    public class DTDAttribut
    {
        public enum PflichtArten { Pflicht, Optional, Konstante };

        /// <summary>
        /// Der Name des Attributes
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ist das Attribut Pflicht?
        /// </summary>
        public PflichtArten Pflicht { get; set; }

        /// <summary>
        /// Die Werte, welche für dieses Attribut erlaubt sind. NULL=Keine Vorgabe, alles ist erlaubt
        /// </summary>
        public string[] ErlaubteWerte { get; set; }

        /// <summary>
        /// Der Vorgabewert
        /// </summary>
        public string StandardWert { get; set; }

        /// <summary>
        /// Der Typ des Attributes
        /// </summary>
        public string Typ { set; get; }

        /// <summary>
        /// Erzeugt ein neues Attribut
        /// </summary>
        public DTDAttribut()
        {
            Name = string.Empty;
            Pflicht = PflichtArten.Optional;
            StandardWert = string.Empty;
            ErlaubteWerte = new string[] { };
        }


    }
}
