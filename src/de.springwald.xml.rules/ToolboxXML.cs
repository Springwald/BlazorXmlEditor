// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace de.springwald.xml
{
    /// <summary>
    /// Hilferoutinen für XML-Verarbeitung
    /// </summary>
    public class ToolboxXML
    {
        /// <summary>
        /// Findet heraus, ob die beiden Nodes in Reihenfolge nacheinander stehen 
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public static bool Node1LaysBeforeNode2(System.Xml.XmlNode node1, System.Xml.XmlNode node2)
        {
            if (node1 == null || node2 == null)
            {
                throw new ApplicationException("Keiner der beiden zu vergleichenden Nodes darf NULL sein (Node1LiegtVorNode2)");
            }

            if (node1.OwnerDocument != node2.OwnerDocument)
            {
                //Debug.Assert(false, "Node1 und Node2 müssen dasselbe OwnerDokument haben");
                return false;
            }
            else
            {
                if (node1 == node2) return false; // Beide Nodes gleich, dann natürlich nicht node1 vor node2
                XPathNavigator naviNode1 = node1.CreateNavigator();
                XPathNavigator naviNode2 = node2.CreateNavigator();
                return naviNode1.ComparePosition(naviNode2) == System.Xml.XmlNodeOrder.Before;
            }

        }

        /// <summary>
        /// Findet (auch über mehrere Stufen der Tiefe hinaus) heraus, ob der angegebene Node als Parent den angegebenen Parent hat
        /// </summary>
        /// <param name="child">Der zu prüfende Node</param>
        /// <param name="parent">Der vermeindliche Parent-Node</param>
        /// <returns></returns>
        public static bool IstChild(System.Xml.XmlNode child, System.Xml.XmlNode parent)
        {
            if (child.ParentNode == null) return false;
            if (child.ParentNode == parent) return true;
            return IstChild(child.ParentNode, parent);
        }


        /// <summary>
        /// Gibt aus einem Textnode den Inhaltstext zurück
        /// </summary>
        /// <param name="textNode">Der Textnode, dessen Inhalt zurückgegeben werden soll</param>
        /// <returns></returns>
        public static string TextAusTextNodeBereinigt(System.Xml.XmlNode textNode)
        {
            if (!(textNode is System.Xml.XmlText) && !(textNode is System.Xml.XmlComment) && !(textNode is System.Xml.XmlWhitespace))
            {
                //"Erhaltener Node ist kein Textnode ({0})"
                throw (new ApplicationException($"Received node is not a textnode  ({textNode.OuterXml})"));
            }
            else
            {
                string ergebnis = textNode.Value.ToString();
                ergebnis = ergebnis.Replace(Environment.NewLine, ""); // Umbrüche aus Text entfernen
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
        /// Behandelt die Whitespaces und lässt nur sichtbare SPACE Whitespaces übrig. Alle Umbrüche und Tabs werden entfernt
        /// </summary>
        public static void CleanUpWhitespaces(System.Xml.XmlNode node)
        {
            if (node == null) return;

            var whites = new List<XmlNode>();
            var restChildren = new List<XmlNode>();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child is XmlWhitespace)
                {
                    whites.Add(child);
                }
                else
                {
                    if (child is XmlElement)
                    {
                        restChildren.Add(child);
                    }
                }
            }

            // Whitespaces behandeln
            foreach (XmlWhitespace white in whites)
            {
                if (white.Data.IndexOf(" ") != -1)
                {
                    // Wenn ein Leerzeichen drin ist, wird das Whitespace auf dieses reduziert, egal
                    // ob noch Umbrüche, Tabs oder ähnliches drin sind
                    XmlText textnode = white.OwnerDocument.CreateTextNode(" ");
                    white.ParentNode.ReplaceChild(textnode, white);
                }
                else
                {
                    // Kein Space im Whitespace, dann das Whitespace löschen
                    white.ParentNode.RemoveChild(white);
                }
            }

            // In den unter-Children die Whitespaces behandeln lassen
            foreach (XmlNode child in restChildren)
            {
                CleanUpWhitespaces(child);
            }

        }
    }
}
