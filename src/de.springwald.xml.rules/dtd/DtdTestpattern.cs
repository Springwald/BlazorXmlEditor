// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Text;

namespace de.springwald.xml.rules.dtd
{
    /// <summary>
    /// An XML block of what it might look like after the intended change. This pattern is chased through the validator. All patterns, which were subsequently flagged as "confirmed", are permitted according to the DTD
    /// </summary>
    public class DtdTestpattern
    {
        private string parentElementName;	    // This element lies over the cursor Pos (drawing:C) to be tested
        private string compareStringForRegEx;
        private StringBuilder elementNameList;

        /// <summary>
        /// The element inserted for testing. If it is NULL, this means that instead of inserting, the deletion was checked
        /// </summary>
        public string ElementName { get; }

        public string CompareStringForRegEx
        {
            get
            {
                if (compareStringForRegEx == null)
                {
                    elementNameList.Append("<");
                    compareStringForRegEx = elementNameList.ToString();
                }
                return compareStringForRegEx;
            }
        }

        /// <summary>
        /// A written summary of this sample
        /// </summary>
        public string Summary
        {
            get
            {
                var result = new StringBuilder();

                // Successfully tested?
                if (this.Success)
                {
                    result.Append("+ ");
                }
                else
                {
                    result.Append("- ");
                }

                // The name of the ParentNode
                result.Append(this.parentElementName);
                result.Append(" (");
                result.Append(CompareStringForRegEx);
                result.Append(")");

                // What was tested?
                if (this.ElementName == null)
                {
                    result.Append(" [tested: delete]");
                }
                else
                {
                    result.AppendFormat("[tested: {0}]", this.ElementName);
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Has the pattern been successfully applied?
        /// </summary>
        public bool Success { get; set; }

        /// <param name="element">The element inserted for testing. If it is NULL, this means that instead of inserting, the deletion was checked</param>
        /// <param name="parentElementName">This element lies over the cursor Pos (drawing:C) to be tested</param>
        public DtdTestpattern(string elementName, string parentElementName)
        {
            elementNameList = new StringBuilder();
            elementNameList.Append(">");

            this.ElementName = elementName;
            this.parentElementName = parentElementName;
            this.Success = false; //  Not yet confirmed
        }

        public void AddElement(string elementName)
        {
            elementNameList.AppendFormat("-{0}", elementName);
        }
    }
}
