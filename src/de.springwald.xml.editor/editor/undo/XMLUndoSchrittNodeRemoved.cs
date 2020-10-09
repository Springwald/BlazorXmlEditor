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
        /// Erzeugt einen neuen Undoschritt f�r das L�schen eines Nodes
        /// </summary>
        /// <param name="nodeVorDemLoeschen">Dieser Node wurde gel�scht</param>
        public XMLUndoSchrittNodeRemoved(System.Xml.XmlNode nodeVorDemLoeschen) : base()
        {

            _geloeschterNode = nodeVorDemLoeschen;

            _parentNode = nodeVorDemLoeschen.ParentNode;
            _previousSibling = nodeVorDemLoeschen.PreviousSibling;
            _nextSibling = nodeVorDemLoeschen.NextSibling;

            if ((_parentNode == null) && (_previousSibling == null) && (_nextSibling == null))
            {
                throw new ApplicationException("L�schen des Nodes kann nicht f�r Undo vermerkt werden, da er keinen Bezug hat '" +
                        nodeVorDemLoeschen.OuterXml + "'");
            }
        }

        #endregion

        #region PUBLIC METHODS

        public override void UnDo()
        {

            // Das L�schen des Nodes r�ckg�ngig machen
            if (_previousSibling != null) // Wenn es einen Vorher-Node gab
            {
                _previousSibling.ParentNode.InsertAfter(_geloeschterNode, _previousSibling); // Node wieder hinter Vorher-Node einf�gen
            }
            else  // Es gab keinen Vorher-Node
            {
                if (_nextSibling != null) // Es gab einen Nachher-Node
                {
                    _nextSibling.ParentNode.InsertBefore(_geloeschterNode, _nextSibling); // Node wieder vor dem Nachher-Node einf�gen
                }
                else // Es gab weder Vorher- noch -Nachher-Node, Parent war also bis auf den gel�schten Node leer
                {
                    _parentNode.AppendChild(_geloeschterNode); // Den gel�schten Node wieder in den ParentNode einsetzen
                }
            }
        }

        #endregion

        #region PRIVATE METHODS
        #endregion
    }
}
