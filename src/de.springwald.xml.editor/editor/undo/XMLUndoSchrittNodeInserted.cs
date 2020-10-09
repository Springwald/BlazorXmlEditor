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
        /// Erzeugt einen neuen Undoschritt f�r das Einf�gen eines neuen Nodes
        /// </summary>
        /// <param name="eingefuegterNode">Dieser Node wurde eingef�gt</param>
        public XMLUndoSchrittNodeInserted(System.Xml.XmlNode eingefuegterNode, System.Xml.XmlNode parentNode)          : base()
        {
            _eingefuegterNode = eingefuegterNode;
            _parentNode = parentNode;

            if ((eingefuegterNode == null))
            {
                throw new ApplicationException("Einf�gen des Nodes kann nicht f�r Undo vermerkt werden, da er NULL ist '" +
                        _eingefuegterNode.OuterXml + "'");
            }
        }

        #endregion

        #region PUBLIC METHODS

        public override void UnDo()
        {
            // Das Einf�gen des Nodes r�ckg�ngig machen
            if (_eingefuegterNode is System.Xml.XmlAttribute) // eingef�gter Node war ein Attribut
            {
                _parentNode.Attributes.Remove((System.Xml.XmlAttribute)_eingefuegterNode);
            }
            else // eingef�gter Node war kein Attribut
            {
                _parentNode.RemoveChild(_eingefuegterNode);
            }
        }

        #endregion

        #region PRIVATE METHODS
        #endregion
    }
}
