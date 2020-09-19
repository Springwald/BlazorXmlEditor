using System;
using System.Collections;
using System.Xml.XPath;

namespace de.springwald.xml
{
    /// <summary>
    /// Hilferoutinen f�r XML-Verarbeitung
    /// </summary>
    /// <remarks>
    /// (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
    public class ToolboxXML
    {

        #region SYSTEM

        #endregion

        #region PRIVATE ATTRIBUTES

        #endregion

        #region PUBLIC ATTRIBUTES

        #endregion

        #region CONSTRUCTOR
        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Findet heraus, ob die beiden Nodes in Reihenfolge nacheinander stehen 
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public static bool Node1LiegtVorNode2(System.Xml.XmlNode node1, System.Xml.XmlNode node2)
        {
            if (node1 == null || node2 == null)
            {
                throw new ApplicationException("Keiner der beiden zu vergleichenden Nodes darf NULL sein (Node1LiegtVorNode2)");
            }

            if (node1.OwnerDocument != node2.OwnerDocument)
            {
                //Debug.Assert(false, "Node1 und Node2 m�ssen dasselbe OwnerDokument haben");
                return false;
            }
            else
            {
                if (node1 == node2) return false; // Beide Nodes gleich, dann nat�rlich nicht node1 vor node2

                XPathNavigator naviNode1 = node1.CreateNavigator();
                XPathNavigator naviNode2 = node2.CreateNavigator();
                return naviNode1.ComparePosition(naviNode2) == System.Xml.XmlNodeOrder.Before;
            }

        }

        /// <summary>
        /// Findet (auch �ber mehrere Stufen der Tiefe hinaus) heraus, ob der angegebene Node als Parent den angegebenen Parent hat
        /// </summary>
        /// <param name="child">Der zu pr�fende Node</param>
        /// <param name="parent">Der vermeindliche Parent-Node</param>
        /// <returns></returns>
        public static bool IstChild(System.Xml.XmlNode child, System.Xml.XmlNode parent)
        {
            if (child.ParentNode == null) return false;
            if (child.ParentNode == parent) return true;
            return IstChild(child.ParentNode, parent);
        }


        /// <summary>
        /// Gibt aus einem Textnode den Inhaltstext zur�ck
        /// </summary>
        /// <param name="textNode">Der Textnode, dessen Inhalt zur�ckgegeben werden soll</param>
        /// <returns></returns>
        public static string TextAusTextNodeBereinigt(System.Xml.XmlNode textNode)
        {

            if (!(textNode is System.Xml.XmlText) && !(textNode is System.Xml.XmlComment) && !(textNode is System.Xml.XmlWhitespace))
            {
                //"Erhaltener Node ist kein Textnode ({0})"
                throw (new ApplicationException(String.Format("Received node is not a textnode  ({0})", textNode.OuterXml)));
            }
            else
            {
                string ergebnis = textNode.Value.ToString();
                ergebnis = ergebnis.Replace(Environment.NewLine, ""); // Umbr�che aus Text entfernen
                ergebnis = ergebnis.Trim(new char[] { '\n', '\t', '\r', '\v' });
                return ergebnis;
            }
        }


        /// <summary>
        /// Ist dieser Node ein als Text bearbeitbarer Node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IstTextOderKommentarNode(System.Xml.XmlNode node)
        {
            return ((node is System.Xml.XmlText) || (node is System.Xml.XmlComment));
        }


        /// <summary>
        /// Behandelt die Whitespaces und l�sst nur sichtbare SPACE Whitespaces �brig. Alle Umbr�che und Tabs werden entfernt
        /// </summary>
        public static void WhitespacesBereinigen(System.Xml.XmlNode node)
        {
            if (node == null) return;

            ArrayList whites = new ArrayList();
            ArrayList restChildren = new ArrayList();

            foreach (System.Xml.XmlNode child in node.ChildNodes)
            {
                if (child is System.Xml.XmlWhitespace)
                {
                    whites.Add(child);
                }
                else
                {
                    if (child is System.Xml.XmlElement)
                    {
                        restChildren.Add(child);
                    }
                }
            }

            // Whitespaces behandeln
            foreach (System.Xml.XmlWhitespace white in whites)
            {
                if (white.Data.IndexOf(" ") != -1)
                {
                    // Wenn ein Leerzeichen drin ist, wird das Whitespace auf dieses reduziert, egal
                    // ob noch Umbr�che, Tabs oder �hnliches drin sind
                    System.Xml.XmlText textnode = white.OwnerDocument.CreateTextNode(" ");
                    white.ParentNode.ReplaceChild(textnode, white);
                }
                else
                {
                    // Kein Space im Whitespace, dann das Whitespace l�schen
                    white.ParentNode.RemoveChild(white);
                }
            }

            // In den unter-Children die Whitespaces behandeln lassen
            foreach (System.Xml.XmlNode child in restChildren)
            {
                WhitespacesBereinigen(child);
            }

        }

        #endregion

        #region PRIVATE METHODS

        #endregion
    }
}
