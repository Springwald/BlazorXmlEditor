using de.springwald.xml.cursor;
using de.springwald.xml.rules;
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
        /// Liefert den selektierten XML-Inhalt als String 
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetSelektionAlsString(XmlCursor cursor)
        {
            if (cursor. IsSomethingSelected) // Es ist was selektiert
            {
                StringBuilder ergebnis = new StringBuilder();

                XmlCursor optimiert = cursor.Clone();
                await optimiert.OptimizeSelection();

                System.Xml.XmlNode node = optimiert.StartPos.ActualNode; // Beim Startnode anfangen

                // Den Startnode ins Ergebnis aufnehmen
                switch (optimiert.StartPos.PosOnNode)
                {
                    case XmlCursorPositions.CursorOnNodeEndTag:
                    case XmlCursorPositions.CursorOnNodeStartTag:
                    case XmlCursorPositions.CursorInFrontOfNode:

                        ergebnis.Append(node.OuterXml);  // Den ganzen Startnode aufnehmen
                        break;

                    case XmlCursorPositions.CursorBehindTheNode:
                    case XmlCursorPositions.CursorInsideTheEmptyNode:
                        // nicht aufnehmen
                        break;

                    case XmlCursorPositions.CursorInsideTextNode: // Nur einen Teil des Textes aufnehmen
                        string textteil = node.InnerText;

                        int start = optimiert.StartPos.PosInTextNode;
                        int laenge = textteil.Length - start;

                        if (node == optimiert.EndPos.ActualNode) // Wenn dieser Textnode sowohl Start als auch Endnode ist
                        {
                            switch (optimiert.EndPos.PosOnNode)
                            {
                                case XmlCursorPositions.CursorOnNodeEndTag:
                                case XmlCursorPositions.CursorBehindTheNode:
                                    // Länge bleibt bis zum Ende des Nodes
                                    break;

                                case XmlCursorPositions.CursorOnNodeStartTag:
                                case XmlCursorPositions.CursorInsideTheEmptyNode:
                                case XmlCursorPositions.CursorInFrontOfNode:
                                    throw new ApplicationException("XMLCursor.SelektionAlsString: unwahrscheinliche EndPos.PosAmNode '" + optimiert.EndPos.PosOnNode + "' für StartPos.CursorInnerhalbDesTextNodes");

                                case XmlCursorPositions.CursorInsideTextNode:
                                    // Nicht ganz bis zum Ende des Textes 
                                    if (optimiert.StartPos.PosInTextNode > optimiert.EndPos.PosInTextNode)
                                    {
                                        throw new ApplicationException("XMLCursor.SelektionAlsString: optimiert.StartPos.PosImTextnode > optimiert.EndPos.PosImTextnode");
                                    }
                                    else
                                    {
                                        // Den Text nach der Selektion von der Laenge abziehen
                                        laenge -= (textteil.Length - optimiert.EndPos.PosInTextNode);
                                    }
                                    break;

                                default:
                                    throw new ApplicationException("XMLCursor.SelektionAlsString: unbehandelte EndPos.PosAmNode'" + optimiert.EndPos.PosOnNode + "' für StartPos.CursorInnerhalbDesTextNodes");

                            }
                        }

                        textteil = textteil.Substring(start, laenge);
                        ergebnis.Append(textteil);
                        break;

                    default:
                        throw new ApplicationException("XMLCursor.SelektionAlsString: unbehandelte StartPos.PosAmNode'" + optimiert.StartPos.PosOnNode + "'");
                }

                if (optimiert.StartPos.ActualNode != optimiert.EndPos.ActualNode) // Wenn noch weitere Nodes nach dem Startnode selektiert sind
                {
                    do
                    {
                        node = node.NextSibling; // Solange weiter zum nächsten Node...

                        if (node != null)
                        {
                            // Den Node ins Ergebnis aufnehmen
                            if (node == optimiert.EndPos.ActualNode) // Dieser Node ist der EndNode
                            {
                                switch (optimiert.EndPos.PosOnNode)
                                {
                                    case XmlCursorPositions.CursorOnNodeEndTag:
                                    case XmlCursorPositions.CursorOnNodeStartTag:
                                    case XmlCursorPositions.CursorBehindTheNode:
                                        ergebnis.Append(node.OuterXml); // Node 1:1 in Ergebnis aufnehmen
                                        break;

                                    case XmlCursorPositions.CursorInsideTextNode:
                                        // Den Anfang des Textnodes übernehmen
                                        string textteil = node.InnerText;
                                        ergebnis.Append(textteil.Substring(0, optimiert.EndPos.PosInTextNode + 1));
                                        break;

                                    case XmlCursorPositions.CursorInsideTheEmptyNode:
                                        throw new ApplicationException("XMLCursor.SelektionAlsString: unwahrscheinliche EndPos.PosAmNode '" + optimiert.EndPos.PosOnNode + "' für StartPos.Node != EndPos.Node");

                                    default:
                                        throw new ApplicationException("XMLCursor.SelektionAlsString: unbehandelte EndPos.PosAmNode'" + optimiert.StartPos.PosOnNode + "' für StartPos.Node != EndPos.Node");

                                }
                            }
                            else // Node 1:1 in Ergebnis aufnehmen
                            {
                                ergebnis.Append(node.OuterXml);
                            }
                        }

                    } while ((node != optimiert.EndPos.ActualNode) && (node != null)); // ... bis der Endnode erreicht ist

                    if (node == null)
                    {
                        throw new ApplicationException("Endnode war nicht als NextSibling von Startnode erreichbar");
                    }
                }
                return ergebnis.ToString();
            }
            else
            {
                return ""; // es ist gar nichts selektiert
            }
        }

        public struct SelectionLoeschenResult
        {
            public bool Success;
            public XmlCursorPos NeueCursorPosNachLoeschen;
        }

        /// <summary>
        /// Löscht die zwischen StartPos und EndPos des Cursors gelegegen Zeichen und Nodes
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="neueCursorPosNachLoeschen"></param>
        internal static async Task<SelectionLoeschenResult> SelektionLoeschen(XmlCursor cursor)
        {
            // Wenn der Cursor gar keine Auswahl enthält
            if (!cursor.IsSomethingSelected)
            {
                return new SelectionLoeschenResult
                {
                    NeueCursorPosNachLoeschen = cursor.StartPos.Clone(), // Cursor wird nicht verändert
                    Success = false // nichts gelöscht
                };
            }
            else
            {
                if (cursor.StartPos.ActualNode == cursor.EndPos.ActualNode) // Wenn beide Nodes identisch sind
                {
                    switch (cursor.StartPos.PosOnNode)
                    {
                        case XmlCursorPositions.CursorOnNodeStartTag:
                        case XmlCursorPositions.CursorOnNodeEndTag:
                            // ein einzelner Node ist selektiert und soll gelöscht werden

                            System.Xml.XmlNode loeschNode = cursor.StartPos.ActualNode;   // Dieser Node soll gelöscht werden
                            System.Xml.XmlNode nodeVorher = loeschNode.PreviousSibling;  // Dieser Node liegt vor dem zu löschenden
                            System.Xml.XmlNode nodeDanach = loeschNode.NextSibling;  // Dieser Node liegt hinter dem zu löschenden

                            var neueCursorPosNachLoeschen = new XmlCursorPos(); // Dieser benachbarte Node wird nach dem Löschen den Cursor bekommen

                            // Wenn der zu löschende Node zwischen zwei Textnodes liegt, dann werden diese
                            // zwei Textnodes zu einem zusammengefasst
                            if (nodeVorher != null && nodeDanach != null)
                            {
                                if (nodeVorher is System.Xml.XmlText && nodeDanach is System.Xml.XmlText)
                                {   //der zu löschende Node liegt zwischen zwei Textnodes , daher werden diese zwei Textnodes zu einem zusammengefasst

                                    // Nachher steht der Cursor an der Einfügestelle zwischen beiden Textbausteinen
                                    neueCursorPosNachLoeschen.SetPos(nodeVorher, XmlCursorPositions.CursorInsideTextNode, nodeVorher.InnerText.Length);

                                    nodeVorher.InnerText += nodeDanach.InnerText; // Den Text von Nachher-Node an den Vorhernode anhängen

                                    // zu löschenden Node löschen
                                    loeschNode.ParentNode.RemoveChild(loeschNode);

                                    // Nachher-Node löschen
                                    nodeDanach.ParentNode.RemoveChild(nodeDanach);

                                    return new SelectionLoeschenResult
                                    {
                                        NeueCursorPosNachLoeschen = neueCursorPosNachLoeschen,
                                        Success = true
                                    };
                                }
                            }

                            // Der zu löschende Node liegt *nicht* zwischen zwei Textnodes 

                            // Bestimmen, was nach dem Löschen selektiert sein soll

                            if (nodeVorher != null)
                            {
                                // Nach dem Löschen steht der Cursor hinter dem vorherigen Node
                                neueCursorPosNachLoeschen.SetPos(nodeVorher, XmlCursorPositions.CursorBehindTheNode);
                            }
                            else
                            {
                                if (nodeDanach != null)
                                {
                                    // Nach dem Löschen steht der Cursor vor dem folgenden Node
                                    neueCursorPosNachLoeschen.SetPos(nodeDanach, XmlCursorPositions.CursorInFrontOfNode);
                                }
                                else
                                {
                                    // Nach dem Löschen steht der Cursor im Parent-Node
                                    neueCursorPosNachLoeschen.SetPos(loeschNode.ParentNode, XmlCursorPositions.CursorInsideTheEmptyNode);
                                }
                            }

                            // Den Node löschen
                            loeschNode.ParentNode.RemoveChild(loeschNode);

                            return new SelectionLoeschenResult
                            {
                                NeueCursorPosNachLoeschen = neueCursorPosNachLoeschen,
                                Success = true
                            }; // Löschen war erfolgreich

                        case XmlCursorPositions.CursorInFrontOfNode:
                            // Start und Ende des Löschbereiches zeigen auf den selben Node und der
                            // der Start liegt vor dem Node: Das macht nur bei einem Textnode Sinn!
                            if (ToolboxXML.IstTextOderKommentarNode(cursor.StartPos.ActualNode))
                            {
                                // Den Cursor in den Textnode vor dem ersten Zeichen setzen und dann neu abschicken
                                cursor.StartPos.SetPos(cursor.StartPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, 0);
                                return await SelektionLoeschen(cursor); // zum löschen neu abschicken
                            }
                            else
                            {
                                // wenn es kein Textnode ist, dann den ganzen Node markieren und dann neu abschicken
                                await cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(cursor.StartPos.ActualNode, XmlCursorPositions.CursorOnNodeStartTag);
                                return await SelektionLoeschen(cursor);// zum löschen neu abschicken
                            }

                        case XmlCursorPositions.CursorBehindTheNode:
                            // Start und Ende des Löschbereiches zeigen auf den selben Node und der
                            // der Start liegt hinter dem Node
                            if (ToolboxXML.IstTextOderKommentarNode(cursor.StartPos.ActualNode))
                            {
                                // Den Cursor in den Textnode vor dem ersten Zeichen setzen und dann neu abschicken
                                cursor.StartPos.SetPos(cursor.StartPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, cursor.StartPos.ActualNode.InnerText.Length);
                                return await SelektionLoeschen(cursor);// zum löschen neu abschicken
                            }
                            else
                            {
                                // wenn es kein Textnode ist, dann den ganzen Node markieren und dann neu abschicken
                                await cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(cursor.StartPos.ActualNode, XmlCursorPositions.CursorOnNodeStartTag);
                                return await SelektionLoeschen(cursor);// zum löschen neu abschicken
                            }

                        case XmlCursorPositions.CursorInsideTextNode:
                            // ein Teil eines Textnodes soll gelöscht werden
                            // Den zu löschenden Teil des Textes bestimmen
                            int startpos = cursor.StartPos.PosInTextNode;
                            int endpos = cursor.EndPos.PosInTextNode;

                            if (cursor.EndPos.PosOnNode == XmlCursorPositions.CursorBehindTheNode)
                            {	// Wenn das Ende der Auswahl hinter dem Textnode ist, dann 
                                // ist der gesamte restliche Text gewählt
                                endpos = cursor.StartPos.ActualNode.InnerText.Length;
                            }

                            // Wenn der ganze Text markiert ist, dann den gesamten Textnode löschen
                            if (startpos == 0 && endpos >= cursor.StartPos.ActualNode.InnerText.Length)
                            {	// Der ganze Textnode ist zu löschen, das geben wir weiter an die Methode zum löschen
                                // einzeln selektierter Nodes
                                XmlCursor einNodeSelektiertCursor = new XmlCursor();
                                await einNodeSelektiertCursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(cursor.StartPos.ActualNode, XmlCursorPositions.CursorOnNodeStartTag);
                                return await SelektionLoeschen(einNodeSelektiertCursor);
                            }
                            else
                            { // Nur ein Teil des Textes ist zu löschen
                                string restText = cursor.StartPos.ActualNode.InnerText;
                                restText = restText.Remove(startpos, endpos - startpos);
                                cursor.StartPos.ActualNode.InnerText = restText;

                                // bestimmen, wo der Cursor nach dem Löschen steht
                                neueCursorPosNachLoeschen = new XmlCursorPos();
                                if (startpos == 0) // Der Cursor steht vor dem ersten Zeichen
                                {
                                    // dann kann er besser vor den Textnode selbst gestellt werden
                                    neueCursorPosNachLoeschen.SetPos(cursor.StartPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                                }
                                else
                                {
                                    neueCursorPosNachLoeschen.SetPos(cursor.StartPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, startpos);
                                }

                                return new SelectionLoeschenResult
                                {
                                    NeueCursorPosNachLoeschen = neueCursorPosNachLoeschen,
                                    Success = true
                                };  // Löschen erfolgreich
                            }

                        case XmlCursorPositions.CursorInsideTheEmptyNode:
                            if (cursor.EndPos.PosOnNode == XmlCursorPositions.CursorBehindTheNode ||
                                cursor.EndPos.PosOnNode == XmlCursorPositions.CursorInFrontOfNode)
                            {
                                XmlCursor neucursor = new XmlCursor();
                                await neucursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(cursor.StartPos.ActualNode, XmlCursorPositions.CursorOnNodeStartTag, 0);
                                return await SelektionLoeschen(neucursor);
                            }
                            else
                            {
                                throw new ApplicationException("AuswahlLoeschen:#6363S undefined Endpos " + cursor.EndPos.PosOnNode + "!");
                            }

                        default:
                            // was bitteschön soll außer Text und dem Node selbst gewählt sein, wenn Startnode und Endnode identisch sind?
                            throw new ApplicationException("AuswahlLoeschen:#63346 StartPos.PosAmNode " + cursor.StartPos.PosOnNode + " not allowed!");
                    }
                }
                else // Beide Nodes sind nicht idenisch
                {

                    // Wenn beide Nodes nicht identisch sind, dann alle dazwischen liegenden Nodes entfernen, bis
                    // die beiden Nodes hintereinander liegen
                    while (cursor.StartPos.ActualNode.NextSibling != cursor.EndPos.ActualNode)
                    {
                        cursor.StartPos.ActualNode.ParentNode.RemoveChild(cursor.StartPos.ActualNode.NextSibling);
                    }

                    // den Endnode oder einen Teil von ihm löschen
                    XmlCursor temp = cursor.Clone();
                    temp.StartPos.SetPos(cursor.EndPos.ActualNode, XmlCursorPositions.CursorInFrontOfNode);
                    await SelektionLoeschen(temp);

                    // den Startnode, oder einen Teil von ihm löschen
                    // -> Geschieht durch Rekursion in der Selektion-Loeschen-Methode
                    cursor.EndPos.SetPos(cursor.StartPos.ActualNode, XmlCursorPositions.CursorBehindTheNode);
                    return await SelektionLoeschen(cursor);
                }
            }
        }



        /// <summary>
        /// Findet den untersten, gemeinsamen Parent von zwei Nodes. Im Extremfall ist dies das Root-Element,
        /// wenn die Wege der Nodes in die Tiefe des DOMs komplett verschieden sind 
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public static System.Xml.XmlNode TiefsterGemeinsamerParent(System.Xml.XmlNode node1, System.Xml.XmlNode node2)
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
        /// Findet heraus, ob der Node oder einer seiner Parent-Nodes selektiert ist
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        public static bool IsThisNodeInsideSelection(XmlCursor cursor, System.Xml.XmlNode node)
        {
            // Prüfen, ob der Node selbst oder einer seiner Parents direkt selektiert sind
            if (cursor.StartPos.IsNodeInsideSelection(node)) return true;
            if (cursor.EndPos.IsNodeInsideSelection(node)) return true;

            if (cursor.StartPos.Equals(cursor.EndPos)) // Beide Positionen gleich, also ist maximal ein einzelner Node selektiert
            {
                return cursor.StartPos.IsNodeInsideSelection(node);
            }
            else // Beide Positionen sind nicht gleich, also ist evtl. etwas selektiert
            {

                if ((cursor.StartPos.ActualNode == node) || (cursor.EndPos.ActualNode == node)) // Start- oder EndNode der Selektion ist dieser Node
                {
                    if (node is System.Xml.XmlText) // Ist ein Textnode
                    {
                        return true;
                    }
                    else // Ist kein textnode
                    {
                        return false;
                    }
                }
                else
                {
                    if (cursor.StartPos.LiesBehindThisPos(node)) // Node liegt hinter der Startpos
                    {
                        if (cursor.EndPos.LiesBeforeThisPos(node)) // Node liegt zwischen Startpos und  Endepos
                        {
                            return true;
                        }
                        else // Node liegt hinter Startpos aber auch hinter Endpos
                        {
                            return false;
                        }
                    }
                    else // Node liegt nicht hinter der Startpos
                    {
                        return false;
                    }
                }
            }

        }
    }
}
