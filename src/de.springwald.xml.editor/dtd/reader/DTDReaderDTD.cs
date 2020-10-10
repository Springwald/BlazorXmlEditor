using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics;
using de.springwald.toolbox;
using System.Linq;

namespace de.springwald.xml.dtd
{
	/// <summary>
	/// Zusammenfassung f�r DTDReaderDTD.
	/// </summary>
	public class DTDReaderDTD
	{
		#region PRIVATE ATTRIBUTES

		private string _rohinhalt;			// Der eingelesene Rohinhalt der DTD
		private string _workingInhalt;		// Der �berarbeitete Inhalt der DTD
		private List<DTDElement> _elemente;		// Die in dieser DTD verf�gbaren Elemente
        private List<DTDEntity> _entities;		// Die bekannten Entity-Eintr�ge dieser DTD

		#endregion

		#region PUBLIC ATTRIBUTES

		/// <summary>
		/// Der eingelesene Rohinhalt der DTD
		/// </summary>
		public string RohInhalt
		{
			get { return _rohinhalt; }
		}

		/// <summary>
		/// Der angepasste Inhalt, in welchem z.B. bereits alle Entities aufgel�st sind
		/// </summary>
		public string WorkingInhalt 
		{
			get { return _workingInhalt; }
		}

		#endregion

		#region CONSTRUCTOR

		public DTDReaderDTD()
		{
		}

		#endregion

		#region PUBLIC METHODS

		/// <summary>
		/// Erzeugt ein DTD-Objekt auf Basis einer DTD-Datei
		/// </summary>
		/// <param name="dateiname">Der Dateiname der DTD-Datei</param>
		public DTD GetDTDFromFile(string dateiname) 
		{
			string inhalt=""; // Der Inhalt der DTD-Datei
			
			try
			{
				StreamReader reader = new StreamReader(dateiname,System.Text.Encoding.GetEncoding("ISO-8859-15")); // Datei �ffnen
				inhalt = reader.ReadToEnd();
				reader.Close();
			}
			catch (FileNotFoundException exc) // Falls die Datei nicht gefunden wurde
			{
                // "Konnte Datei '{0}' nicht einlesen:\n{1}"
				throw new ApplicationException(String.Format(ResReader.Reader.GetString("KonnteDateiNichtEinlesen"), dateiname, exc.Message));
			}

			return GetDTDFromString(inhalt);
		}



		/// <summary>
		/// Erzeugt ein DTD-Objekt auf Basis einer DTD-Datei
		/// </summary>
		/// <param name="inhalt">Der Inhalt der DTD-Datei</param>
		/// <returns></returns>
		public DTD GetDTDFromString(string inhalt) 
		{
			// Tabs aus dem inhalt durch Leerzeichen ersetzen
			inhalt = inhalt.Replace ("\t"," ");

			// Den gelesenen Inhalt merken
			_rohinhalt = inhalt;
			_workingInhalt = inhalt;

			// Elemente suchen und in die DTD f�llen
			_elemente = new List<DTDElement>(); // Noch keine Elemente vorhanden
            _entities = new List<DTDEntity>();
			InhaltAnalysieren();	// definierte Elemente einlesen

			_elemente.Add(CreateElementFromQuellcode("#PCDATA")); // um das Element #PCDATA erg�nzen
            _elemente.Add(CreateElementFromQuellcode("#COMMENT")); // um das Element #COMMENT erg�nzen

            DTD dtd = new DTD(_elemente,_entities);

            return dtd;
		}

		#endregion

		#region PRIVATE METHODS

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
			string ausdruck =  "<!--((?!-->|<!--)([\\t\\r\\n]|.))*-->";
			_workingInhalt =  Regex.Replace(_workingInhalt ,ausdruck,"");
		}

		#region ELEMENTE analysieren

		/// <summary>
		/// Liest alle im DTD-Inhalt enthaltenen DTD-Elemente aus
		/// </summary>
		private void ElementeAuslesen() 
		{
			DTDElement element;
			string elementCode;

			// Regul�ren Ausdruck zum finden von DTD-Elementen zusammenbauen
			// (?<element><!ELEMENT[\t\r\n ]+[^>]+>)
			string ausdruck =  "(?<element><!ELEMENT[\\t\\r\\n ]+[^>]+>)";

			Regex reg = new Regex(ausdruck); //, RegexOptions.IgnoreCase);
			// Auf den DTD-Inhalt anwenden
			Match match = reg.Match(_workingInhalt );

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
                    throw new ApplicationException(String.Format(ResReader.Reader.GetString("FehlerBeimLesenDesDTDELementes"),element.Name,e.Message));
                }
				match = match.NextMatch(); // Zum n�chsten RegEx-Treffer
			}

            // Nun die sortierte Liste in die Elementliste �berf�hren
            for (int i  = 0; i <gefundene.Count;i++) 
            {
                _elemente.Add((DTDElement)gefundene[gefundene.GetKey(i)]);
            }

		}

		/// <summary>
		/// Wertet den Element-Quellcode aus und speichert den Inhalt strukturiert im Element-Objekt
		/// </summary>
		/// <example>
		/// z.B. so etwas k�nnte im Element-Quellcode stehen:
		/// <!ELEMENT template  (#PCDATA | srai | sr | that | get | bot | birthday | set | A | star | random )*>
		/// </example>
		private DTDElement CreateElementFromQuellcode(string elementQuellcode) 
		{
			if (elementQuellcode=="#PCDATA") // Es ist kein in der DTD definiertes Element, sondern das PCDATA-Element
			{
                DTDElement element = new DTDElement();
				element.Name = "#PCDATA";
                element.ChildElemente = new DTDChildElemente("");
				return element;
			}

            if (elementQuellcode == "#COMMENT") // Es ist kein in der DTD definiertes Element, sondern das COMMENT-Element
            {
                DTDElement element = new DTDElement();
                element.Name = "#COMMENT";
                element.ChildElemente = new DTDChildElemente("");
                return element;
            }

			// Der folgende Ausdruck zerteilt das ELEMENT-Tag in seine Bestandteile. Gruppen:
			// element=das ganze Elementes
			// elementname=der Name des Elementes
			// innerelements=Liste der Child-Elemente, die im Element vorkommen d�rfen 
			string regpatternelement = @"(?<element><!ELEMENT[\t\r\n ]+(?<elementname>[\w-_]+?)([\t\r\n ]+(?<innerelements>[(]([\t\r\n]|.)+?[)][*+]?)?)?(?<empty>[\t\r\n ]+EMPTY)? *>)";

			// Regul�ren Ausdruck zum Finden der Element-Teile zusammenbauen
			Regex reg = new Regex(regpatternelement); //, RegexOptions.IgnoreCase);

			// Auf den Element-Quellcode anwenden
			Match match = reg.Match(elementQuellcode);

			if (!match.Success) // Wenn kein Element im Element-Code gefunden wurde
			{
                // "Kein Vorkommen gefunden im Elementcode '{0}'."
                throw new ApplicationException(String.Format(ResReader.Reader.GetString("NichtsImElementCodeGefunden"), elementQuellcode));
			}
			else // ein Element gefunden
			{

				//Element bereitstellen
                DTDElement element = new DTDElement();

				// Name des Elementes herausfinden
				if (!match.Groups["elementname"].Success) 
				{	// kein Name gefunden
                    // "Kein Name gefunden im Elementcode '{0}'."
                    throw new ApplicationException(String.Format(ResReader.Reader.GetString("KeinNameInElementcodegefunden"), elementQuellcode));
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
                    // "Mehr als ein Vorkommen gefunden im Elementcode '{0}'."
					throw new ApplicationException(string.Format(ResReader.Reader.GetString("MehrAlsEinsImElementCodeGefunden"),  elementQuellcode ));
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
		/// w�rde als ChildElementQuellCode erwartet
		/// (#PCDATA | srai | sr | that | get | bot | birthday | set | A | star | random )*
		/// </example>
		private void ChildElementeAuslesen(DTDElement element, string childElementeQuellcode) 
		{
			element.ChildElemente  = new DTDChildElemente(childElementeQuellcode);
		}

		#endregion

		#region ENTITIES analysieren

		/// <summary>
		/// Setzt f�r die verschiedenen Entities an den zitierten Stellen den Inhalt der Entities ein
		/// </summary>
		private void EntitiesAustauschen() 
		{
			string vorher="";
			while (vorher != _workingInhalt)   // Solange das Einsetzen der Enities noch Ver�nderung bewirkt hat
			{
				vorher = _workingInhalt;
				foreach (DTDEntity entity in this._entities) // Alle Enities durchlaufen
				{
					if (entity.IstErsetzungsEntity ) // wenn es eine Ersetzung-Entity ist
					{
						// Nennung des Entity %name; durch den Inhalt der Entity ersetzen
						_workingInhalt = _workingInhalt.Replace ("%"+entity.Name +";",entity.Inhalt ); 
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

			// Regul�ren Ausdruck zum finden von DTD-Entities zusammenbauen
			// (?<entity><!ENTITY[\t\r\n ]+[^>]+>)
			string ausdruck =  "(?<entity><!ENTITY[\\t\\r\\n ]+[^>]+>)";

			Regex reg = new Regex(ausdruck); //, RegexOptions.IgnoreCase);
			// Auf den DTD-Inhalt anwenden
			Match match = reg.Match(_workingInhalt);

			// Alle RegEx-Treffer durchlaufen und daraus Elemente erzeugen
			while (match.Success) 
			{
				entityCode = match.Groups["entity"].Value;
				entity = CreateEntityFromQuellcode(entityCode);
				_entities.Add(entity);
				match = match.NextMatch(); // Zum n�chsten RegEx-Treffer
			}
		}

		/// <summary>
		/// Wertet den Entity-Quellcode aus und speichert den Inhalt strukturiert im Objekt
		/// </summary>
		/// <example>
		/// z.B. so etwas k�nnte im Entity-Quellcode stehen:
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
			string regpatternelement = "(?<entity><!ENTITY[\\t\\r\\n ]+(?:(?<prozent>%)[\\t\\r\\n ]+)?(?<entityname>[\\w-_]+?)[\\t\\r\\n ]+\"(?<inhalt>[^>]+)\"[\\t\\r\\n ]?>)";

			// Regul�ren Ausdruck zum Finden der Entity-Teile zusammenbauen
			Regex reg = new Regex(regpatternelement); //, RegexOptions.IgnoreCase);

			// Auf den Entity-Quellcode anwenden
			Match match = reg.Match(entityQuellcode);

			if (!match.Success) // Wenn keine Entity im Entity-Code gefunden wurde
			{
                // "Kein Vorkommen gefunden im Entityquellcode '{0}'"
                throw new ApplicationException(String.Format(ResReader.Reader.GetString("NichtsImEntityCode"), entityQuellcode));
			}
			else // Genau eine Entity gefunden
			{
				DTDEntity entity = new DTDEntity();

				// am Prozentzeichen festmachen, ob Ersetzungs-Entity
				entity.IstErsetzungsEntity =  (match.Groups["prozent"].Success);

				// Name der Entity herausfinden
				if (!match.Groups["entityname"].Success) 
				{	// kein Name gefunden
                    // "Kein Name gefunden im Entitycode '{0}'"
					throw new ApplicationException(String.Format(ResReader.Reader.GetString("KeinNameImEntityCode") , entityQuellcode  ));
				} 
				else 
				{
					// Name gefunden
					entity.Name = match.Groups["entityname"].Value; // Name merken

					// Inhalt der Entity herausfinden
					if (!match.Groups["inhalt"].Success) 
					{	// kein Inhalt gefunden
                        // "Kein Inhalt gefunden im Entitycode '{0}'"
						throw new ApplicationException(String.Format(ResReader.Reader.GetString("KeinInhaltImEntityCode") , entityQuellcode  ));
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
                    // "Mehr als ein Vorkommen gefunden im Entitycode '{0}'"
					throw new ApplicationException(String.Format(ResReader.Reader.GetString("MehrAlsEinsImEntityQuellCode") , entityQuellcode ));
				}
				return entity;
			}


			
		}

		#endregion analyisieren

		#region ATTRIBUTE analysieren

		/// <summary>
		/// Stellt f�r das angegebene Element die entsprechenden Attribute bereit, sofern sie vorhanden sind
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		private void CreateDTDAttributesForElement(DTDElement element) 
		{
			
			element.Attribute = new List<DTDAttribut> ();

			// Regul�ren Ausdruck zum finden der AttributList-Definition zusammenbauen
			// (?<attributliste><!ATTLIST muster_titel[\t\r\n ]+(?<attribute>[^>]+?)[\t\r\n ]?>)
			string ausdruckListe =  "(?<attributliste><!ATTLIST " + element.Name +"[\\t\\r\\n ]+(?<attribute>[^>]+?)[\\t\\r\\n ]?>)";

			Regex regList = new Regex(ausdruckListe); //, RegexOptions.IgnoreCase);
			// Auf den DTD-Inhalt anwenden
			Match match = regList.Match(_workingInhalt);

			if (match.Success) 
			{
				// Die Liste der Attribute holen
				string attributListeCode = match.Groups["attribute"].Value;

				// In der Liste der Attribute die einzelnen Attribute isolieren
				// Regul�ren Ausdruck zum finden der einzelnen Attribute in der AttribuList zusammenbauen
				// [\t\r\n ]?(?<name>[\w-_]+)[\t\r\n ]+(?<typ>CDATA|ID|IDREF|IDREFS|NMTOKEN|NMTOKENS|ENTITY|ENTITIES|NOTATION|xml:|[(][|\w-_ \t\r\n]+[)])[\t\r\n ]+(?:(?<anzahl>#REQUIRED|#IMPLIED|#FIXED)[\t\r\n ]+)?(?:"(?<vorgabewert>[\w-_]+)")?[\t\r\n ]?
				string ausdruckEinzel =  "[\\t\\r\\n ]?(?<name>[\\w-_]+)[\\t\\r\\n ]+(?<typ>CDATA|ID|IDREF|IDREFS|NMTOKEN|NMTOKENS|ENTITY|ENTITIES|NOTATION|xml:|[(][|\\w-_ \\t\\r\\n]+[)])[\\t\\r\\n ]+(?:(?<anzahl>#REQUIRED|#IMPLIED|#FIXED)[\\t\\r\\n ]+)?(?:\"(?<vorgabewert>[\\w-_]+)\")?[\\t\\r\\n ]?";

				Regex regEinzel = new Regex(ausdruckEinzel); //, RegexOptions.IgnoreCase);
				// Auf den DTD-Inhalt anwenden
				match = regEinzel.Match(attributListeCode);

				if (match.Success) 
				{

					DTDAttribut attribut;
					string typ;
					string[] werteListe;
					string delimStr = "|";
					char [] delimiter = delimStr.ToCharArray();


					// Alle RegEx-Treffer durchlaufen und daraus Attribute f�r das Element erzeugen
					while (match.Success) 
					{
						attribut = new DTDAttribut (); // Attribut erzeugen
						attribut.Name = match.Groups["name"].Value; // Name des Attributes
						attribut.StandardWert = match.Groups["vorgabewert"].Value; // StandardWert des Attributes
						// Die Anzahl / Pflicht des Attributes
						switch (match.Groups["anzahl"].Value) 
						{
							case "#REQUIRED":
								attribut.Pflicht = DTDAttribut.PflichtArten.Pflicht;
								break;
							case "#IMPLIED":
							case "":
								attribut.Pflicht = DTDAttribut.PflichtArten.Optional;
								break;
							case "#FIXED":
								attribut.Pflicht = DTDAttribut.PflichtArten.Konstante;
								break;
							default:
                                //"Unbekannte AttributAnzahl '{0}' in Attribut '{1}' von Element {2}"
                                throw new ApplicationException(String.Format(ResReader.Reader.GetString("UnbekannteAttributAnzahl"), match.Groups["anzahl"].Value, match.Value, element.Name ));

						}
						// Der Typ des Attributes
						typ = match.Groups["typ"].Value; 
						typ = typ.Trim();
						if (typ.StartsWith ("("))  // Es ist eine Aufz�hlung der zul�ssigen Werte dieses Attributes (en1|en2|..)
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

						match = match.NextMatch(); // Zum n�chsten RegEx-Treffer
					}
				} 
				else 
				{
                    //"Keine Attribute in der AttribuListe '{0}' gefunden!"
					throw new ApplicationException(String.Format(ResReader.Reader.GetString("KeineAttributeInAttributListe"), attributListeCode ));
				}

			} 
			else 
			{
				// Zu diesem Element wurden keine Attribute gefunden
                // "Keine Attribute f�r Element {0} vorhanden."
				Trace.WriteLine (String.Format("Keine Attribute f�r Element {0} vorhanden.",element.Name));
			}
		}


		#endregion


		#endregion
	}
}