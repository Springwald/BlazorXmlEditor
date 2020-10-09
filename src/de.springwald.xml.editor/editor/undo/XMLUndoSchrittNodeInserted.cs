using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.editor
{
    public class XMLUndoSchrittNodeInserted : XMLUndoSchritt
    {
        #region SYSTEM
        #endregion

        #region PRIVATE ATTRIBUTES

        private System.Xml.XmlNode _eingefuegterNode;
        private System.Xml.XmlNode _parentNode;

        #endregion

        #region PUBLIC ATTRIBUTES
        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Erzeugt einen neuen Undoschritt für das Einfügen eines neuen Nodes
        /// </summary>
        /// <param name="eingefuegterNode">Dieser Node wurde eingefügt</param>
        public XMLUndoSchrittNodeInserted(System.Xml.XmlNode eingefuegterNode, System.Xml.XmlNode parentNode)          : base()
        {
            _eingefuegterNode = eingefuegterNode;
            _parentNode = parentNode;

            if ((eingefuegterNode == null))
            {
                throw new ApplicationException("Einfügen des Nodes kann nicht für Undo vermerkt werden, da er NULL ist '" +
                        _eingefuegterNode.OuterXml + "'");
            }
        }

        #endregion

        #region PUBLIC METHODS

        public override void UnDo()
        {
            // Das Einfügen des Nodes rückgängig machen
            if (_eingefuegterNode is System.Xml.XmlAttribute) // eingefügter Node war ein Attribut
            {
                _parentNode.Attributes.Remove((System.Xml.XmlAttribute)_eingefuegterNode);
            }
            else // eingefügter Node war kein Attribut
            {
                _parentNode.RemoveChild(_eingefuegterNode);
            }
        }

        #endregion

        #region PRIVATE METHODS
        #endregion
    }
}
