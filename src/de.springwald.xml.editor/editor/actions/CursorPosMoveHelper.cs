using de.springwald.xml.cursor;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.editor.actions
{
    internal static class CursorPosMoveHelper
    {
        /// <summary>
        /// bewegt den Cursor um eine Position nach links
        /// </summary>
        /// <param name="cursor"></param>
        internal static async Task<bool> MoveLeft(XMLCursorPos cursorPos, System.Xml.XmlNode rootnode, XMLRegelwerk regelwerk)
        {
            System.Xml.XmlNode node = cursorPos.AktNode; // Der aktuelle Node

            switch (cursorPos.PosAmNode)
            {
                case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                    // Vor den Node setzen
                    cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorVorDemNode);
                    break;

                case XMLCursorPositionen.CursorVorDemNode:
                    if (node != rootnode)
                    {
                        if (node.PreviousSibling != null) // Vorheriger Geschwisterknoten vorhanden
                        {
                            cursorPos.SetPos(node.PreviousSibling, XMLCursorPositionen.CursorHinterDemNode);
                            await MoveLeft(cursorPos, rootnode, regelwerk);
                        }
                        else // kein vorheriger Geschwisterknoten vorhanden
                        {
                            cursorPos.SetPos(node.ParentNode, XMLCursorPositionen.CursorVorDemNode);
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;

                case XMLCursorPositionen.CursorHinterDemNode:
                    if (ToolboxXML.IstTextOderKommentarNode(node)) // Bei einem Textnode wird der Cursor hinter das letzte Zeichen gesetzt
                    {
                        cursorPos.SetPos(node, XMLCursorPositionen.CursorInnerhalbDesTextNodes, Math.Max(0, ToolboxXML.TextAusTextNodeBereinigt(node).Length - 1));
                    }
                    else
                    {
                        if (node.ChildNodes.Count < 1) // Im Node sind keine Children
                        {
                            if (regelwerk.IstSchliessendesTagSichtbar(node))
                            {
                                // Wenn der Cursor ein Schließen-Tag anzeigt, dann in den leeren Node setzen
                                cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorInDemLeeremNode);
                            }
                            else
                            {
                                // Wenn der Cursor kein Schließen-Tag anzeige, dann vor den leeren Node setzen
                                cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorVorDemNode);
                            }
                        }
                        else // Im Node sind Children
                        {
                            cursorPos.SetPos(node.LastChild, XMLCursorPositionen.CursorHinterDemNode);
                        }
                    }
                    break;

                case XMLCursorPositionen.CursorInDemLeeremNode:
                    // Vor den Node setzen
                    cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorVorDemNode);
                    break;

                case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                    if (ToolboxXML.IstTextOderKommentarNode(node)) // Node ist Textnode 
                    {
                        if (cursorPos.PosImTextnode > 1)
                        {  // Cursor ein Zeichen nach links
                            cursorPos.SetPos(cursorPos.AktNode, cursorPos.PosAmNode, cursorPos.PosImTextnode - 1);
                        }
                        else
                        {
                            // Vor den Node setzen
                            cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorVorDemNode);
                        }
                    }
                    else // Kein Textnode
                    {
                        throw new ApplicationException(string.Format("XMLCursorPos.MoveLeft: CursorPos ist XMLCursorPositionen.CursorInnerhalbDesTextNodes, es ist aber kein Textnode gewählt, sondern der Node {0}", node.OuterXml));
                    }
                    break;

                default:
                    throw new ApplicationException(String.Format("XMLCursorPos.MoveLeft: Unbekannte CursorPos {0}", cursorPos.PosAmNode));
            }
            return true;
        }




        /// <summary>
        /// bewegt den angegebenen Cursor um eine Position nach rechts
        /// </summary>
        /// <param name="cursor"></param>
        internal static async Task<bool> MoveRight(XMLCursorPos cursorPos, System.Xml.XmlNode rootnode, XMLRegelwerk regelwerk)
        {
            System.Xml.XmlNode node = cursorPos.AktNode; // Der aktuelle Node

            switch (cursorPos.PosAmNode)
            {
                case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                    // Hinter den Node setzen
                    cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorHinterDemNode);
                    break;

                case XMLCursorPositionen.CursorHinterDemNode:
                    if (node.NextSibling != null) // Folgegeschwister vorhanden
                    {
                        // Vor das nächste Geschwister setzen
                        cursorPos.SetPos(node.NextSibling, XMLCursorPositionen.CursorVorDemNode);
                        // Da "hinter dem ersten" genauso aussieht wie "vor dem zweiten", noch
                        // einen Schritt weiter nach rechts bewegen
                        await MoveRight(cursorPos, rootnode, regelwerk);
                    }
                    else // Keine Folgegeschwister vorhanden, dann hinter den Parentnode setzen
                    {
                        if (node.ParentNode != rootnode)
                        {
                            cursorPos.SetPos(node.ParentNode, XMLCursorPositionen.CursorHinterDemNode);
                            if (!regelwerk.IstSchliessendesTagSichtbar(node.ParentNode))
                            { // Wenn für den Parent kein geschlossenes Tag angezeigt wird, dann noch einen weiter nach rechts
                                await MoveRight(cursorPos, rootnode, regelwerk);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;

                case XMLCursorPositionen.CursorInDemLeeremNode:
                    // Hinter den Node setzen
                    cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorHinterDemNode);
                    break;

                case XMLCursorPositionen.CursorVorDemNode:
                    if (ToolboxXML.IstTextOderKommentarNode(node))  // Der Node selbst ist Textnode 
                    {
                        if (ToolboxXML.TextAusTextNodeBereinigt(node).Length > 1) // Textnode ist nicht leer
                        {
                            cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, 1); // ein Zeichen vor, also hinter das erste Zeichen
                        }
                        else  // Textnode ist leer
                        {
                            // Hinter den Node setzen
                            cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorHinterDemNode);
                        }
                    }
                    else  // Node ist kein Textnode
                    {
                        if (node.ChildNodes.Count < 1) // Keine Children vorhanden
                        {
                            if (!regelwerk.IstSchliessendesTagSichtbar(node)) // Wenn für diesen Node kein geschlossenes Tag angezeigt wird, dann direkt hinter den Node
                            {
                                // Hinter den Node setzen
                                cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorHinterDemNode);
                            }
                            else  // Node hat schließendes Tag, also dazwischen setzen
                            {
                                // In den leeren Node setzen
                                cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorInDemLeeremNode);
                            }
                        }
                        else // Children vorhanden
                        {
                            cursorPos.SetPos(node.FirstChild, XMLCursorPositionen.CursorVorDemNode);
                        }
                    }
                    break;

                case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                    if (ToolboxXML.IstTextOderKommentarNode(node)) // Node ist Textnode
                    {
                        if (ToolboxXML.TextAusTextNodeBereinigt(node).Length > cursorPos.PosImTextnode + 1) // es folgt rechts noch Text im Textnode
                        {
                            // ein Zeichen vor, also hinter das erste Zeichen
                            cursorPos.SetPos(cursorPos.AktNode, cursorPos.PosAmNode, cursorPos.PosImTextnode + 1);

                            /*if ((XMLEditor.TextAusTextNodeBereinigt(node).Length == cursor.PosInNode) && (node.NextSibling != null)) 
                            {
                                // Wenn hinter dem letzten Zeichnen des Textnodes und folgendes Geschwister vorhanden, dann
                                // vor den folgenden Geschwisternode
								
                            }*/
                        }
                        else  // es folgt kein Text im Textnode
                        {
                            // Cursor hinter den Node setzen
                            cursorPos.SetPos(cursorPos.AktNode, XMLCursorPositionen.CursorHinterDemNode);
                        }
                    }
                    else // Node ist kein Textnode
                    {
                        throw new ApplicationException(String.Format("XMLCurorPos.MoveRight: CursorPos ist XMLCursorPositionen.CursorInnerhalbDesTextNodes, es ist aber kein Textnode gewählt, sondern der Node {0}", node.OuterXml));
                    }
                    break;


                default:
                    throw new ApplicationException(String.Format("XMLCurorPos.MoveRight: Unbekannte CursorPos {0}", cursorPos.PosAmNode));
            }
            return true;
        }
    }
}
