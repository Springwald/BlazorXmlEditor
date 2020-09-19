using de.springwald.xml.cursor;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Ein einzelner Schritt in der Undo-Historie 
    /// </summary>
    /// <remarks>
    /// (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    public class XMLUndoSchritt
    {
        protected string _snapshotName;             //  Wenn dieser Schritt ein Snapshot ist, dann steht hier der Name des Snapshots
        protected XMLCursor _cursorVorher;          // Cursor vor der Änderung

        /// <summary>
        /// Ist dieser Schritt ein benannter Snapshot?
        /// </summary>
        public bool IstSnapshot
        {
            get { return ((_snapshotName != null) && (_snapshotName != "")); }
        }

        /// <summary>
        /// Wenn dieser Schritt ein Snapshot ist, dann steht hier der Name des Snapshots
        /// </summary>
        public string SnapShotName
        {
            get { return _snapshotName; }
            set { _snapshotName = value; }
        }

        /// <summary>
        /// Cursor vor der Änderung
        /// </summary>
        public XMLCursor CursorVorher
        {
            set { _cursorVorher = value.Clone(); }
            get { return _cursorVorher; }
        }

        public XMLUndoSchritt()
        {
            _snapshotName = null;
        }

        /// <summary>
        /// Macht diesen Undo-Schritt rückgängig
        /// </summary>
        public virtual void UnDo()
        {
        }
    }
}
