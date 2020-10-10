using de.springwald.xml.cursor;
using de.springwald.xml.editor.nativeplatform;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.editor.editor
{
    public class EditorStatus : IDisposable
    {

        private INativePlatform nativePlatform;

        internal bool HasFocus => this.nativePlatform?.ControlElement.Focused == true;

        public XMLUndoHandler UndoHandler { get; internal set; }

        /// <summary>
        /// Is the current xml document treated as read-only?
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// The set of rules on which the XML processing is based
        /// </summary>
        public de.springwald.xml.XMLRegelwerk Regelwerk { get; }

        /// <summary>
        /// Gibt an, ob etwas für den Editor in der Zwischenablage ist
        /// </summary>
        public virtual bool IstEtwasInZwischenablage
        {
            get { return this.nativePlatform.Clipboard.ContainsText; }
        }

        /// <summary>
        /// Gibt an, ob im Editor etwas selektiert ist
        /// </summary>
        public virtual bool IstEtwasSelektiert
        {
            get { return this.CursorOptimiert.IstEtwasSelektiert; }
        }

        /// <summary>
        /// This is the topmost node to edit. You must not edit higher, even if there are parents in the DOM
        /// </summary>
        public System.Xml.XmlNode RootNode { get; internal set; }

        internal XMLElement RootElement { get; set; }

        public EditorStatus(INativePlatform nativePlatform, XMLRegelwerk regelwerk)
        {
            this.nativePlatform = nativePlatform;
            this.Regelwerk = regelwerk;
            this.CursorRoh = new XMLCursor();

        }

        public void Dispose()
        {
        }

     

   

        /// <summary>
        /// Gibt an, ob der Rootnode selektiert ist 
        /// </summary>
        public bool IstRootNodeSelektiert
        {
            get
            {
                if (this.IstEtwasSelektiert) // Überhaupt was selektiert
                {
                    XMLCursorPos startpos = CursorOptimiert.StartPos;
                    if (startpos.AktNode == RootNode) // Der Rootnode ist im Cursor
                    {
                        switch (startpos.PosAmNode)
                        {
                            case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                            case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                                return true; // Das Rootnode ist selektiert
                        }
                    }
                }
                return false; // Der Rootnode ist nicht selektiert
            }
        }

        internal async Task FireContentChangedEvent()
        {
            await this.ContentChangedEvent.Trigger(EventArgs.Empty);
        }

        public XmlAsyncEvent<EventArgs> ContentChangedEvent = new XmlAsyncEvent<EventArgs>();

        /// <summary>
        /// Dies ist die CursorPosition, optmimiert darauf, dass die StartPos auch vor der EndPos liegt
        /// </summary>
        public de.springwald.xml.cursor.XMLCursor CursorOptimiert
        {
            get
            {
                XMLCursor cursor = this.CursorRoh.Clone();
                cursor.SelektionOptimieren().Wait();
                return cursor;
            }
        }

        /// <summary>
        /// Dort befindet sich der der Cursor aktuell innerhalb des XML-Dokumentes
        /// </summary>
        public de.springwald.xml.cursor.XMLCursor CursorRoh { get; }




        /// <summary>
        /// Das Name des nächstemöglichen UndoSchrittes
        /// </summary>
        public string UndoSchrittName
        {
            get
            {
                if (UndoMoeglich)
                {
                    return this.UndoHandler.NextUndoSnapshotName;
                }
                else
                {
                    return ResReader.Reader.GetString("KeinUndoSchrittVerfuegbar");
                }
            }
        }

        /// <summary>
        /// Ist nun ein Undo möglich?
        /// </summary>
        public bool UndoMoeglich
        {
            get
            {
                if (this.UndoHandler == null)
                {
                    return false;
                }
                else
                {
                    return this.UndoHandler.UndoMoeglich;
                }
            }
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
