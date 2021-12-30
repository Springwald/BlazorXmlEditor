// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.cursor;
using de.springwald.xml.rules;
using de.springwald.xml.tools;
using System;
using System.Threading.Tasks;
using System.Xml;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.cursor
{
    public enum MouseClickActions { MouseDown, MouseDownMove, MouseUp };

    public partial class XmlCursor : IDisposable
    {
        public XmlAsyncEvent<EventArgs> ChangedEvent { get; }

        public XmlCursorPos StartPos { get; }

        public XmlCursorPos EndPos { get; }

        public XmlCursor()
        {
            this.EndPos = new XmlCursorPos();
            this.StartPos = new XmlCursorPos();
            this.ChangedEvent = new XmlAsyncEvent<EventArgs>();
        }

        public void Dispose()
        {
        }

        public async Task SetPositions(XmlNode bothNodes, XmlCursorPositions posAtBothNodes, int textPosInBothNodes, bool throwChangedEventWhenValuesChanged)
        {
            await this.SetPositions(
                bothNodes, posAtBothNodes, textPosInBothNodes,
                bothNodes, posAtBothNodes, textPosInBothNodes,
                throwChangedEventWhenValuesChanged);
        }

        public async Task SetPositions(
            XmlNode startNode, XmlCursorPositions posAtStartNode, int textPosInStartNode,
            XmlNode endNode, XmlCursorPositions posAtEndNode, int textPosInEndNode, bool throwChangedEventWhenValuesChanged)
        {
            var changed = false;
            if (throwChangedEventWhenValuesChanged)
            {
                changed = (startNode != this.StartPos.ActualNode || posAtStartNode != this.StartPos.PosOnNode || textPosInStartNode != this.StartPos.PosInTextNode ||
                    endNode != this.EndPos.ActualNode || posAtEndNode != this.EndPos.PosOnNode || textPosInEndNode != this.EndPos.PosInTextNode);
            }
            this.StartPos.SetPos(startNode, posAtStartNode, textPosInStartNode);
            this.EndPos.SetPos(endNode, posAtEndNode, textPosInEndNode);
            if (changed) await ChangedEvent.Trigger(EventArgs.Empty);
        }

        public bool Equals(XmlCursor second)
        {
            return second != null && this.StartPos.Equals(second.StartPos) && this.EndPos.Equals(second.EndPos);
        }

        public XmlCursor Clone()
        {
            XmlCursor klon = new XmlCursor();
            klon.StartPos.SetPos(StartPos.ActualNode, StartPos.PosOnNode, StartPos.PosInTextNode);
            klon.EndPos.SetPos(EndPos.ActualNode, EndPos.PosOnNode, EndPos.PosInTextNode);
            return klon;
        }

        /// <summary>
        /// Triggers the Cursor-Changed-Event manually
        /// </summary>
        public async Task ForceChangedEvent()
        {
            await this.ChangedEvent.Trigger(EventArgs.Empty);
        }

        public void SetBotPositionsWithoutChangedEvent(XmlNode node, XmlCursorPositions posAmNode, int posImTextnode)
        {
            this.StartPos.SetPos(node, posAmNode, posImTextnode);
            this.EndPos.SetPos(node, posAmNode, posImTextnode);
        }

        /// <summary>
        /// Sets node and position simultaneously and thus triggers only one Changed-Event instead of two
        /// </summary>
        public async Task SetBothPositionsAndFireChangedEventIfChanged(XmlNode node, XmlCursorPositions posOnNode, int posInTextnode)
        {
            bool changed =
                node != StartPos.ActualNode || posOnNode != StartPos.PosOnNode || posInTextnode != StartPos.PosInTextNode ||
                node != EndPos.ActualNode || posOnNode != EndPos.PosOnNode || posInTextnode != EndPos.PosInTextNode;
            this.SetBotPositionsWithoutChangedEvent(node, posOnNode, posInTextnode);
            if (changed) await this.ChangedEvent.Trigger(EventArgs.Empty);
        }

        /// <summary>
        /// Sets node and position simultaneously and thus triggers only one Changed-Event instead of two
        /// </summary>
        public async Task SetBothPositionsAndFireChangedEventIfChanged(XmlNode node, XmlCursorPositions posAmNode)
        {
            await SetBothPositionsAndFireChangedEventIfChanged(node, posAmNode, 0);
        }

        /// <summary>
        /// Sets the cursor positions for corresponding mouse actions: For MouseDown StartAndEndpos, for Move and Up only the endpos
        /// </summary>
        public async Task SetCursorByMouseAction(XmlNode xmlNode, XmlCursorPositions cursorPos, int posInLine, MouseClickActions action)
        {
            switch (action)
            {
                case MouseClickActions.MouseDown:
                    // move the cursor to the new position
                    await SetPositions(xmlNode, cursorPos, posInLine, throwChangedEventWhenValuesChanged: true);
                    break;
                case MouseClickActions.MouseDownMove:
                case MouseClickActions.MouseUp:
                    // Set end of the select cursor
                    if (EndPos.SetPos(xmlNode, cursorPos, posInLine))
                    {
                        await this.ForceChangedEvent();
                    }
                    break;
            }
        }

        /// <summary>
        /// Sets the cursor positions for corresponding mouse actions: For MausDown StartUndEndpos, for Move and Up only the endpos
        /// </summary>
        public async Task SetCursorByMouseAction(XmlNode xmlNode, XmlCursorPositions cursorPos, MouseClickActions action)
        {
            await SetCursorByMouseAction(xmlNode, cursorPos, 0, action);
        }

        /// <summary>
        /// Optimizes the selected area 
        /// </summary>
        public async Task OptimizeSelection()
        {
            // Define exchange buffer variables
            XmlCursorPositions dummyPos;
            int dummyTextPos;

            if (StartPos.ActualNode == null) return;

            // 1. if the start pos is behind the end pos, then swap both
            if (StartPos.ActualNode == EndPos.ActualNode)  // Both nodes are equal
            {
                if (StartPos.PosOnNode > EndPos.PosOnNode) //  If StartPos is within a node behind EndPos
                {
                    // exchange both positions at the same node
                    dummyPos = StartPos.PosOnNode;
                    dummyTextPos = StartPos.PosInTextNode;
                    StartPos.SetPos(EndPos.ActualNode, EndPos.PosOnNode, EndPos.PosInTextNode);
                    EndPos.SetPos(EndPos.ActualNode, dummyPos, dummyTextPos);
                }
                else // StartPos was not behind Endpos
                {
                    // Is a text part within a text node selected ?
                    if ((StartPos.PosOnNode == XmlCursorPositions.CursorInsideTextNode) && (EndPos.PosOnNode == XmlCursorPositions.CursorInsideTextNode))
                    {  // A part of a text node is selected
                        if (StartPos.PosInTextNode > EndPos.PosInTextNode) // If the TextStartpos is behind the TextEndpos, then change
                        {   // Exchange text selection
                            dummyTextPos = StartPos.PosInTextNode;
                            StartPos.SetPos(StartPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, EndPos.PosInTextNode);
                            EndPos.SetPos(StartPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, dummyTextPos);
                        }
                    }
                }
            }
            else // Both nodes are not equal
            {
                // If the nodes are wrong in the order, then swap both
                if (ToolboxXml.Node1LaisBeforeNode2(EndPos.ActualNode, StartPos.ActualNode))
                {
                    var tempPos = this.StartPos.Clone();
                    this.StartPos.SetPos(this.EndPos.ActualNode, this.EndPos.PosOnNode, this.EndPos.PosInTextNode);
                    this.EndPos.SetPos(tempPos.ActualNode, tempPos.PosOnNode, tempPos.PosInTextNode);
                }

                // If the EndNode is in the StartNode, select the entire surrounding StartNode
                if (ToolboxXml.IsChild(EndPos.ActualNode, StartPos.ActualNode))
                {
                    await SetPositions(StartPos.ActualNode, XmlCursorPositions.CursorOnNodeStartTag, 0, throwChangedEventWhenValuesChanged: false);
                }

                // Find the first common parent of start and end and select the nodes in this height.
                // This leads to the fact that e.g. with LI elements and UL only the whole LI 
                // is selected when dragging the selection over several LI and not only parts of it
                if (StartPos.ActualNode.ParentNode != EndPos.ActualNode.ParentNode) // if start and end are not directly in the same parent
                {
                    // - first find out which is the deepest common parent of start and end node
                    XmlNode commonParent = XmlCursorSelectionHelper.DeepestCommonParent(StartPos.ActualNode, EndPos.ActualNode);
                    // - then upscale start- and end-node to before the parent
                    XmlNode nodeStart = StartPos.ActualNode;
                    while (nodeStart.ParentNode != commonParent) nodeStart = nodeStart.ParentNode;
                    XmlNode nodeEnde = EndPos.ActualNode;
                    while (nodeEnde.ParentNode != commonParent) nodeEnde = nodeEnde.ParentNode;
                    // - finally show the new start and end nodes  
                    StartPos.SetPos(nodeStart, XmlCursorPositions.CursorOnNodeStartTag);
                    EndPos.SetPos(nodeEnde, XmlCursorPositions.CursorOnNodeStartTag);
                }
            }
        }

        /// <summary>
        /// Are characters or nodes enclosed by this cursor?
        /// </summary>
        /// <remarks>
        /// Either a single node is selected by the StartPos, or the selected ranges lie between StartPos and EndPos
        /// </remarks>
        public bool IsSomethingSelected
        {
            get
            {
                // If no cursor is set, then nothing is selected
                if (this.StartPos.ActualNode == null) return false;

                if ((this.StartPos.PosOnNode == XmlCursorPositions.CursorOnNodeStartTag) ||
                    (this.StartPos.PosOnNode == XmlCursorPositions.CursorOnNodeEndTag))
                {
                    return true; // at least one single node is directly selected
                }
                else
                {
                    if (this.StartPos.Equals(this.EndPos))
                    {
                        return false; // obviously the cursor is just a line in the middle without having selected anything
                    }
                    else
                    {
                        return true; // Start and end pos are different, so there should be something in between
                    }
                }
            }
        }
    }
}
