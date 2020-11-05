// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static de.springwald.xml.rules.XmlCursorPos;

namespace de.springwald.xml.rules.dtd
{
    /// <summary>
    /// Checks against a DTD whether and which changes to an XML dome are allowed
    /// </summary>
    public class DtdNodeEditCheck
    {
        private Dtd dtd; // The DTD to be checked against

#if DenkProtokoll
		private StringBuilder _denkProtokoll; // What assumptions were used to generate the result of AtThisPosAllowedTags?
#endif

        /// <summary>
        /// What assumptions were used to generate the result of AtThisPosAllowedTags?
        /// </summary>
        public string DenkProtokoll
        {
            get
            {
#if DenkProtokoll
                        return _denkProtokoll.ToString(); 
#else
                return "DenkProtokoll is per Define deaktivated (DTDNodeEditCheck.cs)";
#endif
            }
        }

        public DtdNodeEditCheck(Dtd dtd)
        {
            this.dtd = dtd;

#if DenkProtokoll
			_denkProtokoll= new StringBuilder(); // What assumptions were used to generate the result of AtThisPosAllowedTags?
#endif
        }

        /// <summary>
        /// Which nodes are allowed in XML at this point?
        /// </summary>
        public string[] AtThisPosAllowedTags(XmlCursorPos cursorPosToCheck, bool allowPcDATA, bool allowComments)
        {
            // To avoid accidentally returning some changes, first clone the CursorPos
            var cursorPos = cursorPosToCheck.Clone();

#if DenkProtokoll
			_denkProtokoll=new StringBuilder();
#endif

            var testPattern = this.GetAllTestPattern(cursorPos);

            // Write elements of valid test patterns into the result
            var result = new List<string>();
            foreach (var muster in testPattern)
            {
                if (muster.Success)
                {
                    if (muster.ElementName == null)
                    {
                        result.Add(""); // the existing element may be deleted
                    }
                    else
                    {
                        switch (muster.ElementName.ToLower())
                        {

                            case "#pcdata":
                                if (allowPcDATA) result.Add(muster.ElementName); // This element may be inserted
                                break;

                            case "#comment":
                                if (allowComments) result.Add(muster.ElementName); // This element may be inserted
                                break;

                            default:
                                result.Add(muster.ElementName); // This element may be inserted
                                break;

                        }
                    }
                }
#if DenkProtokoll
				_denkProtokoll.Append(muster.Zusammenfassung + "\r\n");
#endif
            }

            return (result.ToArray());
        }

        /// <summary>
        ///  Is the specified element allowed at this point in the XML?
        /// </summary>
        public bool IsTheNodeAllowedAtThisPos(System.Xml.XmlNode node)
        {
            if (node.ParentNode is System.Xml.XmlDocument)
            {   // It is the root element, this cannot be checked against the parent node, but must be compared separately. If it is the root element allowed in the DTD, then ok, otherwise not
                // Implementation: TO DO!
                return true;
            }
            else
            {
                var cursorPos = new XmlCursorPos();
                cursorPos.SetPos(node, XmlCursorPositions.CursorOnNodeStartTag);

#if DenkProtokoll
				_denkProtokoll=new StringBuilder();
#endif

                // Create the test patterns to insert for all available elements
                var elementName = Dtd.GetElementNameFromNode(node);
                var pattern = this.CreateTestPattern(elementName, cursorPos);

                // Pack into a test sample list and send the list for testing
                var list = new List<DtdTestpattern>();
                list.Add(pattern);
                this.CheckAllTestPattern(list, cursorPos);

                if (pattern.Success)
                {
#if DenkProtokoll
					_denkProtokoll=new StringBuilder();
					_denkProtokoll.Append(muster.Zusammenfassung + "\r\n");
#endif
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Generates all test patterns including the results whether they are allowed
        /// </summary>
        private List<DtdTestpattern> GetAllTestPattern(XmlCursorPos cursorPos)
        {
            var patternToTest = new List<DtdTestpattern>();
            DtdTestpattern singlePattern;

            if (cursorPos.ActualNode == null)
            {
                // How to check what is allowed for a non-existent node?
                throw new ApplicationException("GetAllTestPattern: cursorPos.AktNode=NULL!");
            }

            // Check deletion (register deletion test pattern)
            switch (cursorPos.PosOnNode)
            {
                case XmlCursorPositions.CursorInsideTheEmptyNode:
                case XmlCursorPositions.CursorInFrontOfNode:
                case XmlCursorPositions.CursorBehindTheNode:
                case XmlCursorPositions.CursorInsideTextNode:
                    // Here no deletion must be tested, because no node is selected
                    break;
                case XmlCursorPositions.CursorOnNodeStartTag:
                case XmlCursorPositions.CursorOnNodeEndTag:
                    // Provide delete pattern for testing if the selected node can be deleted
                    //einMuster = CreateTestMuster(null,cursorPos);
                    //zuTestendeMuster.Add(einMuster);
                    break;
                default:
                    throw new ApplicationException(String.Format("unknown cursorPos.StartPos.PosAmNode '{0}' detected.", cursorPos.PosOnNode));
            }

            if (cursorPos.ActualNode is System.Xml.XmlComment)
            {
                // No tags can be inserted in a comment
            }
            else // Is no comment
            {
                string[] childrenAllowedAtThisPos;
                if (cursorPos.PosOnNode == XmlCursorPositions.CursorInsideTheEmptyNode)
                {
                    // All children of this node are allowed in the node
                    var element = dtd.DTDElementByName(cursorPos.ActualNode.Name, false);
                    if (element == null)
                    {
                        // An element with this name is not known
                        childrenAllowedAtThisPos = new string[] { };
                    }
                    else
                    {
                        childrenAllowedAtThisPos = element.AllChildNamesAllowedAsDirectChild;
                    }
                }
                else
                {
                    // Which elements are *next to* the element allowed?   
                    if (cursorPos.ActualNode.OwnerDocument == null)
                    {
                        // The actual node does not hang in any document? Hm, maybe we are in the middle of an insert process...
#warning Noch eine korrekte Meldung oder Ton einfügen
                        Debug.Assert(false, "Beep!");
                        childrenAllowedAtThisPos = new string[] { };
                    }
                    else
                    {
                        if (cursorPos.ActualNode == cursorPos.ActualNode.OwnerDocument.DocumentElement)
                        {
                            // This node is the document tag itself. This is exclusive on the root, so there can be no other elements besides it
                            childrenAllowedAtThisPos = new string[] { };
                        }
                        else
                        {
                            // All children of the parent are allowed next to or in the place of the node. 
                            // First find out which is the parent element of the node to check for
                            var parentElement = dtd.DTDElementByName(cursorPos.ActualNode.ParentNode.Name, false);
                            if (parentElement == null)
                            {
                                // An element with this name is not known
                                childrenAllowedAtThisPos = new string[] { };
                            }
                            else
                            {
                                childrenAllowedAtThisPos = parentElement.AllChildNamesAllowedAsDirectChild;
                            }
                        }
                    }
                }

                // Create test patterns to insert for all allowed elements
                foreach (string elementName in childrenAllowedAtThisPos)
                {
                    //if (element.Name =="pattern") //  || element.Name =="template") 
                    //if (element.Name == "#PCDATA")
                    //if (element.Name == "meta")
                    {
                        singlePattern = CreateTestPattern(elementName, cursorPos);
                        patternToTest.Add(singlePattern);
                    }
                }
            }
            // check all collected test samples for validity
            this.CheckAllTestPattern(patternToTest, cursorPos);
            return patternToTest;
        }

        /// <summary>
        /// Checks all test patterns for validity within the scope of the read DTD
        /// </summary>
        private void CheckAllTestPattern(List<DtdTestpattern> allPattern, XmlCursorPos cursorPos)
        {
            var node = cursorPos.ActualNode;
            DtdElement element_;

            if (cursorPos.PosOnNode == XmlCursorPositions.CursorInsideTheEmptyNode)
            {
                // Get the DTD element for the node of the cursor 
                element_ = dtd.DTDElementByName(Dtd.GetElementNameFromNode(node), false);
            }
            else
            {
                if ((node.OwnerDocument == null) || (node.OwnerDocument.DocumentElement == null))
                {
                    Debug.Assert(false, "Beep!");
                    return;
                }
                else
                {
                    if (node == node.OwnerDocument.DocumentElement) // The node is the root element
                    {
                        // Only the root element is allowed in place of the root element
                        foreach (DtdTestpattern muster in allPattern) 
                        {
                            if (muster.ElementName == node.Name) // if it is the root element
                            {
                                muster.Success = true; // Only the root element is allowed at the position of the root element 
                            }
                        }
                        return;
                    }
                    else // The node is not the root element
                    {
                        // Get the DTD element for the parent node of the cursor 
                        element_ = dtd.DTDElementByName(Dtd.GetElementNameFromNode(node.ParentNode), false);
                    }
                }
            }

            // Check whether the current DTD run has led to one of the searched test patterns
            foreach (DtdTestpattern muster in allPattern)  // run through all samples to be tested
            {
                if (element_ == null)
                {
                    muster.Success = false; // This element is not known at all
                }
                else
                {
                    if (!muster.Success)
                    {
#if DEBUGTRACE
				    Trace.WriteLine(String.Format("Check für neues Ziel-Muster {0} > {1}",  ElementName(muster.Element) ,  muster.Zusammenfassung_ ));
#endif
                        muster.Success = FitsPatternInElement(muster, element_);
                    }
                }
            }
        }

        private bool FitsPatternInElement(DtdTestpattern pattern, DtdElement element)
        {
            Match match = element.ChildrenRegExObjekt.Match(pattern.CompareStringForRegEx);
            return match.Success;
        }

        /// <summary>
        /// Adds a test pattern
        /// </summary>
        private DtdTestpattern CreateTestPattern(string elementName, XmlCursorPos cursorPos)
        {
            DtdTestpattern testPattern;
            var node = cursorPos.ActualNode;
            System.Xml.XmlNode sibling;

            switch (cursorPos.PosOnNode)
            {
                case XmlCursorPositions.CursorInsideTheEmptyNode:
                    // The parentnode is empty, so we only have to test for the allowed elements in it and not expect any sibling elements on the same level
                    testPattern = new DtdTestpattern(elementName, Dtd.GetElementNameFromNode(node));
                    testPattern.AddElement(elementName);
                    break;

                default:
                    // If the parent node is the XML document itself, then abort here
                    if (node.ParentNode is System.Xml.XmlDocument)
                    {
                        throw new ApplicationException("No test pattern can be created for the root element. Its validity must be guaranteed by comparison with the DTD root element.");
                    }

                    testPattern = new DtdTestpattern(elementName, Dtd.GetElementNameFromNode(node.ParentNode));

                    // Traverse all elements within the parent element
                    sibling = node.ParentNode.FirstChild;
                    while (sibling != null)
                    {
                        if (sibling is System.Xml.XmlWhitespace)
                        {
                            // Whitespace tags can be ignored during the check
                        }
                        else
                        {
                            if (sibling == node) // at this point the node must be inserted
                            {
                                if (sibling is System.Xml.XmlComment)
                                {
                                    testPattern.AddElement("#COMMENT");
                                }
                                else
                                {
                                    if (this.dtd.DTDElementByName(Dtd.GetElementNameFromNode(node), false) == null)
                                    {
                                        // This element is not known at all, therefore the element is sometimes not included
                                        //throw new ApplicationException(String.Format("unknown Node-Element '{0}'", DTD.GetElementNameFromNode(node)));
                                    }
                                    else
                                    {
                                        switch (cursorPos.PosOnNode)
                                        {

                                            case XmlCursorPositions.CursorOnNodeStartTag:	// If the node itself is selected and should be replaced
                                            case XmlCursorPositions.CursorOnNodeEndTag:
                                                if (elementName == null) // check delete
                                                {
                                                    // Omit element
                                                }
                                                else //  check insert/replace
                                                {
                                                    // Instead of the element present at this position, the element to be tested is inserted here
                                                    testPattern.AddElement(elementName);
                                                }
                                                break;

                                            case XmlCursorPositions.CursorBehindTheNode:
                                                if (elementName == null)  // check delete
                                                {
                                                    throw new ApplicationException("CreateTestPattern: Delete must not be checked for XmlCursorPositions.CursorBehindTheNode!");
                                                }
                                                else
                                                {
                                                    // element is inserted behind the element to be tested
                                                    testPattern.AddElement(Dtd.GetElementNameFromNode(node));
                                                    testPattern.AddElement(elementName);
                                                }
                                                break;

                                            case XmlCursorPositions.CursorInsideTheEmptyNode:
                                                if (elementName == null)  // check delete
                                                {
                                                    throw new ApplicationException("CreateTestPattern: Delete must not be checked for XmlCursorPositions.CursorInsideTheEmptyNode!");
                                                }
                                                else
                                                {
                                                    throw new ApplicationException("CreateTestPattern: CursorInsideTheEmptyNode can´t be handled at this place!");
                                                }


                                            case XmlCursorPositions.CursorInFrontOfNode:
                                                if (elementName == null)  // check delete
                                                {
                                                    throw new ApplicationException("CreateTestPattern: Delete must not be checked for XmlCursorPositions.CursorInFrontOfNode!");
                                                }
                                                else
                                                {
                                                    // Element is inserted before the element to be tested
                                                    testPattern.AddElement(elementName);
                                                    testPattern.AddElement(Dtd.GetElementNameFromNode(node));
                                                }
                                                break;

                                            case XmlCursorPositions.CursorInsideTextNode:
                                                if (elementName == null)  // check delete
                                                {
                                                    throw new ApplicationException("CreateTestPattern: Delete must not be checked for XmlCursorPositions.CursorInsideTextNode!");
                                                }
                                                else
                                                {
                                                    if (Dtd.GetElementNameFromNode(node) != "#PCDATA")
                                                    {
                                                        throw new ApplicationException("CreateTestPattern: CursorInsideTextNode, but node.name=" + Dtd.GetElementNameFromNode(node));
                                                    }
                                                    else
                                                    {
                                                        // The element to be tested is placed between two text nodes
                                                        testPattern.AddElement("#PCDATA");
                                                        testPattern.AddElement(elementName);
                                                        testPattern.AddElement("#PCDATA");
                                                    }
                                                }
                                                break;

                                            default:
                                                throw new ApplicationException("Unknown XmlCursorPositions value:" + cursorPos.PosOnNode);
                                        }
                                    }
                                }
                            }
                            else // just continue enumerating the elements as usual
                            {
                                testPattern.AddElement(Dtd.GetElementNameFromNode(sibling));
                            }
                        }
                        sibling = sibling.NextSibling; // to the next element
                    }
                    break;
            }
            return testPattern;
        }
    }
}
