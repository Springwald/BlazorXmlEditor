using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace de.springwald.xml.editor
{
    public class XMLUndoSchrittNodeRemoved:XMLUndoSchritt
    {
        #region SYSTEM
        #endregion

        #region PRIVATE ATTRIBUTES

        private System.Xml.XmlNode _geloeschterNode;

        private System.Xml.XmlNode _parentNode;
        private System.Xml.XmlNode _previousSibling; 
        private System.Xml.XmlNode _nextSibling;

        #endregion

        #region PUBLIC ATTRIBUTES
        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Erzeugt einen neuen Undoschritt für das Löschen eines Nodes
        /// </summary>
        /// <param name="nodeVorDemLoeschen">Dieser Node wurde gelöscht</param>
        public XMLUndoSchrittNodeRemoved(System.Xml.XmlNode nodeVorDemLoeschen) : base()
        {

            _geloeschterNode = nodeVorDemLoeschen;

            _parentNode = nodeVorDemLoeschen.ParentNode;
            _previousSibling = nodeVorDemLoeschen.PreviousSibling;
            _nextSibling = nodeVorDemLoeschen.NextSibling;

            if ((_parentNode == null) && (_previousSibling == null) && (_nextSibling == null))
            {
                throw new ApplicationException("Löschen des Nodes kann nicht für Undo vermerkt werden, da er keinen Bezug hat '" +
                        nodeVorDemLoeschen.OuterXml + "'");
            }
        }

        #endregion

        #region PUBLIC METHODS

        public override void UnDo()
        {

            // Das Löschen des Nodes rückgängig machen
            if (_previousSibling != null) // Wenn es einen Vorher-Node gab
            {
                _previousSibling.ParentNode.InsertAfter(_geloeschterNode, _previousSibling); // Node wieder hinter Vorher-Node einfügen
            }
            else  // Es gab keinen Vorher-Node
            {
                if (_nextSibling != null) // Es gab einen Nachher-Node
                {
                    _nextSibling.ParentNode.InsertBefore(_geloeschterNode, _nextSibling); // Node wieder vor dem Nachher-Node einfügen
                }
                else // Es gab weder Vorher- noch -Nachher-Node, Parent war also bis auf den gelöschten Node leer
                {
                    _parentNode.AppendChild(_geloeschterNode); // Den gelöschten Node wieder in den ParentNode einsetzen
                }
            }
        }

        #endregion

        #region PRIVATE METHODS
        #endregion
    }
}
