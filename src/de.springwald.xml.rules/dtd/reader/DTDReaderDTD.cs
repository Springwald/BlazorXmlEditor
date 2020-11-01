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
	public class DTDReaderDTD
	{
		private List<DtdElement> _elemente;		// Die in dieser DTD verfügbaren Elemente
        private List<DTDEntity> _entities;		// Die bekannten Entity-Einträge dieser DTD

		/// <summary>
		/// Der eingelesene Rohinhalt der DTD
		/// </summary>
		public string RawContent { get; private set; }

		/// <summary>
		/// Der angepasste Inhalt, in welchem z.B. bereits alle Entities aufgelöst sind
		/// </summary>
		public string WorkingContent { get; private set; }

		public DTDReaderDTD()
		{
		}

		/// <summary>
		/// Erzeugt ein DTD-Objekt auf Basis einer DTD-Datei
		/// </summary>
		public Dtd GetDTDFromFile(string filename) 
		{
			string content=""; // Der Inhalt der DTD-Datei
			
			try
			{
				using (var reader = new StreamReader(filename, System.Text.Encoding.GetEncoding("ISO-8859-15")))  // Datei öffnen
				{
					content = reader.ReadToEnd();
					reader.Close();
				}
			}
			catch (FileNotFoundException exc) // Falls die Datei nicht gefunden wurde
			{
                // 
				throw new ApplicationException($"Could not read in file '{filename}:\n{exc.Message}" );
			}

			return this.GetDTDFromString(content);
		}

		/// <summary>
		/// Erzeugt ein DTD-Objekt auf Basis einer DTD-Datei
		/// </summary>
		/// <param name="content">Der Inhalt der DTD-Datei</param>
		/// <returns></returns>
		public Dtd GetDTDFromString(string content) 
		{
			// Tabs aus dem inhalt durch Leerzeichen ersetzen
			content = content.Replace ("\t"," ");

			// Den gelesenen Inhalt merken
			this.RawContent = content;
			this.WorkingContent = content;

			// Elemente suchen und in die DTD füllen
			_elemente = new List<DtdElement>(); // Noch keine Elemente vorhanden
            _entities = new List<DTDEntity>();
			InhaltAnalysieren();	// definierte Elemente einlesen

			_elemente.Add(CreateElementFromQuellcode("#PCDATA")); // um das Element #PCDATA ergänzen
            _elemente.Add(CreateElementFromQuellcode("#COMMENT")); // um das Element #COMMENT ergänzen

            return  new Dtd(_elemente,_entities);
		}

		/// <summary>
		/// Verarbeitet den angegebenen Inhalt und baut die entsprechenden Hintergrundstrukturen auf
		/// </summary>
		private void InhaltAnalysieren() 
		{
			KommentareEntfernen();	// Damit auskommentierte Elemente nicht eingelesen werden
			EntitiesAuslesen();
			EntitiesAustauschen();
			ElementeAuslesen();
		}

		/// <summary>
		/// Damit auskommentierte Elemente nicht eingelesen werden
		/// </summary>
		private void KommentareEntfernen() 
		{
			// Buddy: <!--((?!-->|<!--)([\t\r\n]|.))*-->
			const string regex =  "<!--((?!-->|<!--)([\\t\\r\\n]|.))*-->";
			this.WorkingContent =  Regex.Replace(this.WorkingContent, regex,"");
		}

		#region ELEMENTE analysieren

		/// <summary>
		/// Liest alle im DTD-Inhalt enthaltenen DTD-Elemente aus
		/// </summary>
		private void ElementeAuslesen() 
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
                _elemente.Add((DtdElement)gefundene[gefundene.GetKey(i)]);
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
                DtdElement element = new DtdElement();
				element.Name = "#PCDATA";
                element.ChildElemente = new DtdChildElements("");
				return element;
			}

            if (elementQuellcode == "#COMMENT") // Es ist kein in der DTD definiertes Element, sondern das COMMENT-Element
            {
                DtdElement element = new DtdElement();
                element.Name = "#COMMENT";
                element.ChildElemente = new DtdChildElements("");
                return element;
            }

			// Der folgende Ausdruck zerteilt das ELEMENT-Tag in seine Bestandteile. Gruppen:
			// element=das ganze Elementes
			// elementname=der Name des Elementes
			// innerelements=Liste der Child-Elemente, die im Element vorkommen dürfen 
			string regpatternelement = @"(?<element><!ELEMENT[\t\r\n ]+(?<elementname>[\w-_]+?)([\t\r\n ]+(?<innerelements>[(]([\t\r\n]|.)+?[)][*+]?)?)?(?<empty>[\t\r\n ]+EMPTY)? *>)";

			// Regulären Ausdruck zum Finden der Element-Teile zusammenbauen
			Regex reg = new Regex(regpatternelement); //, RegexOptions.IgnoreCase);

			// Auf den Element-Quellcode anwenden
			Match match = reg.Match(elementQuellcode);

			if (!match.Success) // Wenn kein Element im Element-Code gefunden wurde
			{
                // "Kein Vorkommen gefunden im Elementcode '{0}'."
                throw new ApplicationException($"Kein Vorkommen gefunden im Elementcode '{elementQuellcode}'.");
			}
			else // ein Element gefunden
			{

				//Element bereitstellen
                var element = new DtdElement();

				// Name des Elementes herausfinden
				if (!match.Groups["elementname"].Success) 
				{	// kein Name gefunden
                    // "Kein Name gefunden im Elementcode '{0}'."
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
		private void EntitiesAustauschen() 
		{
			string vorher="";
			while (vorher != this.WorkingContent)   // Solange das Einsetzen der Enities noch Veränderung bewirkt hat
			{
				vorher = this.WorkingContent;
				foreach (DTDEntity entity in this._entities) // Alle Enities durchlaufen
				{
					if (entity.IstErsetzungsEntity ) // wenn es eine Ersetzung-Entity ist
					{
						// Nennung des Entity %name; durch den Inhalt der Entity ersetzen
						this.WorkingContent = this.WorkingContent.Replace ("%"+entity.Name +";",entity.Inhalt ); 
					}
				}
			}
		}

		/// <summary>
		/// Liest alle im DTD-Inhalt enthaltenen Entities aus
		/// </summary>
		private void EntitiesAuslesen() 
		{
			DTDEntity entity;
			string entityCode;

			// Regulären Ausdruck zum finden von DTD-Entities zusammenbauen
			// (?<entity><!ENTITY[\t\r\n ]+[^>]+>)
			const string regex =  "(?<entity><!ENTITY[\\t\\r\\n ]+[^>]+>)";

			Regex reg = new Regex(regex); //, RegexOptions.IgnoreCase);
			// Auf den DTD-Inhalt anwenden
			Match match = reg.Match(this.WorkingContent);

			// Alle RegEx-Treffer durchlaufen und daraus Elemente erzeugen
			while (match.Success) 
			{
				entityCode = match.Groups["entity"].Value;
				entity = CreateEntityFromQuellcode(entityCode);
				_entities.Add(entity);
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
		private DTDEntity CreateEntityFromQuellcode(string entityQuellcode) 
		{
			// Der folgende Ausdruck zerteilt das ENTITY-Tag in seine Bestandteile. Gruppen:
			// entity=die ganze Entity
			// entityname=der Name der entity
			// inhalt=der Inhalt der entity
			// prozent=das Prozent-Zeichen, das angibt, ob es eine Ersetzungs-Entity oder eine Baustein-Entity ist
			//(?<entity><!ENTITY[\t\r\n ]+(?:(?<prozent>%)[\t\r\n ]+)?(?<entityname>[\w-_]+?)[\t\r\n ]+"(?<inhalt>[^>]+)"[\t\r\n ]?>)
			const string  regpatternelement = "(?<entity><!ENTITY[\\t\\r\\n ]+(?:(?<prozent>%)[\\t\\r\\n ]+)?(?<entityname>[\\w-_]+?)[\\t\\r\\n ]+\"(?<inhalt>[^>]+)\"[\\t\\r\\n ]?>)";

			// Regulären Ausdruck zum Finden der Entity-Teile zusammenbauen
			Regex reg = new Regex(regpatternelement); //, RegexOptions.IgnoreCase);

			// Auf den Entity-Quellcode anwenden
			Match match = reg.Match(entityQuellcode);

			if (!match.Success) // Wenn keine Entity im Entity-Code gefunden wurde
			{
                //
                throw new ApplicationException($"Kein Vorkommen gefunden im Entityquellcode '{entityQuellcode}'");
			}
			else // Genau eine Entity gefunden
			{
				DTDEntity entity = new DTDEntity();

				// am Prozentzeichen festmachen, ob Ersetzungs-Entity
				entity.IstErsetzungsEntity =  (match.Groups["prozent"].Success);

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
                        // 
						throw new ApplicationException($"Kein Inhalt gefunden im Entitycode '{entityQuellcode}'" );
					} 
					else 
					{
						// Inhalt gefunden
						entity.Inhalt = match.Groups["inhalt"].Value; // Inhalt merken
					}
				}
				match = match.NextMatch();
				if (match.Success) // Wenn mehr als eine Entity im Element-Code gefunden wurde
				{
                    // 
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
			
			element.Attribute = new List<DtdAttribute> ();

			// Regulären Ausdruck zum finden der AttributList-Definition zusammenbauen
			// (?<attributliste><!ATTLIST muster_titel[\t\r\n ]+(?<attribute>[^>]+?)[\t\r\n ]?>)
			string ausdruckListe =  "(?<attributliste><!ATTLIST " + element.Name +"[\\t\\r\\n ]+(?<attribute>[^>]+?)[\\t\\r\\n ]?>)";

			Regex regList = new Regex(ausdruckListe); //, RegexOptions.IgnoreCase);
			// Auf den DTD-Inhalt anwenden
			Match match = regList.Match(this.WorkingContent);

			if (match.Success) 
			{
				// Die Liste der Attribute holen
				string attributListeCode = match.Groups["attribute"].Value;

				// In der Liste der Attribute die einzelnen Attribute isolieren
				// Regulären Ausdruck zum finden der einzelnen Attribute in der AttribuList zusammenbauen
				// [\t\r\n ]?(?<name>[\w-_]+)[\t\r\n ]+(?<typ>CDATA|ID|IDREF|IDREFS|NMTOKEN|NMTOKENS|ENTITY|ENTITIES|NOTATION|xml:|[(][|\w-_ \t\r\n]+[)])[\t\r\n ]+(?:(?<anzahl>#REQUIRED|#IMPLIED|#FIXED)[\t\r\n ]+)?(?:"(?<vorgabewert>[\w-_]+)")?[\t\r\n ]?
				const string ausdruckEinzel =  "[\\t\\r\\n ]?(?<name>[\\w-_]+)[\\t\\r\\n ]+(?<typ>CDATA|ID|IDREF|IDREFS|NMTOKEN|NMTOKENS|ENTITY|ENTITIES|NOTATION|xml:|[(][|\\w-_ \\t\\r\\n]+[)])[\\t\\r\\n ]+(?:(?<anzahl>#REQUIRED|#IMPLIED|#FIXED)[\\t\\r\\n ]+)?(?:\"(?<vorgabewert>[\\w-_]+)\")?[\\t\\r\\n ]?";

				Regex regEinzel = new Regex(ausdruckEinzel); //, RegexOptions.IgnoreCase);
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
						attribut.StandardWert = match.Groups["vorgabewert"].Value; // StandardWert des Attributes
						// Die Anzahl / Pflicht des Attributes
						switch (match.Groups["anzahl"].Value) 
						{
							case "#REQUIRED":
								attribut.Pflicht = DtdAttribute.PflichtArten.Pflicht;
								break;
							case "#IMPLIED":
							case "":
								attribut.Pflicht = DtdAttribute.PflichtArten.Optional;
								break;
							case "#FIXED":
								attribut.Pflicht = DtdAttribute.PflichtArten.Konstante;
								break;
							default:
                                throw new ApplicationException($"Unbekannte AttributAnzahl '{match.Groups["anzahl"].Value}' in Attribut '{match.Value}' von Element {element.Name}" );

						}
						// Der Typ des Attributes
						typ = match.Groups["typ"].Value; 
						typ = typ.Trim();
						if (typ.StartsWith ("("))  // Es ist eine Aufzählung der zulässigen Werte dieses Attributes (en1|en2|..)
						{
							attribut.Typ = "";
							// Klammern entfernen 
							typ = typ.Replace("(","");
							typ = typ.Replace(")","");
							typ = typ.Replace(")","");
							// In einzelne Werte aufteilen
							werteListe = typ.Split(delimiter, StringSplitOptions.RemoveEmptyEntries); // Die durch | getrennten Werte in ein Array splitten
							attribut.ErlaubteWerte = werteListe.Select(w => w.Replace("\n", " ").Trim()).ToArray();
						}
						else // es ist eine genaue Angabe des Typs dieses Attributes wie z.B. CDATA, ID, IDREF etc.
						{
							attribut.Typ = typ;
						}

						// Attribut im Element speichern
						element.Attribute.Add(attribut);

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
