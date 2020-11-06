// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.rules
{
    public partial class XmlCursorPos
    {
        public enum XmlCursorPositions { CursorInFrontOfNode, CursorOnNodeStartTag, CursorOnNodeEndTag, CursorInsideTheEmptyNode, CursorInsideTextNode, CursorBehindTheNode };

        /// <summary>
        ///  This XML node is currently the focus of the XML editor
        /// </summary>
        public System.Xml.XmlNode ActualNode { get; private set; }

        /// <summary>
        /// There the cursor is located in the continuous text, if the Pos is CursorInsideTextNodes
        /// </summary>
        public int PosInTextNode { get; private set; }

        /// <summary>
        /// There the cursor is located inside or outside the focused XMLNode
        /// </summary>
        public XmlCursorPositions PosOnNode { get; private set; }

        public XmlCursorPos()
        {
            this.ActualNode = null;  // no node selected
            this.PosOnNode = XmlCursorPositions.CursorOnNodeStartTag;
            this.PosInTextNode = 0;
        }

        /// <summary>
        ///  Checks if this position is equal to a second one
        /// </summary>
        public bool Equals(XmlCursorPos otherPos)
        {
            if (this.ActualNode != otherPos.ActualNode) return false;
            if (this.PosOnNode != otherPos.PosOnNode) return false;
            if (this.PosInTextNode != otherPos.PosInTextNode) return false;
            return true;
        }

        /// <summary>
        /// Creates a copy of the cursor position
        /// </summary>
        public XmlCursorPos Clone()
        {
            var clone = new XmlCursorPos();
            clone.SetPos(this.ActualNode, this.PosOnNode, this.PosInTextNode);
            return clone;
        }

        /// <summary>
        /// Checks whether the specified node lies behind this cursor position
        /// </summary>
        public bool LiesBehindThisPos(System.Xml.XmlNode node)
        {
            return ToolboxXml.Node1LaisBeforeNode2(ActualNode, node);
        }

        /// <summary>
        ///  Checks whether the specified node lies before this cursor position
        /// </summary>
        public bool LiesBeforeThisPos(System.Xml.XmlNode node)
        {
            return ToolboxXml.Node1LaisBeforeNode2(node, ActualNode);
        }

        /// <summary>
        /// Sets new values to this cursor pos
        /// </summary>
        /// <returns>true, when values where other than before</returns>
        public bool SetPos(System.Xml.XmlNode actualNode, XmlCursorPositions posAtNode, int posInTextNode = 0)
        {
            bool changed;
            if (actualNode != this.ActualNode)
            {
                changed = true;
            }
            else
            {
                if (posAtNode != this.PosOnNode)
                {
                    changed = true;
                }
                else
                {
                    if (posInTextNode != this.PosInTextNode)
                    {
                        changed = true;
                    }
                    else
                    {
                        changed = false;
                    }
                }
            }
            this.ActualNode = actualNode;
            this.PosOnNode = posAtNode;
            this.PosInTextNode = posInTextNode;
            return changed;
        }

        /// <summary>
        /// Finds out if the node or one of its parent nodes is selected
        /// </summary>
        public bool IsNodeInsideSelection(System.Xml.XmlNode node)
        {
            if (this.ActualNode == null) return false; // no node is selected at all
            if (node == null) return false;// No node passed for test at all
            if (node == this.ActualNode) // the passed node is the current one
            {
                // Return whether the node itself is selected
                return ((this.PosOnNode == XmlCursorPositions.CursorOnNodeStartTag) ||
                    (this.PosOnNode == XmlCursorPositions.CursorOnNodeEndTag));
            }
            else
            {
                return this.IsNodeInsideSelection(node.ParentNode); // Continue testing the parent node
            }
        }
    }
}
