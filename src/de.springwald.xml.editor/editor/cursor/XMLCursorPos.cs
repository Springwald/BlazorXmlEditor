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

namespace de.springwald.xml.cursor
{
    public partial class XMLCursorPos
    {
        private int posImTextnode;                 // Dort befindet sich der Cursor im Fließtext, wenn die Pos CursorInnerhalbDesTextNodes ist


        /// <summary>
        /// Auf diesem XML-Node liegt gerade der Fokus des XMLEditors
        /// </summary>
        public System.Xml.XmlNode AktNode { get; private set; }

        /// <summary>
        /// Dort befindet sich der Cursor im Fließtext, wenn dei Pos CursorInnerhalbDesTextNodes ist
        /// </summary>
        public int PosImTextnode { get; private set; }

        /// <summary>
        /// Dort befindet sich der Cursor innerhalb oder außerhalb des fokusierten XMLNodes
        /// </summary>
        public XMLCursorPositionen PosAmNode { get; private set; }

        public XMLCursorPos()
        {
            this.AktNode = null;  // Kein Node angewählt
            this.PosAmNode = XMLCursorPositionen.CursorAufNodeSelbstVorderesTag;
            this.PosImTextnode = 0;
        }

        /// <summary>
        /// Prüft ob diese Position mit einer zweiten inhaltsgleich ist
        /// </summary>
        /// <param name="zweitePos"></param>
        public bool Equals(XMLCursorPos zweitePos)
        {
            if (this.AktNode != zweitePos.AktNode) return false;
            if (this.PosAmNode != zweitePos.PosAmNode) return false;
            if (this.PosImTextnode != zweitePos.PosImTextnode) return false;
            return true;
        }

        /// <summary>
        /// Erstellt eine Kopie des Cursors
        /// </summary>
        /// <returns></returns>
        public XMLCursorPos Clone()
        {
            XMLCursorPos klon = new XMLCursorPos();
            klon.SetPos(this.AktNode, this.PosAmNode, this.PosImTextnode);
            return klon;
        }

        /// <summary>
        /// Prüft, ob der angegebene Node hinter dieser CursorPosition liegt
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool LiegtNodeHinterDieserPos(System.Xml.XmlNode node)
        {
            return ToolboxXML.Node1LiegtVorNode2(AktNode, node);
        }

        /// <summary>
        /// Prüft, ob der angegebene Node vor dieser CursorPosition liegt
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool LiegtNodeVorDieserPos(System.Xml.XmlNode node)
        {
            return ToolboxXML.Node1LiegtVorNode2(node, AktNode);
        }

        /// <summary>
        /// Sets new values to this cursor pos
        /// </summary>
        /// <param name="aktNode"></param>
        /// <param name="posAmNode"></param>
        /// <param name="posImTextnode"></param>
        /// <returns>true, when values where other than before</returns>
        public bool SetPos(System.Xml.XmlNode aktNode, XMLCursorPositionen posAmNode, int posImTextnode = 0)
        {
            bool changed;
            if (aktNode != this.AktNode)
            {
                changed = true;
            }
            else
            {
                if (posAmNode != this.PosAmNode)
                {
                    changed = true;
                }
                else
                {
                    if (posImTextnode != this.PosImTextnode)
                    {
                        changed = true;
                    }
                    else
                    {
                        changed = false;
                    }
                }
            }

            this.AktNode = aktNode;
            this.PosAmNode = posAmNode;
            this.PosImTextnode = posImTextnode;
            return changed;
        }


    }
}
