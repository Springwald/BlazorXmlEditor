// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.cursor;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// A single step in the undo history 
    /// </summary>
    public  class XmlUndoStep
    {
        protected XmlCursor previousCursor;// Cursor vor der Änderung

        /// <summary>
        /// Is this step a named snapshot?
        /// </summary>
        public bool IsSnapshot => !string.IsNullOrEmpty(this.SnapShotName);

        /// <summary>
        /// If this step is a snapshot, then here is the name of the snapshot
        /// </summary>
        public string SnapShotName { get; set; }

        /// <summary>
        /// Cursor before the change
        /// </summary>
        public XmlCursor CursorBefore
        {
            set { previousCursor = value.Clone(); }
            get { return previousCursor; }
        }

        public XmlUndoStep()
        {
             this.SnapShotName = null;
        }

        /// <summary>
        /// Undoes this undo step
        /// </summary>
        public virtual void UnDo() { }
    }
}
