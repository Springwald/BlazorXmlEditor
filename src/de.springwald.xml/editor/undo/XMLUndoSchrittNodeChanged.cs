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
        /// Erzeugt einen neuen Undoschritt f�r das Ver�ndern eines Node-Values
        /// </summary>
        /// <param name="eingefuegterNode">Dieser Node wurde ver�ndert</param>
        public XMLUndoSchrittNodeChanged(System.Xml.XmlNode geaenderterNode, string valueVorher)  : base()
        {
            _geaenderterNode = geaenderterNode;
            _valueVorher = valueVorher;
            //_valueNachher = valueNachher;

            if ((geaenderterNode == null))
            {
                throw new ApplicationException("Ver�ndern des Nodes kann nicht f�r Undo vermerkt werden, da er NULL ist '" +
                        _geaenderterNode.OuterXml + "'");
            }
        }

        #endregion

        #region PUBLIC METHODS

        public override void UnDo()
        {
            // Das Ver�ndern des Nodes r�ckg�ngig machen
            _geaenderterNode.Value = _valueVorher;
        }

        #endregion

        #region PRIVATE METHODS
        #endregion
    }
}
