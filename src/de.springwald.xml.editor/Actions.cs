using de.springwald.xml.editor;
using de.springwald.xml.events;

namespace de.springwald.xml
{
    class Actions
    {
        /// <summary>
        /// Verarbeitet Tastendrucke für den Editor vor
        /// </summary>
        /// <param name="e"></param>
        /// <param name="naechsteTasteBeiKeyPressAlsTextAufnehmen"></param>
        /// <param name="editor"></param>
        /// <returns>true, wenn der Tastendruck hier verarbeitet wurde</returns>
        public virtual bool PreviewKeyDown(PreviewKeyDownEventArgs e, out bool naechsteTasteBeiKeyPressAlsTextAufnehmen, XMLEditor editor)
        {
            naechsteTasteBeiKeyPressAlsTextAufnehmen = false;

            switch (e.KeyData)
            {

                case Keys.Control | Keys.S:
                    editor.AktionNeuesElementAnAktCursorPosEinfuegen("srai", XMLEditor.UndoSnapshotSetzenOptionen.ja, false);
                    naechsteTasteBeiKeyPressAlsTextAufnehmen = false;
                    return true;
                default:
                    // Die restlichen Tasten werden beim KeyPress als Text übernommen
                    naechsteTasteBeiKeyPressAlsTextAufnehmen = true;
                    return false;
            }
        }
    }
}
