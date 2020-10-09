using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.editor
{
    public class XMLUndoSchrittNodeChanged : XMLUndoSchritt
    {
        #region SYSTEM
        #endregion

        #region PRIVATE ATTRIBUTES

        private System.Xml.XmlNode _geaenderterNode;
        private string _valueVorher;
        //private string _valueNachher;

        #endregion

        #region PUBLIC ATTRIBUTES
        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Erzeugt einen neuen Undoschritt für das Verändern eines Node-Values
        /// </summary>
        /// <param name="eingefuegterNode">Dieser Node wurde verändert</param>
        public XMLUndoSchrittNodeChanged(System.Xml.XmlNode geaenderterNode, string valueVorher)  : base()
        {
            _geaenderterNode = geaenderterNode;
            _valueVorher = valueVorher;
            //_valueNachher = valueNachher;

            if ((geaenderterNode == null))
            {
                throw new ApplicationException("Verändern des Nodes kann nicht für Undo vermerkt werden, da er NULL ist '" +
                        _geaenderterNode.OuterXml + "'");
            }
        }

        #endregion

        #region PUBLIC METHODS

        public override void UnDo()
        {
            // Das Verändern des Nodes rückgängig machen
            _geaenderterNode.Value = _valueVorher;
        }

        #endregion

        #region PRIVATE METHODS
        #endregion
    }
}
