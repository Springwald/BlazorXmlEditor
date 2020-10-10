// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;

namespace de.springwald.xml.editor
{
    public class XMLUndoSchrittAttributRemoved : XMLUndoSchritt
    {
        private System.Xml.XmlAttribute deletedAttribute;
        private System.Xml.XmlNode ownerElement;

        /// <summary>
        /// Creates a new undo step to delete attributes
        /// </summary>
        /// <param name="attributeBeforeDeleting">This attribute was deleted</param>
        public XMLUndoSchrittAttributRemoved(System.Xml.XmlAttribute attributeBeforeDeleting) : base()
        {
            this.deletedAttribute = attributeBeforeDeleting;
            this.ownerElement = attributeBeforeDeleting.OwnerElement;
            if (this.ownerElement == null)
            {
                throw new ApplicationException($"Deleting the attribute cannot be noted for Undo because it has no reference: '{attributeBeforeDeleting.OuterXml}'");
            }
        }
        public override void UnDo()
        {
            // Undo the deletion of the attribute
            this.ownerElement.Attributes.Append(this.deletedAttribute);
        }
    }
}