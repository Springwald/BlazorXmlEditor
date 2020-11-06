// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Threading.Tasks;
using de.springwald.xml.rules;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor.actions
{
    internal static class CursorPosMoveHelper
    {
        internal static async Task<bool> MoveLeft(XmlCursorPos cursorPos, System.Xml.XmlNode rootnode, XmlRules xmlRules)
        {
            var actualNode = cursorPos.ActualNode; 
            
            switch (cursorPos.PosOnNode)
            {
                case XmlCursorPositions.CursorOnNodeStartTag:
                case XmlCursorPositions.CursorOnNodeEndTag:
                    cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                    break;

                case XmlCursorPositions.CursorInFrontOfNode:
                    if (actualNode != rootnode)
                    {
                        if (actualNode.PreviousSibling != null) 
                        {
                            cursorPos.SetPos(actualNode.PreviousSibling, XmlCursorPositions.CursorBehindTheNode);
                            await MoveLeft(cursorPos, rootnode, xmlRules);
                        }
                        else // no previous sibling node available
                        {
                            cursorPos.SetPos(actualNode.ParentNode, XmlCursorPositions.CursorInFrontOfNode);
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;

                case XmlCursorPositions.CursorBehindTheNode:
                    if (ToolboxXml.IsTextOrCommentNode(actualNode)) // With a text node the cursor is placed behind the last character
                    {
                        cursorPos.SetPos(actualNode, XmlCursorPositions.CursorInsideTextNode, Math.Max(0, ToolboxXml.TextFromNodeCleaned(actualNode).Length - 1));
                    }
                    else
                    {
                        if (actualNode.ChildNodes.Count == 0) 
                        {
                            if (xmlRules.HasEndTag(actualNode))
                            {
                                //  If the cursor shows a close tag, place it in the empty node
                                cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInsideTheEmptyNode);
                            }
                            else
                            {
                                // If the cursor does *not* show a close tag, place it before the empty node
                                cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                            }
                        }
                        else // there are children in the node
                        {
                            cursorPos.SetPos(actualNode.LastChild, XmlCursorPositions.CursorBehindTheNode);
                        }
                    }
                    break;

                case XmlCursorPositions.CursorInsideTheEmptyNode:
                    cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                    break;

                case XmlCursorPositions.CursorInsideTextNode:
                    if (ToolboxXml.IsTextOrCommentNode(actualNode)) // Node is text node 
                    {
                        if (cursorPos.PosInTextNode > 1)
                        {  // Cursor one character to the left
                            cursorPos.SetPos(cursorPos.ActualNode, cursorPos.PosOnNode, cursorPos.PosInTextNode - 1);
                        }
                        else
                        {
                            // Put in front of the node
                            cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                        }
                    }
                    else // not a textnode
                    {
                        throw new ApplicationException(string.Format("XMLCursorPos.MoveLeft: CursorPos is XMLCursorPositions.CursorInsideTextNodes, but no textnode is selected, but the node {0}", actualNode.OuterXml));
                    }
                    break;

                default:
                    throw new ApplicationException(String.Format("XMLCursorPos.MoveLeft: unknown CursorPos {0}", cursorPos.PosOnNode));
            }
            return true;
        }

        internal static async Task<bool> MoveRight(XmlCursorPos cursorPos, System.Xml.XmlNode rootnode, XmlRules xmlRules)
        {
            System.Xml.XmlNode node = cursorPos.ActualNode; 

            switch (cursorPos.PosOnNode)
            {
                case XmlCursorPositions.CursorOnNodeStartTag:
                case XmlCursorPositions.CursorOnNodeEndTag:
                    cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                    break;

                case XmlCursorPositions.CursorBehindTheNode:
                    if (node.NextSibling != null) 
                    {
                        // Place in front of the next sibling
                        cursorPos.SetPos(node.NextSibling, XmlCursorPositions.CursorInFrontOfNode);
                        // Since "behind the first" looks the same as "before the second", move one more step to the right
                        await MoveRight(cursorPos, rootnode, xmlRules);
                    }
                    else // No following siblings available, then set behind the parent node
                    {
                        if (node.ParentNode != rootnode)
                        {
                            cursorPos.SetPos(node.ParentNode, XmlCursorPositions.CursorBehindTheNode);
                            if (!xmlRules.HasEndTag(node.ParentNode))
                            { // If no closed tag is displayed for the parent, then one more to the right
                                await MoveRight(cursorPos, rootnode, xmlRules);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;

                case XmlCursorPositions.CursorInsideTheEmptyNode:
                    cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                    break;

                case XmlCursorPositions.CursorInFrontOfNode:
                    if (ToolboxXml.IsTextOrCommentNode(node))  // The node itself is text node 
                    {
                        if (ToolboxXml.TextFromNodeCleaned(node).Length > 1) // Textnode ist nicht leer
                        {
                            cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, 1); // one character forward, i.e. after the first character
                        }
                        else  // Textnode is empty
                        {
                            cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                        }
                    }
                    else  // Node is not a text node
                    {
                        if (node.ChildNodes.Count == 0) 
                        {
                            if (!xmlRules.HasEndTag(node)) // If no closed tag is displayed for this node, then directly behind the node
                            {
                                cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                            }
                            else  // Node has closing tag, so put it in between
                            {
                                // Set to the empty node
                                cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInsideTheEmptyNode);
                            }
                        }
                        else // Children available
                        {
                            cursorPos.SetPos(node.FirstChild, XmlCursorPositions.CursorInFrontOfNode);
                        }
                    }
                    break;

                case XmlCursorPositions.CursorInsideTextNode:
                    if (ToolboxXml.IsTextOrCommentNode(node)) // Node is text node
                    {
                        if (ToolboxXml.TextFromNodeCleaned(node).Length > cursorPos.PosInTextNode + 1) // there is text in the text node on the right
                        {
                            // one character forward, i.e. after the first character
                            cursorPos.SetPos(cursorPos.ActualNode, cursorPos.PosOnNode, cursorPos.PosInTextNode + 1);

                            /*if ((XMLEditor.TextAusTextNodeBereinigt(node).Length == cursor.PosInNode) && (node.NextSibling != null)) 
                            {
                                // Wenn hinter dem letzten Zeichnen des Textnodes und folgendes Geschwister vorhanden, dann
                                // vor den folgenden Geschwisternode
								
                            }*/
                        }
                        else  // no text follows in the text node
                        {
                            cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                        }
                    }
                    else // Node is not a text node
                    {
                        throw new ApplicationException(String.Format("XMLCurorPos.MoveRight: CursorPos is XMLCursorPositions.CursorInsideTextNodes, but no textnode is selected, but the node {0}", node.OuterXml));
                    }
                    break;


                default:
                    throw new ApplicationException(String.Format("XMLCurorPos.MoveRight: unknown CursorPos {0}", cursorPos.PosOnNode));
            }
            return true;
        }
    }
}
