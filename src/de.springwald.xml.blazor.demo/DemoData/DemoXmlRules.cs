// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.rules;
using de.springwald.xml.rules.dtd;
using System.Collections;
using System.Text;
using System.Xml;

namespace de.springwald.xml.blazor.demo.DemoData
{
    /// <summary>
    /// The rules of how the AIML XML elements relate to each other
    /// </summary>
    public class DemoXmlRules : XmlRules
    {
        /// <summary>
        /// The groupings in which the XML elements are offered for insertion
        /// </summary>
        public override List<XmlElementGroup> ElementGroups
        {
            get
            {
                if (elementGroups == null)
                {
                    elementGroups = new List<XmlElementGroup>();

                    // Collapse unimportant groups first?
                    const bool collapsed = true;

                    // The group of standard elements
                    var standard = new XmlElementGroup("standard", false);
                    standard.AddElementName("bot");
                    standard.AddElementName("get");
                    standard.AddElementName("li");
                    standard.AddElementName("pattern");
                    standard.AddElementName("random");
                    standard.AddElementName("set");
                    standard.AddElementName("srai");
                    standard.AddElementName("sr");
                    standard.AddElementName("star");
                    standard.AddElementName("template");
                    standard.AddElementName("that");
                    standard.AddElementName("thatstar");
                    standard.AddElementName("think");
                    elementGroups.Add(standard);

                    // The group of advanced elements
                    var fortschritten = new XmlElementGroup("advanced", collapsed);
                    fortschritten.AddElementName("condition");
                    fortschritten.AddElementName("formal");
                    fortschritten.AddElementName("gender");
                    fortschritten.AddElementName("input");
                    fortschritten.AddElementName("person");
                    fortschritten.AddElementName("person2");
                    fortschritten.AddElementName("sentence");
                    elementGroups.Add(fortschritten);

                    // The group of HTML elements
                    var html = new XmlElementGroup("html", collapsed);
                    html.AddElementName("a");
                    html.AddElementName("applet");
                    html.AddElementName("br");
                    html.AddElementName("em");
                    html.AddElementName("img");
                    html.AddElementName("p");
                    html.AddElementName("table");
                    html.AddElementName("ul");
                    elementGroups.Add(html);

                    // The group of special GaitoBot elements
                    var gaitobot = new XmlElementGroup("GaitoBot", collapsed);
                    gaitobot.AddElementName("script");
                    elementGroups.Add(gaitobot);
                }
                return elementGroups;
            }
        }

        public DemoXmlRules(Dtd dtd) : base(dtd) { }

        /// <summary>
        /// Finds out what color the node should have
        /// </summary>
        public override Color NodeColor(XmlNode node)
        {
            switch (node.Name)
            {
                case "condition":
                    return Color.FromArgb(150,
                                          221,
                                          220);

                case "li":
                    switch (node.ParentNode.Name)
                    {
                        case "random":
                            return Color.FromArgb(255, 243, 187);
                        case "condition":
                            return Color.FromArgb(200, 250, 250);
                    }
                    break;

                case "random":
                    return Color.FromArgb(255, 211, 80);

                case "think":
                    return Color.FromArgb(200, 200, 200);
            }
            return base.NodeColor(node);
        }

        /// <summary>
        /// Is the passed node drawn 2x, once with > and once with < ?
        /// </summary>
        public override bool HasEndTag(XmlNode xmlNode)
        {
            switch (xmlNode.Name)
            {
                case "that": // One "That" in the template is closed continuous text element, one in the category is a separate open line

                    if (xmlNode.ParentNode.Name == "template")  // "that" lies im template node
                    {
                        return false;
                    }
                    else  // is not inside the template tag
                    {
                        return true;
                    }

                default:
                    return base.HasEndTag(xmlNode);
            }
        }

        /// <summary>
        /// In what way should the node be drawn?
        /// </summary>
        public override DisplayTypes DisplayType(XmlNode xmlNode)
        {
            if (xmlNode is System.Xml.XmlElement)
            {
                switch (xmlNode.Name)
                {
                    case "a":
                    case "set":
                    case "bot":
                    case "formal":
                    case "gender":
                    case "person":
                    case "person2":
                        return DisplayTypes.FloatingElement;

                    case "think": // If a "think" occurs directly after the "template", then it gets its own line, if it occurs in the body text, then not

                        if (xmlNode.ParentNode.Name == "template")  // "think" lies inside template node
                        {
                            if (xmlNode.PreviousSibling != null) // there is an element before the "think"
                            {
                                if (xmlNode.PreviousSibling.Name == "think")
                                {
                                    // Two Thinks/Thats on top of each other? To avoid recursion, where both Thinks ask each other as Sibling, the wrap is returned here
                                    // return DarstellungsArten.EigeneZeile ;
                                }
                                else
                                {
                                    if (DisplayType(xmlNode.PreviousSibling) == DisplayTypes.FloatingElement)
                                    {
                                        // directly in front of the "think" is a floating text element, so the think is one too
                                        return DisplayTypes.FloatingElement;
                                    }
                                }
                            }

                            if (xmlNode.NextSibling != null) // there is an element after the "think"
                            {
                                if (DisplayType(xmlNode.NextSibling) == DisplayTypes.FloatingElement)
                                {
                                    // directly in front of the "think" is a floating text element, so the "think" is one too
                                    return DisplayTypes.FloatingElement;
                                }
                            }
                            return DisplayTypes.OwnRow;
                        }
                        else
                        {
                            return DisplayTypes.FloatingElement;  // is not in the template tag
                        }


                    case "that": // One "That" in the template is a flow text element, one in the category a separate line

                        if (xmlNode.ParentNode.Name == "template")  // "that" lies inside template node
                        {
                            return DisplayTypes.FloatingElement;
                        }
                        else  // is not inside the template tag
                        {
                            return DisplayTypes.OwnRow;
                        }

                    case "br":
                    case "p":
                        return DisplayTypes.OwnRow;

                    default: return base.DisplayType(xmlNode);
                }
            }
            return base.DisplayType(xmlNode);
        }

        /// <summary>
        /// Converts / formats a text which is to be inserted into a specific location as required by that location. 
        /// In an AIML DTD, for example, this can mean that the text is converted to uppercase for insertion into the PATTERN tag.
        /// </summary>
        /// <param name="replacementNode">If a node is to be inserted instead of the text. Example: In the AIML template we press *, then a STAR tag is inserted.</param>
        public override string InsertTextTextPreProcessing(string insertText, XmlCursorPos insertWhere, out XmlNode replacementNode)
        {
            XmlNode node;

            if (insertWhere.ActualNode is XmlText)
            { // Pos id a Textnode
                // Node is the parent of the text node
                node = insertWhere.ActualNode.ParentNode;
            }
            else
            { // Pos is not a Textnode
                // The Pos itself is the node
                node = insertWhere.ActualNode;
            }

            string ausgabe;

            // In certain places when pressing * use SRAI instead of it
            if (insertText == "*")
            {
                switch (node.Name)
                {
                    case "pattern":
                    case "that":
                    case "script":
                        // Here is the normal star allowed
                        break;
                    default:
                        replacementNode = insertWhere.ActualNode.OwnerDocument.CreateElement("star");
                        return ""; // Empty the insert-text, because it was already returned as a star-node
                }
            }

            // Allow / filter out different inputs depending on the node
            switch (node.Name)
            {

                case "srai":        // In Srai tag always capital letters and no special characters
                    ausgabe = insertText;
                    ausgabe = ausgabe.Replace("*", ""); // No * allowed in SRAI
                    ausgabe = ausgabe.Replace("_", ""); // No _ allowed in SRAI
                    replacementNode = null!;
                    return ausgabe;

                case "pattern":     // In the pattern tag always capital letters and no special characters

                    StringBuilder sauber = new StringBuilder(insertText); // einfuegeText.ToUpper());
                    // Already write out "german Umlaute" when entering
                    sauber.Replace("Ä", "AE");
                    sauber.Replace("Ö", "OE");
                    sauber.Replace("Ü", "UE");
                    sauber.Replace("ß", "SS");

                    // convert to a char array
                    char[] tempArray = sauber.ToString().ToCharArray();

                    ArrayList doneCharacters = new ArrayList();

                    // iterate through the char array 
                    for (int i = 0; i < tempArray.Length; i++)
                    {
                        if (((tempArray[i] == '*') || (tempArray[i] == '_')) && (node.Name == "pattern"))
                        {
                            doneCharacters.Add((char)tempArray[i]); // * and _ are only allowed in patterns, not in SRAI
                        }
                        else
                        {
                            // check its a valid character...
                            // valid in this case means:
                            // " "(space), "0-9", "a-z" and "A-Z"
                            if ((tempArray[i] > 64) & (tempArray[i] < 91) || // A-Z
                                (tempArray[i] > 96) & (tempArray[i] < 123) || // a-z
                                (tempArray[i] > 47) & (tempArray[i] < 58) || // 0-9
                                (tempArray[i] == 32))  // space
                            {
                                doneCharacters.Add((char)tempArray[i]);
                            }
                        }
                    }

                    // turn the arraylist into a char array
                    char[] done = new char[doneCharacters.Count];

                    for (int i = 0; i < doneCharacters.Count; i++)
                    {
                        done[i] = (char)doneCharacters[i];
                    }
                    ausgabe = new string(done);

                    // et voila!
                    replacementNode = null;
                    return ausgabe;

                default:
                    return base.InsertTextTextPreProcessing(insertText, insertWhere, out replacementNode);
            }
        }

        /// <summary>
        /// Determines which elements are allowed at this position
        /// </summary>
        public override string[] AllowedInsertElements(XmlCursorPos targetPos, bool listPcDataToo, bool listCommentsToo)
        {
            if (targetPos.ActualNode != null)
            {
                if (targetPos.ActualNode.Name.ToLower() == "category")
                {
                    // No other tags are offered as alternatives instead of the category tag.
                    // Otherwise, the tags META and TOPIC would be displayed when editing the category, since they would be allowed at this point according to the DTD.
                    return new string[] { };
                }
            }
            return base.AllowedInsertElements(targetPos, listPcDataToo, listCommentsToo);
        }
    }
}
