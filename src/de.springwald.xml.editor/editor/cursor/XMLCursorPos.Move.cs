using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace de.springwald.xml.cursor
{
    public partial class XMLCursorPos
    {
        /// <summary>
        /// bewegt den Cursor um eine Position nach links
        /// </summary>
        /// <param name="cursor"></param>
        public async Task<bool> MoveLeft(System.Xml.XmlNode rootnode, XMLRegelwerk regelwerk)
        {
            System.Xml.XmlNode node = AktNode; // Der aktuelle Node

            switch (PosAmNode)
            {
                case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                    // Vor den Node setzen
                    SetPos(AktNode, XMLCursorPositionen.CursorVorDemNode);
                    break;

                case XMLCursorPositionen.CursorVorDemNode:
                    if (node != rootnode)
                    {
                        if (node.PreviousSibling != null) // Vorheriger Geschwisterknoten vorhanden
                        {
                            SetPos(node.PreviousSibling, XMLCursorPositionen.CursorHinterDemNode);
                            await MoveLeft(rootnode, regelwerk);
                        }
                        else // kein vorheriger Geschwisterknoten vorhanden
                        {
                            SetPos(node.ParentNode, XMLCursorPositionen.CursorVorDemNode);
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
                        SetPos(node, XMLCursorPositionen.CursorInnerhalbDesTextNodes, Math.Max(0,ToolboxXML.TextAusTextNodeBereinigt(node).Length - 1));
                    }
                    else
                    {
                        if (node.ChildNodes.Count < 1) // Im Node sind keine Children
                        {
                            if (regelwerk.IstSchliessendesTagSichtbar(node))
                            {
                                // Wenn der Cursor ein Schließen-Tag anzeigt, dann in den leeren Node setzen
                                SetPos(AktNode, XMLCursorPositionen.CursorInDemLeeremNode);
                            }
                            else
                            {
                                // Wenn der Cursor kein Schließen-Tag anzeige, dann vor den leeren Node setzen
                                SetPos(AktNode, XMLCursorPositionen.CursorVorDemNode);
                            }
                        }
                        else // Im Node sind Children
                        {
                            SetPos(node.LastChild, XMLCursorPositionen.CursorHinterDemNode);
                        }
                    }
                    break;

                case XMLCursorPositionen.CursorInDemLeeremNode:
                    // Vor den Node setzen
                    SetPos(AktNode, XMLCursorPositionen.CursorVorDemNode);
                    break;

                case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                    if (ToolboxXML.IstTextOderKommentarNode(node)) // Node ist Textnode 
                    {
                        if (PosImTextnode > 1)
                        {  // Cursor ein Zeichen nach links
                            SetPos(AktNode, PosAmNode, PosImTextnode - 1);
                        }
                        else
                        {
                            // Vor den Node setzen
                            SetPos(AktNode, XMLCursorPositionen.CursorVorDemNode);
                        }
                    }
                    else // Kein Textnode
                    {
                        throw new ApplicationException(string.Format("XMLCursorPos.MoveLeft: CursorPos ist XMLCursorPositionen.CursorInnerhalbDesTextNodes, es ist aber kein Textnode gewählt, sondern der Node {0}", node.OuterXml));
                    }
                    break;

                default:
                    throw new ApplicationException(String.Format("XMLCursorPos.MoveLeft: Unbekannte CursorPos {0}", PosAmNode));
            }
            return true;
        }




        /// <summary>
        /// bewegt den angegebenen Cursor um eine Position nach rechts
        /// </summary>
        /// <param name="cursor"></param>
        public async Task<bool> MoveRight(System.Xml.XmlNode rootnode, XMLRegelwerk regelwerk)
        {
            System.Xml.XmlNode node = AktNode; // Der aktuelle Node

            switch (PosAmNode)
            {
                case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                    // Hinter den Node setzen
                    SetPos(AktNode, XMLCursorPositionen.CursorHinterDemNode);
                    break;

                case XMLCursorPositionen.CursorHinterDemNode:
                    if (node.NextSibling != null) // Folgegeschwister vorhanden
                    {
                        // Vor das nächste Geschwister setzen
                        SetPos(node.NextSibling, XMLCursorPositionen.CursorVorDemNode);
                        // Da "hinter dem ersten" genauso aussieht wie "vor dem zweiten", noch
                        // einen Schritt weiter nach rechts bewegen
                        await MoveRight(rootnode, regelwerk);
                    }
                    else // Keine Folgegeschwister vorhanden, dann hinter den Parentnode setzen
                    {
                        if (node.ParentNode != rootnode)
                        {
                            SetPos(node.ParentNode, XMLCursorPositionen.CursorHinterDemNode);
                            if (!regelwerk.IstSchliessendesTagSichtbar(node.ParentNode))
                            { // Wenn für den Parent kein geschlossenes Tag angezeigt wird, dann noch einen weiter nach rechts
                                await MoveRight(rootnode, regelwerk);
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
                    SetPos(AktNode, XMLCursorPositionen.CursorHinterDemNode);
                    break;

                case XMLCursorPositionen.CursorVorDemNode:
                    if (ToolboxXML.IstTextOderKommentarNode(node))  // Der Node selbst ist Textnode 
                    {
                        if (ToolboxXML.TextAusTextNodeBereinigt(node).Length > 1) // Textnode ist nicht leer
                        {
                            SetPos(AktNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, 1); // ein Zeichen vor, also hinter das erste Zeichen
                        }
                        else  // Textnode ist leer
                        {
                            // Hinter den Node setzen
                            SetPos(AktNode, XMLCursorPositionen.CursorHinterDemNode);
                        }
                    }
                    else  // Node ist kein Textnode
                    {
                        if (node.ChildNodes.Count < 1) // Keine Children vorhanden
                        {
                            if (!regelwerk.IstSchliessendesTagSichtbar(node)) // Wenn für diesen Node kein geschlossenes Tag angezeigt wird, dann direkt hinter den Node
                            {
                                // Hinter den Node setzen
                                SetPos(AktNode, XMLCursorPositionen.CursorHinterDemNode);
                            }
                            else  // Node hat schließendes Tag, also dazwischen setzen
                            {
                                // In den leeren Node setzen
                                SetPos(AktNode, XMLCursorPositionen.CursorInDemLeeremNode);
                            }
                        }
                        else // Children vorhanden
                        {
                            SetPos(node.FirstChild, XMLCursorPositionen.CursorVorDemNode);
                        }
                    }
                    break;

                case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                    if (ToolboxXML.IstTextOderKommentarNode(node)) // Node ist Textnode
                    {
                        if (ToolboxXML.TextAusTextNodeBereinigt(node).Length > PosImTextnode + 1) // es folgt rechts noch Text im Textnode
                        {
                            // ein Zeichen vor, also hinter das erste Zeichen
                            SetPos(AktNode, PosAmNode, PosImTextnode + 1);

                            /*if ((XMLEditor.TextAusTextNodeBereinigt(node).Length == cursor.PosInNode) && (node.NextSibling != null)) 
                            {
                                // Wenn hinter dem letzten Zeichnen des Textnodes und folgendes Geschwister vorhanden, dann
                                // vor den folgenden Geschwisternode
								
                            }*/
                        }
                        else  // es folgt kein Text im Textnode
                        {
                            // Cursor hinter den Node setzen
                            SetPos(AktNode, XMLCursorPositionen.CursorHinterDemNode);
                        }
                    }
                    else // Node ist kein Textnode
                    {
                        throw new ApplicationException(String.Format("XMLCurorPos.MoveRight: CursorPos ist XMLCursorPositionen.CursorInnerhalbDesTextNodes, es ist aber kein Textnode gewählt, sondern der Node {0}", node.OuterXml));
                    }
                    break;


                default:
                    throw new ApplicationException(String.Format("XMLCurorPos.MoveRight: Unbekannte CursorPos {0}", PosAmNode));
            }
            return true;
        }

        /// <summary>
        /// Sets new values to this cursor pos
        /// </summary>
        /// <param name="aktNode"></param>
        /// <param name="posAmNode"></param>
        /// <param name="posImTextnode"></param>
        /// <returns>true, when values where other than before</returns>
        public bool SetPos(System.Xml.XmlNode aktNode, XMLCursorPositionen posAmNode, int posImTextnode = 0)
        {
            bool changed;
            if (aktNode != _aktNode)
            {
                changed = true;
            }
            else
            {
                if (posAmNode != _posAmNode)
                {
                    changed = true;
                }
                else
                {
                    if (posImTextnode != _posImTextnode)
                    {
                        changed = true;
                    }
                    else
                    {
                        changed = false;
                    }
                }
            }

            this._aktNode = aktNode;
            this._posAmNode = posAmNode;
            this._posImTextnode = posImTextnode;
            return changed;
        }

        ///// <summary>
        ///// Setzt gleichzeitig Node und Position und löst dadurch nur ein Changed-Event statt zwei aus
        ///// </summary>
        ///// <param name="aktNode"></param>
        ///// <param name="posInNode"></param>
        ///// <param name="posImTextnode"></param>
        //public async Task CursorSetzenMitChangeEventWennGeaendert(System.Xml.XmlNode aktNode, XMLCursorPositionen posAmNode, int posImTextnode = 0)
        //{
        //    bool geaendert;
        //    if (aktNode != _aktNode)
        //    {
        //        geaendert = true;
        //    }
        //    else
        //    {
        //        if (posAmNode != _posAmNode)
        //        {
        //            geaendert = true;
        //        }
        //        else
        //        {
        //            if (posImTextnode != _posImTextnode)
        //            {
        //                geaendert = true;
        //            }
        //            else
        //            {
        //                geaendert = false;
        //            }
        //        }
        //    }
        //    this.CursorSetzenOhneChangeEvent(aktNode, posAmNode, posImTextnode);
        //    if (geaendert) await this.PosChangedEvent.Trigger(EventArgs.Empty); // Bescheid geben, dass nun der Cursor geändert wurde
        //}

        ///// <summary>
        ///// Setzt gleichzeitig Node und Position und löst dadurch nur ein Changed-Event statt zwei aus
        ///// </summary>
        ///// <param name="aktNode"></param>
        ///// <param name="posInNode"></param>
        ///// <param name="posImTextnode"></param>
        //public void CursorSetzenOhneChangeEvent(System.Xml.XmlNode aktNode, XMLCursorPositionen posAmNode, int posImTextnode = 0)
        //{
        //    _aktNode = aktNode;
        //    _posAmNode = posAmNode;
        //    _posImTextnode = posImTextnode;
        //}
    }
}
