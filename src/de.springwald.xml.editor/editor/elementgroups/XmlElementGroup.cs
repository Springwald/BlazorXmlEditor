// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Collections;
using System.Collections.Generic;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// A grouping for XML elements so that they can be grouped in the list of elements offered for insertion
    /// </summary>
    public class XmlElementGroup
    {
        private HashSet<string> elements = new HashSet<string>();

        public string Title { get; }

        public bool CollapsedByDefault { get; }

        public XmlElementGroup(string title, bool collapsedByDefault)
        {
            this.Title = title;
            this.CollapsedByDefault = collapsedByDefault;
        }

        public void AddElementName(string name)
        {
            elements.Add(name.ToLower());
        }

        public bool ContainsElement(string name)
        {
            return elements.Contains(name.ToLower());
        }
    }
}
