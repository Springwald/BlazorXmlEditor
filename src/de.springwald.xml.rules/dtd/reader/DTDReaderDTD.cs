// A platform independent tag-view-style graphical xml editor
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace de.springwald.xml.rules.dtd
{
    public class DtdReaderDtd
    {
        private DtdElement[] elements;
        private DtdEntity[] entities;

        public string RawContent { get; private set; }

        /// <summary>
        /// The customized content, in which e.g. all entities are already resolved
        /// </summary>
        public string WorkingContent { get; private set; }

        public Dtd GetDtdFromFile(string filename)
        {
            var content = string.Empty;
            try
            {
                using (var reader = new StreamReader(filename, System.Text.Encoding.GetEncoding("ISO-8859-15")))
                {
                    content = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (FileNotFoundException exc)
            {
                throw new ApplicationException($"Could not read in file '{filename}':\n{exc.Message}");
            }
            return this.GetDtdFromString(content);
        }

        public Dtd GetDtdFromString(string content)
        {
            // Replace tabs from the content with spaces
            content = content.Replace("\t", " ");
            this.RawContent = content;
            this.WorkingContent = content;
            this.AnalyzeContent();
            return new Dtd(elements, entities);
        }

        private void AnalyzeContent()
        {
            this.RemoveComments();  // So that commented out elements are not read in
            this.entities = this.ReadEntities().ToArray();
            this.ReplaceEntities();

            this.elements = this.ReadElements()
                .Concat(new[] {
                CreateElementFromQuellcode("#PCDATA"),
                CreateElementFromQuellcode("#COMMENT") })
                .ToArray();
        }

        /// <summary>
        /// So that commented out elements are not read in
        /// </summary>
        private void RemoveComments()
        {
            // Buddy: <!--((?!-->|<!--)([\t\r\n]|.))*-->
            const string regex = "<!--((?!-->|<!--)([\\t\\r\\n]|.))*-->";
            this.WorkingContent = Regex.Replace(this.WorkingContent, regex, "");
        }

        #region analyze ELEMENTS

        /// <summary>
        /// Reads all DTD elements contained in the DTD content
        /// </summary>
        private IEnumerable<DtdElement> ReadElements()
        {
            string elementCode;

            // Regular expression to find and assemble DTD elements
            // (?<element><!ELEMENT[\t\r\n ]+[^>]+>)
            const string regex = "(?<element><!ELEMENT[\\t\\r\\n ]+[^>]+>)";

            Regex reg = new Regex(regex); //, RegexOptions.IgnoreCase);
            // Apply to DTD content
            var match = reg.Match(this.WorkingContent);

            // Run through all RegEx hits and create elements from them
            while (match.Success)
            {
                elementCode = match.Groups["element"].Value;
                yield return CreateElementFromQuellcode(elementCode);
                match = match.NextMatch(); // To the next RegEx hit
            }
        }

        /// <summary>
        /// Evaluates the element source code and stores the content structured in the element object
        /// </summary>
        /// <example>
        /// e.g. something like this could be in the element source code:
        /// <!ELEMENT template  (#PCDATA | srai | sr | that | get | bot | birthday | set | A | star | random )*>
        /// </example>
        private DtdElement CreateElementFromQuellcode(string elementSourcecode)
        {
            if (elementSourcecode == "#PCDATA") // It is not an element defined in the DTD, but the PCDATA element
            {
                return new DtdElement()
                {
                    Name = "#PCDATA",
                    ChildElements = new DtdChildElements("")
                };
            }

            if (elementSourcecode == "#COMMENT") // It is not an element defined in the DTD, but the COMMENT element
            {
                return new DtdElement()
                {
                    Name = "#COMMENT",
                    ChildElements = new DtdChildElements("")
                };
            }

            // The following expression splits the ELEMENT tag into its parts. groups:
            // element=the whole element
            // elementname=the name of the element
            // innerelements=List of child elements that may occur in the element 
            const string regpatternelement = @"(?<element><!ELEMENT[\t\r\n ]+(?<elementname>[\w-_]+?)([\t\r\n ]+(?<innerelements>[(]([\t\r\n]|.)+?[)][*+]?)?)?(?<empty>[\t\r\n ]+EMPTY)? *>)";

            //  Assemble regular expression to find the element parts
            Regex reg = new Regex(regpatternelement); //, RegexOptions.IgnoreCase);

            Match match = reg.Match(elementSourcecode);

            if (!match.Success) throw new ApplicationException($"No occurrence found in element code '{elementSourcecode}'.");

            var element = new DtdElement();

            // Find out the name of the element
            if (!match.Groups["elementname"].Success) throw new ApplicationException($"No name found in element code '{elementSourcecode}'.");
            element.Name = match.Groups["elementname"].Value;

            element.Attributes = CreateDtdAttributesForElement(element).ToArray();

            // find child elements
            if (match.Groups["innerelements"].Success)
            {
                ReadChildElements(element, match.Groups["innerelements"].Value);
            }
            else
            {
                ReadChildElements(element, "");
            }

            match = match.NextMatch();
            if (match.Success) throw new ApplicationException($"More than one occurrence found in element code '{elementSourcecode}'.");
            return element;
        }

        /// <summary>
        /// Evaluates the element source code and stores the content structured in the object
        /// </summary>
        /// <remarks>
        /// e.g. with the following element source code
        /// <!ELEMENT template  (#PCDATA | srai | sr | that | get | bot | birthday | set | A | star | random )*>
        /// would be expected as ChildElementSourceCode
        /// (#PCDATA | srai | sr | that | get | bot | birthday | set | A | star | random )*
        /// </remarks>
        private void ReadChildElements(DtdElement element, string childElementsSourcecode)
        {
            element.ChildElements = new DtdChildElements(childElementsSourcecode);
        }

        #endregion

        #region ENTITIES analysieren

        /// <summary>
        /// Inserts the content of the entities for the different entities at the quoted positions
        /// </summary>
        private void ReplaceEntities()
        {
            string last = null;
            while (last != this.WorkingContent)
            {
                last = this.WorkingContent;
                foreach (DtdEntity entity in this.entities)
                {
                    if (entity.IsReplacementEntity)
                    {
                        // Replace the entity %name; with the content of the entity
                        this.WorkingContent = this.WorkingContent.Replace($"%{entity.Name};", entity.Content);
                    }
                }
            }
        }

        /// <summary>
        /// Reads all entities contained in DTD content
        /// </summary>
        private IEnumerable<DtdEntity> ReadEntities()
        {
            // Regular expression to find DTD entities
            // (?<entity><!ENTITY[\t\r\n ]+[^>]+>)
            const string regex = "(?<entity><!ENTITY[\\t\\r\\n ]+[^>]+>)";
            var reg = new Regex(regex); //, RegexOptions.IgnoreCase);
            var match = reg.Match(this.WorkingContent);
            while (match.Success)
            {
                var entityCode = match.Groups["entity"].Value;
                yield return CreateEntityFromSourcecode(entityCode);
                match = match.NextMatch();
            }
        }

        /// <summary>
        /// Evaluates the entity source code and stores the content structured in the object
        /// </summary>
        /// <example>
        /// e.g. something like this could be in the entity source code:
        /// <!ENTITY % html	"a | applet | br | em | img | p | table | ul">
        /// </example>
        private DtdEntity CreateEntityFromSourcecode(string entityQuellcode)
        {
            // The following expression splits the ENTITY tag into its parts. groups:
            // entity=the whole entity
            // entityname=entity name
            // inhalt=entity content
            // prozent=the percent sign, which indicates whether it is a replacement entity or a building block entity
            //(?<entity><!ENTITY[\t\r\n ]+(?:(?<prozent>%)[\t\r\n ]+)?(?<entityname>[\w-_]+?)[\t\r\n ]+"(?<inhalt>[^>]+)"[\t\r\n ]?>)
            const string regpatternelement = "(?<entity><!ENTITY[\\t\\r\\n ]+(?:(?<prozent>%)[\\t\\r\\n ]+)?(?<entityname>[\\w-_]+?)[\\t\\r\\n ]+\"(?<inhalt>[^>]+)\"[\\t\\r\\n ]?>)";

            var reg = new Regex(regpatternelement); //, RegexOptions.IgnoreCase);

            var match = reg.Match(entityQuellcode);

            if (!match.Success) throw new ApplicationException($"No occurrence found in the entity source code '{entityQuellcode}'");
            var entity = new DtdEntity();

            entity.IsReplacementEntity = (match.Groups["prozent"].Success);

            // NFind out the name of the entity
            if (!match.Groups["entityname"].Success) throw new ApplicationException($"No name found in the entity code '{entityQuellcode}'");

            entity.Name = match.Groups["entityname"].Value;

            if (!match.Groups["inhalt"].Success) throw new ApplicationException($"No content found in the entity code '{entityQuellcode}'");

            entity.Content = match.Groups["inhalt"].Value;

            match = match.NextMatch();
            if (match.Success) throw new ApplicationException($"More than one occurrence found in the entity code '{entityQuellcode}'");

            return entity;
        }

        #endregion 

        #region analyze ATTRIBUTE

        /// <summary>
        /// Provides the corresponding attributes for the specified element, if they are available
        /// </summary>
        private IEnumerable<DtdAttribute> CreateDtdAttributesForElement(DtdElement element)
        {
            // Regular expression to find the AttributList-Definition
            // (?<attributliste><!ATTLIST muster_titel[\t\r\n ]+(?<attribute>[^>]+?)[\t\r\n ]?>)
            string patternList = "(?<attributliste><!ATTLIST " + element.Name + "[\\t\\r\\n ]+(?<attribute>[^>]+?)[\\t\\r\\n ]?>)";

            var regList = new Regex(patternList); //, RegexOptions.IgnoreCase);
            var match = regList.Match(this.WorkingContent);

            if (match.Success)
            {
                // Get the list of attributes
                string attributListeCode = match.Groups["attribute"].Value;

                // Isolate the individual attributes in the list of attributes
                // Regular expression to find the individual attributes in the AttribuList
                // [\t\r\n ]?(?<name>[\w-_]+)[\t\r\n ]+(?<typ>CDATA|ID|IDREF|IDREFS|NMTOKEN|NMTOKENS|ENTITY|ENTITIES|NOTATION|xml:|[(][|\w-_ \t\r\n]+[)])[\t\r\n ]+(?:(?<anzahl>#REQUIRED|#IMPLIED|#FIXED)[\t\r\n ]+)?(?:"(?<vorgabewert>[\w-_]+)")?[\t\r\n ]?
                const string singlePattern = "[\\t\\r\\n ]?(?<name>[\\w-_]+)[\\t\\r\\n ]+(?<typ>CDATA|ID|IDREF|IDREFS|NMTOKEN|NMTOKENS|ENTITY|ENTITIES|NOTATION|xml:|[(][|\\w-_ \\t\\r\\n]+[)])[\\t\\r\\n ]+(?:(?<anzahl>#REQUIRED|#IMPLIED|#FIXED)[\\t\\r\\n ]+)?(?:\"(?<vorgabewert>[\\w-_]+)\")?[\\t\\r\\n ]?";

                var singleRegex = new Regex(singlePattern); //, RegexOptions.IgnoreCase);
                match = singleRegex.Match(attributListeCode);

                if (match.Success)
                {
                    DtdAttribute attribut;
                    string type;
                    string[] valuesList;
                    string delimStr = "|";
                    char[] delimiter = delimStr.ToCharArray();

                    // Run through all RegEx hits and create attributes for the element
                    while (match.Success)
                    {
                        attribut = new DtdAttribute(); 
                        attribut.Name = match.Groups["name"].Value; 
                        attribut.StandardValue = match.Groups["vorgabewert"].Value; 
                        switch (match.Groups["anzahl"].Value)
                        {
                            case "#REQUIRED":
                                attribut.Mandatory = DtdAttribute.MandatoryTypes.Mandatory;
                                break;
                            case "#IMPLIED":
                            case "":
                                attribut.Mandatory = DtdAttribute.MandatoryTypes.Optional;
                                break;
                            case "#FIXED":
                                attribut.Mandatory = DtdAttribute.MandatoryTypes.Constant;
                                break;
                            default:
                                throw new ApplicationException($"unknown attribute mandatory value '{match.Groups["anzahl"].Value}' in attribute '{match.Value}' of  element {element.Name}");
                        }
                        type = match.Groups["typ"].Value;
                        type = type.Trim();
                        if (type.StartsWith("("))  // It is an enumeration of the permissible values of this attribute (en1|en2|..)
                        {
                            attribut.Type = "";
                            // remove brackets
                            type = type.Replace("(", "");
                            type = type.Replace(")", "");
                            type = type.Replace(")", "");
                            // split into values
                            valuesList = type.Split(delimiter, StringSplitOptions.RemoveEmptyEntries); // Split the values separated by | into an array
                            attribut.AllowedValues = valuesList.Select(w => w.Replace("\n", " ").Trim()).ToArray();
                        }
                        else // it is an exact specification of the type of this attribute like CDATA, ID, IDREF etc.
                        {
                            attribut.Type = type;
                        }
                        yield return attribut;
                        match = match.NextMatch(); 
                    }
                }
                else
                {
                    throw new ApplicationException($"No attributes found in the AttributeList '{attributListeCode}'!");
                }
            }
            else
            {
                Trace.WriteLine($"No attributes available for element {element.Name}.");
            }
        }
        #endregion
    }
}
