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
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace de.springwald.xml.rules.dtd
{
	public class DtdReaderDtd
	{
		private List<DtdElement> elements;
        private List<DtdEntity> entities;

		public string RawContent { get; private set; }

        /// <summary>
        /// The customized content, in which e.g. all entities are already resolved
        /// </summary>
        public string WorkingContent { get; private set; }

		public DtdReaderDtd()
		{
		}

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
				throw new ApplicationException($"Could not read in file '{filename}:\n{exc.Message}" );
			}
			return this.GetDtdFromString(content);
		}

		public Dtd GetDtdFromString(string content) 
		{
            // Replace tabs from the content with spaces
            content = content.Replace ("\t"," ");

			this.RawContent = content;
			this.WorkingContent = content;

			elements = new List<DtdElement>(); 
            entities = new List<DtdEntity>();
			this.AnalyzeContent();	

			elements.Add(CreateElementFromQuellcode("#PCDATA")); // add element #PCDATA 
            elements.Add(CreateElementFromQuellcode("#COMMENT")); // add element #COMMENT

            return  new Dtd(elements,entities);
		}

		private void AnalyzeContent() 
		{
			this.RemoveComments();  // So that commented out elements are not read in
            this.ReadEntities();
			this.ReplaceEntities();
			this.ReadElements();
		}

        /// <summary>
        /// So that commented out elements are not read in
        /// </summary>
        private void RemoveComments() 
		{
			// Buddy: <!--((?!-->|<!--)([\t\r\n]|.))*-->
			const string regex =  "<!--((?!-->|<!--)([\\t\\r\\n]|.))*-->";
			this.WorkingContent =  Regex.Replace(this.WorkingContent, regex,"");
		}

        #region ELEMENTE analysieren

        /// <summary>
        /// Reads all DTD elements contained in the DTD content
        /// </summary>
        private void ReadElements() 
		{
			DtdElement element;
			string elementCode;

			// Regulären Ausdruck zum finden von DTD-Elementen zusammenbauen
			// (?<element><!ELEMENT[\t\r\n ]+[^>]+>)
			const string regex =  "(?<element><!ELEMENT[\\t\\r\\n ]+[^>]+>)";

			Regex reg = new Regex(regex); //, RegexOptions.IgnoreCase);
			// Auf den DTD-Inhalt anwenden
			Match match = reg.Match(this.WorkingContent);

            SortedList gefundene = new SortedList(); // Zuerst alles in eine sortierte Liste aufnehmen

			// Alle RegEx-Treffer durchlaufen und daraus Elemente erzeugen
			while (match.Success) 
			{
				elementCode = match.Groups["element"].Value;
				element = CreateElementFromQuellcode(elementCode);
                try
                {
                    gefundene.Add(element.Name, element);
                }
                catch (ArgumentException e)
                {
                    throw new ApplicationException(String.Format($"Error reading dtd element {element.Name}: {e.Message}"));
                }
				match = match.NextMatch(); // Zum nächsten RegEx-Treffer
			}

            // Nun die sortierte Liste in die Elementliste überführen
            for (int i  = 0; i <gefundene.Count;i++) 
            {
                elements.Add((DtdElement)gefundene[gefundene.GetKey(i)]);
            }

		}

		/// <summary>
		/// Wertet den Element-Quellcode aus und speichert den Inhalt strukturiert im Element-Objekt
		/// </summary>
		/// <example>
		/// z.B. so etwas könnte im Element-Quellcode stehen:
		/// <!ELEMENT template  (#PCDATA | srai | sr | that | get | bot | birthday | set | A | star | random )*>
		/// </example>
		private DtdElement CreateElementFromQuellcode(string elementQuellcode) 
		{
			if (elementQuellcode=="#PCDATA") // Es ist kein in der DTD definiertes Element, sondern das PCDATA-Element
			{
                var element = new DtdElement();
				element.Name = "#PCDATA";
                element.ChildElemente = new DtdChildElements("");
				return element;
			}

            if (elementQuellcode == "#COMMENT") // Es ist kein in der DTD definiertes Element, sondern das COMMENT-Element
            {
                var element = new DtdElement();
                element.Name = "#COMMENT";
                element.ChildElemente = new DtdChildElements("");
                return element;
            }

			// Der folgende Ausdruck zerteilt das ELEMENT-Tag in seine Bestandteile. Gruppen:
			// element=das ganze Elementes
			// elementname=der Name des Elementes
			// innerelements=Liste der Child-Elemente, die im Element vorkommen dürfen 
			const string regpatternelement = @"(?<element><!ELEMENT[\t\r\n ]+(?<elementname>[\w-_]+?)([\t\r\n ]+(?<innerelements>[(]([\t\r\n]|.)+?[)][*+]?)?)?(?<empty>[\t\r\n ]+EMPTY)? *>)";

			// Regulären Ausdruck zum Finden der Element-Teile zusammenbauen
			Regex reg = new Regex(regpatternelement); //, RegexOptions.IgnoreCase);

			// Auf den Element-Quellcode anwenden
			Match match = reg.Match(elementQuellcode);

			if (!match.Success) // Wenn kein Element im Element-Code gefunden wurde
			{
                throw new ApplicationException($"Kein Vorkommen gefunden im Elementcode '{elementQuellcode}'.");
			}
			else // ein Element gefunden
			{

				//Element bereitstellen
                var element = new DtdElement();

				// Name des Elementes herausfinden
				if (!match.Groups["elementname"].Success) 
				{	// kein Name gefunden
                    throw new ApplicationException($"Kein Name gefunden im Elementcode '{elementQuellcode}'.");
				} 
				else 
				{
					// Name gefunden
					element.Name = match.Groups["elementname"].Value;
				}

				// Die Attribute des Elementes auslesen
				CreateDTDAttributesForElement(element);

				// Childelemente herausfinden
				if (match.Groups["innerelements"].Success) // wenn ChildElemente vorhanden sind
				{
					ChildElementeAuslesen(element,match.Groups["innerelements"].Value);
				} 
				else // Keine ChildElemente in diesem Element angegeben
				{
					//element.ChildElemente = null; //new DTDElementCollection (); // Leere Childliste einsetzen
					ChildElementeAuslesen(element,"");
				}

				match = match.NextMatch();
				if (match.Success) // Wenn mehr als ein Element im Element-Code gefunden wurde
				{
                    // 
					throw new ApplicationException($"Mehr als ein Vorkommen gefunden im Elementcode '{elementQuellcode}'.");
				}
				return element;
			}
		}

		/// <summary>
		/// Wertet den Element-Quellcode aus und speichert den Inhalt strukturiert im Objekt
		/// </summary>
		/// <param name="childElementQuellcode">Der Code der ChildElemente
		/// </param>
		/// <example>
		/// z.B. bei folgendem Element-Quellcode
		/// <!ELEMENT template  (#PCDATA | srai | sr | that | get | bot | birthday | set | A | star | random )*>
		/// würde als ChildElementQuellCode erwartet
		/// (#PCDATA | srai | sr | that | get | bot | birthday | set | A | star | random )*
		/// </example>
		private void ChildElementeAuslesen(DtdElement element, string childElementeQuellcode) 
		{
			element.ChildElemente  = new DtdChildElements(childElementeQuellcode);
		}

		#endregion

		#region ENTITIES analysieren

		/// <summary>
		/// Setzt für die verschiedenen Entities an den zitierten Stellen den Inhalt der Entities ein
		/// </summary>
		private void ReplaceEntities() 
		{
			string vorher="";
			while (vorher != this.WorkingContent)   // Solange das Einsetzen der Enities noch Veränderung bewirkt hat
			{
				vorher = this.WorkingContent;
				foreach (DtdEntity entity in this.entities) // Alle Enities durchlaufen
				{
					if (entity.IsReplacementEntity ) // wenn es eine Ersetzung-Entity ist
					{
						// Nennung des Entity %name; durch den Inhalt der Entity ersetzen
						this.WorkingContent = this.WorkingContent.Replace ("%"+entity.Name +";",entity.Content ); 
					}
				}
			}
		}

		/// <summary>
		/// Liest alle im DTD-Inhalt enthaltenen Entities aus
		/// </summary>
		private void ReadEntities() 
		{
			DtdEntity entity;
			string entityCode;

			// Regulären Ausdruck zum finden von DTD-Entities zusammenbauen
			// (?<entity><!ENTITY[\t\r\n ]+[^>]+>)
			const string regex =  "(?<entity><!ENTITY[\\t\\r\\n ]+[^>]+>)";

			var reg = new Regex(regex); //, RegexOptions.IgnoreCase);
			// Auf den DTD-Inhalt anwenden
			var match = reg.Match(this.WorkingContent);

			// Alle RegEx-Treffer durchlaufen und daraus Elemente erzeugen
			while (match.Success) 
			{
				entityCode = match.Groups["entity"].Value;
				entity = CreateEntityFromQuellcode(entityCode);
				entities.Add(entity);
				match = match.NextMatch(); // Zum nächsten RegEx-Treffer
			}
		}

		/// <summary>
		/// Wertet den Entity-Quellcode aus und speichert den Inhalt strukturiert im Objekt
		/// </summary>
		/// <example>
		/// z.B. so etwas könnte im Entity-Quellcode stehen:
		/// <!ENTITY % html	"a | applet | br | em | img | p | table | ul">
		/// </example>
		private DtdEntity CreateEntityFromQuellcode(string entityQuellcode) 
		{
			// Der folgende Ausdruck zerteilt das ENTITY-Tag in seine Bestandteile. Gruppen:
			// entity=die ganze Entity
			// entityname=der Name der entity
			// inhalt=der Inhalt der entity
			// prozent=das Prozent-Zeichen, das angibt, ob es eine Ersetzungs-Entity oder eine Baustein-Entity ist
			//(?<entity><!ENTITY[\t\r\n ]+(?:(?<prozent>%)[\t\r\n ]+)?(?<entityname>[\w-_]+?)[\t\r\n ]+"(?<inhalt>[^>]+)"[\t\r\n ]?>)
			const string  regpatternelement = "(?<entity><!ENTITY[\\t\\r\\n ]+(?:(?<prozent>%)[\\t\\r\\n ]+)?(?<entityname>[\\w-_]+?)[\\t\\r\\n ]+\"(?<inhalt>[^>]+)\"[\\t\\r\\n ]?>)";

			// Regulären Ausdruck zum Finden der Entity-Teile zusammenbauen
			var reg = new Regex(regpatternelement); //, RegexOptions.IgnoreCase);

			// Auf den Entity-Quellcode anwenden
			var match = reg.Match(entityQuellcode);

			if (!match.Success) // Wenn keine Entity im Entity-Code gefunden wurde
			{
                throw new ApplicationException($"Kein Vorkommen gefunden im Entityquellcode '{entityQuellcode}'");
			}
			else // Genau eine Entity gefunden
			{
				var entity = new DtdEntity();

				// am Prozentzeichen festmachen, ob Ersetzungs-Entity
				entity.IsReplacementEntity =  (match.Groups["prozent"].Success);

				// Name der Entity herausfinden
				if (!match.Groups["entityname"].Success) 
				{	// kein Name gefunden
                    // 
					throw new ApplicationException($"Kein Name gefunden im Entitycode '{entityQuellcode}'");
				} 
				else 
				{
					// Name gefunden
					entity.Name = match.Groups["entityname"].Value; // Name merken

					// Inhalt der Entity herausfinden
					if (!match.Groups["inhalt"].Success) 
					{	// kein Inhalt gefunden
						throw new ApplicationException($"Kein Inhalt gefunden im Entitycode '{entityQuellcode}'" );
					} 
					else 
					{
						// Inhalt gefunden
						entity.Content = match.Groups["inhalt"].Value; // Inhalt merken
					}
				}
				match = match.NextMatch();
				if (match.Success) // Wenn mehr als eine Entity im Element-Code gefunden wurde
				{
					throw new ApplicationException($"Mehr als ein Vorkommen gefunden im Entitycode '{entityQuellcode}'" );
				}
				return entity;
			}
		}

		#endregion analyisieren

		#region ATTRIBUTE analysieren

		/// <summary>
		/// Stellt für das angegebene Element die entsprechenden Attribute bereit, sofern sie vorhanden sind
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		private void CreateDTDAttributesForElement(DtdElement element) 
		{
			
			element.Attributes = new List<DtdAttribute> ();

			// Regulären Ausdruck zum finden der AttributList-Definition zusammenbauen
			// (?<attributliste><!ATTLIST muster_titel[\t\r\n ]+(?<attribute>[^>]+?)[\t\r\n ]?>)
			string ausdruckListe =  "(?<attributliste><!ATTLIST " + element.Name +"[\\t\\r\\n ]+(?<attribute>[^>]+?)[\\t\\r\\n ]?>)";

			var regList = new Regex(ausdruckListe); //, RegexOptions.IgnoreCase);
			// Auf den DTD-Inhalt anwenden
			var match = regList.Match(this.WorkingContent);

			if (match.Success) 
			{
				// Die Liste der Attribute holen
				string attributListeCode = match.Groups["attribute"].Value;

				// In der Liste der Attribute die einzelnen Attribute isolieren
				// Regulären Ausdruck zum finden der einzelnen Attribute in der AttribuList zusammenbauen
				// [\t\r\n ]?(?<name>[\w-_]+)[\t\r\n ]+(?<typ>CDATA|ID|IDREF|IDREFS|NMTOKEN|NMTOKENS|ENTITY|ENTITIES|NOTATION|xml:|[(][|\w-_ \t\r\n]+[)])[\t\r\n ]+(?:(?<anzahl>#REQUIRED|#IMPLIED|#FIXED)[\t\r\n ]+)?(?:"(?<vorgabewert>[\w-_]+)")?[\t\r\n ]?
				const string ausdruckEinzel =  "[\\t\\r\\n ]?(?<name>[\\w-_]+)[\\t\\r\\n ]+(?<typ>CDATA|ID|IDREF|IDREFS|NMTOKEN|NMTOKENS|ENTITY|ENTITIES|NOTATION|xml:|[(][|\\w-_ \\t\\r\\n]+[)])[\\t\\r\\n ]+(?:(?<anzahl>#REQUIRED|#IMPLIED|#FIXED)[\\t\\r\\n ]+)?(?:\"(?<vorgabewert>[\\w-_]+)\")?[\\t\\r\\n ]?";

				var regEinzel = new Regex(ausdruckEinzel); //, RegexOptions.IgnoreCase);
				// Auf den DTD-Inhalt anwenden
				match = regEinzel.Match(attributListeCode);

				if (match.Success) 
				{
					DtdAttribute attribut;
					string typ;
					string[] werteListe;
					string delimStr = "|";
					char [] delimiter = delimStr.ToCharArray();

					// Alle RegEx-Treffer durchlaufen und daraus Attribute für das Element erzeugen
					while (match.Success) 
					{
						attribut = new DtdAttribute (); // Attribut erzeugen
						attribut.Name = match.Groups["name"].Value; // Name des Attributes
						attribut.StandardValue = match.Groups["vorgabewert"].Value; // StandardWert des Attributes
						// Die Anzahl / Pflicht des Attributes
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
                                throw new ApplicationException($"Unbekannte AttributAnzahl '{match.Groups["anzahl"].Value}' in Attribut '{match.Value}' von Element {element.Name}" );

						}
						// Der Typ des Attributes
						typ = match.Groups["typ"].Value; 
						typ = typ.Trim();
						if (typ.StartsWith ("("))  // Es ist eine Aufzählung der zulässigen Werte dieses Attributes (en1|en2|..)
						{
							attribut.Type = "";
							// Klammern entfernen 
							typ = typ.Replace("(","");
							typ = typ.Replace(")","");
							typ = typ.Replace(")","");
							// In einzelne Werte aufteilen
							werteListe = typ.Split(delimiter, StringSplitOptions.RemoveEmptyEntries); // Die durch | getrennten Werte in ein Array splitten
							attribut.AllowedValues = werteListe.Select(w => w.Replace("\n", " ").Trim()).ToArray();
						}
						else // es ist eine genaue Angabe des Typs dieses Attributes wie z.B. CDATA, ID, IDREF etc.
						{
							attribut.Type = typ;
						}

						// Attribut im Element speichern
						element.Attributes.Add(attribut);

						match = match.NextMatch(); // Zum nächsten RegEx-Treffer
					}
				} 
				else 
				{
					throw new ApplicationException($"No attributes found in the AttributeList '{attributListeCode}'!");
				}
			} 
			else 
			{
				Trace.WriteLine ($"No attributes available for element {element.Name}.");
			}
		}
		#endregion
	}
}
