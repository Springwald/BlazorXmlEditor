using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.editor
{
    public class XMLUndoSchrittAttributRemoved:XMLUndoSchritt
    {
        #region SYSTEM
        #endregion

        #region PRIVATE ATTRIBUTES

        private System.Xml.XmlAttribute _geloeschtesAttribut;

        private System.Xml.XmlNode _ownerElement;

        #endregion

        #region PUBLIC ATTRIBUTES
        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Erzeugt einen neuen Undoschritt für das Löschen eines Nodes
        /// </summary>
        /// <param name="nodeVorDemLoeschen">Dieser Node wurde gelöscht</param>
        public XMLUndoSchrittAttributRemoved(System.Xml.XmlAttribute attributVorDemLoeschen)
            : base()
        {

            _geloeschtesAttribut = attributVorDemLoeschen;

            _ownerElement = attributVorDemLoeschen.OwnerElement;

            if (_ownerElement == null)
            {
                throw new ApplicationException("Löschen des Attributes kann nicht für Undo vermerkt werden, da es keinen Bezug hat '" +
                        attributVorDemLoeschen.OuterXml + "'");
            }
        }

        #endregion

        #region PUBLIC METHODS

        public override void UnDo()
        {
            // Das Löschen des Attributes rückgängig machen
            _ownerElement.Attributes.Append(_geloeschtesAttribut);
        }

        #endregion

        #region PRIVATE METHODS
        #endregion
    }
}
