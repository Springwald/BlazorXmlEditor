using de.springwald.xml.cursor;
using de.springwald.xml.events;
using System.Threading.Tasks;

namespace de.springwald.xml.editor
{
    public partial class XMLEditor
    {

        public XmlAsyncEvent<KeyEventArgs> KeyDownEvent = new XmlAsyncEvent<KeyEventArgs>();
        public XmlAsyncEvent<KeyPressEventArgs> KeyPressEvent = new XmlAsyncEvent<KeyPressEventArgs>();

        private void TastaturEventsAnmelden()
        {
            this.NativePlatform.InputEvents.Leave.Add(this.zeichnungsSteuerelement_Leave);
            this.NativePlatform.InputEvents.PreviewKey.Add(this.zeichnungsSteuerelement_PreviewKeyDown);
            this.NativePlatform.InputEvents.KeyPress.Add(this.zeichnungsSteuerelement_KeyPress);
        }

        private bool _naechsteTasteBeiKeyPressAlsTextAufnehmen = true;
        private bool _naechstesLostFokusVerhindern = false; // Damit beim TAB das Verlassen des Fokus ignoriert wird


        async Task zeichnungsSteuerelement_PreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            //if (Regelwerk.PreviewKeyDown(e, out _naechsteTasteBeiKeyPressAlsTextAufnehmen, this))
            //{
            //    // Dieser Tastendruck wurde bereits vom Regelwerk verarbeitet
            //}
            //else
            {
                // Dieser Tastendruck wurde nicht vom Regelwerk verarbeitet

                _naechsteTasteBeiKeyPressAlsTextAufnehmen = false;

                XMLCursorPos dummy;



                switch (e.KeyData)
                {

                    case Keys.Enter: // Enter macht spezielle Dinge, z.B. ein neues Tag gleicher Art beginnen etc.
                    case (Keys.Enter | Keys.Shift):
                        AktionenEnterGedrueckt();
                        break;

                    case Keys.Tab: // Tab springt in das nächste Tag
                        System.Xml.XmlNode node = _cursor.StartPos.AktNode;
                        bool abbruch = false;
                        if (node.FirstChild != null)
                        {
                            node = node.FirstChild;
                        }
                        else
                        {
                            if (node.NextSibling != null)
                            {
                                node = node.NextSibling;
                            }
                            else
                            {
                                if (node.ParentNode.NextSibling != null)
                                {
                                    node = node.ParentNode.NextSibling;
                                }
                                else
                                {
                                    // Hm, wohin könnte TAB denn *noch* gehen? 
                                    abbruch = true;
                                }
                            }
                        }
                        if (!abbruch)
                        {
                            await _cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(node, XMLCursorPositionen.CursorInDemLeeremNode);
                        }
                        _naechstesLostFokusVerhindern = true; // Damit beim TAB das Verlassen des Fokus ignoriert wird
                        break;

                    //case (Keys.Left | Keys.Shift):  // CursorEndPos ein Char nach links
                    //    await _cursor.EndPos.MoveLeft(_rootNode, Regelwerk);
                    //    break;

                    //case (Keys.Right | Keys.Shift): // CursorEnd ein Char nach rechts
                    //    await _cursor.EndPos.MoveRight(_rootNode, Regelwerk);
                    //    break;

                    //case (Keys.A | Keys.Control): // STRG-A -> Alles markieren
                    //    await AktionAllesMarkieren();
                    //    break;

                    //case (Keys.X | Keys.Control): // STRG-X -> Ausschneiden
                    //    AktionCutToClipboard(UndoSnapshotSetzenOptionen.ja);
                    //    break;

                    //case (Keys.C | Keys.Control): // STRG-C -> Kopieren
                    //    AktionCopyToClipboard();
                    //    break;

                    case (Keys.V | Keys.Control): // STRG-V -> Einfügen
                        await AktionPasteFromClipboard(UndoSnapshotSetzenOptionen.ja);
                        break;

                    //case (Keys.Home): // Pos1 
                    //    AktionCursorAufPos1();
                    //    break;

                    case (Keys.Z | Keys.Control): //STRG-Z -> UnDo
                        await UnDo();
                        break;

                    case Keys.Left: // Cursor ein Char nach links
                        dummy = _cursor.StartPos.Clone();
                        await dummy.MoveLeft(_rootNode, Regelwerk);
                        await _cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(dummy.AktNode, dummy.PosAmNode, dummy.PosImTextnode);
                        break;

                    case Keys.Right: // Cursor ein Char nach rechts
                        dummy = _cursor.StartPos.Clone();
                        await dummy.MoveRight(_rootNode, Regelwerk);
                        await _cursor.BeideCursorPosSetzenMitChangeEventWennGeaendert(dummy.AktNode, dummy.PosAmNode, dummy.PosImTextnode);
                        break;

                    case Keys.Back:  // Backspace-Taste
                    // case (Keys.Back | Keys.Shift):    // Shift-Backspace-Taste
                        if (_cursor.IstEtwasSelektiert)
                        {
                            await AktionDelete(UndoSnapshotSetzenOptionen.ja);
                        }
                        else
                        {
                            await AktionNodeOderZeichenVorDerCursorPosLoeschen(_cursor.StartPos, UndoSnapshotSetzenOptionen.ja);
                        }
                        break;

                    //case Keys.Delete:                   // entfernen-Taste
                    //// case (Keys.Delete | Keys.Shift):    // Shift-Entfernen-Taste
                    //    if (_cursor.IstEtwasSelektiert)
                    //    {
                    //        await AktionDelete(UndoSnapshotSetzenOptionen.ja);
                    //    }
                    //    else
                    //    {
                    //        await AktionNodeOderZeichenHinterCursorPosLoeschen(_cursor.StartPos, UndoSnapshotSetzenOptionen.ja);
                    //    }
                    //    break;

                    case Keys.Control:
                    case Keys.Escape:
                        // Bei diesen Tasten passiert nichts
                        break;

                    default:
                        // Die restlichen Tasten werden beim KeyPress als Text übernommen
                        _naechsteTasteBeiKeyPressAlsTextAufnehmen = true;
                        break;

                }
            }
        }


        /// <summary>
        /// Eingegebenen Text übernehmen, wenn nicht vorher im Key-Down abgefangen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task zeichnungsSteuerelement_KeyPress( KeyPressEventArgs e)
        {
            if (_naechsteTasteBeiKeyPressAlsTextAufnehmen)
            {
                await AktionTextAnCursorPosEinfuegen(e.KeyChar.ToString(), UndoSnapshotSetzenOptionen.ja);
            }
            _naechsteTasteBeiKeyPressAlsTextAufnehmen = false;
        }

        /// <summary>
        /// Damit beim TAB das Verlassen des Fokus ignoriert wird
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async Task zeichnungsSteuerelement_Leave(System.EventArgs e)
        {
            if (_naechstesLostFokusVerhindern)
            {
                _naechstesLostFokusVerhindern = false;
                await this.NativePlatform.Focus.FokusAufEingabeFormularSetzen();
            }
        }
    }
}
