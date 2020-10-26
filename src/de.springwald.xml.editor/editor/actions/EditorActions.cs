﻿// A platform indepentend tag-view-style graphical xml editor
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
using System;
using System.Threading.Tasks;
using System.Xml;

namespace de.springwald.xml.editor.actions
{
    public class EditorActions
    {
        private XMLRegelwerk regelwerk => this.editorContext.XmlRules;
        private INativePlatform nativePlatform => this.editorContext.NativePlatform;
        private EditorStatus editorStatus => this.editorContext.EditorStatus;

        private EditorContext editorContext;

        public enum UndoSnapshotSetzenOptionen { ja, nein };

        /// <summary>
        /// Sind überhaupt irgendwelche Aktionen möglich?
        /// </summary>
        private bool ActionsAllowed
        {
            get
            {
                if (this.editorStatus.ReadOnly)
                {
                    return false; // document is read only
                }
                else
                {
                    return this.editorStatus.CursorRoh.StartPos.AktNode != null;
                }
            }
        }

        public EditorActions(EditorContext editorContext)
        {
            this.editorContext = editorContext;
        }


        public async Task<bool> MoveRight(XMLCursorPos cursorPos)
        {
            return await CursorPosMoveHelper.MoveRight(cursorPos, this.editorContext.EditorStatus.RootNode, this.editorContext.XmlRules);
        }

        public async Task<bool> MoveLeft(XMLCursorPos cursorPos)
        {
            return await CursorPosMoveHelper.MoveLeft(cursorPos, this.editorContext.EditorStatus.RootNode, this.editorContext.XmlRules);
        }

        /// <summary>
        /// Fuegt den Zwischenablageinhalt an die aktuelle CursorPos ein
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionPasteFromClipboard(UndoSnapshotSetzenOptionen setUnDoSnapshot)
        {

            if (!ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            string text = "";

            try
            {
                if (await this.nativePlatform.Clipboard.ContainsText()) // wenn Text in der Zwischenablage ist
                {
                    XMLCursorPos startPos;
                    XMLCursorPos endPos;

                    if (this.editorStatus.IstRootNodeSelektiert) // Der Rootnode ist selektiert und soll daher durch den Clipboard-Inhalt ersetzt werden
                    {
                        return await AktionRootNodeDurchClipboardInhaltErsetzen(setUnDoSnapshot);
                    }
                    else // etwas anderes als der Rootnode soll ersetzt werden
                    {
                        // Zuerst eine etwaige Selektion löschen
                        if (this.editorStatus.IstEtwasSelektiert) // Es ist etwas selektiert
                        {
                            if (await AktionDelete(UndoSnapshotSetzenOptionen.nein))
                            {
                                startPos = this.editorStatus.CursorRoh.StartPos;
                            }
                            else // Löschen der Selektion fehlgeschlagen
                            {
                                return false;
                            }
                        }
                        else // Nichts selektiert
                        {
                            startPos = this.editorStatus.CursorOptimiert.StartPos;
                        }
                    }

                    if (setUnDoSnapshot == UndoSnapshotSetzenOptionen.ja)
                    {
                        this.editorStatus.UndoHandler.SnapshotSetzen(
                            ResReader.Reader.GetString("AktionEinfuegen"),
                            this.editorStatus.CursorRoh);
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
                    var pasteNode = this.editorStatus.RootNode.OwnerDocument.ReadNode(reader);

                    // Nun alle Children des virtuellen Paste-Node nacheinander an der CursorPos einfügen
                    endPos = startPos.Clone(); // Vor dem Einfügen sind start- und endPos gleich
                    foreach (XmlNode node in pasteNode.ChildNodes)
                    {
                        if (node is XmlText) // Einen Text einfügen
                        {
                            var pasteResult = InsertAtCursorPosHelper.TextEinfuegen(endPos, node.Clone().Value, this.regelwerk);
                            if (pasteResult.ErsatzNode != null)
                            {
                                // Text konnte nicht eingefügt werden, da aus der Texteingabe eine Node-Eingabe umgewandelt
                                // wurde. Beispiel: Im AIML-Template wird * gedrückt, und dort statt dessen ein <star> eingefügt
                                InsertAtCursorPosHelper.InsertXMLNode(endPos, pasteResult.ErsatzNode.Clone(), this.regelwerk, true);
                            }
                        }
                        else // Einen Node einfügen
                        {
                            InsertAtCursorPosHelper.InsertXMLNode(endPos, node.Clone(), this.regelwerk, true);
                        }
                    }

                    switch (this.editorStatus.CursorRoh.EndPos.PosAmNode)
                    {
                        case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                        case XMLCursorPositionen.CursorVorDemNode:
                            // Ende des Einfügens liegt einem Text oder vor dem Node
                            await this.editorStatus.CursorRoh.SetPositions(endPos.AktNode, endPos.PosAmNode, endPos.PosImTextnode, throwChangedEventWhenValuesChanged: false);
                            break;
                        default:
                            // Ende des Einfügens liegt hinter dem letzten eingefügten Node
                            await this.editorStatus.CursorRoh.SetPositions(endPos.AktNode, XMLCursorPositionen.CursorHinterDemNode, textPosInBothNodes: 0, throwChangedEventWhenValuesChanged: false);
                            break;
                    }
                    await this.editorStatus.FireContentChangedEvent();
                    return true;
                }
                else // Kein Text in der Zwischenablage
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                this.nativePlatform.ProtokolliereFehler(
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
        private async Task<bool> AktionRootNodeDurchClipboardInhaltErsetzen(UndoSnapshotSetzenOptionen setUnDoSnapshot)
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
                var pasteNode = this.editorStatus.RootNode.OwnerDocument.ReadNode(reader);

                if (pasteNode.Name != this.editorStatus.RootNode.Name)
                { // Der Node in der Zwischenablage und der aktuelle Rootnode haben nicht den selben Namen
                    return false; // Nicht erlaubt
                }

                if (setUnDoSnapshot == UndoSnapshotSetzenOptionen.ja)
                {
                    this.editorStatus.UndoHandler.SnapshotSetzen(
                        ResReader.Reader.GetString("RootNodedurchZwischenablageersetzen"),
                        this.editorStatus.CursorRoh);
                }

                // Alle Children + Attribute des bisherigen Rootnodes löschen
                this.editorStatus.RootNode.RemoveAll();

                // Alle Attribute des Clipboard-Root-Nodes in den richtigen Root-Node übernehmen
                while (pasteNode.Attributes.Count > 0)
                {
                    var attrib = pasteNode.Attributes.Remove(pasteNode.Attributes[0]); // von Clipboard-Rootnode entfernen
                    this.editorStatus.RootNode.Attributes.Append(attrib); // an richtigen Rootnode packen
                }

                var startPos = new XMLCursorPos();
                startPos.SetPos(this.editorStatus.RootNode, XMLCursorPositionen.CursorInDemLeeremNode);
                XMLCursorPos endPos;

                // Nun alle Children des virtuellen Root-Node nacheinander an der CursorPos einfügen
                endPos = startPos.Clone(); // Vor dem Einfügen sind start- und endPos gleich
                while (pasteNode.ChildNodes.Count > 0)
                {
                    var child = pasteNode.RemoveChild(pasteNode.FirstChild);
                    this.editorStatus.RootNode.AppendChild(child);
                }
                await this.editorStatus.CursorRoh.SetPositions(this.editorStatus.RootNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, 0, throwChangedEventWhenValuesChanged: false);
                await this.editorStatus.FireContentChangedEvent();
                return true;
            }
            catch (Exception e)
            {
                this.nativePlatform.ProtokolliereFehler($"AktionRootNodeDurchClipboardInhaltErsetzen:Fehler für Einfügetext 'text': {e.Message}");
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

            var content = await XmlCursorSelectionHelper.GetSelektionAlsString(this.editorStatus.CursorRoh);
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
            if (this.editorStatus.RootNode == null)
            {
#warning hier noch beep
                return false;
            }
            else
            {
                if (this.editorStatus.RootNode.FirstChild != null)
                {
                    // Vor das erste Child des Rootnodes
                    await this.editorStatus.CursorRoh.SetPositions(this.editorStatus.RootNode.FirstChild, XMLCursorPositionen.CursorVorDemNode, 0, throwChangedEventWhenValuesChanged: true);
                }
                else
                {
                    // In den leeren Rootnode
                    await this.editorStatus.CursorRoh.SetPositions(this.editorStatus.RootNode, XMLCursorPositionen.CursorInDemLeeremNode, 0, throwChangedEventWhenValuesChanged: true);
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
            await this.editorStatus.CursorRoh.SetPositions(this.editorStatus.RootNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, 0, throwChangedEventWhenValuesChanged: true);
            return true;
        }

        /// <summary>
        /// Schneidet die aktuelle Selektion aus und schiebt sie in die Zwischenablage
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> AktionCutToClipboard(UndoSnapshotSetzenOptionen setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (this.editorStatus.CursorOptimiert.StartPos.AktNode == this.editorStatus.RootNode)
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
        /// <param name="setUnDoSnapshot"></param>
        /// <returns></returns>
        public virtual async Task<bool> AktionDelete(UndoSnapshotSetzenOptionen setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (this.editorStatus.IstRootNodeSelektiert) return false; // Der Root-Node soll gelöscht werden:  Nicht erlaubt

            if (setUnDoSnapshot == UndoSnapshotSetzenOptionen.ja)
            {
                this.editorStatus.UndoHandler.SnapshotSetzen(
                    ResReader.Reader.GetString("AktionLoeschen"),
                    this.editorStatus.CursorRoh);
            }

            var optimized = this.editorStatus.CursorRoh;
            await optimized.SelektionOptimieren();

            var deleteResult = await XmlCursorSelectionHelper.SelektionLoeschen(optimized);
            if (deleteResult.Success)
            {
                await this.editorStatus.CursorRoh.SetPositions(deleteResult.NeueCursorPosNachLoeschen.AktNode, deleteResult.NeueCursorPosNachLoeschen.PosAmNode, deleteResult.NeueCursorPosNachLoeschen.PosImTextnode, throwChangedEventWhenValuesChanged: false);
                await this.editorStatus.FireContentChangedEvent();
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
        public virtual async Task<bool> AktionTextAnCursorPosEinfuegen(string insertText, UndoSnapshotSetzenOptionen setUnDoSnapShot)
        {

            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (setUnDoSnapShot == UndoSnapshotSetzenOptionen.ja)
            {
                var editorStatus = this.editorStatus;
                editorStatus.UndoHandler.SnapshotSetzen(
                    String.Format(
                        ResReader.Reader.GetString("AktionSchreiben"),
                        insertText),
                    this.editorStatus.CursorRoh);
            }

            await TextEinfuegen(this.editorStatus.CursorRoh, insertText, this.regelwerk);
            await this.editorStatus.FireContentChangedEvent();
            return true;
        }

        /// <summary>
        /// Setzt den Inhalt eines Attributes in einem Node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual async Task<bool> AktionAttributWertInNodeSetzen(XmlNode node, string attributName, string value, UndoSnapshotSetzenOptionen setUnDoSnapshot)
        {

            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            var xmlAttrib = node.Attributes[attributName];

            if (string.IsNullOrEmpty(value)) // Kein Inhalt, Attribut löschen, wenn vorhanden
            {
                if (xmlAttrib != null)  // Attribut gibts -> löschen
                {
                    if (setUnDoSnapshot == UndoSnapshotSetzenOptionen.ja)
                    {
                        this.editorStatus.UndoHandler.SnapshotSetzen(
                            string.Format(
                                ResReader.Reader.GetString("AktionAttributGeloescht"),
                                attributName,
                                node.Name),
                            this.editorStatus.CursorRoh);
                    }
                    node.Attributes.Remove(xmlAttrib);
                }
            }
            else // Inhalt in Attribut schreiben
            {
                if (xmlAttrib == null)  // Attribut gibts noch nicht -> neu anlegen
                {
                    if (setUnDoSnapshot == UndoSnapshotSetzenOptionen.ja)
                    {
                        this.editorStatus.UndoHandler.SnapshotSetzen(
                            String.Format(
                                ResReader.Reader.GetString("AktionAttributValueGeaendert"),
                                attributName,
                                node.Name,
                                value),
                            this.editorStatus.CursorRoh);
                    }
                    xmlAttrib = node.OwnerDocument.CreateAttribute(attributName);
                    node.Attributes.Append(xmlAttrib);
                    xmlAttrib.Value = value; // Inhalt in Attribut schreiben
                }
                else
                {
                    if (xmlAttrib.Value != value)
                    {
                        if (setUnDoSnapshot == UndoSnapshotSetzenOptionen.ja)
                        {
                            this.editorStatus.UndoHandler.SnapshotSetzen(
                                 String.Format(
                                     ResReader.Reader.GetString("AktionAttributValueGeaendert"),
                                     attributName,
                                     node.Name,
                                     value),
                                 this.editorStatus.CursorRoh);
                        }
                        xmlAttrib.Value = value; // Inhalt in Attribut schreiben
                    }
                }
            }
            await this.editorStatus.FireContentChangedEvent(); // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
            return true;
        }

        /// <summary>
        /// Den Node oder das Zeichen vor dem Cursor löschen
        /// </summary>
        /// <param name="position"></param>
        public async Task<bool> AktionNodeOderZeichenVorDerCursorPosLoeschen(XMLCursorPos position, UndoSnapshotSetzenOptionen setUnDoSnapshot)
        {

            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            // Den Cursor eine Pos nach links
            var deleteArea = new XMLCursor();
            deleteArea.StartPos.SetPos(position.AktNode, position.PosAmNode, position.PosImTextnode);
            var endPos = deleteArea.StartPos.Clone();
            await CursorPosMoveHelper.MoveLeft(endPos, this.editorStatus.RootNode, this.regelwerk);
            deleteArea.EndPos.SetPos(endPos.AktNode, endPos.PosAmNode, endPos.PosImTextnode);
            await deleteArea.SelektionOptimieren();

            if (deleteArea.StartPos.AktNode == this.editorStatus.RootNode) return false; // Den Rootnot darf man nicht löschen

            if (setUnDoSnapshot == UndoSnapshotSetzenOptionen.ja)
            {
                this.editorStatus.UndoHandler.SnapshotSetzen(
                    ResReader.Reader.GetString("AktionLoeschen"),
                    this.editorStatus.CursorRoh);
            }

            var deleteResult = await XmlCursorSelectionHelper.SelektionLoeschen(deleteArea);
            if (deleteResult.Success)
            {
                // Nach erfolgreichem Löschen wird hier die neue CursorPos zurückgeholt
                await this.editorStatus.CursorRoh.SetPositions(deleteResult.NeueCursorPosNachLoeschen.AktNode, deleteResult.NeueCursorPosNachLoeschen.PosAmNode, deleteResult.NeueCursorPosNachLoeschen.PosImTextnode, throwChangedEventWhenValuesChanged: false);
                await this.editorStatus.FireContentChangedEvent(); // Bescheid sagen, dass der Inhalt des XMLs sich geändert hat
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
        public async Task<bool> AktionNodeOderZeichenHinterCursorPosLoeschen(XMLCursorPos position, UndoSnapshotSetzenOptionen setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false; // Wenn gar keine Aktionen zulässig sind, abbrechen

            if (setUnDoSnapshot == UndoSnapshotSetzenOptionen.ja)
            {
                this.editorStatus.UndoHandler.SnapshotSetzen(
                     ResReader.Reader.GetString("AktionLoeschen"),
                     this.editorStatus.CursorRoh);
            }

            var deleteArea = new XMLCursor();
            deleteArea.StartPos.SetPos(position.AktNode, position.PosAmNode, position.PosImTextnode);
            var endPos = deleteArea.StartPos.Clone();
            await CursorPosMoveHelper.MoveRight(endPos, this.editorStatus.RootNode, this.regelwerk);
            deleteArea.EndPos.SetPos(endPos.AktNode, endPos.PosAmNode, endPos.PosImTextnode);
            await deleteArea.SelektionOptimieren();

            if (deleteArea.StartPos.AktNode == this.editorStatus.RootNode) return false; // Den Rootnot darf man nicht löschen

            var deleteResult = await XmlCursorSelectionHelper.SelektionLoeschen(deleteArea);
            if (deleteResult.Success)
            {
                // Nach erfolgreichem Löschen wird hier die neue CursorPos zurückgeholt
                await this.editorStatus.CursorRoh.SetPositions(deleteResult.NeueCursorPosNachLoeschen.AktNode, deleteResult.NeueCursorPosNachLoeschen.PosAmNode, deleteResult.NeueCursorPosNachLoeschen.PosImTextnode, throwChangedEventWhenValuesChanged: false);
                await this.editorStatus.FireContentChangedEvent();
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
        public virtual async Task<System.Xml.XmlNode> AktionNeuesElementAnAktCursorPosEinfuegen(string nodeName, UndoSnapshotSetzenOptionen setUnDoSnapshot, bool neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
        {
            if (!this.ActionsAllowed) return null; // Wenn gar keine Aktionen zulässig sind, abbrechen

            XmlNode node;

            if (string.IsNullOrEmpty(nodeName))
            {
                // "Es wurde kein Nodename angegeben (xml.InsertNewElementAnCursorPos"
                throw new ApplicationException(ResReader.Reader.GetString("KeinNodeNameAngegeben"));
            }

            if (setUnDoSnapshot == UndoSnapshotSetzenOptionen.ja)
            {
                this.editorStatus.UndoHandler.SnapshotSetzen(String.Format(
                    ResReader.Reader.GetString("AktionInsertNode"),
                    nodeName),
                 this.editorStatus.CursorRoh);
            }

            // Node erzeugen
            if (nodeName == "#COMMENT")
            {
                node = this.editorStatus.RootNode.OwnerDocument.CreateComment("NEW COMMENT");
            }
            else
            {
                node = this.editorStatus.RootNode.OwnerDocument.CreateNode(System.Xml.XmlNodeType.Element, nodeName, null);
            }

            // Node an aktueller CursorPos einfügen
            await XMLNodeEinfuegen(this.editorStatus.CursorRoh, node, this.regelwerk, neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen);
            await this.editorStatus.FireContentChangedEvent();
            return node;
        }


        /// <summary>
        /// Fügt den angegebenen Text an der aktuellen Cursorposition ein, sofern möglich
        /// </summary>
        private async Task TextEinfuegen(XMLCursor cursor, string text, de.springwald.xml.XMLRegelwerk regelwerk)
        {
            XMLCursorPos einfuegePos;

            // Wenn etwas selektiert ist, dann zuerst das löschen, da es ja durch den neuen Text ersetzt wird
            XMLCursor loeschbereich = cursor.Clone();
            await loeschbereich.SelektionOptimieren();
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
            var ersatzNode = InsertAtCursorPosHelper.TextEinfuegen(einfuegePos, text, regelwerk).ErsatzNode;
            if (ersatzNode != null)
            {
                // Text konnte nicht eingefügt werden, da aus der Texteingabe eine Node-Eingabe umgewandelt
                // wurde. Beispiel: Im AIML-Template wird * gedrückt, und dort statt dessen ein <star> eingefügt
                InsertAtCursorPosHelper.InsertXMLNode(einfuegePos, ersatzNode, regelwerk, false);
            }

            // anschließend wird der Cursor nur noch ein Strich hinter dem eingefügten
            await cursor.SetPositions(einfuegePos.AktNode, einfuegePos.PosAmNode, einfuegePos.PosImTextnode, throwChangedEventWhenValuesChanged: false);
        }

        /// <summary>
        /// Fügt den angegebenen Node an der aktuellen Cursorposition ein, sofern möglich
        /// </summary>
        private async Task XMLNodeEinfuegen(XMLCursor cursor, System.Xml.XmlNode node, de.springwald.xml.XMLRegelwerk regelwerk, bool neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
        {
            // Wenn etwas selektiert ist, dann zuerst das löschen, da es ja durch den neuen Text ersetzt wird
            XMLCursor loeschbereich = cursor.Clone();
            await loeschbereich.SelektionOptimieren();
            var loeschResult = await XmlCursorSelectionHelper.SelektionLoeschen(loeschbereich);
            if (loeschResult.Success)
            {
                await cursor.SetPositions(loeschResult.NeueCursorPosNachLoeschen.AktNode, loeschResult.NeueCursorPosNachLoeschen.PosAmNode, loeschResult.NeueCursorPosNachLoeschen.PosImTextnode, throwChangedEventWhenValuesChanged: false);
            }

            // den angegebenen Node an der CursorPosition einfügen
            if (InsertAtCursorPosHelper.InsertXMLNode(cursor.StartPos, node, regelwerk, neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen))
            {
                // anschließen wird der Cursor nur noch ein Strich hinter dem eingefügten
                cursor.EndPos.SetPos(cursor.StartPos.AktNode, cursor.StartPos.PosAmNode, cursor.StartPos.PosImTextnode);
            }
        }

    }
}