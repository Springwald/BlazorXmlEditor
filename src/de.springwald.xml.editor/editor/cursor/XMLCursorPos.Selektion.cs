using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.cursor
{
    public partial class XMLCursorPos
    {

        #region SYSTEM

        #endregion

        #region PRIVATE ATTRIBUTES

        #endregion

        #region PUBLIC ATTRIBUTES

        #endregion

        #region CONSTRUCTOR
        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Findet heraus, ob der Node oder einer seiner Parent-Nodes selektiert ist
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        public bool IstNodeInnerhalbDerSelektion(System.Xml.XmlNode node)
        {
            if (_aktNode == null) // es ist gar kein Node selektiert
            {
                return false;
            }
            else
            {
                if (node == null) // gar kein Node zum Test übergeben
                {
                    return false;
                }
                else
                {
                    if (node == _aktNode) // der übergebene Node ist der aktuelle
                    {
                        // Zurückgeben, ob der Node selbst selektiert ist
                        return ((_posAmNode == XMLCursorPositionen.CursorAufNodeSelbstVorderesTag) ||
                            (_posAmNode == XMLCursorPositionen.CursorAufNodeSelbstHinteresTag));
                    }
                    else
                    {
                        return IstNodeInnerhalbDerSelektion(node.ParentNode); // Den Parentnode weitertesten
                    }
                }
            }
        }

        #endregion

        #region PRIVATE METHODS

        #endregion


      
    }
}
