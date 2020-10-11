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
        private bool _naechsteTasteBeiKeyPressAlsTextAufnehmen = true;
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

                _naechsteTasteBeiKeyPressAlsTextAufnehmen = false;

                XMLCursorPos dummy;

                switch (e.Key)
                {
                   

                    case Keys.Enter: // Enter macht spezielle Dinge, z.B. ein neues Tag gleicher Art beginnen etc.
                    //case (Keys.Enter | Keys.Shift):
                    //    this.actions.AktionenEnterGedrueckt();
                    //    break;

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

                    //case (Keys.Left | Keys.Shift):  // CursorEndPos ein Char nach links
                    //    await _cursor.EndPos.MoveLeft(_rootNode, Regelwerk);
                    //    break;

                    //case (Keys.Right | Keys.Shift): // CursorEnd ein Char nach rechts
                    //    await _cursor.EndPos.MoveRight(_rootNode, Regelwerk);
                    //    break;

                    //case (Keys.A | Keys.Control): // STRG-A -> Alles markieren
                    //    await AktionAllesMarkieren();
                    //    break;

                    //case (Keys.X | Keys.Control): // STRG-X -> Ausschneiden
                    //    AktionCutToClipboard(UndoSnapshotSetzenOptionen.ja);
                    //    break;

                    //case (Keys.C | Keys.Control): // STRG-C -> Kopieren
                    //    AktionCopyToClipboard();
                    //    break;

                    case (Keys.V): // STRG-V -> Einfügen
                        if (e.CtrlKey)
                        {
                            await this.actions.AktionPasteFromClipboard(UndoSnapshotSetzenOptionen.ja);
                        }
                        break;

                    //case (Keys.Home): // Pos1 
                    //    AktionCursorAufPos1();
                    //    break;

                    case (Keys.Z ): //STRG-Z -> UnDo
                        if (e.CtrlKey)
                        {
                             // await this.actions.UnDo();
                        }
                        break;

                    case Keys.Left: // Cursor ein Char nach links
                        dummy = this.editorStatus.CursorRoh.StartPos.Clone();
                        await dummy.MoveLeft(this.editorStatus.RootNode, this.editorStatus.Regelwerk);
                        await this.editorStatus.CursorRoh.BeideCursorPosSetzenMitChangeEventWennGeaendert(dummy.AktNode, dummy.PosAmNode, dummy.PosImTextnode);
                        break;

                    case Keys.Right: // Cursor ein Char nach rechts
                        dummy = this.editorStatus.CursorRoh.StartPos.Clone();
                        await dummy.MoveRight(this.editorStatus.RootNode, this.editorStatus.Regelwerk);
                        await this.editorStatus.CursorRoh.BeideCursorPosSetzenMitChangeEventWennGeaendert(dummy.AktNode, dummy.PosAmNode, dummy.PosImTextnode);
                        break;

                    case Keys.Back:  // Backspace-Taste
                                     // case (Keys.Back | Keys.Shift):    // Shift-Backspace-Taste
                        if (this.editorStatus.CursorRoh.IstEtwasSelektiert)
                        {
                            await this.actions.AktionDelete(UndoSnapshotSetzenOptionen.ja);
                        }
                        else
                        {
                            await this.actions.AktionNodeOderZeichenVorDerCursorPosLoeschen(this.editorStatus.CursorRoh.StartPos, UndoSnapshotSetzenOptionen.ja);
                        }
                        break;

                    //case Keys.Delete:                   // entfernen-Taste
                    //// case (Keys.Delete | Keys.Shift):    // Shift-Entfernen-Taste
                    //    if (_cursor.IstEtwasSelektiert)
                    //    {
                    //        await AktionDelete(UndoSnapshotSetzenOptionen.ja);
                    //    }
                    //    else
                    //    {
                    //        await AktionNodeOderZeichenHinterCursorPosLoeschen(_cursor.StartPos, UndoSnapshotSetzenOptionen.ja);
                    //    }
                    //    break;
                    //case Keys.Control:
                    //case Keys.Escape:
                    //    // Bei diesen Tasten passiert nichts
                    //    break;

                    case Keys.undefined:
                    default:
                        // Die restlichen Tasten werden beim KeyPress als Text übernommen
                        // _naechsteTasteBeiKeyPressAlsTextAufnehmen = true;
                        await this.actions.AktionTextAnCursorPosEinfuegen(e.Content, UndoSnapshotSetzenOptionen.ja);
                        break;

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
            if (_naechsteTasteBeiKeyPressAlsTextAufnehmen)
            {
                // await this.actions.AktionTextAnCursorPosEinfuegen(e.KeyChar.ToString(), UndoSnapshotSetzenOptionen.ja);
            }
            _naechsteTasteBeiKeyPressAlsTextAufnehmen = false;
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
