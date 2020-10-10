using System;
using System.Threading.Tasks;

namespace de.springwald.xml.cursor
{

    public enum XMLCursorPositionen { CursorVorDemNode, CursorAufNodeSelbstVorderesTag, CursorAufNodeSelbstHinteresTag, CursorInDemLeeremNode, CursorInnerhalbDesTextNodes, CursorHinterDemNode };

    public enum MausKlickAktionen { MouseDown, MouseDownMove, MouseUp };

    /// <summary>
    /// Zusammenfassung für XMLCursor.
    /// </summary>
    /// <remarks>
    /// (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    public partial class XMLCursor
    {
        /// <summary>
        /// Event definieren, wenn sich der Cursor geändert hat
        /// </summary>
        // public event System.EventHandler ChangedEvent;
        public XmlAsyncEvent<EventArgs> ChangedEvent { get; }

        private bool _cursorWirdGeradeGesetzt = false; // Um Doppelevents zu vermeiden

        // Beginn des aktuell ausgewählten Bereiches
        public XMLCursorPos StartPos { get; private set; }

        /// <summary>
        /// Ende des aktuell ausgewählten Bereiches
        /// </summary>
        public XMLCursorPos EndPos { get; private set; }

        public XMLCursor()
        {
            EndPos = new XMLCursorPos();
            StartPos = new XMLCursorPos();
            this.ChangedEvent = new XmlAsyncEvent<EventArgs>();
            this.UnterEventsAnmelden();
        }

        /// <summary>
        /// Erzeugt eine Kopie dieses Cursors
        /// </summary>
        /// <returns></returns>
        public XMLCursor Clone()
        {
            XMLCursor klon = new XMLCursor();
            klon.StartPos.CursorSetzenOhneChangeEvent(StartPos.AktNode, StartPos.PosAmNode, StartPos.PosImTextnode);
            klon.EndPos.CursorSetzenOhneChangeEvent(EndPos.AktNode, EndPos.PosAmNode, EndPos.PosImTextnode);
            return klon;
        }

        /// <summary>
        /// Löst den Cursor-Changed-Event manuell aus
        /// </summary>
        public async Task ErzwingeChanged()
        {
            await this.ChangedEvent.Trigger(EventArgs.Empty);
        }

        /// <summary>
        /// Setzt gleichzeitig Node und Position und löst dadurch nur ein Changed-Event statt zwei aus
        /// </summary>
        /// <param name="aktNode"></param>
        /// <param name="posInNode"></param>
        /// <param name="posImTextnode"></param>
        public void BeideCursorPosSetzenOhneChangeEvent(System.Xml.XmlNode node, XMLCursorPositionen posAmNode, int posImTextnode)
        {
            // Cursor setzen
            _cursorWirdGeradeGesetzt = true;
            StartPos.CursorSetzenOhneChangeEvent(node, posAmNode, posImTextnode);
            EndPos.CursorSetzenOhneChangeEvent(node, posAmNode, posImTextnode);
            _cursorWirdGeradeGesetzt = false;
        }

        /// <summary>
        /// Setzt gleichzeitig Node und Position und löst dadurch nur ein Changed-Event statt zwei aus
        /// </summary>
        /// <param name="aktNode"></param>
        /// <param name="posInNode"></param>
        /// <param name="posImTextnode"></param>
        public async Task BeideCursorPosSetzenMitChangeEventWennGeaendert(System.Xml.XmlNode node, XMLCursorPositionen posAmNode, int posImTextnode)
        {
            // Herausfinden, ob sich etwas geändert hat
            bool geaendert;
            if (node != StartPos.AktNode)
            {
                geaendert = true;
            }
            else
            {
                if (posAmNode != StartPos.PosAmNode)
                {
                    geaendert = true;
                }
                else
                {
                    if (posImTextnode != StartPos.PosImTextnode)
                    {
                        geaendert = true;
                    }
                    else
                    {
                        geaendert = false;
                    }
                }
            }
            if (!geaendert)
            {
                if (node != EndPos.AktNode)
                {
                    geaendert = true;
                }
                else
                {
                    if (posAmNode != EndPos.PosAmNode)
                    {
                        geaendert = true;
                    }
                    else
                    {
                        if (posImTextnode != EndPos.PosImTextnode)
                        {
                            geaendert = true;
                        }
                        else
                        {
                            geaendert = false;
                        }
                    }
                }
            }

            this.BeideCursorPosSetzenOhneChangeEvent(node, posAmNode, posImTextnode);
            if (geaendert) await this.ChangedEvent.Trigger(EventArgs.Empty); // Bescheid geben, dass nun der Cursor geändert wurde
        }

        /// <summary>
        /// Setzt gleichzeitig Node und Position und löst dadurch nur ein Changed-Event statt zwei aus
        /// </summary>
        /// <param name="aktNode"></param>
        /// <param name="posInNode"></param>
        public async Task BeideCursorPosSetzenMitChangeEventWennGeaendert(System.Xml.XmlNode node, XMLCursorPositionen posAmNode)
        {
            await BeideCursorPosSetzenMitChangeEventWennGeaendert(node, posAmNode, 0);
        }

        /// <summary>
        /// Setzt gleichzeitig Node und Position und löst dadurch nur ein Changed-Event statt zwei aus
        /// </summary>
        /// <param name="aktNode"></param>
        /// <param name="posInNode"></param>
        public void BeideCursorPosSetzenOhneChangeEvent(System.Xml.XmlNode node, XMLCursorPositionen posAmNode)
        {
            BeideCursorPosSetzenOhneChangeEvent(node, posAmNode, 0);
        }

        /// <summary>
        /// Setzt die Positionen des Cursors für entsprechende Mausaktionen:
        /// Bei MausDown StartUndEndpos, bei Move und Up nur die Endpos
        /// </summary>
        /// <param name="action"></param>
        public async Task CursorPosSetzenDurchMausAktion(System.Xml.XmlNode xmlNode, XMLCursorPositionen cursorPos, int posInZeile, MausKlickAktionen action)
        {
            switch (action)
            {
                case MausKlickAktionen.MouseDown:
                    // den Cursor an die neue Position setzen
                    await BeideCursorPosSetzenMitChangeEventWennGeaendert(xmlNode, cursorPos, posInZeile);
                    break;
                case MausKlickAktionen.MouseDownMove:
                case MausKlickAktionen.MouseUp:
                    // Ende des Select-Cursors setzen
                    await EndPos.CursorSetzenMitChangeEventWennGeaendert(xmlNode, cursorPos, posInZeile);
                    //Debug.WriteLine(SelektionAlsString);
                    break;
            }
        }

        /// <summary>
        /// Setzt die Positionen des Cursors für entsprechende Mausaktionen:
        /// Bei MausDown StartUndEndpos, bei Move und Up nur die Endpos
        /// </summary>
        /// <param name="aktion"></param>
        public async Task CursorPosSetzenDurchMausAktion(System.Xml.XmlNode xmlNode, XMLCursorPositionen cursorPos, MausKlickAktionen aktion)
        {
            await CursorPosSetzenDurchMausAktion(xmlNode, cursorPos, 0, aktion);
        }


        private void UnterEventsAnmelden()
        {
            EndPos.PosChangedEvent.Add(this.endPos_ChangedEvent);
            StartPos.PosChangedEvent.Add(this.startPos_ChangedEvent);
        }

        private async Task endPos_ChangedEvent(EventArgs e)
        {
            if (!_cursorWirdGeradeGesetzt)
            {
                await this.ChangedEvent.Trigger(EventArgs.Empty); // Bescheid geben, dass nun der Cursor geändert wurde
            }
        }

        private async Task startPos_ChangedEvent(EventArgs e)
        {
            if (!_cursorWirdGeradeGesetzt)
            {
                await this.ChangedEvent.Trigger(EventArgs.Empty); // Bescheid geben, dass nun der Cursor geändert wurde
            }
        }

    }
}
