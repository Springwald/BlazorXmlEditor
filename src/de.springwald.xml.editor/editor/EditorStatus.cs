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

namespace de.springwald.xml.editor.editor
{
    public class EditorStatus : IDisposable
    {
        private INativePlatform nativePlatform;

        /// <summary>
        /// This is the topmost node to edit. You must not edit higher, even if there are parents in the DOM
        /// </summary>
        public System.Xml.XmlNode RootNode { get; internal set; }

        internal XMLElement RootElement { get; set; }

        /// <summary>
        /// The set of rules on which the XML processing is based
        /// </summary>
        public XMLRegelwerk Regelwerk { get; }

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

        internal bool HasFocus => this.nativePlatform?.ControlElement.Focused == true;

        /// <summary>
        /// Indicates whether something is on the clipboard for the editor
        /// </summary>
        public bool IstEtwasInZwischenablage => this.nativePlatform.Clipboard.ContainsText;

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
                    if (startpos.AktNode == RootNode) // The root node is in the cursor
                    {
                        switch (startpos.PosAmNode)
                        {
                            case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                            case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                                return true; // The root node is selected
                        }
                    }
                }
                return false; // The root node is not selected
            }
        }

        public XmlAsyncEvent<EventArgs> ContentChangedEvent = new XmlAsyncEvent<EventArgs>();

        public EditorStatus(INativePlatform nativePlatform, XMLRegelwerk regelwerk)
        {
            this.nativePlatform = nativePlatform;
            this.Regelwerk = regelwerk;
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
                    await this.CursorRoh.StartPos.CursorSetzenMitChangeEventWennGeaendert(c.StartPos.AktNode, c.StartPos.PosAmNode, c.StartPos.PosImTextnode);
                    await this.CursorRoh.EndPos.CursorSetzenMitChangeEventWennGeaendert(c.EndPos.AktNode, c.EndPos.PosAmNode, c.EndPos.PosImTextnode);
                }
                await this.FireContentChangedEvent();
            }
        }
    }
}
