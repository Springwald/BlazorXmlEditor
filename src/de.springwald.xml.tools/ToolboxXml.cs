// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace de.springwald.xml.tools
{
    public class ToolboxXml
    {
        /// <summary>
        /// None of the two nodes to be compared must be NULL
        /// </summary>
        public static bool Node1LaisBeforeNode2(XmlNode node1, XmlNode node2)
        {
            if (node1 == null || node2 == null) throw new ApplicationException("None of the two nodes to be compared must be NULL (Node1LaisBeforeNode2)");

            if (node1.OwnerDocument != node2.OwnerDocument) return false;
            if (node1 == node2) return false; // Both nodes equal, then of course not node1 before node2
            XPathNavigator naviNode1 = node1.CreateNavigator();
            XPathNavigator naviNode2 = node2.CreateNavigator();
            return naviNode1.ComparePosition(naviNode2) == System.Xml.XmlNodeOrder.Before;
        }

        /// <summary>
        /// Finds out (even beyond several levels of depth) whether the specified node has the specified parent as a parent
        /// </summary>
        public static bool IsChild(XmlNode child, XmlNode parent)
        {
            if (child.ParentNode == null) return false;
            if (child.ParentNode == parent) return true;
            return IsChild(child.ParentNode, parent);
        }

        private static char[] textCleanSplitChars = new char[] { '\n', '\t', '\r', '\v' };

        /// <summary>
        /// Returns the content text from a text node
        /// </summary>
        public static string TextFromNodeCleaned(XmlNode textNode)
        {
            if (!(textNode is XmlText) && !(textNode is XmlComment) && !(textNode is XmlWhitespace))
            {
                throw (new ApplicationException($"Received node is not a text node  ({textNode.OuterXml})"));
            }
            var result = textNode.Value.ToString();
            result = result.Replace(Environment.NewLine, string.Empty); // Remove line breaks from text
            result = result.Trim(textCleanSplitChars);
            return result;
        }

        /// <summary>
        /// Is this node a text editable node
        /// </summary>
        public static bool IsTextOrCommentNode(XmlNode node)
        {
            return ((node is XmlText) || (node is XmlComment));
        }

        /// <summary>
        /// Handles the whitespace and leaves only visible SPACE whitespace. All wraps and tabs are removed
        /// </summary>
        public static void CleanUpWhitespaces(System.Xml.XmlNode node)
        {
            if (node == null) return;

            var whites = new List<XmlNode>();
            var restChildren = new List<XmlNode>();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child is XmlWhitespace)
                {
                    whites.Add(child);
                }
                else
                {
                    if (child is XmlElement)
                    {
                        restChildren.Add(child);
                    }
                }
            }

            // handle whitespace
            foreach (XmlWhitespace white in whites)
            {
                if (white.Data.IndexOf(" ") != -1)
                {
                    // If there is a space in it, the whitespace will be reduced to it, no matter if there are still breaks, tabs or similar
                    XmlText textnode = white.OwnerDocument.CreateTextNode(" ");
                    white.ParentNode.ReplaceChild(textnode, white);
                }
                else
                {
                    // No space in whitespace, then delete the whitespace
                    white.ParentNode.RemoveChild(white);
                }
            }

            // Treat the whitespace in the sub-children
            foreach (XmlNode child in restChildren)
            {
                CleanUpWhitespaces(child);
            }
        }
    }
}
