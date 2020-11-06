// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Threading.Tasks;
using System.Xml;
using de.springwald.xml.editor.cursor;
using de.springwald.xml.rules;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.cursor
{
    public enum MouseClickActions { MouseDown, MouseDownMove, MouseUp };

    public partial class XmlCursor : IDisposable
    {
        public XmlAsyncEvent<EventArgs> ChangedEvent { get;  }

        public XmlCursorPos StartPos { get;  }

        public XmlCursorPos EndPos { get; }

        public XmlCursor()
        {
            this.EndPos = new XmlCursorPos();
            this.StartPos = new XmlCursorPos();
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

        public bool Equals(XmlCursor second)
        {
            return second != null && this.StartPos.Equals(second.StartPos) && this.EndPos.Equals(second.EndPos);
        }

        public XmlCursor Clone()
        {
            XmlCursor klon = new XmlCursor();
            klon.StartPos.SetPos(StartPos.ActualNode, StartPos.PosOnNode, StartPos.PosInTextNode);
            klon.EndPos.SetPos(EndPos.ActualNode, EndPos.PosOnNode, EndPos.PosInTextNode);
            return klon;
        }

        /// <summary>
        /// Triggers the Cursor-Changed-Event manually
        /// </summary>
        public async Task ForceChangedEvent()
        {
            await this.ChangedEvent.Trigger(EventArgs.Empty);
        }

        public void BeideCursorPosSetzenOhneChangeEvent(XmlNode node, XmlCursorPositions posAmNode, int posImTextnode)
        {
            // Cursor setzen
            this.StartPos.SetPos(node, posAmNode, posImTextnode);
            this.EndPos.SetPos(node, posAmNode, posImTextnode);
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
        public async Task BeideCursorPosSetzenMitChangeEventWennGeaendert(XmlNode node, XmlCursorPositions posAmNode)
        {
            await BeideCursorPosSetzenMitChangeEventWennGeaendert(node, posAmNode, 0);
        }

        /// <summary>
        /// Setzt die Positionen des Cursors für entsprechende Mausaktionen:
        /// Bei MausDown StartUndEndpos, bei Move und Up nur die Endpos
        /// </summary>
        /// <param name="action"></param>
        public async Task CursorPosSetzenDurchMausAktion(System.Xml.XmlNode xmlNode, XmlCursorPositions cursorPos, int posInZeile, MouseClickActions action)
        {
            switch (action)
            {
                case MouseClickActions.MouseDown:
                    // den Cursor an die neue Position setzen
                    await SetPositions(xmlNode, cursorPos, posInZeile, throwChangedEventWhenValuesChanged: true);
                    break;
                case MouseClickActions.MouseDownMove:
                case MouseClickActions.MouseUp:
                    // Ende des Select-Cursors setzen
                    if (EndPos.SetPos(xmlNode, cursorPos, posInZeile))
                    {
                        await this.ForceChangedEvent();
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
        public async Task CursorPosSetzenDurchMausAktion(System.Xml.XmlNode xmlNode, XmlCursorPositions cursorPos, MouseClickActions aktion)
        {
            await CursorPosSetzenDurchMausAktion(xmlNode, cursorPos, 0, aktion);
        }

        /// <summary>
        /// Optimiert den selektierten Bereich 
        /// </summary>
        public async Task OptimizeSelection()
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
                if (ToolboxXml.Node1LaisBeforeNode2(EndPos.ActualNode, StartPos.ActualNode))
                {
                    var tempPos = this.StartPos.Clone();
                    this.StartPos.SetPos(this.EndPos.ActualNode, this.EndPos.PosOnNode, this.EndPos.PosInTextNode);
                    this.EndPos.SetPos(tempPos.ActualNode, tempPos.PosOnNode, tempPos.PosInTextNode);
                }

                // Wenn der EndNode im StartNode liegt, den gesamten, umgebenden Startnode selektieren
                if (ToolboxXml.IsChild(EndPos.ActualNode, StartPos.ActualNode))
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
        public  bool IsSomethingSelected
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
