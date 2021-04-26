// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Collections.Generic;

namespace de.springwald.xml.rules.dtd
{
    /// <summary>
    /// Returns all DTD elements for a given child block, which this block could provide
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
        /// Searches the child block for new elements
        /// </summary>
        private void Search(DtdChildElements childBlock)
        {
            switch (childBlock.ElementType)
            {
                case DtdChildElements.DtdChildElementTypes.Empty:
                    break;

                case DtdChildElements.DtdChildElementTypes.SingleChild:
                    if (!Elements.Contains(childBlock.ElementName))
                    {
                        Elements.Add(childBlock.ElementName);
                    }
                    break;

                case DtdChildElements.DtdChildElementTypes.ChildList:
                    for (int iChild = 0; iChild < childBlock.ChildrenCount; iChild++)
                    {
                        this.Search(childBlock.Child(iChild));
                    }
                    break;
            }
        }
    }
}
