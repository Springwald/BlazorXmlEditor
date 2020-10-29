// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.cursor;
using de.springwald.xml.rules;
using System;
using System.Threading.Tasks;
using System.Xml;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.cursor
{
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
        public XmlCursorPos StartPos { get; private set; }

        /// <summary>
        /// Ende des aktuell ausgewählten Bereiches
        /// </summary>
        public XmlCursorPos EndPos { get; private set; }

        public XMLCursor()
        {
            EndPos = new XmlCursorPos();
            StartPos = new XmlCursorPos();
            this.ChangedEvent = new XmlAsyncEvent<EventArgs>();
        }

        public void Dispose()
        {
        }

        public async Task SetPositions(XmlNode bothNodes, XmlCursorPositions posAtBothNodes, int textPosInBothNodes, bool throwChangedEventWhenValuesChanged)
        {
            await this.SetPositions(
                bothNodes, posAtBothNodes, textPosInBothNodes,
                bothNodes, posAtBothNodes, textPosInBothNodes,
                throwChangedEventWhenValuesChanged);
        }

        public async Task SetPositions(
            XmlNode startNode, XmlCursorPositions posAtStartNode, int textPosInStartNode,
            XmlNode endNode, XmlCursorPositions posAtEndNode, int textPosInEndNode, bool throwChangedEventWhenValuesChanged)
        {
            var changed = false;
            if (throwChangedEventWhenValuesChanged)
            {
                changed = (startNode != this.StartPos.ActualNode || posAtStartNode != this.StartPos.PosOnNode || textPosInStartNode != this.StartPos.PosInTextNode ||
                    endNode != this.EndPos.ActualNode || posAtEndNode != this.EndPos.PosOnNode || textPosInEndNode != this.EndPos.PosInTextNode);
            }
            this.StartPos.SetPos(startNode, posAtStartNode, textPosInStartNode);
            this.EndPos.SetPos(endNode, posAtEndNode, textPosInEndNode);
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
            klon.StartPos.SetPos(StartPos.ActualNode, StartPos.PosOnNode, StartPos.PosInTextNode);
            klon.EndPos.SetPos(EndPos.ActualNode, EndPos.PosOnNode, EndPos.PosInTextNode);
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
        public void BeideCursorPosSetzenOhneChangeEvent(XmlNode node, XmlCursorPositions posAmNode, int posImTextnode)
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
        public async Task BeideCursorPosSetzenMitChangeEventWennGeaendert(XmlNode node, XmlCursorPositions posAmNode, int posImTextnode)
        {
            // Herausfinden, ob sich etwas geändert hat
            bool geaendert;
            if (node != StartPos.ActualNode)
            {
                geaendert = true;
            }
            else
            {
                if (posAmNode != StartPos.PosOnNode)
                {
                    geaendert = true;
                }
                else
                {
                    if (posImTextnode != StartPos.PosInTextNode)
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
                if (node != EndPos.ActualNode)
                {
                    geaendert = true;
                }
                else
                {
                    if (posAmNode != EndPos.PosOnNode)
                    {
                        geaendert = true;
                    }
                    else
                    {
                        if (posImTextnode != EndPos.PosInTextNode)
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
        public async Task BeideCursorPosSetzenMitChangeEventWennGeaendert(System.Xml.XmlNode node, XmlCursorPositions posAmNode)
        {
            await BeideCursorPosSetzenMitChangeEventWennGeaendert(node, posAmNode, 0);
        }

        /// <summary>
        /// Setzt gleichzeitig Node und Position und löst dadurch nur ein Changed-Event statt zwei aus
        /// </summary>
        /// <param name="aktNode"></param>
        /// <param name="posInNode"></param>
        public void BeideCursorPosSetzenOhneChangeEvent(System.Xml.XmlNode node, XmlCursorPositions posAmNode)
        {
            BeideCursorPosSetzenOhneChangeEvent(node, posAmNode, 0);
        }

        /// <summary>
        /// Setzt die Positionen des Cursors für entsprechende Mausaktionen:
        /// Bei MausDown StartUndEndpos, bei Move und Up nur die Endpos
        /// </summary>
        /// <param name="action"></param>
        public async Task CursorPosSetzenDurchMausAktion(System.Xml.XmlNode xmlNode, XmlCursorPositions cursorPos, int posInZeile, MausKlickAktionen action)
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
        public async Task CursorPosSetzenDurchMausAktion(System.Xml.XmlNode xmlNode, XmlCursorPositions cursorPos, MausKlickAktionen aktion)
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
        /// <summary>
        /// Optimiert den selektierten Bereich 
        /// </summary>
        public async Task SelektionOptimieren()
        {
            // Tauschbuffer-Variablen definieren
            XmlCursorPositions dummyPos;
            int dummyTextPos;

            if (StartPos.ActualNode == null) return; // Nix im Cursor, daher nix zu optimieren

            // 1. Wenn die Startpos hinter der Endpos liegt, dann beide vertauschen
            if (StartPos.ActualNode == EndPos.ActualNode)  // Beide Nodes sind gleich
            {
                if (StartPos.PosOnNode > EndPos.PosOnNode) // Wenn StartPos innerhalb eines Nodes hinter EndPos liegt
                {
                    // beide Positionen am selben Node tauschen
                    dummyPos = StartPos.PosOnNode;
                    dummyTextPos = StartPos.PosInTextNode;
                    StartPos.SetPos(EndPos.ActualNode, EndPos.PosOnNode, EndPos.PosInTextNode);
                    EndPos.SetPos(EndPos.ActualNode, dummyPos, dummyTextPos);
                }
                else // StartPos lag nicht hinter Endpos
                {
                    // Ist ist ein Textteil innerhalb eines Textnodes selektiert ?
                    if ((StartPos.PosOnNode == XmlCursorPositions.CursorInsideTextNode) && (EndPos.PosOnNode == XmlCursorPositions.CursorInsideTextNode))
                    {  // Ein Teil eines Textnodes ist selektiert
                        if (StartPos.PosInTextNode > EndPos.PosInTextNode) // Wenn die TextStartpos hinter der TextEndpos liegt, dann wechseln
                        {   // Textauswahl tauschen
                            dummyTextPos = StartPos.PosInTextNode;
                            StartPos.SetPos(StartPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, EndPos.PosInTextNode);
                            EndPos.SetPos(StartPos.ActualNode, XmlCursorPositions.CursorInsideTextNode, dummyTextPos);
                        }
                    }
                }
            }
            else // Beide Nodes sind nicht gleich
            {
                // Wenn die Nodes in der Reihenfolge falsch sind, dann beide vertauschen
                if (ToolboxXML.Node1LaysBeforeNode2(EndPos.ActualNode, StartPos.ActualNode))
                {
                    XmlCursorPos tempPos = StartPos;
                    StartPos = EndPos;
                    EndPos = tempPos;
                }

                // Wenn der EndNode im StartNode liegt, den gesamten, umgebenden Startnode selektieren
                if (ToolboxXML.IstChild(EndPos.ActualNode, StartPos.ActualNode))
                {
                    await SetPositions(StartPos.ActualNode, XmlCursorPositions.CursorOnNodeStartTag, 0, throwChangedEventWhenValuesChanged: false);
                }

                // Den ersten gemeinsamen Parent von Start und Ende finden, und in dieser Höhe die Nodes selektieren.
                // Das führt dazu, dass z.B. bei LI-Elemente und UL beim Ziehen der Selektion über mehrere LI immer
                // nur ganze LI selektiert werden und nicht nur Teile davon
                if (StartPos.ActualNode.ParentNode != EndPos.ActualNode.ParentNode) // wenn Start und Ende nicht direkt im selben Parent stecken
                {
                    // - zuerst herausfinden, welches der tiefste, gemeinsame Parent von Start- und End-node ist
                    System.Xml.XmlNode gemeinsamerParent = XmlCursorSelectionHelper.TiefsterGemeinsamerParent(StartPos.ActualNode, EndPos.ActualNode);
                    // - dann Start- und End-Node bis vor dem Parent hoch-eskalieren
                    System.Xml.XmlNode nodeStart = StartPos.ActualNode;
                    while (nodeStart.ParentNode != gemeinsamerParent) nodeStart = nodeStart.ParentNode;
                    System.Xml.XmlNode nodeEnde = EndPos.ActualNode;
                    while (nodeEnde.ParentNode != gemeinsamerParent) nodeEnde = nodeEnde.ParentNode;
                    // - schließlich die neuen Start- und End-Nodes anzeigen  
                    StartPos.SetPos(nodeStart, XmlCursorPositions.CursorOnNodeStartTag);
                    EndPos.SetPos(nodeEnde, XmlCursorPositions.CursorOnNodeStartTag);
                }
            }
        }

        /// <summary>
        /// Sind Zeichen oder Nodes von diesem Cursor eingeschlossen
        /// </summary>
        /// <remarks>
        /// Entweder ist ein einzelner Node von der Startpos selektiert, oder die selektierten Bereiche liegen
        /// zwischen StartPos und EndPos
        /// </remarks>
        public  bool IstEtwasSelektiert
        {
            get
            {
                // Wenn gar kein Cursor gesetzt ist, dann ist auch nix selektiert
                if (this.StartPos.ActualNode == null) return false;

                if ((this.StartPos.PosOnNode == XmlCursorPositions.CursorOnNodeStartTag) ||
                    (this.StartPos.PosOnNode == XmlCursorPositions.CursorOnNodeEndTag))
                {
                    return true; // mindestens ein einzelner Node ist direkt selektiert
                }
                else
                {
                    if (this.StartPos.Equals(this.EndPos))
                    {
                        return false; // offenbar ist der Cursor nur ein Strich mittendrin, ohne etwas selektiert zu haben
                    }
                    else
                    {
                        return true; // Start- und Endpos sind unterschiedlich, daher sollte etwas dazwischen liegen
                    }
                }
            }
        }
    }
}
