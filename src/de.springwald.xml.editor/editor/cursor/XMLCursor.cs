// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;

namespace de.springwald.xml.cursor
{
    public enum XMLCursorPositionen { CursorVorDemNode, CursorAufNodeSelbstVorderesTag, CursorAufNodeSelbstHinteresTag, CursorInDemLeeremNode, CursorInnerhalbDesTextNodes, CursorHinterDemNode };

    public enum MausKlickAktionen { MouseDown, MouseDownMove, MouseUp };

    public partial class XMLCursor : IDisposable
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
        }

        public void Dispose()
        {
        }

        public async Task SetPositions(XmlNode bothNodes, XMLCursorPositionen posAtBothNodes, int textPosInBothNodes, bool throwChangedEventWhenValuesChanged)
        {
            await this.SetPositions(
                bothNodes, posAtBothNodes, textPosInBothNodes,
                bothNodes, posAtBothNodes, textPosInBothNodes,
                throwChangedEventWhenValuesChanged);
        }

        public async Task SetPositions(
            XmlNode startNode, XMLCursorPositionen posAtStartNode, int textPosInStartNode,
            XmlNode endNode, XMLCursorPositionen posAtEndNode, int textPosInEndNode, bool throwChangedEventWhenValuesChanged)
        {
            var changed = false;
            if (throwChangedEventWhenValuesChanged)
            {
                changed = (startNode != this.StartPos.AktNode || posAtStartNode != this.StartPos.PosAmNode || textPosInStartNode != this.StartPos.PosImTextnode ||
                    endNode != this.EndPos.AktNode || posAtEndNode != this.EndPos.PosAmNode || textPosInEndNode != this.EndPos.PosImTextnode);
            }
            this.StartPos.SetPos(StartPos.AktNode, StartPos.PosAmNode, StartPos.PosImTextnode);
            this.EndPos.SetPos(EndPos.AktNode, EndPos.PosAmNode, EndPos.PosImTextnode);
            if (changed) await ChangedEvent.Trigger(EventArgs.Empty);
        }

        public bool Equals(XMLCursor second)
        {
            return second != null && this.StartPos.Equals(second.StartPos) && this.EndPos.Equals(second.EndPos);
        }

        /// <summary>
        /// Erzeugt eine Kopie dieses Cursors
        /// </summary>
        /// <returns></returns>
        public XMLCursor Clone()
        {
            XMLCursor klon = new XMLCursor();
            klon.StartPos.SetPos(StartPos.AktNode, StartPos.PosAmNode, StartPos.PosImTextnode);
            klon.EndPos.SetPos(EndPos.AktNode, EndPos.PosAmNode, EndPos.PosImTextnode);
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
            StartPos.SetPos(node, posAmNode, posImTextnode);
            EndPos.SetPos(node, posAmNode, posImTextnode);
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
                    await SetPositions(xmlNode, cursorPos, posInZeile, throwChangedEventWhenValuesChanged: true);
                    break;
                case MausKlickAktionen.MouseDownMove:
                case MausKlickAktionen.MouseUp:
                    // Ende des Select-Cursors setzen
                    if (EndPos.SetPos(xmlNode, cursorPos, posInZeile))
                    {
                        await this.ErzwingeChanged();
                    }
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
