// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Threading.Tasks;
using de.springwald.xml.cursor;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.events;
using static de.springwald.xml.editor.editor.EditorActions;

namespace de.springwald.xml.editor.editor
{
    internal class KeyboardHandler : IDisposable
    {
        private bool _naechstesLostFokusVerhindern = false; // Damit beim TAB das Verlassen des Fokus ignoriert wird
        private INativePlatform nativePlatform;
        private EditorActions actions;
        private EditorStatus editorStatus;
        public XmlAsyncEvent<KeyEventArgs> KeyDownEvent = new XmlAsyncEvent<KeyEventArgs>();
        public XmlAsyncEvent<KeyEventArgs> KeyPressEvent = new XmlAsyncEvent<KeyEventArgs>();

        public KeyboardHandler(INativePlatform nativePlatform, EditorStatus editorStatus, EditorActions actions)
        {
            this.nativePlatform = nativePlatform;
            this.actions = actions;
            this.editorStatus = editorStatus;
            this.nativePlatform.InputEvents.Leave.Add(this.zeichnungsSteuerelement_Leave);
            this.nativePlatform.InputEvents.PreviewKey.Add(this.zeichnungsSteuerelement_PreviewKeyDown);
            this.nativePlatform.InputEvents.KeyPress.Add(this.zeichnungsSteuerelement_KeyPress);
        }

        public void Dispose()
        {
            this.nativePlatform.InputEvents.Leave.Remove(this.zeichnungsSteuerelement_Leave);
            this.nativePlatform.InputEvents.PreviewKey.Remove(this.zeichnungsSteuerelement_PreviewKeyDown);
            this.nativePlatform.InputEvents.KeyPress.Remove(this.zeichnungsSteuerelement_KeyPress);
        }

        public async Task zeichnungsSteuerelement_PreviewKeyDown(KeyEventArgs e)
        {
            //if (Regelwerk.PreviewKeyDown(e, out _naechsteTasteBeiKeyPressAlsTextAufnehmen, this))
            //{
            //    // Dieser Tastendruck wurde bereits vom Regelwerk verarbeitet
            //}
            //else
            {
                // Dieser Tastendruck wurde nicht vom Regelwerk verarbeitet
                var useKeyContent = e.CtrlKey == false;

                XMLCursorPos dummy;

                switch (e.Key)
                {
                    // >>> special character keys >>>

                    case (Keys.A): // STRG-A -> Alles markieren
                        if (e.CtrlKey) await this.actions.AktionAllesMarkieren();
                        break;

                    case (Keys.C): // STRG-C -> Kopieren
                        if (e.CtrlKey) await this.actions.AktionCopyToClipboard();
                        break;

                    case (Keys.V): // STRG-V -> Einfügen
                        if (e.CtrlKey)  await this.actions.AktionPasteFromClipboard(UndoSnapshotSetzenOptionen.ja);
                        break;

                    case (Keys.X): // STRG-X -> Ausschneiden
                        if (e.CtrlKey) await this.actions.AktionCutToClipboard(UndoSnapshotSetzenOptionen.ja);
                        break;

                    case (Keys.Z): //STRG-Z -> UnDo
                        if (e.CtrlKey) await this.actions.Undo();
                        break;

                    // >>>> cursor keys

                    case Keys.Left: // Cursor ein Char nach links
                        if (e.ShiftKey)
                        {
                            await this.editorStatus.CursorRoh.EndPos.MoveLeft(this.editorStatus.RootNode, this.editorStatus.Regelwerk);
                        }
                        else
                        {
                            dummy = this.editorStatus.CursorRoh.StartPos.Clone();
                            await dummy.MoveLeft(this.editorStatus.RootNode, this.editorStatus.Regelwerk);
                            await this.editorStatus.CursorRoh.BeideCursorPosSetzenMitChangeEventWennGeaendert(dummy.AktNode, dummy.PosAmNode, dummy.PosImTextnode);
                        }
                        break;

                    case Keys.Right: // Cursor ein Char nach rechts
                        if (e.ShiftKey)
                        {
                            await this.editorStatus.CursorRoh.EndPos.MoveRight(this.editorStatus.RootNode, this.editorStatus.Regelwerk);
                        }
                        else
                        {
                            dummy = this.editorStatus.CursorRoh.StartPos.Clone();
                            await dummy.MoveRight(this.editorStatus.RootNode, this.editorStatus.Regelwerk);
                            await this.editorStatus.CursorRoh.BeideCursorPosSetzenMitChangeEventWennGeaendert(dummy.AktNode, dummy.PosAmNode, dummy.PosImTextnode);
                        }
                        break;

                    // >>>> command keys

                    case (Keys.Home): // Pos1 
                        await this.actions.AktionCursorAufPos1();
                        break;

                    case Keys.Enter: // Enter macht spezielle Dinge, z.B. ein neues Tag gleicher Art beginnen etc.
                        this.actions.AktionenEnterGedrueckt();
                        break;

                    case Keys.Tab: // Tab springt in das nächste Tag
                        System.Xml.XmlNode node = this.editorStatus.CursorRoh.StartPos.AktNode;
                        bool abbruch = false;
                        if (node.FirstChild != null)
                        {
                            node = node.FirstChild;
                        }
                        else
                        {
                            if (node.NextSibling != null)
                            {
                                node = node.NextSibling;
                            }
                            else
                            {
                                if (node.ParentNode.NextSibling != null)
                                {
                                    node = node.ParentNode.NextSibling;
                                }
                                else
                                {
                                    // Hm, wohin könnte TAB denn *noch* gehen? 
                                    abbruch = true;
                                }
                            }
                        }
                        if (!abbruch)
                        {
                            await this.editorStatus.CursorRoh.BeideCursorPosSetzenMitChangeEventWennGeaendert(node, XMLCursorPositionen.CursorInDemLeeremNode);
                        }
                        _naechstesLostFokusVerhindern = true; // Damit beim TAB das Verlassen des Fokus ignoriert wird
                        break;

                    case Keys.Back:  // Backspace-Taste
                        if (this.editorStatus.CursorRoh.IstEtwasSelektiert)
                        {
                            await this.actions.AktionDelete(UndoSnapshotSetzenOptionen.ja);
                        }
                        else
                        {
                            await this.actions.AktionNodeOderZeichenVorDerCursorPosLoeschen(this.editorStatus.CursorRoh.StartPos, UndoSnapshotSetzenOptionen.ja);
                        }
                        break;

                    case Keys.Delete:                   // entfernen-Taste
                        if (this.editorStatus.CursorRoh.IstEtwasSelektiert)
                        {
                            await this.actions.AktionDelete(UndoSnapshotSetzenOptionen.ja);
                        }
                        else
                        {
                            await this.actions.AktionNodeOderZeichenHinterCursorPosLoeschen(this.editorStatus.CursorRoh.StartPos, UndoSnapshotSetzenOptionen.ja);
                        }
                        break;

                    case Keys.Escape:
                    case Keys.undefined:
                    default:
                        useKeyContent = true;
                        break;

                }

                if (useKeyContent)
                {
                    await this.actions.AktionTextAnCursorPosEinfuegen(e.Content, UndoSnapshotSetzenOptionen.ja);
                }
            }
        }


        /// <summary>
        /// Eingegebenen Text übernehmen, wenn nicht vorher im Key-Down abgefangen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async Task zeichnungsSteuerelement_KeyPress(KeyEventArgs e)
        {
            //if (_naechsteTasteBeiKeyPressAlsTextAufnehmen)
            //{
            //    // await this.actions.AktionTextAnCursorPosEinfuegen(e.KeyChar.ToString(), UndoSnapshotSetzenOptionen.ja);
            //}
            //_naechsteTasteBeiKeyPressAlsTextAufnehmen = false;
        }

        /// <summary>
        /// Damit beim TAB das Verlassen des Fokus ignoriert wird
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task zeichnungsSteuerelement_Leave(System.EventArgs e)
        {
            if (_naechstesLostFokusVerhindern)
            {
                _naechstesLostFokusVerhindern = false;
                await this.nativePlatform.Focus.FokusAufEingabeFormularSetzen();
            }
        }


    }
}
