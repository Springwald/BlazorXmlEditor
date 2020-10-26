using de.springwald.xml.cursor;
using de.springwald.xml.rules;
using System;
using System.Diagnostics;
using static de.springwald.xml.rules.XMLCursorPos;

namespace de.springwald.xml.editor.actions
{
    internal static class InsertAtCursorPosHelper
    {
        internal struct TextEinfuegeResult
        {
            public System.Xml.XmlNode ErsatzNode;
        }

        /// <summary>
        /// Fügt den angegebenen Text an der aktuellen Cursorposition ein, sofern möglich
        /// </summary>
        /// <param name="text">Der einzufügende Text</param>
        /// <param name="cursor">An dieser Stelle soll eingefügt werden</param>
        /// <param name="ersatzNode">Wenn statt des Textes ein Node eingefügt werden soll. Beispiel: Im
        /// AIML-Template wir * gedrückt, dann wird ein STAR-Tag eingefügt</param>
        internal static TextEinfuegeResult TextEinfuegen(XMLCursorPos cursorPos, string rohText, XMLRegelwerk regelwerk)
        {
            // Den eingegebenen Text im Preprocessing ggf. überarbeiten.
            // In einer AIML-DTD kann dies z.B. bedeuten, dass der
            // Text zum Einfügen in das PATTERN Tag auf Großbuchstaben umgestellt wird
            string text = regelwerk.EinfuegeTextPreProcessing(rohText, cursorPos, out System.Xml.XmlNode ersatzNode);

            if (ersatzNode != null)
            {
                // Wenn aus dem eingegebenen Text statt dessen ein Node geworden ist, z.B. bei
                // AIML, wenn man in einem Template * drückt und dann statt dessen ein <star>
                // eingefügt werden soll
                return new TextEinfuegeResult { ErsatzNode = ersatzNode };
            }
            else
            {
                switch (cursorPos.PosAmNode)
                {
                    case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                    case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                        // Zuerst checken, ob dieser Node durch durch einen Textnode ersetzt werden darf
                        if (regelwerk.IstDiesesTagAnDieserStelleErlaubt("#PCDATA", cursorPos))
                        {
                            // Den gewählten Node durch einen neu erzeugten Textnode 
                            System.Xml.XmlText neuerTextNode = cursorPos.AktNode.OwnerDocument.CreateTextNode(text);
                            cursorPos.AktNode.ParentNode.ReplaceChild(cursorPos.AktNode, neuerTextNode);
                            cursorPos.SetPos(neuerTextNode, XMLCursorPositionen.CursorHinterDemNode);
                        }
                        throw new ApplicationException(String.Format("TextEinfuegen: unbehandelte CursorPos {0}", cursorPos.PosAmNode));

                    case XMLCursorPositionen.CursorHinterDemNode:
                        TextZwischenZweiNodesEinfuegen(cursorPos, cursorPos.AktNode, cursorPos.AktNode.NextSibling, text, regelwerk);
                        break;

                    case XMLCursorPositionen.CursorVorDemNode:
                        TextZwischenZweiNodesEinfuegen(cursorPos, cursorPos.AktNode.PreviousSibling, cursorPos.AktNode, text, regelwerk);
                        break;

                    case XMLCursorPositionen.CursorInDemLeeremNode:
                        // Zuerst checken, ob innerhalb des leeren Nodes Text erlaubt ist
                        if (regelwerk.IstDiesesTagAnDieserStelleErlaubt("#PCDATA", cursorPos))
                        {
                            // Dann innerhalb des leeren Nodes einen Textnode mit dem gewünschten Textinhalt erzeugen
                            System.Xml.XmlText neuerTextNode = cursorPos.AktNode.OwnerDocument.CreateTextNode(text);
                            cursorPos.AktNode.AppendChild(neuerTextNode);
                            cursorPos.SetPos(neuerTextNode, XMLCursorPositionen.CursorHinterDemNode);
                        }
                        else
                        {
                            //BEEEEP!
                        }
                        break;

                    case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                        string textVorCursor = cursorPos.AktNode.InnerText.Substring(0, cursorPos.PosImTextnode);
                        string textNachCursor = cursorPos.AktNode.InnerText.Substring(cursorPos.PosImTextnode, cursorPos.AktNode.InnerText.Length - cursorPos.PosImTextnode);
                        // Das Zeichen der gedrückten Tasten nach dem Cursor einsetzen
                        cursorPos.AktNode.InnerText = textVorCursor + text + textNachCursor;
                        cursorPos.SetPos(cursorPos.AktNode, cursorPos.PosAmNode, cursorPos.PosImTextnode + text.Length);
                        break;

                    default:
                        throw new ApplicationException(String.Format("TextEinfuegen: Unbekannte CursorPos {0}", cursorPos.PosAmNode));
                }
            }
            return new TextEinfuegeResult { ErsatzNode = ersatzNode };
        }


        /// <summary>
        /// Fügt den angegebenen XML-Node an der angegebenen Stelle ein
        /// </summary>
        /// <param name="node">Dieses XML-Element soll eingefügt werden</param>
        /// <returns></returns>
        internal static bool InsertXMLNode(XMLCursorPos cursorPos, System.Xml.XmlNode node, XMLRegelwerk regelwerk, bool neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
        {
            System.Xml.XmlNode parentNode = cursorPos.AktNode.ParentNode;

            switch (cursorPos.PosAmNode)
            {
                case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag: // den aktuellen Node austauschen
                case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                    parentNode.ReplaceChild(node, cursorPos.AktNode);
                    break;

                case XMLCursorPositionen.CursorVorDemNode: // vor dem aktuellen Node einsetzen
                    parentNode.InsertBefore(node, cursorPos.AktNode);
                    break;

                case XMLCursorPositionen.CursorHinterDemNode: // hinter dem aktuellen Node einsetzen
                    parentNode.InsertAfter(node, cursorPos.AktNode);
                    break;

                case XMLCursorPositionen.CursorInDemLeeremNode: // im leeren  Node einsetzen
                    cursorPos.AktNode.AppendChild(node);
                    break;

                case XMLCursorPositionen.CursorInnerhalbDesTextNodes: // innerhalb eines Textnodes einsetzen

                    // Den Text vor der Einfügeposition als Node bereitstellen
                    string textDavor = cursorPos.AktNode.InnerText.Substring(0, cursorPos.PosImTextnode);
                    System.Xml.XmlNode textDavorNode = parentNode.OwnerDocument.CreateTextNode(textDavor);

                    // Den Text hinter der Einfügeposition als Node bereitstellen
                    string textDanach = cursorPos.AktNode.InnerText.Substring(cursorPos.PosImTextnode, cursorPos.AktNode.InnerText.Length - cursorPos.PosImTextnode);
                    System.Xml.XmlNode textDanachNode = parentNode.OwnerDocument.CreateTextNode(textDanach);

                    // Einzufügenden Node zwischen dem neuen vorher- und nachher-Textnode einsetzen
                    // -> also den alten Textnode ersetzen durch
                    // textdavor - neuerNode - textdanach
                    parentNode.ReplaceChild(textDavorNode, cursorPos.AktNode);
                    parentNode.InsertAfter(node, textDavorNode);
                    parentNode.InsertAfter(textDanachNode, node);

                    break;

                default:
                    throw new ApplicationException(String.Format("InsertElementAnCursorPos: Unbekannte PosAmNode {0}", cursorPos.PosAmNode));
            }

            // Cursor setzen
            if (neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
            {
                // Cursor hinter den neuen Node setzen
                cursorPos.SetPos(node, XMLCursorPositionen.CursorHinterDemNode);
            }
            else
            {
                if (regelwerk.IstSchliessendesTagSichtbar(node))
                {
                    // Cursor in den neuen Node setzen
                    cursorPos.SetPos(node, XMLCursorPositionen.CursorInDemLeeremNode);
                }
                else
                {
                    // Cursor hinter den neuen Node setzen
                    cursorPos.SetPos(node, XMLCursorPositionen.CursorHinterDemNode);
                }
            }
            return true;
        }


        /// <summary>
        /// Fügt einen Text zwischen zwei Nodes ein
        /// </summary>
        /// <param name="nodeVorher"></param>
        /// <param name="nodeNachher"></param>
        /// <param name="text"></param>
        internal static void TextZwischenZweiNodesEinfuegen(XMLCursorPos cursorPos, System.Xml.XmlNode nodeVorher, System.Xml.XmlNode nodeNachher, string text, XMLRegelwerk regelwerk)
        {
            if (ToolboxXML.IstTextOderKommentarNode(nodeVorher))  // wenn der Node vorher schon Text ist, dann einfach an ihn anhängen
            {
                nodeVorher.InnerText += text;
                cursorPos.SetPos(nodeVorher, XMLCursorPositionen.CursorInnerhalbDesTextNodes, nodeVorher.InnerText.Length);
            }
            else  // der Node vorher ist kein Text
            {
                if (ToolboxXML.IstTextOderKommentarNode(nodeNachher))  // wenn der Node dahinter schon Text istm dann einfach an in einfügen
                {
                    nodeNachher.InnerText = text + nodeNachher.InnerText;
                    cursorPos.SetPos(nodeNachher, XMLCursorPositionen.CursorInnerhalbDesTextNodes, text.Length);
                }
                else // der Node dahinter ist auch kein Text
                {
                    // Zwischen zwei Nicht-Text-Nodes einfügen
                    if (regelwerk.IstDiesesTagAnDieserStelleErlaubt("#PCDATA", cursorPos))
                    {
                        System.Xml.XmlText neuerTextNode = cursorPos.AktNode.OwnerDocument.CreateTextNode(text); // Text als Textnode
                        InsertXMLNode(cursorPos, neuerTextNode, regelwerk, false);
                    }
                    else
                    {
#warning Noch eine korrekte Meldung oder Ton einfügen
                        Debug.Assert(false, "Beep!");
                        //BEEEP
                    }

                }
            }
        }
    }
}
