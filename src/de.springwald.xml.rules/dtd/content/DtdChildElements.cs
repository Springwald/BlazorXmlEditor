// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace de.springwald.xml.rules.dtd
{
    /// <summary>
    /// Manages the child elements of a DTD element, i.e. the parts within the brackets of the element tag
    /// </summary>
    public class DtdChildElements
    {
        /// <summary>
        /// What kind of child is the given child?
        /// </summary>
        public enum DtdChildElementTypes { Empty = 0, SingleChild = -1, ChildList = 2 };

        /// <summary>
        /// This element may occur at the specified position as often as
        /// </summary>
        /// <remarks>
        /// ExactOnce=no char
        /// NoneAndOnce=*
        /// OnceAndMore=+
        /// </remarks>
        public enum DtdChildElementAmounts { ExactOnce = 0, NoneAndMore = -1, NoneAndOnce = 2, OnceAndMore = 3 };

        /// <summary>
        /// Are the child elements in this list separated by OR, or must they occur in the specified order
        /// </summary>
        public enum DtdChildElementOperators { FollowedBy = 0, Or = -1 };

        private string sourceCode;
        private List<DtdChildElements> _children;		    // The children of this child area
        private Dtd _dtd;                                   // The DTD, on which everything is based

        private AllPossibleElementsOfAChildBlock allPossibleElements; // Determines all elements which this childblock can ever cover / contain

        private string regExValue;  // The RegEx expression corresponding to this Childblock

        /// <summary>
        /// The RegEx expression corresponding to this Childblock
        /// </summary>
        public string RegExAusdruck
        {
            get
            {
                if (regExValue == null)
                {
                    var value = new StringBuilder();
                    value.Append("(");

                    switch (this.ElementType)
                    {
                        case DtdChildElementTypes.Empty:
                            break;
                        case DtdChildElementTypes.SingleChild:
                            if (this.ElementName != "#COMMENT")
                            {
                                value.AppendFormat("((-#COMMENT)*-{0}(-#COMMENT)*)", this.ElementName);
                            }
                            else
                            {
                                value.AppendFormat("(-{0})", this.ElementName);
                            }

                            break;
                        case DtdChildElementTypes.ChildList:
                            value.Append("(");
                            for (int i = 0; i < _children.Count; i++)
                            {
                                if (i != 0)
                                {
                                    switch (this.Operator)
                                    {
                                        case DtdChildElementOperators.Or:
                                            value.Append("|");
                                            break;
                                        case DtdChildElementOperators.FollowedBy:
                                            break;
                                        default:
                                            throw new ApplicationException("Unhandled DTDChildElementOperatoren '" + this.Operator + "'");
                                    }
                                }
                                value.Append(((DtdChildElements)_children[i]).RegExAusdruck);
                            }
                            value.Append(")");
                            break;
                        default:
                            throw new ApplicationException("Unhandled DTDChildElementArt '" + this.ElementType + "'");
                    }

                    // Die Anzahl anfügen
                    switch (this.DefCount)
                    {
                        case DtdChildElementAmounts.OnceAndMore:
                            value.Append("+");
                            break;
                        case DtdChildElementAmounts.ExactOnce:
                            break;
                        case DtdChildElementAmounts.NoneAndOnce:
                            value.Append("?");
                            break;
                        case DtdChildElementAmounts.NoneAndMore:
                            value.Append("*");
                            break;
                        default:
                            throw new ApplicationException("Unhandled DTDChildElementAnzahl '" + this.DefCount + "'");
                    }

                    value.Append(")");
                    regExValue = value.ToString();
                }
                return regExValue;
            }
        }

        /// <summary>
        /// Determines all elements of the specified DTD, which this Childblock can ever cover / contain
        /// </summary>
        public AllPossibleElementsOfAChildBlock AllPossibleElements
        {
            get
            {
                if (this.allPossibleElements == null)
                {
                    this.allPossibleElements = new AllPossibleElementsOfAChildBlock(this);
                }
                return this.allPossibleElements;
            }
        }

        public string SourceCode => this.sourceCode;

        /// <summary>
        /// This type is this child area
        /// </summary>
        public DtdChildElementTypes ElementType { get; protected set; }

        /// <summary>
        /// So often this Childblock may occur
        /// </summary>
        public DtdChildElementAmounts DefCount { get; protected set; }

        /// <summary>
        /// Are the child elements in this list separated by OR, or must they occur in the specified order
        /// </summary>
        public DtdChildElementOperators Operator { get; protected set; }

        public int ChildrenCount
        {
            get { return _children.Count; }
        }

        /// <summary>
        /// Wenn dies ein EinzelChild ist, dann wird hier der Name des ChildElementes vermerkt
        /// </summary>
        /*public DTDElement Element
		{
			get {
                if (_element == null)
                {
                    if (_art != DTDChildElementArten.EinzelChild) // Nur für EinzelChildren kann es einen Elementnamen geben
                    {
                        throw new ApplicationException(
                            // "Der ElementName für DTDChildElemente kann nur abgerufen werden, wenn der ChildElementBlock der Art 'EinzelChild' ist.\n\n(Betroffener Block:'{0}', erkannte Art:{1})",
                            String.Format(ResReader.Reader.GetString("ElementNameKannNichtAbgerufenWerden"), _quellcode, _art));
                    }
                    _element = _dtd.DTDElementByName(_elementName);
                }
				return _element; 
			}
		}*/


        /// <summary>
        /// Wenn dies ein EinzelChild ist, dann wird hier der Name des ChildElementes vermerkt
        /// </summary>
        public string ElementName { get; protected set; }

        private DtdChildElements()
        {
        }

        /// <summary>
        /// Stellt einen ChildElemente-Block auf Basis des übergebenen DTD-Quellcodes bereit
        /// </summary>
        /// <param name="childrenSourcCode">
        /// Der DTD-Quellcode der ChildElemente
        /// </param>
        /// <example>
        /// So kann z.B. eine Quellcodeangabe aussehen:
        /// (#PCDATA | srai | sr | that | get | bot | birthday | set | A | star | random )*
        /// </example>
        public DtdChildElements(string childrenSourcCode)
        {
            // Grundwerte initialisieren
            this.ElementType = DtdChildElementTypes.Empty;
            this._children = new List<DtdChildElements>();
            this.DefCount = DtdChildElementAmounts.ExactOnce;
            this.ElementName = "";
            this.Operator = DtdChildElementOperators.Or;
            this.sourceCode = childrenSourcCode;

            // remove tabs, line breaks and double space 
            this.sourceCode = this.sourceCode.Replace("\t", " ");
            this.sourceCode = this.sourceCode.Replace("\r\n", " ");
            this.sourceCode = this.sourceCode.Trim();

            if (this.sourceCode.Length == 0) // No child specified
            {
                this.ElementType = DtdChildElementTypes.Empty;
            }
            else // There are children available
            {
                this.ReadCode();
            }

        }

        /// <summary>
        /// Creates a copy of this ChildBlock
        /// </summary>
        public DtdChildElements Clone()
        {
            var clone = new DtdChildElements()
            {
                allPossibleElements = null,
                ElementName = this.ElementName,
                ElementType = this.ElementType,
                Operator = this.Operator,
                DefCount = this.DefCount,
                _dtd = this._dtd,
                sourceCode = this.sourceCode,
            };
            clone._children = new List<DtdChildElements>();
            clone._children.AddRange(this._children);
            return clone;
        }

        /// <summary>
        /// Assigns this child which DTD it belongs to
        /// </summary>
        /// <param name="dtd"></param>
        public void AssignDtd(Dtd dtd)
        {
            // Pass on to the sub-children
            foreach (DtdChildElements child in _children)
            {
                child.AssignDtd(dtd);
            }

            this._dtd = dtd;
        }

        /// <summary>
        ///  checks whether the specified number is allowed for this element block
        /// </summary>
        public bool CountAllowed(int count)
        {
            switch (this.DefCount)
            {
                case (DtdChildElementAmounts.OnceAndMore):
                    if (count >= 1) return true; else return false;
                case (DtdChildElementAmounts.ExactOnce):
                    if (count == 1) return true; else return false; ;
                case (DtdChildElementAmounts.NoneAndOnce):
                    if (count == 0 || count == 1) return true; else return false; ;
                case (DtdChildElementAmounts.NoneAndMore):
                    if (count >= 0) return true; else return false; ;
                default:
                    // "unknown DTDChildElementAnzahl: {0}"
                    throw new ApplicationException($"unknown DTDChildElementAnzahl: {this.DefCount}");
            }
        }

        /// <summary>
        /// The index'ht child or childlist
        /// </summary>
        /// <param name="index">Number of the desired Child, zero-based</param>
        public DtdChildElements Child(int index)
        {
            return (DtdChildElements)this._children[index];
        }

        /// <summary>
        /// Processes the source code to Children
        /// </summary>
        private void ReadCode()
        {
            string code = this.SourceCode;

            // How often may this ChildBlock occur
            var countString = code.Substring(code.Length - 1, 1);
            switch (countString)
            {
                case "+":
                    this.DefCount = DtdChildElementAmounts.OnceAndMore;
                    code = code.Remove(code.Length - 1, 1); // remove +
                    break;
                case "*":
                    this.DefCount = DtdChildElementAmounts.NoneAndMore;
                    code = code.Remove(code.Length - 1, 1); // remove *
                    break;
                case "?":
                    this.DefCount = DtdChildElementAmounts.NoneAndOnce;
                    code = code.Remove(code.Length - 1, 1); // remove ?
                    break;
                default:
                    this.DefCount = DtdChildElementAmounts.ExactOnce;
                    break;
            }
            code = code.Trim();

            // Check whether a clamp block is present
            if ((code.Substring(0, 1) == "(") && (code.Substring(code.Length - 1, 1) == ")"))
            { // There are brackets, so there are several children 
                code = code.Substring(1, code.Length - 2); // Remove the brackets
                this.ReadChildren(code); // Recognize the children
            }
            else
            { // No brackets available, then it is probably only a single child
                this.ElementType = DtdChildElementTypes.SingleChild;
                this.ElementName = code;
            }
        }

        /// <summary>
        /// Evaluates the existing Childran
        /// </summary>
        private void ReadChildren(string code)
        {
            var rawCode = code;

            this.ElementType = DtdChildElementTypes.ChildList;

            int bracketDepth = 0;
            var actualElement = new StringBuilder();

            // As long as there is still content, search for additional elements
            while (code.Length > 0)
            {
                string nextChar = code.Substring(0, 1); // next char
                code = code.Remove(0, 1); // remove processed char

                // If parentheses are used, the parenthesized area is considered as one child. 
                // It is therefore passed as a block recursively to ChildErkennung and is not analyzed here.
                switch (nextChar)
                {
                    case "(":
                        bracketDepth++;
                        break;
                    case ")":
                        bracketDepth--;
                        break;
                }
                if (IsOperator(nextChar)) // the actual child is closed
                {
                    if (bracketDepth == 0) //  we are not within a child encapsulation
                    {
                        // Determine which is the operator between the child elements
                        this.Operator = GetOperatorFromChar(nextChar);

                        string done = actualElement.ToString().Trim();

                        if (done.Length == 0)
                        {
                            throw new ApplicationException("found empty childcode in '" + rawCode + "'");
                        }
                        else
                        {
                            // Save the previously collected element string as child
                            SaveChildElement(done);
                        }
                        // start new element
                        actualElement = new StringBuilder();
                    }
                    else // Operator belongs to the inside enclosed Childblock
                    {
                        actualElement.Append(nextChar);
                    }
                }
                else // character is no operator
                {
                    actualElement.Append(nextChar);
                }
            }

            // If there is a started child element left at the end, close and save it
            if (actualElement.Length > 0)
            {
                SaveChildElement(actualElement.ToString());
            }
        }

        /// <summary>
        /// Lines up a child element in the list
        /// </summary>
        private void SaveChildElement(string code)
        {
            code = code.Trim();
            var child = new DtdChildElements(code); // Create new child or childlist from the code
                                                    //if (child.Art ==DTDChildElementArten.EinzelChild) 
                                                    //{
                                                    //	Trace.WriteLine(code + child._defAnzahl.ToString());
                                                    //}
            this._children.Add(child); // Save child to list

        }

        /// <summary>
        /// Is the specified string an operator?
        /// </summary>
        /// <returns></returns>
        private bool IsOperator(string code)
        {
            switch (code)
            {
                case "|":
                case ",": return true;
                default: return false;
            }
        }

        /// <summary>
        /// What type of operator is the given string?
        /// </summary>
        private DtdChildElementOperators GetOperatorFromChar(string code)
        {
            switch (code)
            {
                case "|": return DtdChildElementOperators.Or;
                case ",": return DtdChildElementOperators.FollowedBy;
            }

            // "The specified string '" + code + "' is not an operator!
            throw new ApplicationException("The specified string '" + code + "' is not an operator!");
        }


    }
}
