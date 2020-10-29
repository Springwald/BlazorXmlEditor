﻿// A platform indepentend tag-view-style graphical xml editor
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
using de.springwald.xml.editor.nativeplatform;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.editor
{
    public class EditorStatus : IDisposable
    {
        // private INativePlatform nativePlatform;

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

        internal XMLElement RootElement { get; set; }

        ///// <summary>
        ///// The set of rules on which the XML processing is based
        ///// </summary>
        //public XMLRegelwerk Regelwerk { get; }

        /// <summary>
        /// Is the current xml document treated as read-only?
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Dort befindet sich der der Cursor aktuell innerhalb des XML-Dokumentes
        /// </summary>
        public XMLCursor CursorRoh { get; }

        /// <summary>
        /// Dies ist die CursorPosition, optmimiert darauf, dass die StartPos auch vor der EndPos liegt
        /// </summary>
        public XMLCursor CursorOptimiert
        {
            get
            {
                var cursor = this.CursorRoh.Clone();
                cursor.SelektionOptimieren().Wait();
                return cursor;
            }
        }

        ///// <summary>
        ///// Indicates whether something is on the clipboard for the editor
        ///// </summary>
        //public bool IstEtwasInZwischenablage => this.nativePlatform.Clipboard.ContainsText;

        /// <summary>
        /// Indicates whether something is selected in the editor
        /// </summary>
        public bool IstEtwasSelektiert => this.CursorOptimiert.IstEtwasSelektiert;

        public XMLUndoHandler UndoHandler { get; internal set; }

        /// <summary>
        /// Das Name des nächstemöglichen UndoSchrittes
        /// </summary>
        public string UndoSchrittName => this.UndoMoeglich ? this.UndoHandler.NextUndoSnapshotName : ResReader.Reader.GetString("KeinUndoSchrittVerfuegbar");

        /// <summary>
        /// Ist nun ein Undo möglich?
        /// </summary>
        public bool UndoMoeglich => this.UndoHandler == null ? false : this.UndoHandler.UndoMoeglich;

        /// <summary>
        /// Specifies whether the root node is selected 
        /// </summary>
        public bool IstRootNodeSelektiert
        {
            get
            {
                if (this.IstEtwasSelektiert) //  Anything selected
                {
                    var startpos = CursorOptimiert.StartPos;
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

        public EditorStatus()
        {
            this.CursorRoh = new XMLCursor();
        }

        public void Dispose()
        {
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
                XMLCursor c = this.UndoHandler.Undo();
                if (c != null) // Wenn für diesen UndoSchritt eine CursorPos gespeichert war
                {

                    await this.CursorRoh.SetPositions(
                        c.StartPos.ActualNode, c.StartPos.PosOnNode, c.StartPos.PosInTextNode,
                        c.EndPos.ActualNode, c.EndPos.PosOnNode, c.EndPos.PosInTextNode,
                         throwChangedEventWhenValuesChanged: true);
                }
                await this.FireContentChangedEvent();
            }
        }
    }
}
