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
using System.Xml;
using de.springwald.xml.cursor;
using de.springwald.xml.editor.cursor;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor
{
    public class EditorState : IDisposable
    {
        internal CursorBlink CursorBlink { get; }

        public async Task SetRootNode(XmlNode rootNode)
        {
            if (this.RootNode != rootNode)
            {
                this.RootNode = rootNode;
                await this.RootNodeChanged.Trigger(rootNode);
            }
        }
        public XmlAsyncEvent<XmlNode> RootNodeChanged { get; set; } = new XmlAsyncEvent<XmlNode>();

        /// <summary>
        /// This is the topmost node to edit. You must not edit higher, even if there are parents in the DOM
        /// </summary>
        public XmlNode RootNode { get; private set; }

        internal XmlElement RootElement { get; set; }

        ///// <summary>
        ///// The set of rules on which the XML processing is based
        ///// </summary>
        //public XMLRegelwerk Regelwerk { get; }

        /// <summary>
        /// Is the current xml document treated as read-only?
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// This is where the cursor is currently located within the XML document
        /// </summary>
        public XmlCursor CursorRaw { get; }

        /// <summary>
        /// This is the cursor position, optimizes that the StartPos is also before the EndPos
        /// </summary>
        public XmlCursor CursorOptimized
        {
            get
            {
                var cursor = this.CursorRaw.Clone();
                cursor.OptimizeSelection().Wait();
                return cursor;
            }
        }


        /// <summary>
        /// Indicates whether something is selected in the editor
        /// </summary>
        public bool IsSomethingSelected => this.CursorOptimized.IsSomethingSelected;

        public XmlUndoHandler UndoHandler { get; internal set; }

        /// <summary>
        ///  The name of the next possible undo step
        /// </summary>
        public string UndoStepName => this.UndoPossible ? this.UndoHandler.NextUndoSnapshotName : "no undo step available";

        /// <summary>
        /// Is an Undo now possible?
        /// </summary>
        public bool UndoPossible => this.UndoHandler == null ? false : this.UndoHandler.UndoPossible;

        /// <summary>
        /// Specifies whether the root node is selected 
        /// </summary>
        public bool IsRootNodeSelected
        {
            get
            {
                if (this.IsSomethingSelected) //  something is selected
                {
                    var startpos = CursorOptimized.StartPos;
                    if (startpos.ActualNode == RootNode) // The root node is in the cursor
                    {
                        switch (startpos.PosOnNode)
                        {
                            case XmlCursorPositions.CursorOnNodeStartTag:
                            case XmlCursorPositions.CursorOnNodeEndTag:
                                return true; // The root node is selected
                        }
                    }
                }
                return false; // The root node is not selected
            }
        }

        public XmlAsyncEvent<EventArgs> ContentChangedEvent = new XmlAsyncEvent<EventArgs>();

        public EditorState()
        {
            this.CursorRaw = new XmlCursor();
            this.CursorBlink = new CursorBlink();
          
        }

        public void Dispose()
        {
            this.CursorBlink.Dispose();
        }

        internal async Task FireContentChangedEvent()
        {
            await this.ContentChangedEvent.Trigger(EventArgs.Empty);
        }

        public async Task UnDo()
        {
            if (this.UndoHandler == null)
            {
                throw new ApplicationException("No Undo-Handler attached, but Undo invoked!");
            }
            else
            {
                XmlCursor c = this.UndoHandler.Undo();
                if (c != null) // If a CursorPos was stored for this undo step
                {

                    await this.CursorRaw.SetPositions(
                        c.StartPos.ActualNode, c.StartPos.PosOnNode, c.StartPos.PosInTextNode,
                        c.EndPos.ActualNode, c.EndPos.PosOnNode, c.EndPos.PosInTextNode,
                         throwChangedEventWhenValuesChanged: true);
                }
                await this.FireContentChangedEvent();
            }
        }
    }
}
