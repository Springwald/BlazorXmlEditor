using System.Threading.Tasks;

namespace de.springwald.xml.cursor
{
    public partial class XMLCursor
    {
        /// <summary>
        /// F�gt den angegebenen Text an der aktuellen Cursorposition ein, sofern m�glich
        /// </summary>
        public async Task TextEinfuegen(string text, de.springwald.xml.XMLRegelwerk regelwerk)
        {
            XMLCursorPos einfuegePos;

            // Wenn etwas selektiert ist, dann zuerst das l�schen, da es ja durch den neuen Text ersetzt wird
            XMLCursor loeschbereich = Clone();
            await loeschbereich.SelektionOptimieren();
            var loeschResult = await loeschbereich.SelektionLoeschen();
            if (loeschResult.Success)
            {
                einfuegePos = loeschResult.NeueCursorPosNachLoeschen;
            }
            else
            {
                einfuegePos = StartPos.Clone();
            }

            // den angegebenen Text an der CursorPosition einf�gen
            var ersatzNode = (await einfuegePos.TextEinfuegen(text, regelwerk)).ErsatzNode;
            if (ersatzNode != null)
            {
                // Text konnte nicht eingef�gt werden, da aus der Texteingabe eine Node-Eingabe umgewandelt
                // wurde. Beispiel: Im AIML-Template wird * gedr�ckt, und dort statt dessen ein <star> eingef�gt
                await einfuegePos.InsertXMLNode(ersatzNode, regelwerk, false);
            }

            // anschlie�end wird der Cursor nur noch ein Strich hinter dem eingef�gten
            await BeideCursorPosSetzenMitChangeEventWennGeaendert(einfuegePos.AktNode, einfuegePos.PosAmNode, einfuegePos.PosImTextnode);

        }

        /// <summary>
        /// F�gt den angegebenen Node an der aktuellen Cursorposition ein, sofern m�glich
        /// </summary>
        public async Task XMLNodeEinfuegen(System.Xml.XmlNode node, de.springwald.xml.XMLRegelwerk regelwerk, bool neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
        {
            // Wenn etwas selektiert ist, dann zuerst das l�schen, da es ja durch den neuen Text ersetzt wird
            XMLCursor loeschbereich = Clone();
            await loeschbereich.SelektionOptimieren();
            var loeschResult = await loeschbereich.SelektionLoeschen();
            if (loeschResult.Success)
            {
                await BeideCursorPosSetzenMitChangeEventWennGeaendert(loeschResult.NeueCursorPosNachLoeschen.AktNode, loeschResult.NeueCursorPosNachLoeschen.PosAmNode, loeschResult.NeueCursorPosNachLoeschen.PosImTextnode);
            }

            // den angegebenen Node an der CursorPosition einf�gen
            if (await StartPos.InsertXMLNode(node, regelwerk, neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen))
            {
                // anschlie�en wird der Cursor nur noch ein Strich hinter dem eingef�gten
                await EndPos.CursorSetzenMitChangeEventWennGeaendert(StartPos.AktNode, StartPos.PosAmNode, StartPos.PosImTextnode);
            }
        }
    }
}
