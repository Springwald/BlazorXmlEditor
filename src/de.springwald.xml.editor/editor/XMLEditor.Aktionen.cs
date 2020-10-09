using de.springwald.toolbox;
using de.springwald.xml.cursor;
using System;
using System.Threading.Tasks;
using System.Xml;

namespace de.springwald.xml.editor
{
    public partial class XMLEditor
    {
        public enum UndoSnapshotSetzenOptionen { ja, nein };

        /// <summary>
        /// Sind überhaupt irgendwelche Aktionen möglich?
        /// </summary>
        private bool AktionenMoeglich
        {
            get
            {
                if (ReadOnly)
                {
                    // Datei ist schreibgeschützt
                    return false;
                }
                else
                {
                    return _cursor.StartPos.AktNode != null;
                }
            }
        }

        /// <summary>
        /// Gibt an, ob etwas für den Editor in der Zwischenablage ist
        /// </summary>
        public virtual bool IstEtwasInZwischenablage
        {
            get { return this.NativePlatform.Clipboard.ContainsText; }
        }

        /// <summary>
        /// Gibt an, ob im Editor etwas selektiert ist
        /// </summary>
        public virtual bool IstEtwasSelektiert
        {
            get { return CursorOptimiert.IstEtwasSelektiert; }
        }

        /// <summary>
        /// Fuegt den Zwischenablageinhalt an die aktuelle CursorPos ein
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionPasteFromClipboard(UndoSnapshotSetzenOptionen undoSnapshotSetzen)
        {

            if (!AktionenMoeglich) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            string text = "";

            try
            {
                if (this.NativePlatform.Clipboard.ContainsText) // wenn Text in der Zwischenablage ist
                {
                    XMLCursorPos startPos;
                    XMLCursorPos endPos;

                    if (IstRootNodeSelektiert) // Der Rootnode ist selektiert und soll daher durch den Clipboard-Inhalt ersetzt werden
                    {
                        return await AktionRootNodeDurchClipboardInhaltErsetzen(undoSnapshotSetzen);
                    }
                    else // etwas anderes als der Rootnode soll ersetzt werden
                    {
                        // Zuerst eine etwaige Selektion löschen
                        if (IstEtwasSelektiert) // Es ist etwas selektiert
                        {
                            if (await AktionDelete(UndoSnapshotSetzenOptionen.nein))
                            {
                                startPos = _cursor.StartPos;
                            }
                            else // Löschen der Selektion fehlgeschlagen
                            {
                                return false;
                            }
                        }
                        else // Nichts selektiert
                        {
                            startPos = CursorOptimiert.StartPos;
                        }
                    }

                    if (undoSnapshotSetzen == UndoSnapshotSetzenOptionen.ja)
                    {
                        _undoHandler.SnapshotSetzen(
                            ResReader.Reader.GetString("AktionEinfuegen"),
                            _cursor);
                    }

                    // Den Text mit einem umschließenden, virtuellen Tag umschließen
                    text = this.NativePlatform.Clipboard.GetText();

                    // Whitespaces entschärfen
                    text = text.Replace("\r\n", " ");
                    text = text.Replace("\n\r", " ");
                    text = text.Replace("\r", " ");
                    text = text.Replace("\n", " ");
                    text = text.Replace("\t", " ");

                    string inhalt = String.Format("<paste>{0}</paste>", text);

                    // den XML-Reader erzeugen
                    XmlTextReader reader = new XmlTextReader(inhalt, System.Xml.XmlNodeType.Element, null);
                    reader.MoveToContent(); //Move to the cd element node.

                    // Den virtuellen Paste-Node erstellen
                    System.Xml.XmlNode pasteNode = _rootNode.OwnerDocument.ReadNode(reader);

                    // Nun alle Children des virtuellen Paste-Node nacheinander an der CursorPos einfügen
                    endPos = startPos.Clone(); // Vor dem Einfügen sind start- und endPos gleich
                    foreach (System.Xml.XmlNode node in pasteNode.ChildNodes)
                    {
                        if (node is System.Xml.XmlText) // Einen Text einfügen
                        {
                            var einfuegeResult = await endPos.TextEinfuegen(node.Clone().Value, Regelwerk);
                            if (einfuegeResult.ErsatzNode != null)
                            {
                                // Text konnte nicht eingefügt werden, da aus der Texteingabe eine Node-Eingabe umgewandelt
                                // wurde. Beispiel: Im AIML-Template wird * gedrückt, und dort statt dessen ein <star> eingefügt
                                await endPos.InsertXMLNode(einfuegeResult.ErsatzNode.Clone(), Regelwerk, true);
                            }
                        }
                        else // Einen Node einfügen
                        {
                            await endPos.InsertXMLNode(node.Clone(), Regelwerk, true);
                        }
                    }

                    switch (_cursor.EndPos.PosAmNode)
                    {
                        case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                        case XMLCursorPositionen.CursorVorDemNode:
                            // Ende des Einfügens liegt einem Text oder vor dem Node
                            await _cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(endPos.AktNode, endPos.PosAmNode, endPos.PosImTextnode);
                            break;
                        default:
                            // Ende des Einfügens liegt hinter dem letzten eingefügten Node
                            await _cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(endPos.AktNode, XMLCursorPositionen.CursorHinterDemNode);
                            break;
                    }
                    return true;
                }
                else // Kein Text in der Zwischenablage
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                this.NativePlatform.ProtokolliereFehler(
                    String.Format("AktionPasteFromClipboard:Fehler für Einfügetext '{0}':{1}", text, e.Message));

#warning Hier noch beep

                return false;
            }
        }

        /// <summary>
        /// Bei der Enter-Taste kann z.B. versucht werden, das gleiche Tag nochmal hinter das aktuelle zu setzen,
        /// oder das aktuelle an der jetzigen Stelle in zwei gleiche Tags zu splitten
        /// </summary>
        private void AktionenEnterGedrueckt()
        {
#warning To Do!
        }

        /// <summary>
        /// Ersetzt den Root-Node des Editors durch den Inhalt der Zwischenablage
        /// </summary>
        private async Task<bool> AktionRootNodeDurchClipboardInhaltErsetzen(UndoSnapshotSetzenOptionen undoSnapshotSetzen)
        {
            if (!AktionenMoeglich) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            string text = "";

            try
            {

                // den XML-Reader erzeugen
                text = this.NativePlatform.Clipboard.GetText();
                XmlTextReader reader = new XmlTextReader(text, System.Xml.XmlNodeType.Element, null);
                reader.MoveToContent(); //Move to the cd element node.

                // Aus das Zwischenablage einen neuen Rootnode erstellen, dem wir dann die Kinder klauen können
                System.Xml.XmlNode pasteNode = _rootNode.OwnerDocument.ReadNode(reader);

                if (pasteNode.Name != _rootNode.Name)
                { // Der Node in der Zwischenablage und der aktuelle Rootnode haben nicht den selben Namen
                    return false; // Nicht erlaubt
                }

                if (undoSnapshotSetzen == UndoSnapshotSetzenOptionen.ja)
                {
                    _undoHandler.SnapshotSetzen(
                        ResReader.Reader.GetString("RootNodedurchZwischenablageersetzen"),
                        _cursor);
                }

                // Alle Children + Attribute des bisherigen Rootnodes löschen
                _rootNode.RemoveAll();

                // Alle Attribute des Clipboard-Root-Nodes in den richtigen Root-Node übernehmen
                while (pasteNode.Attributes.Count > 0)
                {
                    XmlAttribute attrib = pasteNode.Attributes.Remove(pasteNode.Attributes[0]); // von Clipboard-Rootnode entfernen
                    _rootNode.Attributes.Append(attrib); // an richtigen Rootnode packen
                }

                XMLCursorPos startPos = new XMLCursorPos();
                startPos.CursorSetzenOhneChangeEvent(_rootNode, XMLCursorPositionen.CursorInDemLeeremNode);
                XMLCursorPos endPos;

                // Nun alle Children des virtuellen Root-Node nacheinander an der CursorPos einfügen
                endPos = startPos.Clone(); // Vor dem Einfügen sind start- und endPos gleich
                while (pasteNode.ChildNodes.Count > 0)
                {
                    XmlNode child = pasteNode.RemoveChild(pasteNode.FirstChild);
                    _rootNode.AppendChild(child);
                }

                await ContentChanged();

                _cursor.BeideCursorPosSetzenOhneChangeEvent(_rootNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
                await _cursor.ErzwingeChanged();

                return true;
            }
            catch (Exception e)
            {
                this.NativePlatform.ProtokolliereFehler($"AktionRootNodeDurchClipboardInhaltErsetzen:Fehler für Einfügetext 'text': {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kopiert die aktuelle Selektion in die Zwischenablage
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionCopyToClipboard()
        {
            if (!AktionenMoeglich) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            string inhalt = await _cursor.GetSelektionAlsString();
            if (string.IsNullOrEmpty(inhalt)) // Nix selektiert
            {
                return false;
            }
            else // es ist etwas selektiert
            {
                try
                {
                    this.NativePlatform.Clipboard.Clear();
                    this.NativePlatform.Clipboard.SetText(inhalt); // Selektion als Text in die Zwischenablage kopieren
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Setzt den Cursor auf die Position1
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionCursorAufPos1()
        {
            if (_rootNode == null)
            {
#warning hier noch beep
                return false;
            }
            else
            {
                if (_rootNode.FirstChild != null)
                {
                    // Vor das erste Child des Rootnodes
                    await _cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(_rootNode.FirstChild, XMLCursorPositionen.CursorVorDemNode);
                }
                else
                {
                    // In den leeren Rootnode
                    await _cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(_rootNode, XMLCursorPositionen.CursorInDemLeeremNode);
                }
                return true;
            }
        }

        /// <summary>
        /// Markiert den gesamten Inhalt
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionAllesMarkieren()
        {
            // Den Rootnode selbst markieren
            await _cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(_rootNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
            return true;
        }

        /// <summary>
        /// Schneidet die aktuelle Selektion aus und schiebt sie in die Zwischenablage
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionCutToClipboard(UndoSnapshotSetzenOptionen undoSnapshotSetzen)
        {

            if (!AktionenMoeglich) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (CursorOptimiert.StartPos.AktNode == _rootNode)
            {
                // Der Root-Node kann nicht ausgeschnitten werden
                return false;
            }
            else // ok, der Rootnode ist nicht selektiert
            {
                if (await AktionCopyToClipboard()) // Kopieren in Zwischenablage hat geklappt
                {
                    if (await AktionDelete(UndoSnapshotSetzenOptionen.ja)) // Löschen der Selektion hat geklappt
                    {
                        return true;
                    }
                    else // Löschen der Selektion fehlgeschlagen
                    {
                        return false;
                    }
                }
                else // Kopieren in Zwischenablage fehlgeschlagen
                {
                    return false;
                }
            }
        }



        /// <summary>
        /// Löscht die aktuelle Selektion des Cursors
        /// </summary>
        /// <returns>true, wenn erfolgreich gelöscht</returns>
        /// <param name="rootNodeLoeschenZulassig">Falls der Rootnode selektiert ist, muss hier TRUE angegeben werden, damit dessen Löschung zulässig ist</param>
        /// <param name="undoSnapshotSetzen"></param>
        /// <returns></returns>
        public virtual async Task<bool> AktionDelete(UndoSnapshotSetzenOptionen undoSnapshotSetzen)
        {
            if (!AktionenMoeglich) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (IstRootNodeSelektiert) return false; // Der Root-Node soll gelöscht werden:  Nicht erlaubt

            if (undoSnapshotSetzen == UndoSnapshotSetzenOptionen.ja)
            {
                _undoHandler.SnapshotSetzen(
                    ResReader.Reader.GetString("AktionLoeschen"),
                    _cursor);
            }

            XMLCursor optimized = _cursor;
            await optimized.SelektionOptimieren();

            var loeschResult = await optimized.SelektionLoeschen();
            if (loeschResult.Success)
            {
                await _cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(loeschResult.NeueCursorPosNachLoeschen.AktNode, loeschResult.NeueCursorPosNachLoeschen.PosAmNode, loeschResult.NeueCursorPosNachLoeschen.PosImTextnode);
                await ContentChanged();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Fügt an der angegebenen Cursor-Pos Text ein
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionTextAnCursorPosEinfuegen(string einfuegeText, UndoSnapshotSetzenOptionen undoSnapshotSetzen)
        {

            if (!AktionenMoeglich) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (undoSnapshotSetzen == UndoSnapshotSetzenOptionen.ja)
            {
                _undoHandler.SnapshotSetzen(
                    String.Format(
                        ResReader.Reader.GetString("AktionSchreiben"),
                        einfuegeText),
                    _cursor);
            }

            await _cursor.TextEinfuegen(einfuegeText, Regelwerk);
            await ContentChanged(); // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
            return true;
        }

        /// <summary>
        /// Setzt den Inhalt eines Attributes in einem Node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="wert"></param>
        /// <returns></returns>
        public virtual async Task<bool> AktionAttributWertInNodeSetzen(System.Xml.XmlNode node, string attributName, string wert, UndoSnapshotSetzenOptionen undoSnapshotSetzen)
        {

            if (!AktionenMoeglich) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            System.Xml.XmlAttribute xmlAttrib = node.Attributes[attributName];

            if (wert == "") // Kein Inhalt, Attribut löschen, wenn vorhanden
            {
                if (xmlAttrib != null)  // Attribut gibts -> löschen
                {
                    if (undoSnapshotSetzen == UndoSnapshotSetzenOptionen.ja)
                    {
                        _undoHandler.SnapshotSetzen(
                            string.Format(
                                ResReader.Reader.GetString("AktionAttributGeloescht"),
                                attributName,
                                node.Name),
                            _cursor);
                    }
                    node.Attributes.Remove(xmlAttrib);
                }
            }
            else // Inhalt in Attribut schreiben
            {
                if (xmlAttrib == null)  // Attribut gibts noch nicht -> neu anlegen
                {
                    if (undoSnapshotSetzen == UndoSnapshotSetzenOptionen.ja)
                    {
                        _undoHandler.SnapshotSetzen(
                            String.Format(
                                ResReader.Reader.GetString("AktionAttributValueGeaendert"),
                                attributName,
                                node.Name,
                                wert),
                            _cursor);
                    }
                    xmlAttrib = node.OwnerDocument.CreateAttribute(attributName);
                    node.Attributes.Append(xmlAttrib);
                    xmlAttrib.Value = wert; // Inhalt in Attribut schreiben
                }
                else
                {
                    if (xmlAttrib.Value != wert)
                    {
                        if (undoSnapshotSetzen == UndoSnapshotSetzenOptionen.ja)
                        {
                            _undoHandler.SnapshotSetzen(
                                 String.Format(
                                     ResReader.Reader.GetString("AktionAttributValueGeaendert"),
                                     attributName,
                                     node.Name,
                                     wert),
                                 _cursor);
                        }
                        xmlAttrib.Value = wert; // Inhalt in Attribut schreiben
                    }
                }
            }
            await ContentChanged(); // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
            return true;
        }

        /// <summary>
        /// Den Node oder das Zeichen vor dem Cursor löschen
        /// </summary>
        /// <param name="position"></param>
        public async Task<bool> AktionNodeOderZeichenVorDerCursorPosLoeschen(XMLCursorPos position, UndoSnapshotSetzenOptionen undoSnapshotSetzen)
        {

            if (!AktionenMoeglich) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            // Den Cursor eine Pos nach links
            XMLCursor loeschbereich = new XMLCursor();
            loeschbereich.StartPos.CursorSetzenOhneChangeEvent(position.AktNode, position.PosAmNode, position.PosImTextnode);
            XMLCursorPos endPos = loeschbereich.StartPos.Clone();
            await endPos.MoveLeft(_rootNode, Regelwerk);
            loeschbereich.EndPos.CursorSetzenOhneChangeEvent(endPos.AktNode, endPos.PosAmNode, endPos.PosImTextnode);
            await loeschbereich.SelektionOptimieren();

            if (loeschbereich.StartPos.AktNode == _rootNode) return false; // Den Rootnot darf man nicht löschen

            if (undoSnapshotSetzen == UndoSnapshotSetzenOptionen.ja)
            {
                _undoHandler.SnapshotSetzen(
                    ResReader.Reader.GetString("AktionLoeschen"),
                    _cursor);
            }

            var loeschResult = await loeschbereich.SelektionLoeschen();
            if (loeschResult.Success)
            {
                // Nach erfolgreichem Löschen wird hier die neue CursorPos zurückgeholt
                await _cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(loeschResult.NeueCursorPosNachLoeschen.AktNode, loeschResult.NeueCursorPosNachLoeschen.PosAmNode, loeschResult.NeueCursorPosNachLoeschen.PosImTextnode);
                await ContentChanged(); // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Den Node oder das Zeichen hinter dem Cursor löschen
        /// </summary>
        /// <param name="position"></param>
        public async Task<bool> AktionNodeOderZeichenHinterCursorPosLoeschen(XMLCursorPos position, UndoSnapshotSetzenOptionen undoSnapshotSetzen)
        {
            if (!AktionenMoeglich) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (undoSnapshotSetzen == UndoSnapshotSetzenOptionen.ja)
            {
                _undoHandler.SnapshotSetzen(
                     ResReader.Reader.GetString("AktionLoeschen"),
                     _cursor);
            }

            XMLCursor loeschbereich = new XMLCursor();
            loeschbereich.StartPos.CursorSetzenOhneChangeEvent(position.AktNode, position.PosAmNode, position.PosImTextnode);
            XMLCursorPos endPos = loeschbereich.StartPos.Clone();
            await endPos.MoveRight(_rootNode, Regelwerk);
            loeschbereich.EndPos.CursorSetzenOhneChangeEvent(endPos.AktNode, endPos.PosAmNode, endPos.PosImTextnode);
            await loeschbereich.SelektionOptimieren();

            if (loeschbereich.StartPos.AktNode == _rootNode) return false; // Den Rootnot darf man nicht löschen

            var loeschResult = await loeschbereich.SelektionLoeschen();
            if (loeschResult.Success)
            {
                // Nach erfolgreichem Löschen wird hier die neue CursorPos zurückgeholt
                _cursor.BeideCursorPosSetzenOhneChangeEvent(loeschResult.NeueCursorPosNachLoeschen.AktNode, loeschResult.NeueCursorPosNachLoeschen.PosAmNode, loeschResult.NeueCursorPosNachLoeschen.PosImTextnode);
                await _cursor.ErzwingeChanged();	// Weil der Cursor nach dem löschen des folgenden Zeichen nachher exakt noch an der
                // selben Stelle steht, wird kein automatischer Change-Event ausgeführt, sondern muss
                // hier manuell erzwungen werden
                await ContentChanged(); // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Fügt ein neues XML-Element an der aktuell CursorPosition ein
        /// </summary>
        /// <param name="nodeName">Solch ein Node soll erzeugt werden</param>
        /// <returns>Der neu erzeugte Node</returns>
        public virtual async Task<System.Xml.XmlNode> AktionNeuesElementAnAktCursorPosEinfuegen(string nodeName, UndoSnapshotSetzenOptionen undoSnapshotSetzen, bool neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
        {
            if (!AktionenMoeglich) return null; // Wenn gar keine Aktionen zulässig sind, abbrechen

            System.Xml.XmlNode node;

            if (nodeName == "")
            {
                // "Es wurde kein Nodename angegeben (xml.InsertNewElementAnCursorPos"
                throw new ApplicationException(ResReader.Reader.GetString("KeinNodeNameAngegeben"));
            }

            if (undoSnapshotSetzen == UndoSnapshotSetzenOptionen.ja)
            {
                _undoHandler.SnapshotSetzen(String.Format(
                    ResReader.Reader.GetString("AktionInsertNode"),
                    nodeName),
                 _cursor);
            }

            // Node erzeugen
            if (nodeName == "#COMMENT")
            {
                node = _rootNode.OwnerDocument.CreateComment("NEW COMMENT");
            }
            else
            {
                node = _rootNode.OwnerDocument.CreateNode(System.Xml.XmlNodeType.Element, nodeName, null);
            }

            // Node an aktueller CursorPos einfügen
           await  _cursor.XMLNodeEinfuegen(node, Regelwerk, neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen);
           await ContentChanged(); // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
            return node;
        }
    }
}
