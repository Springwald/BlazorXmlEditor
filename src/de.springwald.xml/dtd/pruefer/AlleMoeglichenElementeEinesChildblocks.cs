using System.Collections.Generic;

namespace de.springwald.xml.dtd
{
    /// <summary>
    /// Liefert zu einem angegebenen Childblock alle DTD-Elemente, welche dieser Block liefern könnte
    /// </summary>
    public class AlleMoeglichenElementeEinesChildblocks
    {
        public HashSet<string> Elements { get; }

        public AlleMoeglichenElementeEinesChildblocks(DTDChildElemente childBlock)
        {
            this.Elements = new HashSet<string>();
            this.Search(childBlock);
        }

        /// <summary>
        /// Durchsucht den Childblock nach neuen Elementen
        /// </summary>
        /// <param name="childBlock"></param>
        private void Search(DTDChildElemente childBlock)
        {
            switch (childBlock.Art)
            {
                case DTDChildElemente.DTDChildElementArten.Leer:
                    break;

                case DTDChildElemente.DTDChildElementArten.EinzelChild:
                    this.AddElement(childBlock.ElementName);
                    break;

                case DTDChildElemente.DTDChildElementArten.ChildListe:
                    // Alle children dieses Childblocks durchlaufen
                    for (int iChild = 0; iChild < childBlock.AnzahlChildren; iChild++)
                    {
                        this.Search(childBlock.Child(iChild));
                    }
                    break;
            }
        }

        /// <summary>
        /// Merkt sich ein Element als Ergebnis
        /// </summary>
        /// <param name="element"></param>
        private void AddElement(string elementName)
        {
            if (!Elements.Contains(elementName))
            {
                Elements.Add(elementName);
            }
        }

    }
}
