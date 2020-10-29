// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.cursor;
using de.springwald.xml.editor.cursor;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.rules;
using System;
using System.Threading.Tasks;
using System.Xml;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor.actions
{
    public class EditorActions
    {
        private XmlRules xmlRules => this.editorContext.XmlRules;
        private INativePlatform nativePlatform => this.editorContext.NativePlatform;
        private EditorStatus editorState => this.editorContext.EditorState;

        private EditorContext editorContext;

        public enum SetUndoSnapshotOptions { Yes, nein };

        /// <summary>
        /// Sind überhaupt irgendwelche Aktionen möglich?
        /// </summary>
        private bool ActionsAllowed
        {
            get
            {
                if (this.editorState.ReadOnly)
                {
                    return false; // document is read only
                }
                else
                {
                    return this.editorState.CursorRaw.StartPos.ActualNode != null;
                }
            }
        }

        public EditorActions(EditorContext editorContext)
        {
            this.editorContext = editorContext;
        }


        public async Task<bool> MoveRight(XmlCursorPos cursorPos)
        {
            return await CursorPosMoveHelper.MoveRight(cursorPos, this.editorContext.EditorState.RootNode, this.editorContext.XmlRules);
        }

        public async Task<bool> MoveLeft(XmlCursorPos cursorPos)
        {
            return await CursorPosMoveHelper.MoveLeft(cursorPos, this.editorContext.EditorState.RootNode, this.editorContext.XmlRules);
        }

        /// <summary>
        /// Fuegt den Zwischenablageinhalt an die aktuelle CursorPos ein
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionPasteFromClipboard(SetUndoSnapshotOptions setUnDoSnapshot)
        {

            if (!ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            string text = "";

            try
            {
                if (await this.nativePlatform.Clipboard.ContainsText()) // wenn Text in der Zwischenablage ist
                {
                    XmlCursorPos startPos;
                    XmlCursorPos endPos;

                    if (this.editorState.IstRootNodeSelektiert) // Der Rootnode ist selektiert und soll daher durch den Clipboard-Inhalt ersetzt werden
                    {
                        return await AktionRootNodeDurchClipboardInhaltErsetzen(setUnDoSnapshot);
                    }
                    else // etwas anderes als der Rootnode soll ersetzt werden
                    {
                        // Zuerst eine etwaige Selektion löschen
                        if (this.editorState.IsSomethingSelected) // Es ist etwas selektiert
                        {
                            if (await AktionDelete(SetUndoSnapshotOptions.nein))
                            {
                                startPos = this.editorState.CursorRaw.StartPos;
                            }
                            else // Löschen der Selektion fehlgeschlagen
                            {
                                return false;
                            }
                        }
                        else // Nichts selektiert
                        {
                            startPos = this.editorState.CursorOptimiert.StartPos;
                        }
                    }

                    if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
                    {
                        this.editorState.UndoHandler.SetSnapshot(
                            ResReader.Reader.GetString("AktionEinfuegen"),
                            this.editorState.CursorRaw);
                    }

                    // Den Text mit einem umschließenden, virtuellen Tag umschließen
                    text = await this.nativePlatform.Clipboard.GetText();

                    // Whitespaces entschärfen
                    text = text.Replace("\r\n", " ");
                    text = text.Replace("\n\r", " ");
                    text = text.Replace("\r", " ");
                    text = text.Replace("\n", " ");
                    text = text.Replace("\t", " ");

                    string content = String.Format("<paste>{0}</paste>", text);

                    // den XML-Reader erzeugen
                    var reader = new XmlTextReader(content, XmlNodeType.Element, null);
                    reader.MoveToContent(); //Move to the cd element node.

                    // Den virtuellen Paste-Node erstellen
                    var pasteNode = this.editorState.RootNode.OwnerDocument.ReadNode(reader);

                    // Nun alle Children des virtuellen Paste-Node nacheinander an der CursorPos einfügen
                    endPos = startPos.Clone(); // Vor dem Einfügen sind start- und endPos gleich
                    foreach (XmlNode node in pasteNode.ChildNodes)
                    {
                        if (node is XmlText) // Einen Text einfügen
                        {
                            var pasteResult = InsertAtCursorPosHelper.InsertText(endPos, node.Clone().Value, this.xmlRules);
                            if (pasteResult.ErsatzNode != null)
                            {
                                // Text konnte nicht eingefügt werden, da aus der Texteingabe eine Node-Eingabe umgewandelt
                                // wurde. Beispiel: Im AIML-Template wird * gedrückt, und dort statt dessen ein <star> eingefügt
                                InsertAtCursorPosHelper.InsertXMLNode(endPos, pasteResult.ErsatzNode.Clone(), this.xmlRules, true);
                            }
                        }
                        else // Einen Node einfügen
                        {
                            InsertAtCursorPosHelper.InsertXMLNode(endPos, node.Clone(), this.xmlRules, true);
                        }
                    }

                    switch (this.editorState.CursorRaw.EndPos.PosOnNode)
                    {
                        case XmlCursorPositions.CursorInsideTextNode:
                        case XmlCursorPositions.CursorInFrontOfNode:
                            // Ende des Einfügens liegt einem Text oder vor dem Node
                            await this.editorState.CursorRaw.SetPositions(endPos.ActualNode, endPos.PosOnNode, endPos.PosInTextNode, throwChangedEventWhenValuesChanged: false);
                            break;
                        default:
                            // Ende des Einfügens liegt hinter dem letzten eingefügten Node
                            await this.editorState.CursorRaw.SetPositions(endPos.ActualNode, XmlCursorPositions.CursorBehindTheNode, textPosInBothNodes: 0, throwChangedEventWhenValuesChanged: false);
                            break;
                    }
                    await this.editorState.FireContentChangedEvent();
                    return true;
                }
                else // Kein Text in der Zwischenablage
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                this.nativePlatform.LogError(
                    String.Format("AktionPasteFromClipboard:Fehler für Einfügetext '{0}':{1}", text, e.Message));
                return false;
            }
        }

        /// <summary>
        /// Bei der Enter-Taste kann z.B. versucht werden, das gleiche Tag nochmal hinter das aktuelle zu setzen,
        /// oder das aktuelle an der jetzigen Stelle in zwei gleiche Tags zu splitten
        /// </summary>
        internal void AktionenEnterGedrueckt()
        {
#warning To Do!
        }

        internal async Task Undo()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Ersetzt den Root-Node des Editors durch den Inhalt der Zwischenablage
        /// </summary>
        private async Task<bool> AktionRootNodeDurchClipboardInhaltErsetzen(SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            string text = "";

            try
            {

                // den XML-Reader erzeugen
                text = await this.nativePlatform.Clipboard.GetText();
                var reader = new XmlTextReader(text, System.Xml.XmlNodeType.Element, null);
                reader.MoveToContent(); //Move to the cd element node.

                // Aus das Zwischenablage einen neuen Rootnode erstellen, dem wir dann die Kinder klauen können
                var pasteNode = this.editorState.RootNode.OwnerDocument.ReadNode(reader);

                if (pasteNode.Name != this.editorState.RootNode.Name)
                { // Der Node in der Zwischenablage und der aktuelle Rootnode haben nicht den selben Namen
                    return false; // Nicht erlaubt
                }

                if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
                {
                    this.editorState.UndoHandler.SetSnapshot(
                        ResReader.Reader.GetString("RootNodedurchZwischenablageersetzen"),
                        this.editorState.CursorRaw);
                }

                // Alle Children + Attribute des bisherigen Rootnodes löschen
                this.editorState.RootNode.RemoveAll();

                // Alle Attribute des Clipboard-Root-Nodes in den richtigen Root-Node übernehmen
                while (pasteNode.Attributes.Count > 0)
                {
                    var attrib = pasteNode.Attributes.Remove(pasteNode.Attributes[0]); // von Clipboard-Rootnode entfernen
                    this.editorState.RootNode.Attributes.Append(attrib); // an richtigen Rootnode packen
                }

                var startPos = new XmlCursorPos();
                startPos.SetPos(this.editorState.RootNode, XmlCursorPositions.CursorInsideTheEmptyNode);
                XmlCursorPos endPos;

                // Nun alle Children des virtuellen Root-Node nacheinander an der CursorPos einfügen
                endPos = startPos.Clone(); // Vor dem Einfügen sind start- und endPos gleich
                while (pasteNode.ChildNodes.Count > 0)
                {
                    var child = pasteNode.RemoveChild(pasteNode.FirstChild);
                    this.editorState.RootNode.AppendChild(child);
                }
                await this.editorState.CursorRaw.SetPositions(this.editorState.RootNode, XmlCursorPositions.CursorOnNodeStartTag, 0, throwChangedEventWhenValuesChanged: false);
                await this.editorState.FireContentChangedEvent();
                return true;
            }
            catch (Exception e)
            {
                this.nativePlatform.LogError($"AktionRootNodeDurchClipboardInhaltErsetzen:Fehler für Einfügetext 'text': {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kopiert die aktuelle Selektion in die Zwischenablage
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionCopyToClipboard()
        {
            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            var content = await XmlCursorSelectionHelper.GetSelektionAlsString(this.editorState.CursorRaw);
            if (string.IsNullOrEmpty(content)) // Nix selektiert
            {
                return false;
            }
            else // es ist etwas selektiert
            {
                try
                {
                    await this.nativePlatform.Clipboard.Clear();
                    await this.nativePlatform.Clipboard.SetText(content); // Selektion als Text in die Zwischenablage kopieren
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
            if (this.editorState.RootNode == null)
            {
#warning hier noch beep
                return false;
            }
            else
            {
                if (this.editorState.RootNode.FirstChild != null)
                {
                    // Vor das erste Child des Rootnodes
                    await this.editorState.CursorRaw.SetPositions(this.editorState.RootNode.FirstChild, XmlCursorPositions.CursorInFrontOfNode, 0, throwChangedEventWhenValuesChanged: true);
                }
                else
                {
                    // In den leeren Rootnode
                    await this.editorState.CursorRaw.SetPositions(this.editorState.RootNode, XmlCursorPositions.CursorInsideTheEmptyNode, 0, throwChangedEventWhenValuesChanged: true);
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
            await this.editorState.CursorRaw.SetPositions(this.editorState.RootNode, XmlCursorPositions.CursorOnNodeStartTag, 0, throwChangedEventWhenValuesChanged: true);
            return true;
        }

        /// <summary>
        /// Schneidet die aktuelle Selektion aus und schiebt sie in die Zwischenablage
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionCutToClipboard(SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (this.editorState.CursorOptimiert.StartPos.ActualNode == this.editorState.RootNode)
            {
                // Der Root-Node kann nicht ausgeschnitten werden
                return false;
            }
            else // ok, der Rootnode ist nicht selektiert
            {
                if (await AktionCopyToClipboard()) // Kopieren in Zwischenablage hat geklappt
                {
                    if (await AktionDelete(SetUndoSnapshotOptions.Yes)) // Löschen der Selektion hat geklappt
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
        /// <param name="setUnDoSnapshot"></param>
        /// <returns></returns>
        public virtual async Task<bool> AktionDelete(SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (this.editorState.IstRootNodeSelektiert) return false; // Der Root-Node soll gelöscht werden:  Nicht erlaubt

            if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
            {
                this.editorState.UndoHandler.SetSnapshot(
                    ResReader.Reader.GetString("AktionLoeschen"),
                    this.editorState.CursorRaw);
            }

            var optimized = this.editorState.CursorRaw;
            await optimized.OptimizeSelection();

            var deleteResult = await XmlCursorSelectionHelper.SelektionLoeschen(optimized);
            if (deleteResult.Success)
            {
                await this.editorState.CursorRaw.SetPositions(deleteResult.NeueCursorPosNachLoeschen.ActualNode, deleteResult.NeueCursorPosNachLoeschen.PosOnNode, deleteResult.NeueCursorPosNachLoeschen.PosInTextNode, throwChangedEventWhenValuesChanged: false);
                await this.editorState.FireContentChangedEvent();
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
        public virtual async Task<bool> AktionTextAnCursorPosEinfuegen(string insertText, SetUndoSnapshotOptions setUnDoSnapShot)
        {

            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (setUnDoSnapShot == SetUndoSnapshotOptions.Yes)
            {
                var editorStatus = this.editorState;
                editorStatus.UndoHandler.SetSnapshot(
                    String.Format(
                        ResReader.Reader.GetString("AktionSchreiben"),
                        insertText),
                    this.editorState.CursorRaw);
            }

            await TextEinfuegen(this.editorState.CursorRaw, insertText, this.xmlRules);
            await this.editorState.FireContentChangedEvent();
            return true;
        }

        /// <summary>
        /// Setzt den Inhalt eines Attributes in einem Node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual async Task<bool> AktionAttributWertInNodeSetzen(XmlNode node, string attributName, string value, SetUndoSnapshotOptions setUnDoSnapshot)
        {

            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            var xmlAttrib = node.Attributes[attributName];

            if (string.IsNullOrEmpty(value)) // Kein Inhalt, Attribut löschen, wenn vorhanden
            {
                if (xmlAttrib != null)  // Attribut gibts -> löschen
                {
                    if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
                    {
                        this.editorState.UndoHandler.SetSnapshot(
                            string.Format(
                                ResReader.Reader.GetString("AktionAttributGeloescht"),
                                attributName,
                                node.Name),
                            this.editorState.CursorRaw);
                    }
                    node.Attributes.Remove(xmlAttrib);
                }
            }
            else // Inhalt in Attribut schreiben
            {
                if (xmlAttrib == null)  // Attribut gibts noch nicht -> neu anlegen
                {
                    if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
                    {
                        this.editorState.UndoHandler.SetSnapshot(
                            String.Format(
                                ResReader.Reader.GetString("AktionAttributValueGeaendert"),
                                attributName,
                                node.Name,
                                value),
                            this.editorState.CursorRaw);
                    }
                    xmlAttrib = node.OwnerDocument.CreateAttribute(attributName);
                    node.Attributes.Append(xmlAttrib);
                    xmlAttrib.Value = value; // Inhalt in Attribut schreiben
                }
                else
                {
                    if (xmlAttrib.Value != value)
                    {
                        if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
                        {
                            this.editorState.UndoHandler.SetSnapshot(
                                 String.Format(
                                     ResReader.Reader.GetString("AktionAttributValueGeaendert"),
                                     attributName,
                                     node.Name,
                                     value),
                                 this.editorState.CursorRaw);
                        }
                        xmlAttrib.Value = value; // Inhalt in Attribut schreiben
                    }
                }
            }
            await this.editorState.FireContentChangedEvent(); // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
            return true;
        }

        /// <summary>
        /// Den Node oder das Zeichen vor dem Cursor löschen
        /// </summary>
        /// <param name="position"></param>
        public async Task<bool> AktionNodeOderZeichenVorDerCursorPosLoeschen(XmlCursorPos position, SetUndoSnapshotOptions setUnDoSnapshot)
        {

            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            // Den Cursor eine Pos nach links
            var deleteArea = new XmlCursor();
            deleteArea.StartPos.SetPos(position.ActualNode, position.PosOnNode, position.PosInTextNode);
            var endPos = deleteArea.StartPos.Clone();
            await CursorPosMoveHelper.MoveLeft(endPos, this.editorState.RootNode, this.xmlRules);
            deleteArea.EndPos.SetPos(endPos.ActualNode, endPos.PosOnNode, endPos.PosInTextNode);
            await deleteArea.OptimizeSelection();

            if (deleteArea.StartPos.ActualNode == this.editorState.RootNode) return false; // Den Rootnot darf man nicht löschen

            if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
            {
                this.editorState.UndoHandler.SetSnapshot(
                    ResReader.Reader.GetString("AktionLoeschen"),
                    this.editorState.CursorRaw);
            }

            var deleteResult = await XmlCursorSelectionHelper.SelektionLoeschen(deleteArea);
            if (deleteResult.Success)
            {
                // Nach erfolgreichem Löschen wird hier die neue CursorPos zurückgeholt
                await this.editorState.CursorRaw.SetPositions(deleteResult.NeueCursorPosNachLoeschen.ActualNode, deleteResult.NeueCursorPosNachLoeschen.PosOnNode, deleteResult.NeueCursorPosNachLoeschen.PosInTextNode, throwChangedEventWhenValuesChanged: false);
                await this.editorState.FireContentChangedEvent(); // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
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
        public async Task<bool> AktionNodeOderZeichenHinterCursorPosLoeschen(XmlCursorPos position, SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
            {
                this.editorState.UndoHandler.SetSnapshot(
                     ResReader.Reader.GetString("AktionLoeschen"),
                     this.editorState.CursorRaw);
            }

            var deleteArea = new XmlCursor();
            deleteArea.StartPos.SetPos(position.ActualNode, position.PosOnNode, position.PosInTextNode);
            var endPos = deleteArea.StartPos.Clone();
            await CursorPosMoveHelper.MoveRight(endPos, this.editorState.RootNode, this.xmlRules);
            deleteArea.EndPos.SetPos(endPos.ActualNode, endPos.PosOnNode, endPos.PosInTextNode);
            await deleteArea.OptimizeSelection();

            if (deleteArea.StartPos.ActualNode == this.editorState.RootNode) return false; // Den Rootnot darf man nicht löschen

            var deleteResult = await XmlCursorSelectionHelper.SelektionLoeschen(deleteArea);
            if (deleteResult.Success)
            {
                // Nach erfolgreichem Löschen wird hier die neue CursorPos zurückgeholt
                await this.editorState.CursorRaw.SetPositions(deleteResult.NeueCursorPosNachLoeschen.ActualNode, deleteResult.NeueCursorPosNachLoeschen.PosOnNode, deleteResult.NeueCursorPosNachLoeschen.PosInTextNode, throwChangedEventWhenValuesChanged: false);
                await this.editorState.FireContentChangedEvent();
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
        public virtual async Task<System.Xml.XmlNode> AktionNeuesElementAnAktCursorPosEinfuegen(string nodeName, SetUndoSnapshotOptions setUnDoSnapshot, bool neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
        {
            if (!this.ActionsAllowed) return null; // Wenn gar keine Aktionen zulässig sind, abbrechen

            XmlNode node;

            if (string.IsNullOrEmpty(nodeName))
            {
                // "Es wurde kein Nodename angegeben (xml.InsertNewElementAnCursorPos"
                throw new ApplicationException(ResReader.Reader.GetString("KeinNodeNameAngegeben"));
            }

            if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
            {
                this.editorState.UndoHandler.SetSnapshot(String.Format(
                    ResReader.Reader.GetString("AktionInsertNode"),
                    nodeName),
                 this.editorState.CursorRaw);
            }

            // Node erzeugen
            if (nodeName == "#COMMENT")
            {
                node = this.editorState.RootNode.OwnerDocument.CreateComment("NEW COMMENT");
            }
            else
            {
                node = this.editorState.RootNode.OwnerDocument.CreateNode(System.Xml.XmlNodeType.Element, nodeName, null);
            }

            // Node an aktueller CursorPos einfügen
            await XMLNodeEinfuegen(this.editorState.CursorRaw, node, this.xmlRules, neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen);
            await this.editorState.FireContentChangedEvent();
            return node;
        }


        /// <summary>
        /// Fügt den angegebenen Text an der aktuellen Cursorposition ein, sofern möglich
        /// </summary>
        private async Task TextEinfuegen(XmlCursor cursor, string text, de.springwald.xml.XmlRules regelwerk)
        {
            XmlCursorPos einfuegePos;

            // Wenn etwas selektiert ist, dann zuerst das löschen, da es ja durch den neuen Text ersetzt wird
            XmlCursor loeschbereich = cursor.Clone();
            await loeschbereich.OptimizeSelection();
            var loeschResult = await XmlCursorSelectionHelper.SelektionLoeschen(loeschbereich);
            if (loeschResult.Success)
            {
                einfuegePos = loeschResult.NeueCursorPosNachLoeschen;
            }
            else
            {
                einfuegePos = cursor.StartPos.Clone();
            }

            // den angegebenen Text an der CursorPosition einfügen
            var ersatzNode = InsertAtCursorPosHelper.InsertText(einfuegePos, text, regelwerk).ErsatzNode;
            if (ersatzNode != null)
            {
                // Text konnte nicht eingefügt werden, da aus der Texteingabe eine Node-Eingabe umgewandelt
                // wurde. Beispiel: Im AIML-Template wird * gedrückt, und dort statt dessen ein <star> eingefügt
                InsertAtCursorPosHelper.InsertXMLNode(einfuegePos, ersatzNode, regelwerk, false);
            }

            // anschließend wird der Cursor nur noch ein Strich hinter dem eingefügten
            await cursor.SetPositions(einfuegePos.ActualNode, einfuegePos.PosOnNode, einfuegePos.PosInTextNode, throwChangedEventWhenValuesChanged: false);
        }

        /// <summary>
        /// Fügt den angegebenen Node an der aktuellen Cursorposition ein, sofern möglich
        /// </summary>
        private async Task XMLNodeEinfuegen(XmlCursor cursor, System.Xml.XmlNode node, de.springwald.xml.XmlRules regelwerk, bool neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
        {
            // Wenn etwas selektiert ist, dann zuerst das löschen, da es ja durch den neuen Text ersetzt wird
            XmlCursor loeschbereich = cursor.Clone();
            await loeschbereich.OptimizeSelection();
            var loeschResult = await XmlCursorSelectionHelper.SelektionLoeschen(loeschbereich);
            if (loeschResult.Success)
            {
                await cursor.SetPositions(loeschResult.NeueCursorPosNachLoeschen.ActualNode, loeschResult.NeueCursorPosNachLoeschen.PosOnNode, loeschResult.NeueCursorPosNachLoeschen.PosInTextNode, throwChangedEventWhenValuesChanged: false);
            }

            // den angegebenen Node an der CursorPosition einfügen
            if (InsertAtCursorPosHelper.InsertXMLNode(cursor.StartPos, node, regelwerk, neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen))
            {
                // anschließen wird der Cursor nur noch ein Strich hinter dem eingefügten
                cursor.EndPos.SetPos(cursor.StartPos.ActualNode, cursor.StartPos.PosOnNode, cursor.StartPos.PosInTextNode);
            }
        }

    }
}
