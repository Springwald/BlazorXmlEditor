// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Xml;
using de.springwald.xml.cursor;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Allows logging of undo steps and execution of undo and redo
    /// </summary>
    public class XmlUndoHandler : IDisposable
    {
        private bool disposed = false;

        /// <summary>
        /// Where are we in the Undo steps right now? With Undo it goes backwards, with Redo forward
        /// </summary>
        private int pos = 0;
       
        private XmlDocument document;
        private List<XmlUndoStep> undoSteps = new List<XmlUndoStep>();
        private bool working = false;

        /// <summary>
        /// Calculates the previous snapshot pos before the current pos, if available
        /// </summary>
        private int PreviousSnapshotPos
        {
            get
            {
                int run = pos;
                do
                {
                    run--; // go to previous step
                } while ((run > 0) && (!undoSteps[run].IsSnapshot)); // Until the next snapshot
                return run;
            }
        }

        public XmlNode RootNode { get; }

        /// <summary>
        /// Are Undo steps currently available?
        /// </summary>
        public bool UndoPossible => this.pos > 0;

        /// <summary>
        /// The name of the next undo step (if a name was assigned via snapshot)
        /// </summary>
        public string NextUndoSnapshotName 
        {
            get
            {
                if (this.UndoPossible)
                {
                    return String.Format(
                        ResReader.Reader.GetString("NameUndoSchritt"),
                        undoSteps[PreviousSnapshotPos].SnapShotName);
                }
                else
                {
                    return ResReader.Reader.GetString("KeinUndoSchrittVerfuegbar");
                }
            }
        }

        /// <summary>
        ///  Creates an XMLUndo handler that logs all changes starting from the specified root node
        /// </summary>
		public XmlUndoHandler(XmlNode rootNode)
        {
            this.undoSteps.Add(new XmlUndoStep()); // Insert the basic step for snapshot and cursor; it does not have undo data
            this.RootNode = rootNode;
            this.document = this.RootNode.OwnerDocument;

            // Hook into the chain of changes in the DOM
            this.document.NodeChanging += new XmlNodeChangedEventHandler(this.dokument_NodeChanging);
            this.document.NodeInserted += new XmlNodeChangedEventHandler(this.dokument_NodeInserted);
            this.document.NodeRemoving += new XmlNodeChangedEventHandler(this.document_NodeRemoving);
        }

        public void Dispose()
        {
            if (this.disposed) return;
            this.disposed = true;
            this.document.NodeChanging -= new XmlNodeChangedEventHandler(this.dokument_NodeChanging);
            this.document.NodeInserted -= new XmlNodeChangedEventHandler(this.dokument_NodeInserted);
            this.document.NodeRemoving -= new XmlNodeChangedEventHandler(this.document_NodeRemoving);
        }

        public void SetSnapshot(string snapShotName, XmlCursor cursor)
        {
            this.undoSteps[pos].SnapShotName = snapShotName;
            this.undoSteps[pos].CursorBefore = cursor;
        }

        /// <summary>
        /// Undo the last step 
        /// </summary>
        /// <returns>The new cursor after successful undo</returns>
        public XmlCursor Undo()
        {
            if (this.UndoPossible)
            {
                this.working = true;
                do
                {
                    this.undoSteps[this.pos].UnDo();  // Undo this step
                    this.pos--; // go to previous step
                } while ((this.pos != 0) && (!this.undoSteps[pos].IsSnapshot)); // Until the next snapshot

                this.working = false;
                return this.undoSteps[this.pos].CursorBefore; // Return cursor from previous step for restore
            }
            else
            {
                return null;
            }
        }

        private void document_NodeRemoving(object sender, XmlNodeChangedEventArgs e)
        {
            if (this.working) return;
            if (e.Node is XmlAttribute)
            {
                // The removed node was an attribute
                this.AddNewUndoStep(new XmlUndoStepAttributeRemoved((XmlAttribute)e.Node));
            }
            else
            {
                // the remote node was not an attribute
                this.AddNewUndoStep(new XmlUndoStepNodeRemoved(e.Node));
            }
        }

        private void dokument_NodeChanging(object sender, XmlNodeChangedEventArgs e)
        {
            if (this.working) return;
            this.AddNewUndoStep(new XmlUndoStepNodeChanged(e.Node, e.OldValue));
        }

        private void dokument_NodeInserted(object sender, XmlNodeChangedEventArgs e)
        {
            if (this.working) return;
            this.AddNewUndoStep(new XmlUndoStepNodeInserted(e.Node, e.NewParent));
        }

        /// <summary>
        /// Adds an Undo step
        /// </summary>
        private void AddNewUndoStep(XmlUndoStep newUndoStep)
        {
            // If there are still redo's following at the current position, then all redo's become obsolete
            var delete = new List<XmlUndoStep>();
            for (int i = pos + 1; i < this.undoSteps.Count; i++)
            {
                delete.Add(undoSteps[i]);
            }
            foreach (XmlUndoStep schritt in delete)
            {
                this.undoSteps.Remove(schritt);
            }

            // Append the new step
            this.undoSteps.Add(newUndoStep);
            this.pos++;
            if (this.pos != this.undoSteps.Count - 1)
            {
                throw new Exception($"Undo-Pos should match with undoSteps.Count-1 Instead of this pos: {this.pos}, _undoSchritte.Count -1:{(this.undoSteps.Count - 1)}");
            }
        }
    }
}
