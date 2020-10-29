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
    public class XMLUndoSchrittNodeChanged : XmlUndoStep
    {
        private System.Xml.XmlNode changedNode;
        private string previousValue;

        /// <summary>
        /// Creates a new undo step for changing a node value
        /// </summary>
        /// <param name="changedNode">This node was changed</param>
        public XMLUndoSchrittNodeChanged(System.Xml.XmlNode changedNode, string previousValue) : base()
        {
            this.changedNode = changedNode;
            this.previousValue = previousValue;
            if ((changedNode == null))
            {
                throw new ApplicationException($"Changing the node cannot be noted for Undo because it is NULL '{this.changedNode.OuterXml}'");
            }
        }
        public override void UnDo()
        {
            // Undo the modification of the node
            this.changedNode.Value = previousValue;
        }
    }
}
