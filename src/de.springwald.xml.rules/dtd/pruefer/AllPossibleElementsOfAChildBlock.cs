using System.Collections.Generic;

namespace de.springwald.xml.rules.dtd
{
    /// <summary>
    /// Liefert zu einem angegebenen Childblock alle DTD-Elemente, welche dieser Block liefern könnte
    /// </summary>
    public class AllPossibleElementsOfAChildBlock
    {
        public HashSet<string> Elements { get; }

        public AllPossibleElementsOfAChildBlock(DtdChildElements childBlock)
        {
            this.Elements = new HashSet<string>();
            this.Search(childBlock);
        }

        /// <summary>
        /// Durchsucht den Childblock nach neuen Elementen
        /// </summary>
        /// <param name="childBlock"></param>
        private void Search(DtdChildElements childBlock)
        {
            switch (childBlock.ElementType)
            {
                case DtdChildElements.DtdChildElementTypes.Empty:
                    break;

                case DtdChildElements.DtdChildElementTypes.SingleChild:
                    this.AddElement(childBlock.ElementName);
                    break;

                case DtdChildElements.DtdChildElementTypes.ChildList:
                    // Alle children dieses Childblocks durchlaufen
                    for (int iChild = 0; iChild < childBlock.ChildrenCount; iChild++)
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
