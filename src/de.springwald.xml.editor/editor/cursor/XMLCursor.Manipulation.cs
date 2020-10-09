using System.Threading.Tasks;

namespace de.springwald.xml.cursor
{
    public partial class XMLCursor
    {
        /// <summary>
        /// Fügt den angegebenen Text an der aktuellen Cursorposition ein, sofern möglich
        /// </summary>
        public async Task TextEinfuegen(string text, de.springwald.xml.XMLRegelwerk regelwerk)
        {
            XMLCursorPos einfuegePos;

            // Wenn etwas selektiert ist, dann zuerst das löschen, da es ja durch den neuen Text ersetzt wird
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

            // den angegebenen Text an der CursorPosition einfügen
            var ersatzNode = (await einfuegePos.TextEinfuegen(text, regelwerk)).ErsatzNode;
            if (ersatzNode != null)
            {
                // Text konnte nicht eingefügt werden, da aus der Texteingabe eine Node-Eingabe umgewandelt
                // wurde. Beispiel: Im AIML-Template wird * gedrückt, und dort statt dessen ein <star> eingefügt
                await einfuegePos.InsertXMLNode(ersatzNode, regelwerk, false);
            }

            // anschließend wird der Cursor nur noch ein Strich hinter dem eingefügten
            await BeideCursorPosSetzenMitChangeEventWennGeaendert(einfuegePos.AktNode, einfuegePos.PosAmNode, einfuegePos.PosImTextnode);

        }

        /// <summary>
        /// Fügt den angegebenen Node an der aktuellen Cursorposition ein, sofern möglich
        /// </summary>
        public async Task XMLNodeEinfuegen(System.Xml.XmlNode node, de.springwald.xml.XMLRegelwerk regelwerk, bool neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen)
        {
            // Wenn etwas selektiert ist, dann zuerst das löschen, da es ja durch den neuen Text ersetzt wird
            XMLCursor loeschbereich = Clone();
            await loeschbereich.SelektionOptimieren();
            var loeschResult = await loeschbereich.SelektionLoeschen();
            if (loeschResult.Success)
            {
                await BeideCursorPosSetzenMitChangeEventWennGeaendert(loeschResult.NeueCursorPosNachLoeschen.AktNode, loeschResult.NeueCursorPosNachLoeschen.PosAmNode, loeschResult.NeueCursorPosNachLoeschen.PosImTextnode);
            }

            // den angegebenen Node an der CursorPosition einfügen
            if (await StartPos.InsertXMLNode(node, regelwerk, neueCursorPosAufJedenFallHinterDenEingefuegtenNodeSetzen))
            {
                // anschließen wird der Cursor nur noch ein Strich hinter dem eingefügten
                await EndPos.CursorSetzenMitChangeEventWennGeaendert(StartPos.AktNode, StartPos.PosAmNode, StartPos.PosImTextnode);
            }
        }
    }
}
