using de.springwald.toolbox;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace de.springwald.xml.cursor
{
    partial class XMLCursor
    {
        /// <summary>
        /// Sind Zeichen oder Nodes von diesem Cursor eingeschlossen
        /// </summary>
        /// <remarks>
        /// Entweder ist ein einzelner Node von der Startpos selektiert, oder die selektierten Bereiche liegen
        /// zwischen StartPos und EndPos
        /// </remarks>
        public bool IstEtwasSelektiert
        {
            get
            {
                // Wenn gar kein Cursor gesetzt ist, dann ist auch nix selektiert
                if (StartPos.AktNode == null) return false;

                if ((this.StartPos.PosAmNode == XMLCursorPositionen.CursorAufNodeSelbstVorderesTag) ||
                    (this.StartPos.PosAmNode == XMLCursorPositionen.CursorAufNodeSelbstHinteresTag))
                {
                    return true; // mindestens ein einzelner Node ist direkt selektiert
                }
                else
                {
                    if (this.StartPos.Equals(this.EndPos))
                    {
                        return false; // offenbar ist der Cursor nur ein Strich mittendrin, ohne etwas selektiert zu haben
                    }
                    else
                    {
                        return true; // Start- und Endpos sind unterschiedlich, daher sollte etwas dazwischen liegen
                    }
                }
            }
        }

        /// <summary>
        /// Liefert den selektierten XML-Inhalt als String 
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetSelektionAlsString()
        {
            if (IstEtwasSelektiert) // Es ist was selektiert
            {
                StringBuilder ergebnis = new StringBuilder();

                XMLCursor optimiert = this.Clone();
                await optimiert.SelektionOptimieren();

                System.Xml.XmlNode node = optimiert.StartPos.AktNode; // Beim Startnode anfangen

                // Den Startnode ins Ergebnis aufnehmen
                switch (optimiert.StartPos.PosAmNode)
                {
                    case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                    case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                    case XMLCursorPositionen.CursorVorDemNode:

                        ergebnis.Append(node.OuterXml);  // Den ganzen Startnode aufnehmen
                        break;

                    case XMLCursorPositionen.CursorHinterDemNode:
                    case XMLCursorPositionen.CursorInDemLeeremNode:
                        // nicht aufnehmen
                        break;

                    case XMLCursorPositionen.CursorInnerhalbDesTextNodes: // Nur einen Teil des Textes aufnehmen
                        string textteil = node.InnerText;

                        int start = optimiert.StartPos.PosImTextnode;
                        int laenge = textteil.Length - start;

                        if (node == optimiert.EndPos.AktNode) // Wenn dieser Textnode sowohl Start als auch Endnode ist
                        {
                            switch (optimiert.EndPos.PosAmNode)
                            {
                                case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                                case XMLCursorPositionen.CursorHinterDemNode:
                                    // L�nge bleibt bis zum Ende des Nodes
                                    break;

                                case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                                case XMLCursorPositionen.CursorInDemLeeremNode:
                                case XMLCursorPositionen.CursorVorDemNode:
                                    throw new ApplicationException("XMLCursor.SelektionAlsString: unwahrscheinliche EndPos.PosAmNode '" + optimiert.EndPos.PosAmNode + "' f�r StartPos.CursorInnerhalbDesTextNodes");

                                case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                                    // Nicht ganz bis zum Ende des Textes 
                                    if (optimiert.StartPos.PosImTextnode > optimiert.EndPos.PosImTextnode)
                                    {
                                        throw new ApplicationException("XMLCursor.SelektionAlsString: optimiert.StartPos.PosImTextnode > optimiert.EndPos.PosImTextnode");
                                    }
                                    else
                                    {
                                        // Den Text nach der Selektion von der Laenge abziehen
                                        laenge -= (textteil.Length - optimiert.EndPos.PosImTextnode);
                                    }
                                    break;

                                default:
                                    throw new ApplicationException("XMLCursor.SelektionAlsString: unbehandelte EndPos.PosAmNode'" + optimiert.EndPos.PosAmNode + "' f�r StartPos.CursorInnerhalbDesTextNodes");

                            }
                        }

                        textteil = textteil.Substring(start, laenge);
                        ergebnis.Append(textteil);
                        break;

                    default:
                        throw new ApplicationException("XMLCursor.SelektionAlsString: unbehandelte StartPos.PosAmNode'" + optimiert.StartPos.PosAmNode + "'");
                }

                if (optimiert.StartPos.AktNode != optimiert.EndPos.AktNode) // Wenn noch weitere Nodes nach dem Startnode selektiert sind
                {
                    do
                    {
                        node = node.NextSibling; // Solange weiter zum n�chsten Node...

                        if (node != null)
                        {
                            // Den Node ins Ergebnis aufnehmen
                            if (node == optimiert.EndPos.AktNode) // Dieser Node ist der EndNode
                            {
                                switch (optimiert.EndPos.PosAmNode)
                                {
                                    case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                                    case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                                    case XMLCursorPositionen.CursorHinterDemNode:
                                        ergebnis.Append(node.OuterXml); // Node 1:1 in Ergebnis aufnehmen
                                        break;

                                    case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                                        // Den Anfang des Textnodes �bernehmen
                                        string textteil = node.InnerText;
                                        ergebnis.Append(textteil.Substring(0, optimiert.EndPos.PosImTextnode + 1));
                                        break;

                                    case XMLCursorPositionen.CursorInDemLeeremNode:
                                        throw new ApplicationException("XMLCursor.SelektionAlsString: unwahrscheinliche EndPos.PosAmNode '" + optimiert.EndPos.PosAmNode + "' f�r StartPos.Node != EndPos.Node");

                                    default:
                                        throw new ApplicationException("XMLCursor.SelektionAlsString: unbehandelte EndPos.PosAmNode'" + optimiert.StartPos.PosAmNode + "' f�r StartPos.Node != EndPos.Node");

                                }
                            }
                            else // Node 1:1 in Ergebnis aufnehmen
                            {
                                ergebnis.Append(node.OuterXml);
                            }
                        }

                    } while ((node != optimiert.EndPos.AktNode) && (node != null)); // ... bis der Endnode erreicht ist

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
            public XMLCursorPos NeueCursorPosNachLoeschen;
        }

        /// <summary>
        /// L�scht die zwischen StartPos und EndPos des Cursors gelegegen Zeichen und Nodes
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="neueCursorPosNachLoeschen"></param>
        public async Task<SelectionLoeschenResult> SelektionLoeschen()
        {
            // Wenn der Cursor gar keine Auswahl enth�lt
            if (!IstEtwasSelektiert)
            {
                return new SelectionLoeschenResult
                {
                    NeueCursorPosNachLoeschen = StartPos.Clone(), // Cursor wird nicht ver�ndert
                    Success = false // nichts gel�scht
                };
            }
            else
            {
                if (StartPos.AktNode == EndPos.AktNode) // Wenn beide Nodes identisch sind
                {
                    switch (StartPos.PosAmNode)
                    {
                        case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                        case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                            // ein einzelner Node ist selektiert und soll gel�scht werden

                            System.Xml.XmlNode loeschNode = StartPos.AktNode;   // Dieser Node soll gel�scht werden
                            System.Xml.XmlNode nodeVorher = loeschNode.PreviousSibling;  // Dieser Node liegt vor dem zu l�schenden
                            System.Xml.XmlNode nodeDanach = loeschNode.NextSibling;  // Dieser Node liegt hinter dem zu l�schenden

                            var neueCursorPosNachLoeschen = new XMLCursorPos(); // Dieser benachbarte Node wird nach dem L�schen den Cursor bekommen

                            // Wenn der zu l�schende Node zwischen zwei Textnodes liegt, dann werden diese
                            // zwei Textnodes zu einem zusammengefasst
                            if (nodeVorher != null && nodeDanach != null)
                            {
                                if (nodeVorher is System.Xml.XmlText && nodeDanach is System.Xml.XmlText)
                                {   //der zu l�schende Node liegt zwischen zwei Textnodes , daher werden diese zwei Textnodes zu einem zusammengefasst

                                    // Nachher steht der Cursor an der Einf�gestelle zwischen beiden Textbausteinen
                                    await neueCursorPosNachLoeschen.CursorSetzenMitChangeEventWennGeaendert(nodeVorher, XMLCursorPositionen.CursorInnerhalbDesTextNodes, nodeVorher.InnerText.Length);

                                    nodeVorher.InnerText += nodeDanach.InnerText; // Den Text von Nachher-Node an den Vorhernode anh�ngen

                                    // zu l�schenden Node l�schen
                                    loeschNode.ParentNode.RemoveChild(loeschNode);

                                    // Nachher-Node l�schen
                                    nodeDanach.ParentNode.RemoveChild(nodeDanach);

                                    return new SelectionLoeschenResult
                                    {
                                        NeueCursorPosNachLoeschen = neueCursorPosNachLoeschen,
                                        Success = true
                                    };
                                }
                            }

                            // Der zu l�schende Node liegt *nicht* zwischen zwei Textnodes 

                            // Bestimmen, was nach dem L�schen selektiert sein soll

                            if (nodeVorher != null)
                            {
                                // Nach dem L�schen steht der Cursor hinter dem vorherigen Node
                                await neueCursorPosNachLoeschen.CursorSetzenMitChangeEventWennGeaendert(nodeVorher, XMLCursorPositionen.CursorHinterDemNode);
                            }
                            else
                            {
                                if (nodeDanach != null)
                                {
                                    // Nach dem L�schen steht der Cursor vor dem folgenden Node
                                    await neueCursorPosNachLoeschen.CursorSetzenMitChangeEventWennGeaendert(nodeDanach, XMLCursorPositionen.CursorVorDemNode);
                                }
                                else
                                {
                                    // Nach dem L�schen steht der Cursor im Parent-Node
                                    await neueCursorPosNachLoeschen.CursorSetzenMitChangeEventWennGeaendert(loeschNode.ParentNode, XMLCursorPositionen.CursorInDemLeeremNode);
                                }
                            }

                            // Den Node l�schen
                            loeschNode.ParentNode.RemoveChild(loeschNode);

                            return new SelectionLoeschenResult
                            {
                                NeueCursorPosNachLoeschen = neueCursorPosNachLoeschen,
                                Success = true
                            }; // L�schen war erfolgreich

                        case XMLCursorPositionen.CursorVorDemNode:
                            // Start und Ende des L�schbereiches zeigen auf den selben Node und der
                            // der Start liegt vor dem Node: Das macht nur bei einem Textnode Sinn!
                            if (ToolboxXML.IstTextOderKommentarNode(StartPos.AktNode))
                            {
                                // Den Cursor in den Textnode vor dem ersten Zeichen setzen und dann neu abschicken
                                await StartPos.CursorSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, 0);
                                return await SelektionLoeschen(); // zum l�schen neu abschicken
                            }
                            else
                            {
                                // wenn es kein Textnode ist, dann den ganzen Node markieren und dann neu abschicken
                                await BeideCursorPosSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
                                return await SelektionLoeschen();// zum l�schen neu abschicken
                            }

                        case XMLCursorPositionen.CursorHinterDemNode:
                            // Start und Ende des L�schbereiches zeigen auf den selben Node und der
                            // der Start liegt hinter dem Node
                            if (ToolboxXML.IstTextOderKommentarNode(StartPos.AktNode))
                            {
                                // Den Cursor in den Textnode vor dem ersten Zeichen setzen und dann neu abschicken
                                await StartPos.CursorSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, StartPos.AktNode.InnerText.Length);
                                return await SelektionLoeschen();// zum l�schen neu abschicken
                            }
                            else
                            {
                                // wenn es kein Textnode ist, dann den ganzen Node markieren und dann neu abschicken
                                await BeideCursorPosSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
                                return await SelektionLoeschen();// zum l�schen neu abschicken
                            }

                        case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                            // ein Teil eines Textnodes soll gel�scht werden
                            // Den zu l�schenden Teil des Textes bestimmen
                            int startpos = StartPos.PosImTextnode;
                            int endpos = EndPos.PosImTextnode;

                            if (EndPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode)
                            {	// Wenn das Ende der Auswahl hinter dem Textnode ist, dann 
                                // ist der gesamte restliche Text gew�hlt
                                endpos = StartPos.AktNode.InnerText.Length;
                            }

                            // Wenn der ganze Text markiert ist, dann den gesamten Textnode l�schen
                            if (startpos == 0 && endpos >= StartPos.AktNode.InnerText.Length)
                            {	// Der ganze Textnode ist zu l�schen, das geben wir weiter an die Methode zum l�schen
                                // einzeln selektierter Nodes
                                XMLCursor einNodeSelektiertCursor = new XMLCursor();
                                await einNodeSelektiertCursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
                                return await einNodeSelektiertCursor.SelektionLoeschen();
                            }
                            else
                            { // Nur ein Teil des Textes ist zu l�schen
                                string restText = StartPos.AktNode.InnerText;
                                restText = restText.Remove(startpos, endpos - startpos);
                                StartPos.AktNode.InnerText = restText;

                                // bestimmen, wo der Cursor nach dem L�schen steht
                                neueCursorPosNachLoeschen = new XMLCursorPos();
                                if (startpos == 0) // Der Cursor steht vor dem ersten Zeichen
                                {
                                    // dann kann er besser vor den Textnode selbst gestellt werden
                                    await neueCursorPosNachLoeschen.CursorSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorVorDemNode);
                                }
                                else
                                {
                                    await neueCursorPosNachLoeschen.CursorSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, startpos);
                                }

                                return new SelectionLoeschenResult
                                {
                                    NeueCursorPosNachLoeschen = neueCursorPosNachLoeschen,
                                    Success = true
                                };  // L�schen erfolgreich
                            }

                        case XMLCursorPositionen.CursorInDemLeeremNode:
                            if (EndPos.PosAmNode == XMLCursorPositionen.CursorHinterDemNode ||
                                EndPos.PosAmNode == XMLCursorPositionen.CursorVorDemNode)
                            {
                                XMLCursor neucursor = new XMLCursor();
                                await neucursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, 0);
                                return await neucursor.SelektionLoeschen();
                            }
                            else
                            {
                                throw new ApplicationException("AuswahlLoeschen:#6363S undefined Endpos " + EndPos.PosAmNode + "!");
                            }

                        default:
                            // was bittesch�n soll au�er Text und dem Node selbst gew�hlt sein, wenn Startnode und Endnode identisch sind?
                            throw new ApplicationException("AuswahlLoeschen:#63346 StartPos.PosAmNode " + StartPos.PosAmNode + " not allowed!");
                    }
                }
                else // Beide Nodes sind nicht idenisch
                {

                    // Wenn beide Nodes nicht identisch sind, dann alle dazwischen liegenden Nodes entfernen, bis
                    // die beiden Nodes hintereinander liegen
                    while (StartPos.AktNode.NextSibling != EndPos.AktNode)
                    {
                        StartPos.AktNode.ParentNode.RemoveChild(StartPos.AktNode.NextSibling);
                    }

                    // den Endnode oder einen Teil von ihm l�schen
                    XMLCursor temp = this.Clone();
                    await temp.StartPos.CursorSetzenMitChangeEventWennGeaendert(EndPos.AktNode, XMLCursorPositionen.CursorVorDemNode);
                    await temp.SelektionLoeschen();

                    // den Startnode, oder einen Teil von ihm l�schen
                    // -> Geschieht durch Rekursion in der Selektion-Loeschen-Methode
                    await EndPos.CursorSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorHinterDemNode);
                    return await SelektionLoeschen();
                }
            }
        }

        /// <summary>
        /// Optimiert den selektierten Bereich 
        /// </summary>
        public async Task SelektionOptimieren()
        {
            // Tauschbuffer-Variablen definieren
            XMLCursorPositionen dummyPos;
            int dummyTextPos;

            if (StartPos.AktNode == null) return; // Nix im Cursor, daher nix zu optimieren

            // 1. Wenn die Startpos hinter der Endpos liegt, dann beide vertauschen
            if (StartPos.AktNode == EndPos.AktNode)  // Beide Nodes sind gleich
            {
                if (StartPos.PosAmNode > EndPos.PosAmNode) // Wenn StartPos innerhalb eines Nodes hinter EndPos liegt
                {
                    // beide Positionen am selben Node tauschen
                    dummyPos = StartPos.PosAmNode;
                    dummyTextPos = StartPos.PosImTextnode;
                    await StartPos.CursorSetzenMitChangeEventWennGeaendert(EndPos.AktNode, EndPos.PosAmNode, EndPos.PosImTextnode);
                    await EndPos.CursorSetzenMitChangeEventWennGeaendert(EndPos.AktNode, dummyPos, dummyTextPos);
                }
                else // StartPos lag nicht hinter Endpos
                {
                    // Ist ist ein Textteil innerhalb eines Textnodes selektiert ?
                    if ((StartPos.PosAmNode == XMLCursorPositionen.CursorInnerhalbDesTextNodes) && (EndPos.PosAmNode == XMLCursorPositionen.CursorInnerhalbDesTextNodes))
                    {  // Ein Teil eines Textnodes ist selektiert
                        if (StartPos.PosImTextnode > EndPos.PosImTextnode) // Wenn die TextStartpos hinter der TextEndpos liegt, dann wechseln
                        {   // Textauswahl tauschen
                            dummyTextPos = StartPos.PosImTextnode;
                            await StartPos.CursorSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, EndPos.PosImTextnode);
                            await EndPos.CursorSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, dummyTextPos);
                        }
                    }
                }
            }
            else // Beide Nodes sind nicht gleich
            {
                // Wenn die Nodes in der Reihenfolge falsch sind, dann beide vertauschen
                if (ToolboxXML.Node1LiegtVorNode2(EndPos.AktNode, StartPos.AktNode))
                {
                    XMLCursorPos tempPos = StartPos;
                    StartPos = EndPos;
                    EndPos = tempPos;
                }

                // Wenn der EndNode im StartNode liegt, den gesamten, umgebenden Startnode selektieren
                if (ToolboxXML.IstChild(EndPos.AktNode, StartPos.AktNode))
                {
                    await BeideCursorPosSetzenMitChangeEventWennGeaendert(StartPos.AktNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
                }

                // Den ersten gemeinsamen Parent von Start und Ende finden, und in dieser H�he die Nodes selektieren.
                // Das f�hrt dazu, dass z.B. bei LI-Elemente und UL beim Ziehen der Selektion �ber mehrere LI immer
                // nur ganze LI selektiert werden und nicht nur Teile davon
                if (StartPos.AktNode.ParentNode != EndPos.AktNode.ParentNode) // wenn Start und Ende nicht direkt im selben Parent stecken
                {
                    // - zuerst herausfinden, welches der tiefste, gemeinsame Parent von Start- und End-node ist
                    System.Xml.XmlNode gemeinsamerParent = TiefsterGemeinsamerParent(StartPos.AktNode, EndPos.AktNode);
                    // - dann Start- und End-Node bis vor dem Parent hoch-eskalieren
                    System.Xml.XmlNode nodeStart = StartPos.AktNode;
                    while (nodeStart.ParentNode != gemeinsamerParent) nodeStart = nodeStart.ParentNode;
                    System.Xml.XmlNode nodeEnde = EndPos.AktNode;
                    while (nodeEnde.ParentNode != gemeinsamerParent) nodeEnde = nodeEnde.ParentNode;
                    // - schlie�lich die neuen Start- und End-Nodes anzeigen  
                    await StartPos.CursorSetzenMitChangeEventWennGeaendert(nodeStart, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
                    await EndPos.CursorSetzenMitChangeEventWennGeaendert(nodeEnde, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
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
        public System.Xml.XmlNode TiefsterGemeinsamerParent(System.Xml.XmlNode node1, System.Xml.XmlNode node2)
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
        public bool IstNodeInnerhalbDerSelektion(System.Xml.XmlNode node)
        {
            // Pr�fen, ob der Node selbst oder einer seiner Parents direkt selektiert sind
            if (StartPos.IstNodeInnerhalbDerSelektion(node)) return true;
            if (EndPos.IstNodeInnerhalbDerSelektion(node)) return true;

            if (StartPos.Equals(EndPos)) // Beide Positionen gleich, also ist maximal ein einzelner Node selektiert
            {
                return StartPos.IstNodeInnerhalbDerSelektion(node);
            }
            else // Beide Positionen sind nicht gleich, also ist evtl. etwas selektiert
            {

                if ((StartPos.AktNode == node) || (EndPos.AktNode == node)) // Start- oder EndNode der Selektion ist dieser Node
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
                    if (StartPos.LiegtNodeHinterDieserPos(node)) // Node liegt hinter der Startpos
                    {
                        if (EndPos.LiegtNodeVorDieserPos(node)) // Node liegt zwischen Startpos und  Endepos
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
