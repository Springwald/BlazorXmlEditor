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
using de.springwald.xml.editor.actions;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.events;
using de.springwald.xml.rules;
using static de.springwald.xml.editor.actions.EditorActions;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor
{
    internal class KeyboardHandler : IDisposable
    {
        // private bool _naechstesLostFokusVerhindern = false; // So that leaving the focus is ignored on TAB
        private INativePlatform nativePlatform;
        private XmlRules regelwerk;
        private EditorActions actions;
        private EditorState editorState;

        public XmlAsyncEvent<KeyEventArgs> KeyDownEvent = new XmlAsyncEvent<KeyEventArgs>();
        public XmlAsyncEvent<KeyEventArgs> KeyPressEvent = new XmlAsyncEvent<KeyEventArgs>();

        public KeyboardHandler(EditorContext editorContext)
        {
            this.actions = editorContext.Actions;
            this.editorState = editorContext.EditorState;
            this.regelwerk = editorContext.XmlRules;

            this.nativePlatform = editorContext.NativePlatform;
            this.nativePlatform.InputEvents.PreviewKey.Add(this.zeichnungsSteuerelement_PreviewKeyDown);
            this.nativePlatform.InputEvents.KeyPress.Add(this.zeichnungsSteuerelement_KeyPress);
        }

        public void Dispose()
        {
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

                XmlCursorPos dummy;

                switch (e.Key)
                {
                    // >>> special character keys >>>

                    case (Keys.A): // CTRL-A -> select all
                        if (e.CtrlKey) await this.actions.ActionSelectAll();
                        break;

                    case (Keys.C): // CTRL-C -> copy
                        if (e.CtrlKey) await this.actions.ActionCopyToClipboard();
                        break;

                    case (Keys.V): // CTRL-V -> paste
                        if (e.CtrlKey)  await this.actions.ActionPasteFromClipboard(SetUndoSnapshotOptions.Yes);
                        break;

                    case (Keys.X): // CTRL-X -> cut
                        if (e.CtrlKey) await this.actions.AktionCutToClipboard(SetUndoSnapshotOptions.Yes);
                        break;

                    case (Keys.Y): //CTRL-Y -> ReDo (not implemented yet)
                        if (e.CtrlKey) {  }
                        break;

                    case (Keys.Z): //CTRL-Z -> UnDo
                        if (e.CtrlKey) await this.actions.Undo();
                        break;

                    // >>>> cursor keys

                    case Keys.Left: // move cursor to left
                        if (e.ShiftKey)
                        {
                            await CursorPosMoveHelper.MoveLeft(this.editorState.CursorRaw.EndPos, this.editorState.RootNode, this.regelwerk);
                        }
                        else
                        {
                            dummy = this.editorState.CursorRaw.StartPos.Clone();
                            await CursorPosMoveHelper.MoveLeft(dummy, this.editorState.RootNode, this.regelwerk);
                            await this.editorState.CursorRaw.BeideCursorPosSetzenMitChangeEventWennGeaendert(dummy.ActualNode, dummy.PosOnNode, dummy.PosInTextNode);
                        }
                        useKeyContent = false;
                        break;

                    case Keys.Right: // move cursor to right
                        if (e.ShiftKey)
                        {
                            await CursorPosMoveHelper.MoveRight(this.editorState.CursorRaw.EndPos, this.editorState.RootNode, this.regelwerk);
                        }
                        else
                        {
                            dummy = this.editorState.CursorRaw.StartPos.Clone();
                            await CursorPosMoveHelper.MoveRight(dummy, this.editorState.RootNode, this.regelwerk);
                            await this.editorState.CursorRaw.BeideCursorPosSetzenMitChangeEventWennGeaendert(dummy.ActualNode, dummy.PosOnNode, dummy.PosInTextNode);
                        }
                        useKeyContent = false;
                        break;

                    // >>>> command keys

                    case (Keys.Home): // Pos1 
                        await this.actions.ActionCursorOnPos1();
                        useKeyContent = false;
                        break;

                    case Keys.Enter: // Enter does special things, e.g. start a new day of the same kind etc.
                        this.actions.ActionEnterPressed();
                        useKeyContent = false;
                        break;

                    case Keys.Tab: // Tab jumps to the next day
                        System.Xml.XmlNode node = this.editorState.CursorRaw.StartPos.ActualNode;
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
                                    // Hm, where could TAB *still* go? 
                                    abbruch = true;
                                }
                            }
                        }
                        if (!abbruch)
                        {
                            await this.editorState.CursorRaw.BeideCursorPosSetzenMitChangeEventWennGeaendert(node, XmlCursorPositions.CursorInsideTheEmptyNode);
                        }
                        // _naechstesLostFokusVerhindern = true; // So that leaving the focus is ignored on TAB
                        useKeyContent = false;
                        break;

                    case Keys.Back:  
                        if (this.editorState.CursorRaw.IsSomethingSelected)
                        {
                            await this.actions.ActionDelete(SetUndoSnapshotOptions.Yes);
                        }
                        else
                        {
                            await this.actions.ActionDeleteNodeOrCharInForntOfCursorPos(this.editorState.CursorRaw.StartPos, SetUndoSnapshotOptions.Yes);
                        }
                        useKeyContent = false;
                        break;

                    case Keys.Delete:          
                        if (this.editorState.CursorRaw.IsSomethingSelected)
                        {
                            await this.actions.ActionDelete(SetUndoSnapshotOptions.Yes);
                        }
                        else
                        {
                            await this.actions.ActionDeleteNodeOrSignBehindCursorPos(this.editorState.CursorRaw.StartPos, SetUndoSnapshotOptions.Yes);
                        }
                        useKeyContent = false;
                        break;

                    case Keys.Escape:
                    case Keys.undefined:
                    default:
                        useKeyContent = true;
                        break;

                }

                if (useKeyContent)
                {
                    await this.actions.ActionInsertTextAtCursorPos(e.Content, SetUndoSnapshotOptions.Yes);
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
        //async Task zeichnungsSteuerelement_Leave(System.EventArgs e)
        //{
        //    if (_naechstesLostFokusVerhindern)
        //    {
        //        _naechstesLostFokusVerhindern = false;
        //        await this.nativePlatform.Focus.FokusAufEingabeFormularSetzen();
        //    }
        //}


    }
}
