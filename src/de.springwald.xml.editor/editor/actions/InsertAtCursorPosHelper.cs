// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.rules;
using de.springwald.xml.tools;
using System;
using System.Diagnostics;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor.actions
{
    internal static class InsertAtCursorPosHelper
    {
        internal struct InsertTextResult
        {
            public System.Xml.XmlNode ReplaceNode;
        }

        /// <summary>
        /// Inserts the specified text at the current cursor position, if possible
        /// </summary>
        internal static InsertTextResult InsertText(XmlCursorPos cursorPos, string rawText, XmlRules xmlRules)
        {
            // Revise the entered text in preprocessing if necessary.
            // In an AIML 1.1 DTD this can mean, for example, that the text for insertion into the PATTERN tag is changed to upper case
            string text = xmlRules.InsertTextTextPreProcessing(rawText, cursorPos, out System.Xml.XmlNode replacementNode);

            if (replacementNode != null)
            {
                // If the entered text has become a node instead, e.g. with AIML, 
                // if you press * in a template and then want to insert a <star> instead
                return new InsertTextResult { ReplaceNode = replacementNode };
            }

            switch (cursorPos.PosOnNode)
            {
                case XmlCursorPositions.CursorOnNodeStartTag:
                case XmlCursorPositions.CursorOnNodeEndTag:
                    // First check whether this node may be replaced by a text node
                    if (xmlRules.IsThisTagAllowedAtThisPos("#PCDATA", cursorPos))
                    {
                        // The selected node by a newly created text node 
                        System.Xml.XmlText neuerTextNode = cursorPos.ActualNode.OwnerDocument.CreateTextNode(text);
                        cursorPos.ActualNode.ParentNode.ReplaceChild(cursorPos.ActualNode, neuerTextNode);
                        cursorPos.SetPos(neuerTextNode, XmlCursorPositions.CursorBehindTheNode);
                    }
                    throw new ApplicationException(String.Format("InsertText: unknown CursorPos {0}", cursorPos.PosOnNode));

                case XmlCursorPositions.CursorBehindTheNode:
                    InsertTextBetweenTwoNodes(cursorPos, cursorPos.ActualNode, cursorPos.ActualNode.NextSibling, text, xmlRules);
                    break;

                case XmlCursorPositions.CursorInFrontOfNode:
                    InsertTextBetweenTwoNodes(cursorPos, cursorPos.ActualNode.PreviousSibling, cursorPos.ActualNode, text, xmlRules);
                    break;

                case XmlCursorPositions.CursorInsideTheEmptyNode:
                    // First check if text is allowed inside the empty node
                    if (xmlRules.IsThisTagAllowedAtThisPos("#PCDATA", cursorPos))
                    {
                        // Then create a text node within the empty node with the desired text content
                        System.Xml.XmlText newTextNode = cursorPos.ActualNode.OwnerDocument.CreateTextNode(text);
                        cursorPos.ActualNode.AppendChild(newTextNode);
                        cursorPos.SetPos(newTextNode, XmlCursorPositions.CursorBehindTheNode);
                    }
                    else
                    {
                        // Error BEEEEP!
                    }
                    break;

                case XmlCursorPositions.CursorInsideTextNode:
                    string textBeforeNode = cursorPos.ActualNode.InnerText.Substring(0, cursorPos.PosInTextNode);
                    string textAfterCursor = cursorPos.ActualNode.InnerText.Substring(cursorPos.PosInTextNode, cursorPos.ActualNode.InnerText.Length - cursorPos.PosInTextNode);
                    // Insert the character of the pressed keys after the cursor
                    cursorPos.ActualNode.InnerText = $"{textBeforeNode}{text}{textAfterCursor}";
                    cursorPos.SetPos(cursorPos.ActualNode, cursorPos.PosOnNode, cursorPos.PosInTextNode + text.Length);
                    break;

                default:
                    throw new ApplicationException(String.Format("InsertText: unknown CursorPos {0}", cursorPos.PosOnNode));
            }
            return new InsertTextResult { ReplaceNode = replacementNode };
        }


        /// <summary>
        /// Inserts the specified XML node at the specified position
        /// </summary>
        internal static bool InsertXmlNode(XmlCursorPos cursorPos, System.Xml.XmlNode node, XmlRules xmlRules, bool setNewCursorPosBehindNewInsertedNode)
        {
            System.Xml.XmlNode parentNode = cursorPos.ActualNode.ParentNode;

            switch (cursorPos.PosOnNode)
            {
                case XmlCursorPositions.CursorOnNodeStartTag: // replace the acual node
                case XmlCursorPositions.CursorOnNodeEndTag:
                    parentNode.ReplaceChild(node, cursorPos.ActualNode);
                    break;

                case XmlCursorPositions.CursorInFrontOfNode: // insert before actual node
                    parentNode.InsertBefore(node, cursorPos.ActualNode);
                    break;

                case XmlCursorPositions.CursorBehindTheNode: // insert after actual node
                    parentNode.InsertAfter(node, cursorPos.ActualNode);
                    break;

                case XmlCursorPositions.CursorInsideTheEmptyNode: // insert into empty node
                    cursorPos.ActualNode.AppendChild(node);
                    break;

                case XmlCursorPositions.CursorInsideTextNode: // insert into textnode

                    // Make the text available as a node before the insertion position
                    string textDavor = cursorPos.ActualNode.InnerText.Substring(0, cursorPos.PosInTextNode);
                    System.Xml.XmlNode textDavorNode = parentNode.OwnerDocument.CreateTextNode(textDavor);

                    // Provide the text behind the insert position as a node
                    string textAfter = cursorPos.ActualNode.InnerText.Substring(cursorPos.PosInTextNode, cursorPos.ActualNode.InnerText.Length - cursorPos.PosInTextNode);
                    System.Xml.XmlNode textAfterNode = parentNode.OwnerDocument.CreateTextNode(textAfter);

                    // Insert the node to be inserted between the new before and after text node
                    // -> so replace the old text node with
                    // textbefore - newNode - textafter
                    parentNode.ReplaceChild(textDavorNode, cursorPos.ActualNode);
                    parentNode.InsertAfter(node, textDavorNode);
                    parentNode.InsertAfter(textAfterNode, node);
                    break;

                default:
                    throw new ApplicationException(String.Format("InsertXmlNode: unknown PosOnNode {0}", cursorPos.PosOnNode));
            }

            // set cursor
            if (setNewCursorPosBehindNewInsertedNode)
            {
                // Place cursor behind the new node
                cursorPos.SetPos(node, XmlCursorPositions.CursorBehindTheNode);
            }
            else
            {
                if (xmlRules.HasEndTag(node))
                {
                    // Place cursor in the new node
                    cursorPos.SetPos(node, XmlCursorPositions.CursorInsideTheEmptyNode);
                }
                else
                {
                    // Place cursor behind the new node
                    cursorPos.SetPos(node, XmlCursorPositions.CursorBehindTheNode);
                }
            }
            return true;
        }

        /// <summary>
        /// Inserts a text between two nodes
        /// </summary>
        internal static void InsertTextBetweenTwoNodes(XmlCursorPos cursorPos, System.Xml.XmlNode nodeBefore, System.Xml.XmlNode nodeAfter, string text, XmlRules xmlRules)
        {
            if (ToolboxXml.IsTextOrCommentNode(nodeBefore))  // if the node is already text before, then simply append to it
            {
                nodeBefore.InnerText += text;
                cursorPos.SetPos(nodeBefore, XmlCursorPositions.CursorInsideTextNode, nodeBefore.InnerText.Length);
            }
            else  // the node before is no text
            {
                if (ToolboxXml.IsTextOrCommentNode(nodeAfter))  // if the node behind it is already text then just paste it into
                {
                    nodeAfter.InnerText = $"{text}{nodeAfter.InnerText}";
                    cursorPos.SetPos(nodeAfter, XmlCursorPositions.CursorInsideTextNode, text.Length);
                }
                else // the node behind is also no text
                {
                    // Insert between two non-text nodes
                    if (xmlRules.IsThisTagAllowedAtThisPos("#PCDATA", cursorPos))
                    {
                        System.Xml.XmlText newTextNode = cursorPos.ActualNode.OwnerDocument.CreateTextNode(text); 
                        InsertXmlNode(cursorPos, newTextNode, xmlRules, false);
                    }
                    else
                    {
#warning Insert another correct message or sound
                        Debug.Assert(false, "Error - Beep!");
                    }

                }
            }
        }
    }
}
