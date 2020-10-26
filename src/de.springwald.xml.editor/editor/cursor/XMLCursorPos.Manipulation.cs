using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace de.springwald.xml.cursor
{
    public partial class XMLCursorPos
    {
        public struct TextEinfuegeResult
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
        internal async Task<TextEinfuegeResult> TextEinfuegen(string rohText, XMLRegelwerk regelwerk)
        {
            // Den eingegebenen Text im Preprocessing ggf. überarbeiten.
            // In einer AIML-DTD kann dies z.B. bedeuten, dass der
            // Text zum Einfügen in das PATTERN Tag auf Großbuchstaben umgestellt wird
            string text = regelwerk.EinfuegeTextPreProcessing(rohText, this, out System.Xml.XmlNode ersatzNode);

            if (ersatzNode != null)
            {
                // Wenn aus dem eingegebenen Text statt dessen ein Node geworden ist, z.B. bei
                // AIML, wenn man in einem Template * drückt und dann statt dessen ein <star>
                // eingefügt werden soll
                return new TextEinfuegeResult { ErsatzNode = ersatzNode };
            }
            else
            {
                switch (PosAmNode)
                {
                    case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                    case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                        // Zuerst checken, ob dieser Node durch durch einen Textnode ersetzt werden darf
                        if (regelwerk.IstDiesesTagAnDieserStelleErlaubt("#PCDATA", this))
                        {
                            // Den gewählten Node durch einen neu erzeugten Textnode 
                            System.Xml.XmlText neuerTextNode = AktNode.OwnerDocument.CreateTextNode(text);
                            AktNode.ParentNode.ReplaceChild(AktNode, neuerTextNode);
                            SetPos(neuerTextNode, XMLCursorPositionen.CursorHinterDemNode);
                        }
                        throw new ApplicationException(String.Format("TextEinfuegen: unbehandelte CursorPos {0}", PosAmNode));

                    case XMLCursorPositionen.CursorHinterDemNode:
                        await TextZwischenZweiNodesEinfuegen(AktNode, AktNode.NextSibling, text, regelwerk);
                        break;

                    case XMLCursorPositionen.CursorVorDemNode:
                        await TextZwischenZweiNodesEinfuegen(AktNode.PreviousSibling, AktNode, text, regelwerk);
                        break;

                    case XMLCursorPositionen.CursorInDemLeeremNode:
                        // Zuerst checken, ob innerhalb des leeren Nodes Text erlaubt ist
                        if (regelwerk.IstDiesesTagAnDieserStelleErlaubt("#PCDATA", this))
                        {
                            // Dann innerhalb des leeren Nodes einen Textnode mit dem gewünschten Textinhalt erzeugen
                            System.Xml.XmlText neuerTextNode = AktNode.OwnerDocument.CreateTextNode(text);
                            AktNode.AppendChild(neuerTextNode);
                             SetPos(neuerTextNode, XMLCursorPositionen.CursorHinterDemNode);
                        }
                        else
                        {
                            //BEEEEP!
                        }
                        break;

                    case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                        string textVorCursor = AktNode.InnerText.Substring(0, PosImTextnode);
                        string textNachCursor = AktNode.InnerText.Substring(PosImTextnode, AktNode.InnerText.Length - PosImTextnode);
                        // Das Zeichen der gedrückten Tasten nach dem Cursor einsetzen
                        AktNode.InnerText = textVorCursor + text + textNachCursor;
                        SetPos(AktNode, PosAmNode, PosImTextnode + text.Length);
                        break;

                    default:
                        throw new ApplicationException(String.Format("TextEinfuegen: Unbekannte CursorPos {0}", PosAmNode));
                }
            }
            return new TextEinfuegeResult { ErsatzNode = ersatzNode };
        }


        /// <summary>
        /// Fügt den angegebenen XML-Node an der angegebenen Stelle ein
        /// </summary>
        /// <param name="node">Dieses XML-Element soll eingefügt werden</param>
        /// <returns></returns>
        public async Task<bool> InsertXMLNode(System.Xml.XmlNode node, XMLRegelwerk regelwerk, bool neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
        {
            System.Xml.XmlNode parentNode = AktNode.ParentNode;

            switch (PosAmNode)
            {
                case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag: // den aktuellen Node austauschen
                case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                    parentNode.ReplaceChild(node, AktNode);
                    break;

                case XMLCursorPositionen.CursorVorDemNode: // vor dem aktuellen Node einsetzen
                    parentNode.InsertBefore(node, AktNode);
                    break;

                case XMLCursorPositionen.CursorHinterDemNode: // hinter dem aktuellen Node einsetzen
                    parentNode.InsertAfter(node, AktNode);
                    break;

                case XMLCursorPositionen.CursorInDemLeeremNode: // im leeren  Node einsetzen
                    AktNode.AppendChild(node);
                    break;

                case XMLCursorPositionen.CursorInnerhalbDesTextNodes: // innerhalb eines Textnodes einsetzen

                    // Den Text vor der Einfügeposition als Node bereitstellen
                    string textDavor = AktNode.InnerText.Substring(0, PosImTextnode);
                    System.Xml.XmlNode textDavorNode = parentNode.OwnerDocument.CreateTextNode(textDavor);

                    // Den Text hinter der Einfügeposition als Node bereitstellen
                    string textDanach = AktNode.InnerText.Substring(PosImTextnode, AktNode.InnerText.Length - PosImTextnode);
                    System.Xml.XmlNode textDanachNode = parentNode.OwnerDocument.CreateTextNode(textDanach);

                    // Einzufügenden Node zwischen dem neuen vorher- und nachher-Textnode einsetzen
                    // -> also den alten Textnode ersetzen durch
                    // textdavor - neuerNode - textdanach
                    parentNode.ReplaceChild(textDavorNode, AktNode);
                    parentNode.InsertAfter(node, textDavorNode);
                    parentNode.InsertAfter(textDanachNode, node);

                    break;

                default:
                    throw new ApplicationException(String.Format("InsertElementAnCursorPos: Unbekannte PosAmNode {0}", PosAmNode));
            }

            // Cursor setzen
            if (neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
            {
                // Cursor hinter den neuen Node setzen
                SetPos(node, XMLCursorPositionen.CursorHinterDemNode);
            }
            else
            {
                if (regelwerk.IstSchliessendesTagSichtbar(node))
                {
                    // Cursor in den neuen Node setzen
                    SetPos(node, XMLCursorPositionen.CursorInDemLeeremNode);
                }
                else
                {
                    // Cursor hinter den neuen Node setzen
                    SetPos(node, XMLCursorPositionen.CursorHinterDemNode);
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
        private async Task TextZwischenZweiNodesEinfuegen(System.Xml.XmlNode nodeVorher, System.Xml.XmlNode nodeNachher, string text, XMLRegelwerk regelwerk)
        {
            if (ToolboxXML.IstTextOderKommentarNode(nodeVorher))  // wenn der Node vorher schon Text ist, dann einfach an ihn anhängen
            {
                nodeVorher.InnerText += text;
                SetPos(nodeVorher, XMLCursorPositionen.CursorInnerhalbDesTextNodes, nodeVorher.InnerText.Length);
            }
            else  // der Node vorher ist kein Text
            {
                if (ToolboxXML.IstTextOderKommentarNode(nodeNachher))  // wenn der Node dahinter schon Text istm dann einfach an in einfügen
                {
                    nodeNachher.InnerText = text + nodeNachher.InnerText;
                    SetPos(nodeNachher, XMLCursorPositionen.CursorInnerhalbDesTextNodes, text.Length);
                }
                else // der Node dahinter ist auch kein Text
                {
                    // Zwischen zwei Nicht-Text-Nodes einfügen
                    if (regelwerk.IstDiesesTagAnDieserStelleErlaubt("#PCDATA", this))
                    {
                        System.Xml.XmlText neuerTextNode = AktNode.OwnerDocument.CreateTextNode(text); // Text als Textnode
                        await InsertXMLNode(neuerTextNode, regelwerk, false);
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
