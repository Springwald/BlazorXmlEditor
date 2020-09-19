using de.springwald.toolbox;
using de.springwald.xml.cursor;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    public partial class XMLEditor
    {
        private XMLUndoHandler _undoHandler;

        /// <summary>
        /// Das Name des nächstemöglichen UndoSchrittes
        /// </summary>
        public string UndoSchrittName
        {
            get
            {
                if (UndoMoeglich)
                {
                    return _undoHandler.NextUndoSnapshotName;
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
                if (_undoHandler == null)
                {
                    return false;
                }
                else
                {
                    return _undoHandler.UndoMoeglich;
                }
            }
        }


        public async Task UnDo()
        {
            if (_undoHandler == null)
            {
                throw new ApplicationException("No Undo-Handler attached, but Undo invoked!");
            }
            else
            {
                XMLCursor c = _undoHandler.Undo();
                if (c != null) // Wenn für diesen UndoSchritt eine CursorPos gespeichert war
                {
                    await _cursor.StartPos.CursorSetzenMitChangeEventWennGeaendert(c.StartPos.AktNode, c.StartPos.PosAmNode, c.StartPos.PosImTextnode);
                    await _cursor.EndPos.CursorSetzenMitChangeEventWennGeaendert(c.EndPos.AktNode, c.EndPos.PosAmNode, c.EndPos.PosImTextnode);
                }
                await ContentChanged();
            }
        }
    }
}
