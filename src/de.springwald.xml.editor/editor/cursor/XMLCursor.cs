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
using static de.springwald.xml.rules.XMLCursorPos;

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
        public void BeideCursorPosSetzenOhneChangeEvent(XmlNode node, XMLCursorPositionen posAmNode, int posImTextnode)
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
        public async Task BeideCursorPosSetzenMitChangeEventWennGeaendert(XmlNode node, XMLCursorPositionen posAmNode, int posImTextnode)
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
        /// <summary>
        /// Optimiert den selektierten Bereich 
        /// </summary>
        public async Task SelektionOptimieren()
        {
            // Tauschbuffer-Variablen definieren
            XMLCursorPositionen dummyPos;
            int dummyTextPos;

            if (StartPos.AktNode == null) return; // Nix im Cursor, daher nix zu optimieren

            // 1. Wenn die Startpos hinter der Endpos liegt, dann beide vertauschen
            if (StartPos.AktNode == EndPos.AktNode)  // Beide Nodes sind gleich
            {
                if (StartPos.PosAmNode > EndPos.PosAmNode) // Wenn StartPos innerhalb eines Nodes hinter EndPos liegt
                {
                    // beide Positionen am selben Node tauschen
                    dummyPos = StartPos.PosAmNode;
                    dummyTextPos = StartPos.PosImTextnode;
                    StartPos.SetPos(EndPos.AktNode, EndPos.PosAmNode, EndPos.PosImTextnode);
                    EndPos.SetPos(EndPos.AktNode, dummyPos, dummyTextPos);
                }
                else // StartPos lag nicht hinter Endpos
                {
                    // Ist ist ein Textteil innerhalb eines Textnodes selektiert ?
                    if ((StartPos.PosAmNode == XMLCursorPositionen.CursorInnerhalbDesTextNodes) && (EndPos.PosAmNode == XMLCursorPositionen.CursorInnerhalbDesTextNodes))
                    {  // Ein Teil eines Textnodes ist selektiert
                        if (StartPos.PosImTextnode > EndPos.PosImTextnode) // Wenn die TextStartpos hinter der TextEndpos liegt, dann wechseln
                        {   // Textauswahl tauschen
                            dummyTextPos = StartPos.PosImTextnode;
                            StartPos.SetPos(StartPos.AktNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, EndPos.PosImTextnode);
                            EndPos.SetPos(StartPos.AktNode, XMLCursorPositionen.CursorInnerhalbDesTextNodes, dummyTextPos);
                        }
                    }
                }
            }
            else // Beide Nodes sind nicht gleich
            {
                // Wenn die Nodes in der Reihenfolge falsch sind, dann beide vertauschen
                if (ToolboxXML.Node1LiegtVorNode2(EndPos.AktNode, StartPos.AktNode))
                {
                    XMLCursorPos tempPos = StartPos;
                    StartPos = EndPos;
                    EndPos = tempPos;
                }

                // Wenn der EndNode im StartNode liegt, den gesamten, umgebenden Startnode selektieren
                if (ToolboxXML.IstChild(EndPos.AktNode, StartPos.AktNode))
                {
                    await SetPositions(StartPos.AktNode, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag, 0, throwChangedEventWhenValuesChanged: false);
                }

                // Den ersten gemeinsamen Parent von Start und Ende finden, und in dieser Höhe die Nodes selektieren.
                // Das führt dazu, dass z.B. bei LI-Elemente und UL beim Ziehen der Selektion über mehrere LI immer
                // nur ganze LI selektiert werden und nicht nur Teile davon
                if (StartPos.AktNode.ParentNode != EndPos.AktNode.ParentNode) // wenn Start und Ende nicht direkt im selben Parent stecken
                {
                    // - zuerst herausfinden, welches der tiefste, gemeinsame Parent von Start- und End-node ist
                    System.Xml.XmlNode gemeinsamerParent = XmlCursorSelectionHelper.TiefsterGemeinsamerParent(StartPos.AktNode, EndPos.AktNode);
                    // - dann Start- und End-Node bis vor dem Parent hoch-eskalieren
                    System.Xml.XmlNode nodeStart = StartPos.AktNode;
                    while (nodeStart.ParentNode != gemeinsamerParent) nodeStart = nodeStart.ParentNode;
                    System.Xml.XmlNode nodeEnde = EndPos.AktNode;
                    while (nodeEnde.ParentNode != gemeinsamerParent) nodeEnde = nodeEnde.ParentNode;
                    // - schließlich die neuen Start- und End-Nodes anzeigen  
                    StartPos.SetPos(nodeStart, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
                    EndPos.SetPos(nodeEnde, XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
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
                if (this.StartPos.AktNode == null) return false;

                if ((this.StartPos.PosAmNode == XMLCursorPositionen.CursorAufNodeSelbstVorderesTag) ||
                    (this.StartPos.PosAmNode == XMLCursorPositionen.CursorAufNodeSelbstHinteresTag))
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
