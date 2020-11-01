// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.rules;
using System;
using System.Diagnostics;
using static de.springwald.xml.rules.XmlCursorPos;

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
        internal static TextEinfuegeResult InsertText(XmlCursorPos cursorPos, string rohText, XmlRules regelwerk)
        {
            // Den eingegebenen Text im Preprocessing ggf. überarbeiten.
            // In einer AIML-DTD kann dies z.B. bedeuten, dass der
            // Text zum Einfügen in das PATTERN Tag auf Großbuchstaben umgestellt wird
            string text = regelwerk.InsertTextTextPreProcessing(rohText, cursorPos, out System.Xml.XmlNode ersatzNode);

            if (ersatzNode != null)
            {
                // Wenn aus dem eingegebenen Text statt dessen ein Node geworden ist, z.B. bei
                // AIML, wenn man in einem Template * drückt und dann statt dessen ein <star>
                // eingefügt werden soll
                return new TextEinfuegeResult { ErsatzNode = ersatzNode };
            }
            else
            {
                switch (cursorPos.PosOnNode)
                {
                    case XmlCursorPositions.CursorOnNodeStartTag:
                    case XmlCursorPositions.CursorOnNodeEndTag:
                        // Zuerst checken, ob dieser Node durch durch einen Textnode ersetzt werden darf
                        if (regelwerk.IsThisTagAllowedAtThisPos("#PCDATA", cursorPos))
                        {
                            // Den gewählten Node durch einen neu erzeugten Textnode 
                            System.Xml.XmlText neuerTextNode = cursorPos.ActualNode.OwnerDocument.CreateTextNode(text);
                            cursorPos.ActualNode.ParentNode.ReplaceChild(cursorPos.ActualNode, neuerTextNode);
                            cursorPos.SetPos(neuerTextNode, XmlCursorPositions.CursorBehindTheNode);
                        }
                        throw new ApplicationException(String.Format("TextEinfuegen: unbehandelte CursorPos {0}", cursorPos.PosOnNode));

                    case XmlCursorPositions.CursorBehindTheNode:
                        TextZwischenZweiNodesEinfuegen(cursorPos, cursorPos.ActualNode, cursorPos.ActualNode.NextSibling, text, regelwerk);
                        break;

                    case XmlCursorPositions.CursorInFrontOfNode:
                        TextZwischenZweiNodesEinfuegen(cursorPos, cursorPos.ActualNode.PreviousSibling, cursorPos.ActualNode, text, regelwerk);
                        break;

                    case XmlCursorPositions.CursorInsideTheEmptyNode:
                        // Zuerst checken, ob innerhalb des leeren Nodes Text erlaubt ist
                        if (regelwerk.IsThisTagAllowedAtThisPos("#PCDATA", cursorPos))
                        {
                            // Dann innerhalb des leeren Nodes einen Textnode mit dem gewünschten Textinhalt erzeugen
                            System.Xml.XmlText neuerTextNode = cursorPos.ActualNode.OwnerDocument.CreateTextNode(text);
                            cursorPos.ActualNode.AppendChild(neuerTextNode);
                            cursorPos.SetPos(neuerTextNode, XmlCursorPositions.CursorBehindTheNode);
                        }
                        else
                        {
                            //BEEEEP!
                        }
                        break;

                    case XmlCursorPositions.CursorInsideTextNode:
                        string textVorCursor = cursorPos.ActualNode.InnerText.Substring(0, cursorPos.PosInTextNode);
                        string textNachCursor = cursorPos.ActualNode.InnerText.Substring(cursorPos.PosInTextNode, cursorPos.ActualNode.InnerText.Length - cursorPos.PosInTextNode);
                        // Das Zeichen der gedrückten Tasten nach dem Cursor einsetzen
                        cursorPos.ActualNode.InnerText = textVorCursor + text + textNachCursor;
                        cursorPos.SetPos(cursorPos.ActualNode, cursorPos.PosOnNode, cursorPos.PosInTextNode + text.Length);
                        break;

                    default:
                        throw new ApplicationException(String.Format("TextEinfuegen: Unbekannte CursorPos {0}", cursorPos.PosOnNode));
                }
            }
            return new TextEinfuegeResult { ErsatzNode = ersatzNode };
        }


        /// <summary>
        /// Fügt den angegebenen XML-Node an der angegebenen Stelle ein
        /// </summary>
        /// <param name="node">Dieses XML-Element soll eingefügt werden</param>
        /// <returns></returns>
        internal static bool InsertXMLNode(XmlCursorPos cursorPos, System.Xml.XmlNode node, XmlRules regelwerk, bool neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
        {
            System.Xml.XmlNode parentNode = cursorPos.ActualNode.ParentNode;

            switch (cursorPos.PosOnNode)
            {
                case XmlCursorPositions.CursorOnNodeStartTag: // den aktuellen Node austauschen
                case XmlCursorPositions.CursorOnNodeEndTag:
                    parentNode.ReplaceChild(node, cursorPos.ActualNode);
                    break;

                case XmlCursorPositions.CursorInFrontOfNode: // vor dem aktuellen Node einsetzen
                    parentNode.InsertBefore(node, cursorPos.ActualNode);
                    break;

                case XmlCursorPositions.CursorBehindTheNode: // hinter dem aktuellen Node einsetzen
                    parentNode.InsertAfter(node, cursorPos.ActualNode);
                    break;

                case XmlCursorPositions.CursorInsideTheEmptyNode: // im leeren  Node einsetzen
                    cursorPos.ActualNode.AppendChild(node);
                    break;

                case XmlCursorPositions.CursorInsideTextNode: // innerhalb eines Textnodes einsetzen

                    // Den Text vor der Einfügeposition als Node bereitstellen
                    string textDavor = cursorPos.ActualNode.InnerText.Substring(0, cursorPos.PosInTextNode);
                    System.Xml.XmlNode textDavorNode = parentNode.OwnerDocument.CreateTextNode(textDavor);

                    // Den Text hinter der Einfügeposition als Node bereitstellen
                    string textDanach = cursorPos.ActualNode.InnerText.Substring(cursorPos.PosInTextNode, cursorPos.ActualNode.InnerText.Length - cursorPos.PosInTextNode);
                    System.Xml.XmlNode textDanachNode = parentNode.OwnerDocument.CreateTextNode(textDanach);

                    // Einzufügenden Node zwischen dem neuen vorher- und nachher-Textnode einsetzen
                    // -> also den alten Textnode ersetzen durch
                    // textdavor - neuerNode - textdanach
                    parentNode.ReplaceChild(textDavorNode, cursorPos.ActualNode);
                    parentNode.InsertAfter(node, textDavorNode);
                    parentNode.InsertAfter(textDanachNode, node);

                    break;

                default:
                    throw new ApplicationException(String.Format("InsertElementAnCursorPos: Unbekannte PosAmNode {0}", cursorPos.PosOnNode));
            }

            // Cursor setzen
            if (neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
            {
                // Cursor hinter den neuen Node setzen
                cursorPos.SetPos(node, XmlCursorPositions.CursorBehindTheNode);
            }
            else
            {
                if (regelwerk.HasEndTag(node))
                {
                    // Cursor in den neuen Node setzen
                    cursorPos.SetPos(node, XmlCursorPositions.CursorInsideTheEmptyNode);
                }
                else
                {
                    // Cursor hinter den neuen Node setzen
                    cursorPos.SetPos(node, XmlCursorPositions.CursorBehindTheNode);
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
        internal static void TextZwischenZweiNodesEinfuegen(XmlCursorPos cursorPos, System.Xml.XmlNode nodeVorher, System.Xml.XmlNode nodeNachher, string text, XmlRules regelwerk)
        {
            if (ToolboxXML.IstTextOderKommentarNode(nodeVorher))  // wenn der Node vorher schon Text ist, dann einfach an ihn anhängen
            {
                nodeVorher.InnerText += text;
                cursorPos.SetPos(nodeVorher, XmlCursorPositions.CursorInsideTextNode, nodeVorher.InnerText.Length);
            }
            else  // der Node vorher ist kein Text
            {
                if (ToolboxXML.IstTextOderKommentarNode(nodeNachher))  // wenn der Node dahinter schon Text istm dann einfach an in einfügen
                {
                    nodeNachher.InnerText = text + nodeNachher.InnerText;
                    cursorPos.SetPos(nodeNachher, XmlCursorPositions.CursorInsideTextNode, text.Length);
                }
                else // der Node dahinter ist auch kein Text
                {
                    // Zwischen zwei Nicht-Text-Nodes einfügen
                    if (regelwerk.IsThisTagAllowedAtThisPos("#PCDATA", cursorPos))
                    {
                        System.Xml.XmlText neuerTextNode = cursorPos.ActualNode.OwnerDocument.CreateTextNode(text); // Text als Textnode
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
