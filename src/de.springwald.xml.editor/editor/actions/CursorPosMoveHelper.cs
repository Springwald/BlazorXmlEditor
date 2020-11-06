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
        /// <summary>
        /// bewegt den Cursor um eine Position nach links
        /// </summary>
        internal static async Task<bool> MoveLeft(XmlCursorPos cursorPos, System.Xml.XmlNode rootnode, XmlRules xmlRules)
        {
            var actualNode = cursorPos.ActualNode; 

            switch (cursorPos.PosOnNode)
            {
                case XmlCursorPositions.CursorOnNodeStartTag:
                case XmlCursorPositions.CursorOnNodeEndTag:
                    // Vor den Node setzen
                    cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                    break;

                case XmlCursorPositions.CursorInFrontOfNode:
                    if (actualNode != rootnode)
                    {
                        if (actualNode.PreviousSibling != null) // Vorheriger Geschwisterknoten vorhanden
                        {
                            cursorPos.SetPos(actualNode.PreviousSibling, XmlCursorPositions.CursorBehindTheNode);
                            await MoveLeft(cursorPos, rootnode, xmlRules);
                        }
                        else // kein vorheriger Geschwisterknoten vorhanden
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
                    if (ToolboxXml.IsTextOrCommentNode(actualNode)) // Bei einem Textnode wird der Cursor hinter das letzte Zeichen gesetzt
                    {
                        cursorPos.SetPos(actualNode, XmlCursorPositions.CursorInsideTextNode, Math.Max(0, ToolboxXml.TextFromNodeCleaned(actualNode).Length - 1));
                    }
                    else
                    {
                        if (actualNode.ChildNodes.Count < 1) // Im Node sind keine Children
                        {
                            if (xmlRules.HasEndTag(actualNode))
                            {
                                // Wenn der Cursor ein Schließen-Tag anzeigt, dann in den leeren Node setzen
                                cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInsideTheEmptyNode);
                            }
                            else
                            {
                                // Wenn der Cursor kein Schließen-Tag anzeige, dann vor den leeren Node setzen
                                cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                            }
                        }
                        else // Im Node sind Children
                        {
                            cursorPos.SetPos(actualNode.LastChild, XmlCursorPositions.CursorBehindTheNode);
                        }
                    }
                    break;

                case XmlCursorPositions.CursorInsideTheEmptyNode:
                    // Vor den Node setzen
                    cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                    break;

                case XmlCursorPositions.CursorInsideTextNode:
                    if (ToolboxXml.IsTextOrCommentNode(actualNode)) // Node ist Textnode 
                    {
                        if (cursorPos.PosInTextNode > 1)
                        {  // Cursor ein Zeichen nach links
                            cursorPos.SetPos(cursorPos.ActualNode, cursorPos.PosOnNode, cursorPos.PosInTextNode - 1);
                        }
                        else
                        {
                            // Vor den Node setzen
                            cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                        }
                    }
                    else // Kein Textnode
                    {
                        throw new ApplicationException(string.Format("XMLCursorPos.MoveLeft: CursorPos ist XMLCursorPositionen.CursorInnerhalbDesTextNodes, es ist aber kein Textnode gewählt, sondern der Node {0}", actualNode.OuterXml));
                    }
                    break;

                default:
                    throw new ApplicationException(String.Format("XMLCursorPos.MoveLeft: Unbekannte CursorPos {0}", cursorPos.PosOnNode));
            }
            return true;
        }

        /// <summary>
        /// bewegt den angegebenen Cursor um eine Position nach rechts
        /// </summary>
        internal static async Task<bool> MoveRight(XmlCursorPos cursorPos, System.Xml.XmlNode rootnode, XmlRules xmlRules)
        {
            System.Xml.XmlNode node = cursorPos.ActualNode; // Der aktuelle Node

            switch (cursorPos.PosOnNode)
            {
                case XmlCursorPositions.CursorOnNodeStartTag:
                case XmlCursorPositions.CursorOnNodeEndTag:
                    // Hinter den Node setzen
                    cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                    break;

                case XmlCursorPositions.CursorBehindTheNode:
                    if (node.NextSibling != null) // Folgegeschwister vorhanden
                    {
                        // Vor das nächste Geschwister setzen
                        cursorPos.SetPos(node.NextSibling, XmlCursorPositions.CursorInFrontOfNode);
                        // Da "hinter dem ersten" genauso aussieht wie "vor dem zweiten", noch
                        // einen Schritt weiter nach rechts bewegen
                        await MoveRight(cursorPos, rootnode, xmlRules);
                    }
                    else // Keine Folgegeschwister vorhanden, dann hinter den Parentnode setzen
                    {
                        if (node.ParentNode != rootnode)
                        {
                            cursorPos.SetPos(node.ParentNode, XmlCursorPositions.CursorBehindTheNode);
                            if (!xmlRules.HasEndTag(node.ParentNode))
                            { // Wenn für den Parent kein geschlossenes Tag angezeigt wird, dann noch einen weiter nach rechts
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
                    // Hinter den Node setzen
                    cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                    break;

                case XmlCursorPositions.CursorInFrontOfNode:
                    if (ToolboxXml.IsTextOrCommentNode(node))  // Der Node selbst ist Textnode 
                    {
                        if (ToolboxXml.TextFromNodeCleaned(node).Length > 1) // Textnode ist nicht leer
                        {
                            cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, 1); // ein Zeichen vor, also hinter das erste Zeichen
                        }
                        else  // Textnode ist leer
                        {
                            // Hinter den Node setzen
                            cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                        }
                    }
                    else  // Node ist kein Textnode
                    {
                        if (node.ChildNodes.Count < 1) // Keine Children vorhanden
                        {
                            if (!xmlRules.HasEndTag(node)) // Wenn für diesen Node kein geschlossenes Tag angezeigt wird, dann direkt hinter den Node
                            {
                                // Hinter den Node setzen
                                cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                            }
                            else  // Node hat schließendes Tag, also dazwischen setzen
                            {
                                // In den leeren Node setzen
                                cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorInsideTheEmptyNode);
                            }
                        }
                        else // Children vorhanden
                        {
                            cursorPos.SetPos(node.FirstChild, XmlCursorPositions.CursorInFrontOfNode);
                        }
                    }
                    break;

                case XmlCursorPositions.CursorInsideTextNode:
                    if (ToolboxXml.IsTextOrCommentNode(node)) // Node ist Textnode
                    {
                        if (ToolboxXml.TextFromNodeCleaned(node).Length > cursorPos.PosInTextNode + 1) // es folgt rechts noch Text im Textnode
                        {
                            // ein Zeichen vor, also hinter das erste Zeichen
                            cursorPos.SetPos(cursorPos.ActualNode, cursorPos.PosOnNode, cursorPos.PosInTextNode + 1);

                            /*if ((XMLEditor.TextAusTextNodeBereinigt(node).Length == cursor.PosInNode) && (node.NextSibling != null)) 
                            {
                                // Wenn hinter dem letzten Zeichnen des Textnodes und folgendes Geschwister vorhanden, dann
                                // vor den folgenden Geschwisternode
								
                            }*/
                        }
                        else  // es folgt kein Text im Textnode
                        {
                            // Cursor hinter den Node setzen
                            cursorPos.SetPos(cursorPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                        }
                    }
                    else // Node ist kein Textnode
                    {
                        throw new ApplicationException(String.Format("XMLCurorPos.MoveRight: CursorPos ist XMLCursorPositionen.CursorInnerhalbDesTextNodes, es ist aber kein Textnode gewählt, sondern der Node {0}", node.OuterXml));
                    }
                    break;


                default:
                    throw new ApplicationException(String.Format("XMLCurorPos.MoveRight: Unbekannte CursorPos {0}", cursorPos.PosOnNode));
            }
            return true;
        }
    }
}
