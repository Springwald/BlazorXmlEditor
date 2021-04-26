using de.springwald.xml.cursor;
using de.springwald.xml.rules;
using de.springwald.xml.tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor.cursor
{
    internal static class XmlCursorSelectionHelper
    {
        /// <summary>
        /// Returns the selected XML content as string 
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetSelectionAsString(XmlCursor cursor)
        {
            if (cursor. IsSomethingSelected) 
            {
                var result = new StringBuilder();

                var optimized = cursor.Clone();
                await optimized.OptimizeSelection();

                System.Xml.XmlNode node = optimized.StartPos.ActualNode; // begin at the start node

                // Include the start node in the result
                switch (optimized.StartPos.PosOnNode)
                {
                    case XmlCursorPositions.CursorOnNodeEndTag:
                    case XmlCursorPositions.CursorOnNodeStartTag:
                    case XmlCursorPositions.CursorInFrontOfNode:
                        result.Append(node.OuterXml);  // take the entire start node
                        break;

                    case XmlCursorPositions.CursorBehindTheNode:
                    case XmlCursorPositions.CursorInsideTheEmptyNode:
                        break;

                    case XmlCursorPositions.CursorInsideTextNode: // take only a part of the text
                        string textPart = node.InnerText;

                        int start = optimized.StartPos.PosInTextNode;
                        int length = textPart.Length - start;

                        if (node == optimized.EndPos.ActualNode) // If this text node is both start and end node
                        {
                            switch (optimized.EndPos.PosOnNode)
                            {
                                case XmlCursorPositions.CursorOnNodeEndTag:
                                case XmlCursorPositions.CursorBehindTheNode:
                                    // Length stays to the end of the node
                                    break;

                                case XmlCursorPositions.CursorOnNodeStartTag:
                                case XmlCursorPositions.CursorInsideTheEmptyNode:
                                case XmlCursorPositions.CursorInFrontOfNode:
                                    throw new ApplicationException("XMLCursor.GetSelectionAsString: implausible EndPos.PosOnNode '" + optimized.EndPos.PosOnNode + "' for StartPos.CursorInsideTextNode");

                                case XmlCursorPositions.CursorInsideTextNode:
                                    // Not quite to the end of the text 
                                    if (optimized.StartPos.PosInTextNode > optimized.EndPos.PosInTextNode)
                                    {
                                        throw new ApplicationException("XMLCursor.GetSelectionAsString: optimized.StartPos.PosInTextNode > optimized.EndPos.PosInTextNode");
                                    }
                                    else
                                    {
                                        // Subtract the text from the length after selection
                                        length -= (textPart.Length - optimized.EndPos.PosInTextNode);
                                    }
                                    break;

                                default:
                                    throw new ApplicationException("XMLCursor.GetSelectionAsString: unhandled optimized.EndPos.PosOnNode'" + optimized.EndPos.PosOnNode + "' for StartPos.CursorInsideTextNode");
                            }
                        }
                        textPart = textPart.Substring(start, length);
                        result.Append(textPart);
                        break;

                    default:
                        throw new ApplicationException("XMLCursor.GetSelectionAsStringg: unhandled optimized.StartPos.PosOnNode'" + optimized.StartPos.PosOnNode + "'");
                }

                if (optimized.StartPos.ActualNode != optimized.EndPos.ActualNode) // If more nodes are selected after the start node
                {
                    do
                    {
                        node = node.NextSibling; // to the next node...

                        if (node != null)
                        {
                            // Include the node in the result
                            if (node == optimized.EndPos.ActualNode) // This node is the EndNode
                            {
                                switch (optimized.EndPos.PosOnNode)
                                {
                                    case XmlCursorPositions.CursorOnNodeEndTag:
                                    case XmlCursorPositions.CursorOnNodeStartTag:
                                    case XmlCursorPositions.CursorBehindTheNode:
                                        result.Append(node.OuterXml); // Include node 1:1 in result
                                        break;

                                    case XmlCursorPositions.CursorInsideTextNode:
                                        // TRake the beginning of the text node
                                        string textPart = node.InnerText;
                                        result.Append(textPart.Substring(0, optimized.EndPos.PosInTextNode + 1));
                                        break;

                                    case XmlCursorPositions.CursorInsideTheEmptyNode:
                                        throw new ApplicationException("XMLCursor.GetSelectionAsString: implausible optimized.EndPos.PosOnNode '" + optimized.EndPos.PosOnNode + "' for StartPos.Node != EndPos.Node");

                                    default:
                                        throw new ApplicationException("XMLCursor.GetSelectionAsString: implausible optimized.StartPos.PosOnNode'" + optimized.StartPos.PosOnNode + "' for StartPos.Node != EndPos.Node");

                                }
                            }
                            else // Include node 1:1 in result
                            {
                                result.Append(node.OuterXml);
                            }
                        }

                    } while ((node != optimized.EndPos.ActualNode) && (node != null)); // ... until the end node is reached

                    if (node == null)
                    {
                        throw new ApplicationException("Endnode was not reachable as NextSibling from Startnode");
                    }
                }
                return result.ToString();
            }
            else
            {
                return string.Empty; // nothing is selected at all
            }
        }

        public struct DeleteSelectionResult
        {
            public bool Success;
            public XmlCursorPos NewCursorPosAfterDelete;
        }

        /// <summary>
        /// Deletes the characters and nodes between StartPos and EndPos of the cursor
        /// </summary>
        internal static async Task<DeleteSelectionResult> DeleteSelection(XmlCursor cursor)
        {
            // If the cursor contains no selection at all
            if (!cursor.IsSomethingSelected)
            {
                return new DeleteSelectionResult
                {
                    NewCursorPosAfterDelete = cursor.StartPos.Clone(), // Cursor is not changed
                    Success = false // nothing deleted
                };
            }
            else
            {
                if (cursor.StartPos.ActualNode == cursor.EndPos.ActualNode) // If both nodes are identical
                {
                    switch (cursor.StartPos.PosOnNode)
                    {
                        case XmlCursorPositions.CursorOnNodeStartTag:
                        case XmlCursorPositions.CursorOnNodeEndTag:
                            // a single node is selected and should be deleted
                            System.Xml.XmlNode nodeDelete = cursor.StartPos.ActualNode;   // This node should be deleted
                            System.Xml.XmlNode nodeBefore = nodeDelete.PreviousSibling;   // This node is before the node to be deleted
                            System.Xml.XmlNode nodeAfter = nodeDelete.NextSibling;       // This node lies behind the node to be deleted

                            var newPosAfterDelete = new XmlCursorPos(); // This neighboring node will get the cursor after deletion

                            // If the node to be deleted is located between two text nodes, then these two text nodes are combined into one
                            if (nodeBefore != null && nodeAfter != null)
                            {
                                if (nodeBefore is System.Xml.XmlText && nodeAfter is System.Xml.XmlText)
                                {
                                    // the node to be deleted lies between two text nodes, therefore these two text nodes are combined into one

                                    // Afterwards, the cursor is positioned at the insertion point between the two text modules
                                    newPosAfterDelete.SetPos(nodeBefore, XmlCursorPositions.CursorInsideTextNode, nodeBefore.InnerText.Length);

                                    nodeBefore.InnerText += nodeAfter.InnerText; // Append the text from after node to the before node

                                    // Delete node to be deleted
                                    nodeDelete.ParentNode.RemoveChild(nodeDelete);

                                    // delete after node
                                    nodeAfter.ParentNode.RemoveChild(nodeAfter);

                                    return new DeleteSelectionResult
                                    {
                                        NewCursorPosAfterDelete = newPosAfterDelete,
                                        Success = true
                                    };
                                }
                            }

                            // The node to be deleted is *not* between two text nodes 

                            // Determine what should be selected after deletion

                            if (nodeBefore != null)
                            {
                                // After deletion, the cursor is positioned behind the previous node
                                newPosAfterDelete.SetPos(nodeBefore, XmlCursorPositions.CursorBehindTheNode);
                            }
                            else
                            {
                                if (nodeAfter != null)
                                {
                                    // After deletion, the cursor is positioned before the following node
                                    newPosAfterDelete.SetPos(nodeAfter, XmlCursorPositions.CursorInFrontOfNode);
                                }
                                else
                                {
                                    // After deletion, the cursor is in the parent node
                                    newPosAfterDelete.SetPos(nodeDelete.ParentNode, XmlCursorPositions.CursorInsideTheEmptyNode);
                                }
                            }

                            // delete the node
                            nodeDelete.ParentNode.RemoveChild(nodeDelete);
                            return new DeleteSelectionResult
                            {
                                NewCursorPosAfterDelete = newPosAfterDelete,
                                Success = true
                            }; 

                        case XmlCursorPositions.CursorInFrontOfNode:
                            // Start and end of the deletion area point to the same node and the start is before the node: This only makes sense with a text node!
                            if (ToolboxXml.IsTextOrCommentNode(cursor.StartPos.ActualNode))
                            {
                                // Place the cursor in the text node before the first character and then resend
                                cursor.StartPos.SetPos(cursor.StartPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, 0);
                                return await DeleteSelection(cursor); // zum löschen neu abschicken
                            }
                            else
                            {
                                // if it is not a text node, then select the whole node and send it again
                                await cursor.SetBothPositionsAndFireChangedEventIfChanged(cursor.StartPos.ActualNode, XmlCursorPositions.CursorOnNodeStartTag);
                                return await DeleteSelection(cursor); // resend to delete
                            }

                        case XmlCursorPositions.CursorBehindTheNode:
                            // Start and end of the deletion area point to the same node and the start is behind the node
                            if (ToolboxXml.IsTextOrCommentNode(cursor.StartPos.ActualNode))
                            {
                                // Place the cursor in the text node before the first character and then resend
                                cursor.StartPos.SetPos(cursor.StartPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, cursor.StartPos.ActualNode.InnerText.Length);
                                return await DeleteSelection(cursor); // resend to delete
                            }
                            else
                            {
                                // if it is not a text node, then select the whole node and send it again
                                await cursor.SetBothPositionsAndFireChangedEventIfChanged(cursor.StartPos.ActualNode, XmlCursorPositions.CursorOnNodeStartTag);
                                return await DeleteSelection(cursor); // resend to delete
                            }

                        case XmlCursorPositions.CursorInsideTextNode:
                            // a part of a text node is to be deleted
                            // Determine the part of the text to be deleted
                            int startpos = cursor.StartPos.PosInTextNode;
                            int endpos = cursor.EndPos.PosInTextNode;

                            if (cursor.EndPos.PosOnNode == XmlCursorPositions.CursorBehindTheNode)
                            {	
                                // If the end of the selection is behind the text node, then all remaining text is selected
                                endpos = cursor.StartPos.ActualNode.InnerText.Length;
                            }

                            // If all text is selected, then delete the entire text node
                            if (startpos == 0 && endpos >= cursor.StartPos.ActualNode.InnerText.Length)
                            {
                                // The whole text node is to be deleted, this is passed on to the method for deleting individually selected nodes
                                XmlCursor nodeSelectedCursor = new XmlCursor();
                                await nodeSelectedCursor.SetBothPositionsAndFireChangedEventIfChanged(cursor.StartPos.ActualNode, XmlCursorPositions.CursorOnNodeStartTag);
                                return await DeleteSelection(nodeSelectedCursor);
                            }
                            else
                            {
                                // Only a part of the text is to be deleted
                                string restText = cursor.StartPos.ActualNode.InnerText;
                                restText = restText.Remove(startpos, endpos - startpos);
                                cursor.StartPos.ActualNode.InnerText = restText;

                                // determine where the cursor is after deletion
                                newPosAfterDelete = new XmlCursorPos();
                                if (startpos == 0) // The cursor is positioned before the first character
                                {
                                    // then it can better be placed before the text node itself
                                    newPosAfterDelete.SetPos(cursor.StartPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                                }
                                else
                                {
                                    newPosAfterDelete.SetPos(cursor.StartPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, startpos);
                                }

                                return new DeleteSelectionResult
                                {
                                    NewCursorPosAfterDelete = newPosAfterDelete,
                                    Success = true
                                };  
                            }

                        case XmlCursorPositions.CursorInsideTheEmptyNode:
                            if (cursor.EndPos.PosOnNode == XmlCursorPositions.CursorBehindTheNode ||
                                cursor.EndPos.PosOnNode == XmlCursorPositions.CursorInFrontOfNode)
                            {
                                XmlCursor newCursor = new XmlCursor();
                                await newCursor.SetBothPositionsAndFireChangedEventIfChanged(cursor.StartPos.ActualNode, XmlCursorPositions.CursorOnNodeStartTag, 0);
                                return await DeleteSelection(newCursor);
                            }
                            else
                            {
                                throw new ApplicationException("DeleteSelection:#6363S undefined Endpos " + cursor.EndPos.PosOnNode + "!");
                            }

                        default:
                            //  what else should be selected besides text and the node itself, if start node and end node are identical?
                            throw new ApplicationException("DeleteSelection:#63346 StartPos.PosAmNode " + cursor.StartPos.PosOnNode + " not allowed!");
                    }
                }
                else // Both nodes are not identical
                {
                    // If both nodes are not identical, then remove all nodes in between until the two nodes are behind each other
                    while (cursor.StartPos.ActualNode.NextSibling != cursor.EndPos.ActualNode)
                    {
                        cursor.StartPos.ActualNode.ParentNode.RemoveChild(cursor.StartPos.ActualNode.NextSibling);
                    }

                    // delete the endnode or a part of it
                    XmlCursor temp = cursor.Clone();
                    temp.StartPos.SetPos(cursor.EndPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                    await DeleteSelection(temp);

                    // delete the start node, or a part of it
                    // -> Done by recursion in the selection delete method
                    cursor.EndPos.SetPos(cursor.StartPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                    return await DeleteSelection(cursor);
                }
            }
        }

        /// <summary>
        /// Finds the lowest common parent of two nodes. In extreme cases this is the root element, if the paths of the nodes into the depth of the DOM are completely different 
        /// </summary>
        public static System.Xml.XmlNode DeepestCommonParent(System.Xml.XmlNode node1, System.Xml.XmlNode node2)
        {
            System.Xml.XmlNode parent1 = node1.ParentNode;
            while (parent1 != null)
            {
                System.Xml.XmlNode parent2 = node2.ParentNode;
                while (parent2 != null)
                {
                    if (parent1 == parent2) return parent1;
                    parent2 = parent2.ParentNode;
                }
                parent1 = parent1.ParentNode;
            }
            return null;

        }

        /// <summary>
        /// Finds out if the node or one of its parent nodes is selected
        /// </summary>
        public static bool IsThisNodeInsideSelection(XmlCursor cursor, System.Xml.XmlNode node)
        {
            // Check if the node itself or one of its parents is directly selected
            if (cursor.StartPos.IsNodeInsideSelection(node)) return true;
            if (cursor.EndPos.IsNodeInsideSelection(node)) return true;

            if (cursor.StartPos.Equals(cursor.EndPos)) // Both positions are the same, so a maximum of one single node is selected
            {
                return cursor.StartPos.IsNodeInsideSelection(node);
            }
            else // Both positions are not equal, so something may be selected
            {
                if ((cursor.StartPos.ActualNode == node) || (cursor.EndPos.ActualNode == node)) // Start or EndNode of the selection is this node
                {
                    if (node is System.Xml.XmlText) // is a text node
                    {
                        return true;
                    }
                    else // not a text node
                    {
                        return false;
                    }
                }
                else
                {
                    if (cursor.StartPos.LiesBehindThisPos(node)) // Node is behind the starting position
                    {
                        if (cursor.EndPos.LiesBeforeThisPos(node)) // Node is located between Startpos and Endepos
                        {
                            return true;
                        }
                        else // Node is behind Startpos but also behind Endpos
                        {
                            return false;
                        }
                    }
                    else // Node is not behind the start pos
                    {
                        return false;
                    }
                }
            }
        }
    }
}
