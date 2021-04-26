// A platform independent tag-view-style graphical XML editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2021 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License


using System;
using System.Xml;
using System.Threading.Tasks;
using de.springwald.xml.cursor;
using de.springwald.xml.editor.cursor;
using de.springwald.xml.editor.nativeplatform;
using de.springwald.xml.rules;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor.actions
{
    public class EditorActions
    {
        private XmlRules xmlRules => this.editorContext.XmlRules;
        private INativePlatform nativePlatform => this.editorContext.NativePlatform;
        private EditorState editorState => this.editorContext.EditorState;

        private EditorContext editorContext;

        public enum SetUndoSnapshotOptions { Yes, nein };

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

        public virtual async Task<bool> ActionPasteFromClipboard(SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!ActionsAllowed) return false; // If no actions are allowed at all, cancel
            string text = string.Empty;

            try
            {
                if (await this.nativePlatform.Clipboard.ContainsText()) // if text is in the clipboard
                {
                    XmlCursorPos startPos;
                    XmlCursorPos endPos;

                    if (this.editorState.IsRootNodeSelected) // The root node is selected and should therefore be replaced by the clipboard content
                    {
                        return await ActionReplaceRootNodeByClipboardContent(setUnDoSnapshot);
                    }
                    else // something other than the root node should be replaced
                    {
                        // First delete any selection
                        if (this.editorState.IsSomethingSelected) // Something is selected
                        {
                            if (await ActionDelete(SetUndoSnapshotOptions.nein))
                            {
                                startPos = this.editorState.CursorRaw.StartPos;
                            }
                            else // Failed to delete the selection
                            {
                                return false;
                            }
                        }
                        else // nothing selected
                        {
                            startPos = this.editorState.CursorOptimized.StartPos;
                        }
                    }

                    if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
                    {
                        this.editorState.UndoHandler.SetSnapshot("insert", this.editorState.CursorRaw);
                    }

                    // Wrap the text with an enclosing virtual tag
                    text = await this.nativePlatform.Clipboard.GetText();

                    // clean white-spaces
                    text = text.Replace("\r\n", " ");
                    text = text.Replace("\n\r", " ");
                    text = text.Replace("\r", " ");
                    text = text.Replace("\n", " ");
                    text = text.Replace("\t", " ");

                    string content = String.Format("<paste>{0}</paste>", text);

                    // create the XML reader
                    using (var reader = new XmlTextReader(content, XmlNodeType.Element, null))
                    {
                        reader.MoveToContent(); // Move to the cd element node.

                        // Creating the Virtual Paste Node
                        var pasteNode = this.editorState.RootNode.OwnerDocument.ReadNode(reader);

                        // Now insert all Children of the virtual Paste-Node one after the other at the CursorPos
                        endPos = startPos.Clone(); // Before inserting start- and endPos are equal
                        foreach (XmlNode node in pasteNode.ChildNodes)
                        {
                            if (node is XmlText) // Insert a text
                            {
                                var pasteResult = InsertAtCursorPosHelper.InsertText(endPos, node.Clone().Value, this.xmlRules);
                                if (pasteResult.ReplaceNode != null)
                                {
                                    // Text could not be inserted because a node input was converted from text input.
                                    // Example: In the AIML template, * is pressed, and a <star> is inserted there instead
                                    InsertAtCursorPosHelper.InsertXmlNode(endPos, pasteResult.ReplaceNode.Clone(), this.xmlRules, true);
                                }
                            }
                            else //  Insert a Node
                            {
                                InsertAtCursorPosHelper.InsertXmlNode(endPos, node.Clone(), this.xmlRules, true);
                            }
                        }

                        switch (this.editorState.CursorRaw.EndPos.PosOnNode)
                        {
                            case XmlCursorPositions.CursorInsideTextNode:
                            case XmlCursorPositions.CursorInFrontOfNode:
                                // The end of the insert is a text or before the node
                                await this.editorState.CursorRaw.SetPositions(endPos.ActualNode, endPos.PosOnNode, endPos.PosInTextNode, throwChangedEventWhenValuesChanged: false);
                                break;
                            default:
                                // End of the insert is behind the last inserted node
                                await this.editorState.CursorRaw.SetPositions(endPos.ActualNode, XmlCursorPositions.CursorBehindTheNode, textPosInBothNodes: 0, throwChangedEventWhenValuesChanged: false);
                                break;
                        }
                        await this.editorState.FireContentChangedEvent(needToSetFocusOnEditorWhenLost: false, forceFullRepaint: false);
                        return true;
                    }
                }
                else //  No text on the clipboard
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                this.nativePlatform.LogError(
                    String.Format("AktionPasteFromClipboard:Error for insert text '{0}':{1}", text, e.Message));
                return false;
            }
        }

        /// <summary>
        /// With the Enter key you can e.g. try to put the same tag behind the current one again, or split the current one at the current position into two equal tags
        /// </summary>
        internal void ActionEnterPressed()
        {
#warning To Do!
        }

        internal async Task Undo()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Replaces the root node of the editor with the content of the clipboard
        /// </summary>
        private async Task<bool> ActionReplaceRootNodeByClipboardContent(SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!ActionsAllowed) return false;

            try
            {
                var text = await this.nativePlatform.Clipboard.GetText();
                using (var reader = new XmlTextReader(text, System.Xml.XmlNodeType.Element, null))
                {
                    reader.MoveToContent(); // Move to the cd element node.

                    // Create a new root node from the clipboard, from which we can then steal the children
                    var pasteNode = this.editorState.RootNode.OwnerDocument.ReadNode(reader);

                    if (pasteNode.Name != this.editorState.RootNode.Name)
                    {
                        // The node in the clipboard and the current root node do not have the same name
                        return false; // not allowed
                    }

                    if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
                    {
                        this.editorState.UndoHandler.SetSnapshot("replace root node by clipboard content", this.editorState.CursorRaw);
                    }

                    // Delete all children + attributes of the previous root node
                    this.editorState.RootNode.RemoveAll();

                    // Copy all attributes of the clipboard root node to the correct root node
                    while (pasteNode.Attributes.Count > 0)
                    {
                        var attrib = pasteNode.Attributes.Remove(pasteNode.Attributes[0]); // Remove from clipboard root node
                        this.editorState.RootNode.Attributes.Append(attrib); // put to the right root node
                    }

                    var startPos = new XmlCursorPos();
                    startPos.SetPos(this.editorState.RootNode, XmlCursorPositions.CursorInsideTheEmptyNode);
                    XmlCursorPos endPos;

                    // Now insert all children of the virtual root node one after the other at the CursorPos
                    endPos = startPos.Clone(); // Before inserting start- and endPos are equal
                    while (pasteNode.ChildNodes.Count > 0)
                    {
                        var child = pasteNode.RemoveChild(pasteNode.FirstChild);
                        this.editorState.RootNode.AppendChild(child);
                    }
                    await this.editorState.CursorRaw.SetPositions(this.editorState.RootNode, XmlCursorPositions.CursorOnNodeStartTag, 0, throwChangedEventWhenValuesChanged: false);
                    await this.editorState.FireContentChangedEvent(needToSetFocusOnEditorWhenLost: false, forceFullRepaint: false);
                    return true;
                }
            }
            catch (Exception e)
            {
                this.nativePlatform.LogError($"ActionReplaceRootNodeByClipboardContent: error for insert text 'text': {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Copies the current selection to the clipboard
        /// </summary>
        public virtual async Task<bool> ActionCopyToClipboard()
        {
            if (!this.ActionsAllowed) return false;

            var content = await XmlCursorSelectionHelper.GetSelectionAsString(this.editorState.CursorRaw);
            if (string.IsNullOrEmpty(content)) return false; //Nothing selected
            try
            {
                await this.nativePlatform.Clipboard.Clear();
                await this.nativePlatform.Clipboard.SetText(content); // Copy selection as text to clipboard
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Places the cursor on position1
        /// </summary>
        public virtual async Task<bool> ActionCursorOnPos1()
        {
            if (this.editorState.RootNode == null)
            {
                // TODO: notify error
                return false;
            }
            else
            {
                if (this.editorState.RootNode.FirstChild != null)
                {
                    // in front of the first child of the root node
                    await this.editorState.CursorRaw.SetPositions(this.editorState.RootNode.FirstChild, XmlCursorPositions.CursorInFrontOfNode, 0, throwChangedEventWhenValuesChanged: true);
                }
                else
                {
                    // in the empty root node
                    await this.editorState.CursorRaw.SetPositions(this.editorState.RootNode, XmlCursorPositions.CursorInsideTheEmptyNode, 0, throwChangedEventWhenValuesChanged: true);
                }
                return true;
            }
        }

        /// <summary>
        /// selects the complete content
        /// </summary>
        public virtual async Task<bool> ActionSelectAll()
        {
            // select the root node
            await this.editorState.CursorRaw.SetPositions(this.editorState.RootNode, XmlCursorPositions.CursorOnNodeStartTag, 0, throwChangedEventWhenValuesChanged: true);
            return true;
        }

        /// <summary>
        /// Cuts the current selection and moves it to the clipboard
        /// </summary>
        public virtual async Task<bool> AktionCutToClipboard(SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false;

            if (this.editorState.CursorOptimized.StartPos.ActualNode == this.editorState.RootNode)
            {
                // The root node cannot be cut
                return false;
            }

            // ok, the root node is not selected
            if (await ActionCopyToClipboard()) //  Copy to clipboard worked
            {
                if (await ActionDelete(SetUndoSnapshotOptions.Yes)) // Deleting the selection worked fine
                {
                    return true;
                }
                else // Failed to delete the selection
                {
                    return false;
                }
            }
            else // Copy to clipboard failed
            {
                return false;
            }
        }

        /// <summary>
        /// deletes the actual cursor selection
        /// </summary>
        /// <returns>true, if deleted successfully</returns>
        public virtual async Task<bool> ActionDelete(SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false;

            if (this.editorState.IsRootNodeSelected) return false; // The root node is to be deleted:  Not allowed

            if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
            {
                this.editorState.UndoHandler.SetSnapshot("delete", this.editorState.CursorRaw);
            }

            var optimized = this.editorState.CursorRaw;
            await optimized.OptimizeSelection();

            var deleteResult = await XmlCursorSelectionHelper.DeleteSelection(optimized);
            if (deleteResult.Success)
            {
                await this.editorState.CursorRaw.SetPositions(deleteResult.NewCursorPosAfterDelete.ActualNode, deleteResult.NewCursorPosAfterDelete.PosOnNode, deleteResult.NewCursorPosAfterDelete.PosInTextNode, throwChangedEventWhenValuesChanged: false);
                await this.editorState.FireContentChangedEvent(needToSetFocusOnEditorWhenLost: false, forceFullRepaint: false);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Inserts text at the specified cursor position
        /// </summary>
        public virtual async Task<bool> ActionInsertTextAtCursorPos(string insertText, SetUndoSnapshotOptions setUnDoSnapShot)
        {
            if (!this.ActionsAllowed) return false;

            if (setUnDoSnapShot == SetUndoSnapshotOptions.Yes)
            {
                var editorStatus = this.editorState;
                editorStatus.UndoHandler.SetSnapshot($"write text '{insertText}'", this.editorState.CursorRaw);
            }

            await InsertText(this.editorState.CursorRaw, insertText, this.xmlRules);
            await this.editorState.FireContentChangedEvent(needToSetFocusOnEditorWhenLost: false, forceFullRepaint: false);
            return true;
        }

        /// <summary>
        /// Sets the content of an attribute in a node
        /// </summary>
        public virtual async Task<bool> ActionSetAttributeValueInNode(XmlNode node, string attributName, string value, SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false;

            var xmlAttrib = node.Attributes[attributName];

            if (string.IsNullOrEmpty(value)) // No content: delete attribute, if available
            {
                if (xmlAttrib != null)  // Attribute exists -> delete
                {
                    if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
                    {
                        this.editorState.UndoHandler.SetSnapshot($"delete attribute '{attributName}' in '{node.Name}'", this.editorState.CursorRaw);
                    }
                    node.Attributes.Remove(xmlAttrib);
                }
            }
            else // Write content to attribute
            {
                if (xmlAttrib == null)  // Attribute does not exist yet -> create new
                {
                    if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
                    {
                        this.editorState.UndoHandler.SetSnapshot($"created attribute '{attributName}' value of node '{node.Name}' with value '{value}'", this.editorState.CursorRaw);
                    }
                    xmlAttrib = node.OwnerDocument.CreateAttribute(attributName);
                    node.Attributes.Append(xmlAttrib);
                    xmlAttrib.Value = value;
                }
                else
                {
                    if (xmlAttrib.Value != value)
                    {
                        if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
                        {
                            this.editorState.UndoHandler.SetSnapshot($"changed attribute '{attributName}' value of node '{node.Name}' to value '{value}'", this.editorState.CursorRaw);
                        }
                        xmlAttrib.Value = value;
                    }
                }
            }
            await this.editorState.FireContentChangedEvent(needToSetFocusOnEditorWhenLost: false, forceFullRepaint: false);
            return true;
        }

        /// <summary>
        /// Delete the node or character in front of the cursor
        /// </summary>
        public async Task<bool> ActionDeleteNodeOrCharInFrontOfCursorPos(XmlCursorPos position, SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false;

            // Move the cursor one position to the left
            var deleteArea = new XmlCursor();
            deleteArea.StartPos.SetPos(position.ActualNode, position.PosOnNode, position.PosInTextNode);
            var endPos = deleteArea.StartPos.Clone();
            await CursorPosMoveHelper.MoveLeft(endPos, this.editorState.RootNode, this.xmlRules);
            deleteArea.EndPos.SetPos(endPos.ActualNode, endPos.PosOnNode, endPos.PosInTextNode);
            await deleteArea.OptimizeSelection();

            if (deleteArea.StartPos.ActualNode == this.editorState.RootNode) return false; // You must not delete the root node

            if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
            {
                this.editorState.UndoHandler.SetSnapshot("delete", this.editorState.CursorRaw);
            }

            var deleteResult = await XmlCursorSelectionHelper.DeleteSelection(deleteArea);
            if (deleteResult.Success)
            {
                // After successful deletion the new CursorPos is retrieved here
                await this.editorState.CursorRaw.SetPositions(deleteResult.NewCursorPosAfterDelete.ActualNode, deleteResult.NewCursorPosAfterDelete.PosOnNode, deleteResult.NewCursorPosAfterDelete.PosInTextNode, throwChangedEventWhenValuesChanged: false);
                await this.editorState.FireContentChangedEvent(needToSetFocusOnEditorWhenLost: false, forceFullRepaint: false);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete the node or the character behind the cursor
        /// </summary>
        public async Task<bool> ActionDeleteNodeOrSignBehindCursorPos(XmlCursorPos position, SetUndoSnapshotOptions setUnDoSnapshot)
        {
            if (!this.ActionsAllowed) return false;

            if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
            {
                this.editorState.UndoHandler.SetSnapshot("delete", this.editorState.CursorRaw);
            }

            var deleteArea = new XmlCursor();
            deleteArea.StartPos.SetPos(position.ActualNode, position.PosOnNode, position.PosInTextNode);
            var endPos = deleteArea.StartPos.Clone();
            await CursorPosMoveHelper.MoveRight(endPos, this.editorState.RootNode, this.xmlRules);
            deleteArea.EndPos.SetPos(endPos.ActualNode, endPos.PosOnNode, endPos.PosInTextNode);
            await deleteArea.OptimizeSelection();

            if (deleteArea.StartPos.ActualNode == this.editorState.RootNode) return false; // You must not delete the rootnot

            var deleteResult = await XmlCursorSelectionHelper.DeleteSelection(deleteArea);
            if (deleteResult.Success)
            {
                // After successful deletion the new CursorPos is retrieved here
                await this.editorState.CursorRaw.SetPositions(deleteResult.NewCursorPosAfterDelete.ActualNode, deleteResult.NewCursorPosAfterDelete.PosOnNode, deleteResult.NewCursorPosAfterDelete.PosInTextNode, throwChangedEventWhenValuesChanged: false);
                await this.editorState.FireContentChangedEvent(needToSetFocusOnEditorWhenLost: false, forceFullRepaint: false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Inserts a new XML element at the current cursor position
        /// </summary>
        /// <param name="nodeName">Such a node should be created</param>
        public virtual async Task<XmlNode> ActionInsertNewElementAtActCursorPos(string nodeName, SetUndoSnapshotOptions setUnDoSnapshot, bool setNewCursorPosBehindNewInsertedNode)
        {
            if (!this.ActionsAllowed) return null;

            XmlNode node;

            if (string.IsNullOrEmpty(nodeName)) throw new ApplicationException("ActionInsertNewElementAnActCursorPos: entered no node name!");

            if (setUnDoSnapshot == SetUndoSnapshotOptions.Yes)
            {
                this.editorState.UndoHandler.SetSnapshot($"insert '{nodeName}' node", this.editorState.CursorRaw);
            }

            // create node
            if (nodeName == "#COMMENT")
            {
                node = this.editorState.RootNode.OwnerDocument.CreateComment("NEW COMMENT");
            }
            else
            {
                node = this.editorState.RootNode.OwnerDocument.CreateNode(XmlNodeType.Element, nodeName, null);
            }

            // Insert node at current CursorPos
            await XMLNodeEinfuegen(this.editorState.CursorRaw, node, this.xmlRules, setNewCursorPosBehindNewInsertedNode);
            await this.editorState.FireContentChangedEvent(needToSetFocusOnEditorWhenLost: true, forceFullRepaint: false);

            
            return node;
        }


        /// <summary>
        /// Inserts the specified text at the current cursor position, if possible
        /// </summary>
        private async Task InsertText(XmlCursor cursor, string text, XmlRules xmlRules)
        {
            XmlCursorPos insertPos;

            // If something is selected, then delete it first, because it will be replaced by the new text
            XmlCursor deleteArea = cursor.Clone();
            await deleteArea.OptimizeSelection();
            var deleteResult = await XmlCursorSelectionHelper.DeleteSelection(deleteArea);
            if (deleteResult.Success)
            {
                insertPos = deleteResult.NewCursorPosAfterDelete;
            }
            else
            {
                insertPos = cursor.StartPos.Clone();
            }

            // insert the specified text at the cursor position
            var replacementNode = InsertAtCursorPosHelper.InsertText(insertPos, text, xmlRules).ReplaceNode;
            if (replacementNode != null)
            {
                // Text could not be inserted because a node input was converted from text input. 
                // Example: In the AIML template, * is pressed, and a <star> is inserted there instead
                InsertAtCursorPosHelper.InsertXmlNode(insertPos, replacementNode, xmlRules, false);
            }

            // then the cursor is only one line behind the inserted text
            await cursor.SetPositions(insertPos.ActualNode, insertPos.PosOnNode, insertPos.PosInTextNode, throwChangedEventWhenValuesChanged: false);
        }

        /// <summary>
        /// FInserts the specified node at the current cursor position, if possible
        /// </summary>
        private async Task XMLNodeEinfuegen(XmlCursor cursor, XmlNode node, XmlRules xmlRules, bool setNewCursorPosBehindNewInsertedNode)
        {
            // If something is selected, then delete it first, because it will be replaced by the new text
            XmlCursor deleteArea = cursor.Clone();
            await deleteArea.OptimizeSelection();
            var deleteResult = await XmlCursorSelectionHelper.DeleteSelection(deleteArea);
            if (deleteResult.Success)
            {
                await cursor.SetPositions(deleteResult.NewCursorPosAfterDelete.ActualNode, deleteResult.NewCursorPosAfterDelete.PosOnNode, deleteResult.NewCursorPosAfterDelete.PosInTextNode, throwChangedEventWhenValuesChanged: false);
            }

            // insert the specified node at the cursor position
            if (InsertAtCursorPosHelper.InsertXmlNode(cursor.StartPos, node, xmlRules, setNewCursorPosBehindNewInsertedNode))
            {
                // then the cursor is only one line behind the inserted
                cursor.EndPos.SetPos(cursor.StartPos.ActualNode, cursor.StartPos.PosOnNode, cursor.StartPos.PosInTextNode);
            }
        }
    }
}
