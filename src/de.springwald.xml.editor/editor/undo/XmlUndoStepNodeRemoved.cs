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
    public class XmlUndoStepNodeRemoved : XmlUndoStep
    {
        private System.Xml.XmlNode deletedNode;
        private System.Xml.XmlNode parentNode;
        private System.Xml.XmlNode previousSibling;
        private System.Xml.XmlNode nextSibling;

        /// <summary>
        /// Erzeugt einen neuen Undoschritt für das Löschen eines Nodes
        /// </summary>
        /// <param name="deletedNode">Dieser Node wurde gelöscht</param>
        public XmlUndoStepNodeRemoved(System.Xml.XmlNode deletedNode) : base()
        {
            this.deletedNode = deletedNode;
            this.parentNode = deletedNode.ParentNode;
            this.previousSibling = deletedNode.PreviousSibling;
            this.nextSibling = deletedNode.NextSibling;

            if ((this.parentNode == null) && (this.previousSibling == null) && (this.nextSibling == null))
            {
                throw new ApplicationException($"Deleting the node cannot be noted for Undo because it has no reference '{this.deletedNode.OuterXml}'");
            }
        }
        public override void UnDo()
        {
            // Undo the deletion of the node
            if (this.previousSibling != null) // If there was a before node
            {
                this.previousSibling.ParentNode.InsertAfter(this.deletedNode, this.previousSibling); // Insert Node again behind Prev-Node
            }
            else  // There was no before node
            {
                if (this.nextSibling != null) // There was an after-node
                {
                    this.nextSibling.ParentNode.InsertBefore(this.deletedNode, this.nextSibling); // Insert node again before the after node
                }
                else // There was no before or after node, so Parent was empty except for the deleted node
                {
                    this.parentNode.AppendChild(this.deletedNode); // Put the deleted node back into the ParentNode
                }
            }
        }
    }
}
