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
    public class XMLUndoHandler : IDisposable
    {
        private bool disposed = false;

        /// <summary>
        /// Where are we in the Undo steps right now? With Undo it goes backwards, with Redo forward
        /// </summary>
        private int pos = 0;

       
        private XmlDocument dokument;
        private List<XMLUndoSchritt> undoSchritte;

        private bool interneVeraenderungLaeuft = false;

        /// <summary>
        /// Calculates the previous snapshot pos before the current pos, if available
        /// </summary>
        private int VorherigeSnapshotPos
        {
            get
            {
                int lauf = pos;
                do
                {
                    lauf--; // go to previous step
                } while ((lauf > 0) && (!undoSchritte[lauf].IstSnapshot)); // Until the next snapshot
                return lauf;
            }
        }

        public XmlNode RootNode { get; }

        /// <summary>
        /// Are Undo steps currently available?
        /// </summary>
        public bool UndoMoeglich => this.pos > 0;

        /// <summary>
        /// The name of the next undo step (if a name was assigned via snapshot)
        /// </summary>
        public string NextUndoSnapshotName
        {
            get
            {
                if (UndoMoeglich)
                {
                    return String.Format(
                        ResReader.Reader.GetString("NameUndoSchritt"),
                        undoSchritte[VorherigeSnapshotPos].SnapShotName);
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
		public XMLUndoHandler(System.Xml.XmlNode rootNode)
        {
            this.undoSchritte = new List<XMLUndoSchritt>();
            this.undoSchritte.Add(new XMLUndoSchritt()); // Insert the basic step for snapshot and cursor; it does not have undo data

            this.RootNode = rootNode;
            this.dokument = this.RootNode.OwnerDocument;

            // Hook into the chain of changes in the DOM
            this.dokument.NodeChanging += new XmlNodeChangedEventHandler(this.dokument_NodeChanging);
            this.dokument.NodeInserted += new XmlNodeChangedEventHandler(this.dokument_NodeInserted);
            this.dokument.NodeRemoving += new XmlNodeChangedEventHandler(this.dokument_NodeRemoving);
        }

        public void Dispose()
        {
            if (this.disposed) return;
            this.disposed = true;
            this.dokument.NodeChanging -= new XmlNodeChangedEventHandler(this.dokument_NodeChanging);
            this.dokument.NodeInserted -= new XmlNodeChangedEventHandler(this.dokument_NodeInserted);
            this.dokument.NodeRemoving -= new XmlNodeChangedEventHandler(this.dokument_NodeRemoving);
        }

        public void SnapshotSetzen(string snapShotName, XMLCursor cursor)
        {
            this.undoSchritte[pos].SnapShotName = snapShotName;
            this.undoSchritte[pos].CursorVorher = cursor;
        }

        /// <summary>
        /// Undo the last step 
        /// </summary>
        /// <returns>The new cursor after successful undo</returns>
        public XMLCursor Undo()
        {
            if (this.UndoMoeglich)
            {
                this.interneVeraenderungLaeuft = true;
                do
                {
                    this.undoSchritte[this.pos].UnDo();  // Undo this step
                    this.pos--; // go to previous step
                } while ((this.pos != 0) && (!this.undoSchritte[pos].IstSnapshot)); // Until the next snapshot

                this.interneVeraenderungLaeuft = false;
                return this.undoSchritte[this.pos].CursorVorher; // Return cursor from previous step for restore
            }
            else
            {
                return null;
            }
        }

        private void dokument_NodeRemoving(object sender, System.Xml.XmlNodeChangedEventArgs e)
        {
            if (this.interneVeraenderungLaeuft) return;
            if (e.Node is XmlAttribute)
            {
                // The removed node was an attribute
                this.NeuenUndoSchrittAnhaengen(new XMLUndoSchrittAttributRemoved((XmlAttribute)e.Node));
            }
            else
            {
                // the remote node was not an attribute
                this.NeuenUndoSchrittAnhaengen(new XMLUndoSchrittNodeRemoved(e.Node));
            }
        }

        private void dokument_NodeChanging(object sender, System.Xml.XmlNodeChangedEventArgs e)
        {
            if (interneVeraenderungLaeuft) return;
            this.NeuenUndoSchrittAnhaengen(new XMLUndoSchrittNodeChanged(e.Node, e.OldValue));
        }

        private void dokument_NodeInserted(object sender, System.Xml.XmlNodeChangedEventArgs e)
        {
            if (interneVeraenderungLaeuft) return;
            this.NeuenUndoSchrittAnhaengen(new XMLUndoSchrittNodeInserted(e.Node, e.NewParent));
        }

        /// <summary>
        /// Adds an Undo step
        /// </summary>
        private void NeuenUndoSchrittAnhaengen(XMLUndoSchritt neuerUndoSchritt)
        {
            // If there are still redo's following at the current position, then all redo's become obsolete
            var loeschen = new List<XMLUndoSchritt>();
            for (int i = pos + 1; i < this.undoSchritte.Count; i++)
            {
                loeschen.Add(undoSchritte[i]);
            }
            foreach (XMLUndoSchritt schritt in loeschen)
            {
                this.undoSchritte.Remove(schritt);
            }

            // Append the new step
            this.undoSchritte.Add(neuerUndoSchritt);
            this.pos++;
            if (this.pos != this.undoSchritte.Count - 1)
            {
                throw new Exception($"Undo-Pos should match with undoSteps.Count-1 Instead of this pos: {this.pos}, _undoSchritte.Count -1:{(this.undoSchritte.Count - 1)}");
            }
        }
    }
}
