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
    public class XmlUndoStepNodeInserted : XmlUndoStep
    {
        private System.Xml.XmlNode insertedNode;
        private System.Xml.XmlNode parentNode;

        /// <summary>
        /// Creates a new undo step for inserting a new node
        /// </summary>
        /// <param name="insertedNode">This node was inserted</param>
        public XmlUndoStepNodeInserted(System.Xml.XmlNode insertedNode, System.Xml.XmlNode parentNode) : base()
        {
            this.insertedNode = insertedNode;
            this.parentNode = parentNode;
            if ((insertedNode == null))
            {
                throw new ApplicationException($"Inserting the node cannot be noted for Undo because it is NULL  '{this.insertedNode.OuterXml}'");
            }
        }
        public override void UnDo()
        {
            //  Undo the insertion of the node
            if (this.insertedNode is System.Xml.XmlAttribute) // inserted node was an attribute
            {
                this.parentNode.Attributes.Remove((System.Xml.XmlAttribute)this.insertedNode);
            }
            else // inserted node was not an attribute
            {
                this.parentNode.RemoveChild(insertedNode);
            }
        }
    }
}
