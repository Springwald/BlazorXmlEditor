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
        /// Erzeugt einen neuen Undoschritt f�r das L�schen eines Nodes
        /// </summary>
        /// <param name="nodeVorDemLoeschen">Dieser Node wurde gel�scht</param>
        public XMLUndoSchrittAttributRemoved(System.Xml.XmlAttribute attributVorDemLoeschen)
            : base()
        {

            _geloeschtesAttribut = attributVorDemLoeschen;

            _ownerElement = attributVorDemLoeschen.OwnerElement;

            if (_ownerElement == null)
            {
                throw new ApplicationException("L�schen des Attributes kann nicht f�r Undo vermerkt werden, da es keinen Bezug hat '" +
                        attributVorDemLoeschen.OuterXml + "'");
            }
        }

        #endregion

        #region PUBLIC METHODS

        public override void UnDo()
        {
            // Das L�schen des Attributes r�ckg�ngig machen
            _ownerElement.Attributes.Append(_geloeschtesAttribut);
        }

        #endregion

        #region PRIVATE METHODS
        #endregion
    }
}
