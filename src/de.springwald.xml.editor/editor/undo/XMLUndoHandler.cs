using System;
using System.Collections.Generic;
using System.Xml;
using System.Collections;
using System.Diagnostics;
using de.springwald.xml.cursor;
using de.springwald.toolbox;

namespace de.springwald.xml.editor
{
	/// <summary>
	/// Ermöglicht das Protokollieren von Undo-Schritten und das Ausführen von
	/// Undo und Redo
	/// </summary>
	/// <remarks>
	/// (C)2006 Daniel Springwald, Herne Germany
	/// Springwald Software  - www.springwald.de
	/// daniel@springwald.de -   0700-SPRINGWALD
	/// all rights reserved
	/// </remarks>
	public class XMLUndoHandler:IDisposable     
	{
        private bool _disposed = false;

        private int _pos=0;           // Wo sind wir gerade in den Undo-Schritten? Bei Undo gehts rückwärts, bei Redo vorwärts

        private XmlNode _rootNode;
        private XmlDocument _dokument;

        private List<XMLUndoSchritt> _undoSchritte;

        private bool _interneVeraenderungLaeuft = false;

        /// <summary>
        /// Ermittelt die vorherige Snapshotpos vor der aktuellen Pos, wenn vorhanden
        /// </summary>
        private int VorherigeSnapshotPos
        {
            get
            {
                int lauf = _pos;
                do
                {
                    lauf--;                      // zum vorherigen Schritt gehen
                } while ((lauf > 0) && (!_undoSchritte[lauf].IstSnapshot)); // Bis zum nächsten Snapshot
                return lauf;
            }
        }

        /// <summary>
        ///  Der Rootnode am welche alle Veränderungen protokolliert werden
        /// </summary>
        public System.Xml.XmlNode RootNode
        {
            get { return _rootNode; }
        }

        /// <summary>
        /// Sind aktuell Undo-Schritt verfügbar
        /// </summary>
        public bool UndoMoeglich
        {
            get { return (_pos > 0); }
        }

        /// <summary>
        /// Der Name des nächsten UndoSchrittes (wenn per Snapshot ein Name vergeben wurde)
        /// </summary>
        public string NextUndoSnapshotName
        {
            get
            {
                if (UndoMoeglich)
                {
                    return String.Format(
                        ResReader.Reader.GetString("NameUndoSchritt"),
                        _undoSchritte[VorherigeSnapshotPos].SnapShotName);
                }
                else
                {
                    return ResReader.Reader.GetString("KeinUndoSchrittVerfuegbar");
                } 
            }
        }

        /// Erzeugt einen XMLUndo-Handler, welcher alle Veränderungen ab dem angegebenen Root-Node protokolliert
        /// </summary>
        /// <param name="rootNode"></param>
		public XMLUndoHandler(System.Xml.XmlNode rootNode)
		{
            _undoSchritte = new List<XMLUndoSchritt>();
            _undoSchritte.Add(new XMLUndoSchritt()); // Den Grundschritt für Snapshot und Cursor einsetzen; Undo Daten hat er nicht

            _rootNode = rootNode;
            _dokument = _rootNode.OwnerDocument;

            // In die Kette der Veränderungen im DOM einhängen
            _dokument.NodeChanging += new System.Xml.XmlNodeChangedEventHandler(_dokument_NodeChanging);
            _dokument.NodeInserted += new System.Xml.XmlNodeChangedEventHandler(_dokument_NodeInserted);
            _dokument.NodeRemoving += new System.Xml.XmlNodeChangedEventHandler(_dokument_NodeRemoving);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="snapShotName"></param>
        public void SnapshotSetzen(string snapShotName, XMLCursor cursor)
        {
            _undoSchritte[_pos].SnapShotName = snapShotName; // Dem jetzt-Schritt den Snapshotnamen zuweisen
            _undoSchritte[_pos].CursorVorher = cursor; // Dem jetzt-Schritt den Cursor zuweisen
        }

        /// <summary>
        /// Den letzten Schritt rückgängig machen 
        /// </summary>
        /// <returns>Den neuen Cursor nach erfolgreichem Undo</returns>
        public XMLCursor Undo()
        {
            if (UndoMoeglich)
            {
                _interneVeraenderungLaeuft = true;

                do
                {
                    _undoSchritte[_pos].UnDo();  // Undo dieses Schrittes durchführen
                    _pos--;                      // zum vorherigen Schritt gehen
                } while ((_pos != 0) && (!_undoSchritte[_pos].IstSnapshot)); // Bis zum nächsten Snapshot

                _interneVeraenderungLaeuft = false;
                return _undoSchritte[_pos].CursorVorher; // Cursor von vorherigem Schritt für Wiederherstellung zurückgeben
            }
            else
            {
                return null;
            }
        }

        void _dokument_NodeRemoving(object sender, System.Xml.XmlNodeChangedEventArgs e)
        {
            if (_interneVeraenderungLaeuft) return;
            if (e.Node is XmlAttribute)
            {
                // Der entfernte Node war ein Attribut
                NeuenUndoSchrittAnhaengen(new XMLUndoSchrittAttributRemoved((XmlAttribute)e.Node));
            }
            else
            {
                // der entfernte Node war kein Attribut
                NeuenUndoSchrittAnhaengen(new XMLUndoSchrittNodeRemoved(e.Node));
            }
        }

        void _dokument_NodeChanging(object sender, System.Xml.XmlNodeChangedEventArgs e)
        {
            if (_interneVeraenderungLaeuft) return;
            NeuenUndoSchrittAnhaengen(new XMLUndoSchrittNodeChanged(e.Node,e.OldValue));
        }

        void _dokument_NodeInserted(object sender, System.Xml.XmlNodeChangedEventArgs e)
        {
            if (_interneVeraenderungLaeuft) return;
            NeuenUndoSchrittAnhaengen(new XMLUndoSchrittNodeInserted(e.Node, e.NewParent));
        }

        /// <summary>
        /// Den Undo-Handler zerstören
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _dokument.NodeChanging -= new System.Xml.XmlNodeChangedEventHandler(_dokument_NodeChanging);
            _dokument.NodeInserted -= new System.Xml.XmlNodeChangedEventHandler(_dokument_NodeInserted);
            _dokument.NodeRemoving -= new System.Xml.XmlNodeChangedEventHandler(_dokument_NodeRemoving);
            
            _disposed = true;
        }

        /// <summary>
        /// Nimmt einen UndoSchritt auf
        /// </summary>
        /// <param name="schritt"></param>
        private void NeuenUndoSchrittAnhaengen(XMLUndoSchritt neuerUndoSchritt)
        {
            // Wenn an der aktuellen Stelle noch ReDos folgen, dann werden alle hinfällig
            List<XMLUndoSchritt> loeschen = new List<XMLUndoSchritt>();
            for (int i = _pos + 1; i < _undoSchritte.Count; i++)
            {
                loeschen.Add(_undoSchritte[i]);
            }
            foreach (XMLUndoSchritt schritt in loeschen) {
                _undoSchritte.Remove(schritt);
            }

            // Den neuen Schritt anhängen
            _undoSchritte.Add(neuerUndoSchritt);
            _pos++;
            if (_pos != _undoSchritte.Count-1) {
                throw new Exception ("Undo-Pos sollte mit undoSchritte.Count-1 übereinstimmen. Statt dessen pos: " + _pos + ", _undoSchritte.Count -1:" + (_undoSchritte.Count -1));
            }
        }
    }
}
