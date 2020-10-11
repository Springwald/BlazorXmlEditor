
namespace de.springwald.xml.cursor
{
    public partial class XMLCursorPos
    {
        /// <summary>
        /// Findet heraus, ob der Node oder einer seiner Parent-Nodes selektiert ist
        /// </summary>
        public bool IstNodeInnerhalbDerSelektion(System.Xml.XmlNode node)
        {
            if (_aktNode == null) // es ist gar kein Node selektiert
            {
                return false;
            }
            else
            {
                if (node == null) // gar kein Node zum Test �bergeben
                {
                    return false;
                }
                else
                {
                    if (node == _aktNode) // der �bergebene Node ist der aktuelle
                    {
                        // Zur�ckgeben, ob der Node selbst selektiert ist
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
    }
}
