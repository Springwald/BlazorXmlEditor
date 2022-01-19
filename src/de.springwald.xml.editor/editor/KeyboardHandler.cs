// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.actions;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.events;
using de.springwald.xml.rules;
using System;
using System.Threading.Tasks;
using System.Xml;
using static de.springwald.xml.editor.actions.EditorActions;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor
{
    internal class KeyboardHandler : IDisposable
    {
        private readonly INativePlatform nativePlatform;
        private readonly XmlRules xmlRules;
        private readonly EditorActions actions;
        private readonly EditorState editorState;

        public XmlAsyncEvent<KeyEventArgs> KeyDownEvent = new XmlAsyncEvent<KeyEventArgs>();
        public XmlAsyncEvent<KeyEventArgs> KeyPressEvent = new XmlAsyncEvent<KeyEventArgs>();

        public KeyboardHandler(EditorContext editorContext)
        {
            this.actions = editorContext.Actions;
            this.editorState = editorContext.EditorState;
            this.xmlRules = editorContext.XmlRules;
            this.nativePlatform = editorContext.NativePlatform;
            this.nativePlatform.InputEvents.PreviewKey.Add(this.ControlKeyPreview);
        }

        public void Dispose()
        {
            this.nativePlatform.InputEvents.PreviewKey.Remove(this.ControlKeyPreview);
        }

        public async Task ControlKeyPreview(KeyEventArgs e)
        {
            if (xmlRules.KeyPreviewHandled(e))
            {
                // This keystroke has already been processed by the ruleset
            }
            else
            {
                // This keystroke was not processed by the rules
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
                        if (e.CtrlKey) await this.actions.ActionPasteFromClipboard(SetUndoSnapshotOptions.Yes);
                        break;

                    case (Keys.X): // CTRL-X -> cut
                        if (e.CtrlKey) await this.actions.AktionCutToClipboard(SetUndoSnapshotOptions.Yes);
                        break;

                    case (Keys.Z): //CTRL-Z -> UnDo
                    case (Keys.Y): //CTRL-Y -> ReDo (not implemented yet)
                        if (e.CtrlKey)
                        {
                            if (e.Content == "z") //Y and Z are sometimes swapped by keyboard localization (e.G. german keyboard) 😬
                            {
                                await this.actions.Undo();
                            }
                            if (e.Content == "y")
                            {
                                // ReDo (not implemented yet)
                            }
                        }
                        break;

                    // >>>> cursor keys
                    case Keys.Left: // move cursor to left
                        if (e.ShiftKey)
                        {
                            await CursorPosMoveHelper.MoveLeft(this.editorState.CursorRaw.EndPos, this.editorState.RootNode, this.xmlRules);
                        }
                        else
                        {
                            dummy = this.editorState.CursorRaw.StartPos.Clone();
                            await CursorPosMoveHelper.MoveLeft(dummy, this.editorState.RootNode, this.xmlRules);
                            await this.editorState.CursorRaw.SetBothPositionsAndFireChangedEventIfChanged(dummy.ActualNode, dummy.PosOnNode, dummy.PosInTextNode);
                        }
                        useKeyContent = false;
                        break;

                    case Keys.Right: // move cursor to right
                        if (e.ShiftKey)
                        {
                            await CursorPosMoveHelper.MoveRight(this.editorState.CursorRaw.EndPos, this.editorState.RootNode, this.xmlRules);
                        }
                        else
                        {
                            dummy = this.editorState.CursorRaw.StartPos.Clone();
                            await CursorPosMoveHelper.MoveRight(dummy, this.editorState.RootNode, this.xmlRules);
                            await this.editorState.CursorRaw.SetBothPositionsAndFireChangedEventIfChanged(dummy.ActualNode, dummy.PosOnNode, dummy.PosInTextNode);
                        }
                        useKeyContent = false;
                        break;

                    case Keys.Up:
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

                    case Keys.Tab:  // Tab jumps to the next tag
                    case Keys.Down: // down too
                        System.Xml.XmlNode node = this.editorState.CursorRaw.StartPos.ActualNode;
                        var jumpToNode = this.HandleTabKeypress(node);
                        if (jumpToNode != null)
                        {
                            if (jumpToNode.NodeType == XmlNodeType.Text)
                            {
                                await this.editorState.CursorRaw.SetBothPositionsAndFireChangedEventIfChanged(jumpToNode, XmlCursorPositions.CursorInsideTextNode);
                            } else
                            {
                                await this.editorState.CursorRaw.SetBothPositionsAndFireChangedEventIfChanged(jumpToNode, XmlCursorPositions.CursorInsideTheEmptyNode);
                            }
                            
                        }
                        useKeyContent = false;
                        break;

                    case Keys.Back:
                        if (this.editorState.CursorRaw.IsSomethingSelected)
                        {
                            await this.actions.ActionDelete(SetUndoSnapshotOptions.Yes);
                        }
                        else
                        {
                            await this.actions.ActionDeleteNodeOrCharInFrontOfCursorPos(this.editorState.CursorRaw.StartPos, SetUndoSnapshotOptions.Yes);
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
                        useKeyContent = true;
                        break;

                    case Keys.undefined:
                    default:
                        break;
                }

                if (useKeyContent)
                {
                    await this.actions.ActionInsertTextAtCursorPos(e.Content, SetUndoSnapshotOptions.Yes);
                }
            }
        }

        private XmlNode HandleTabKeypress(XmlNode node)
        {
            if (node.FirstChild != null) return node.FirstChild;
            if (node.NextSibling != null) return node.NextSibling;
            if (node.ParentNode != null)
            {
                if (node.ParentNode.NextSibling != null)
                {
                    if (node.ParentNode.NextSibling.FirstChild != null) return node.ParentNode.NextSibling.FirstChild;
                    return node.ParentNode.NextSibling;
                }

                if (node.ParentNode.ParentNode.NextSibling != null)
                {
                    if (node.ParentNode.ParentNode.NextSibling.FirstChild != null) return node.ParentNode.ParentNode.NextSibling.FirstChild;
                    return node.ParentNode.ParentNode.NextSibling;
                }
            }
            return null;
        }
    }
}

