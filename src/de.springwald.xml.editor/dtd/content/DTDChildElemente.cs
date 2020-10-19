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
using de.springwald.toolbox;

namespace de.springwald.xml.dtd
{
    /// <summary>
    /// Manages the child elements of a DTD element, i.e. the parts within the brackets of the element tag
    /// </summary>
    public class DTDChildElemente
	{
        /// <summary>
        /// What kind of child is the given child?
        /// </summary>
        public enum DTDChildElementArten  { Leer=0, EinzelChild=-1, ChildListe=2 };

        /// <summary>
        /// This element may occur at the specified position as often as
        /// </summary>
        /// <remarks>
        /// GenauEinmal=kein Zeichen
        /// NullUndMehr=*
        /// EinsUndMehr=+
        /// </remarks>
        public enum DTDChildElementAnzahl { GenauEinmal=0, NullUndMehr=-1, NullOderEinmal= 2, EinsUndMehr=3 };

		/// <summary>
		/// Sind die ChildElemente in dieser Liste durch ODER getrennt, oder müssen
		/// sie in der angebenen Reihenfolge auftreten
		/// </summary>
		public enum DTDChildElementOperatoren { GefolgtVon=0, Oder=-1 };

		private DTDChildElementArten _art;              // Dieser Art ist dieser Child-Bereich
		private DTDChildElementAnzahl _defAnzahl;       // So oft darf dieser Childblock vorkommen
		private DTDChildElementOperatoren _operator;	// Sind die ChildElemente in dieser Liste durch ODER getrennt, oder müssen sie in der angebenen Reihenfolge auftreten

		private ArrayList _children;		// Die Children dieses Child-Bereiches
        private string _elementName;	    // Wenn dies ein EinzelChild oder eine Entity ist, dann wird hier das Name des Elementes vermerkt

        private DTD _dtd;                   // Die DTD, auf welcher alles basiert
		private string _quellcode;			// Der Quellcode, aus dem dieser Childbereich entstanden ist

        private AlleMoeglichenElementeEinesChildblocks _alleMoeglichenElemente; // Ermittelt alle Elemente, welche dieser Childblock überhaupt jemals abdecken / enthalten kann

        private string _regExAusdruck;  // Der diesem Childblock entsprechende RegEx-Ausdruck


        /// <summary>
        /// Der diesem Childblock entsprechende RegEx-Ausdruck
        /// </summary>
        public string RegExAusdruck
        {
            get {
                if (_regExAusdruck == null)
                {
                    StringBuilder ausdruck = new StringBuilder();
                    ausdruck.Append("(");

                    // Den Inhalt zusammenbauen
                    switch (_art)
                    {
                        case DTDChildElementArten.Leer:
                            break;
                        case DTDChildElementArten.EinzelChild:

                            if (_elementName != "#COMMENT")
                            {
                                ausdruck.AppendFormat("((-#COMMENT)*-{0}(-#COMMENT)*)", _elementName);
                            }
                            else
                            {
                                ausdruck.AppendFormat("(-{0})", _elementName);
                            }

                            break;
                        case DTDChildElementArten.ChildListe:
                            ausdruck.Append("(");
                            for (int i = 0; i < _children.Count; i++)
                            {
                                if (i != 0)
                                {
                                    switch (_operator)
                                    {
                                        case DTDChildElementOperatoren.Oder:
                                            ausdruck.Append("|");
                                            break;
                                        case DTDChildElementOperatoren.GefolgtVon:
                                            break;
                                        default:
                                            throw new ApplicationException("Unhandled DTDChildElementOperatoren '" + _operator + "'");
                                    }
                                }
                                ausdruck.Append(((DTDChildElemente)_children[i]).RegExAusdruck);
                            }
                            ausdruck.Append(")");
                            break;
                        default:
                            throw new ApplicationException("Unhandled DTDChildElementArt '" + _art + "'");
                    }

                    // Die Anzahl anfügen
                    switch (_defAnzahl)
                    {
                        case DTDChildElementAnzahl.EinsUndMehr:
                            ausdruck.Append("+");
                            break;
                        case DTDChildElementAnzahl.GenauEinmal:
                            break;
                        case DTDChildElementAnzahl.NullOderEinmal:
                            ausdruck.Append("?");
                            break;
                        case DTDChildElementAnzahl.NullUndMehr:
                            ausdruck.Append("*");
                            break;
                        default:
                            throw new ApplicationException("Unhandled DTDChildElementAnzahl '" + _defAnzahl + "'");
                    }

                    ausdruck.Append(")");
                    _regExAusdruck = ausdruck.ToString();
                }

                return _regExAusdruck;
            }
        }

        /// <summary>
        /// Ermittelt alle Elemente der angebenen DTD, welche dieser Childblock überhaupt jemals abdecken / enthalten kann
        /// </summary>
        public AlleMoeglichenElementeEinesChildblocks AlleMoeglichenElemente
        {
            get
            {
                if (_alleMoeglichenElemente == null)
                {
                    _alleMoeglichenElemente = new AlleMoeglichenElementeEinesChildblocks(this);
                }
                return _alleMoeglichenElemente;
            }
        }

		public string Quellcode 
		{
			get { return _quellcode; }
		}

		/// <summary>
		/// Dieser Art ist dieser Child-Bereich
		/// </summary>
		public DTDChildElementArten Art 
		{
			get { return _art; }
		}

		/// <summary>
		/// So oft darf dieser Childblock vorkommen
		/// </summary>
		public DTDChildElementAnzahl DefAnzahl 
		{
			get { return _defAnzahl; }
		}

		/// <summary>
		/// Sind die ChildElemente in dieser Liste durch ODER getrennt, oder müssen sie in der angebenen Reihenfolge auftreten
		/// </summary>
		public DTDChildElementOperatoren Operator 
		{
			get { return _operator; }
		}

		/// <summary>
		/// So viele Children sind für dieses 
		/// </summary>
		public int AnzahlChildren 
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
        public string ElementName
        {
            get {  return _elementName; }
        }

		/// <summary>
		/// Stellt einen ChildElemente-Block auf Basis des übergebenen DTD-Quellcodes bereit
		/// </summary>
		/// <param name="childrenQuellcode">
		/// Der DTD-Quellcode der ChildElemente
		/// </param>
		/// <example>
		/// So kann z.B. eine Quellcodeangabe aussehen:
		/// (#PCDATA | srai | sr | that | get | bot | birthday | set | A | star | random )*
		/// </example>
		public DTDChildElemente(string childrenQuellcode)
		{
			// Grundwerte initialisieren
			this._art = DTDChildElementArten.Leer;
			this._children = new ArrayList();
			this._defAnzahl = DTDChildElementAnzahl.GenauEinmal;
			this._elementName = "";
			this._operator = DTDChildElementOperatoren.Oder;

			_quellcode = childrenQuellcode;

			// Tabs, Umbrüche und doppelte Space raus
			_quellcode = _quellcode.Replace("\t"," ");
			_quellcode = _quellcode.Replace("\r\n"," ");
			_quellcode = _quellcode.Trim();

			if (_quellcode.Length == 0) // Kein Child angegeben
			{
				this._art = DTDChildElementArten.Leer;
			} 
			else // Es sind Children vorhanden
			{
				ReadCode();
			}

		}

        /// <summary>
        /// Stellt einen ChildElemente-Block auf Basis des übergebenen DTD-Quellcodes bereit
        /// </summary>
        /// <remarks>Nur für internen Gebrauch, z.B. für die Clone-Methode</remarks>
        protected DTDChildElemente()
        {
        }


        /// <summary>
        /// Erzeugt eine Kopie dieses ChildBlocks
        /// </summary>
        /// <returns></returns>
        public DTDChildElemente Clone()
        {
            DTDChildElemente klon = new DTDChildElemente();

            klon._alleMoeglichenElemente = null;

            klon._art = _art;
            klon._operator = _operator;
            klon._defAnzahl = _defAnzahl;

            klon._children = (ArrayList)_children.Clone();
            
            klon._dtd = _dtd;
            klon._elementName = _elementName;

            klon._quellcode = _quellcode + "(geklont)";
            
            return klon;
        }

        /// <summary>
        /// Weist diesem Child zu, welcher DTD es angehört
        /// </summary>
        /// <param name="dtd"></param>
        public void DTDZuweisen(DTD dtd)
        {
            // An die Unter-Children weiterreichen
            foreach (DTDChildElemente child in _children)
            {
                child.DTDZuweisen(dtd);
            }

            // auch selbst auswerten
            _dtd = dtd;
        }

		/// <summary>
		/// prüft, ob die angegebene Anzahl für diesen Elementeblock zulässig ist
		/// </summary>
		/// <param name="anzahl"></param>
		/// <returns></returns>
		public bool AnzahlZulaessig(int anzahl) 
		{
			switch (_defAnzahl) 
			{
				case (DTDChildElementAnzahl.EinsUndMehr):
					if (anzahl >=1) return true; else return false;
				case (DTDChildElementAnzahl.GenauEinmal):
					if (anzahl ==1) return true; else return false;;
				case (DTDChildElementAnzahl.NullOderEinmal):
					if (anzahl ==0 || anzahl==1) return true; else return false;;
				case (DTDChildElementAnzahl.NullUndMehr):
					if (anzahl >=0) return true; else return false;;
				default:
                    // "unknown DTDChildElementAnzahl: {0}"
					throw new ApplicationException(String.Format(ResReader.Reader.GetString("UnbekannteDTDChildElementAnzahl"), _art));
			}
		}

		/// <summary>
		/// Das index´te Child bzw. Childliste
		/// </summary>
		/// <param name="index">Nummer des gewünschten Child, nullbasiert</param>
		/// <returns></returns>
		public DTDChildElemente Child(int index) 
		{
			return (DTDChildElemente)this._children[index];
		}

	
		/// <summary>
		/// Verarbeitet den Quellcode zu Children
		/// </summary>
		private void ReadCode() 
		{
			string code=_quellcode;

			// Wie oft darf dieser ChildBlock vorkommen
			string anzahlAngabe = code.Substring(code.Length-1,1);
			switch (anzahlAngabe) 
			{
				case "+": 
					this._defAnzahl = DTDChildElementAnzahl.EinsUndMehr;
					code = code.Remove (code.Length-1,1); // + entfernen
					break;
				case "*":
					this._defAnzahl = DTDChildElementAnzahl.NullUndMehr;
					code = code.Remove (code.Length-1,1); // * entfernen
					break;
				case "?":
					this._defAnzahl = DTDChildElementAnzahl.NullOderEinmal;
					code = code.Remove (code.Length-1,1); // ? entfernen
					break;
				default: 
					this._defAnzahl = DTDChildElementAnzahl.GenauEinmal;
					break;
			}
			code = code.Trim();

			// Prüfen ob ein Klammerblock vorhanden ist
			if ((code.Substring(0,1)=="(") && (code.Substring(code.Length-1,1)==")")) 
			{ // Es sind Klammern vorhanden, also sind es mehrere Children 
				code = code.Substring(1,code.Length-2); // Die Klammern entfernen
				ReadChildren(code); // Die Children erkennen
			} 
			else 
			{ // Keine Klammern vorhanden, dann ist es wohl nur ein einzelnes Child
				this._art = DTDChildElementArten.EinzelChild;
				this._elementName = code;
			}
		}

		/// <summary>
		/// Wertet die vorhandenen Childran aus
		/// </summary>
		/// <param name="code"></param>
		private void ReadChildren(string code) 
		{
			string rohcode = code;

			_art = DTDChildElementArten.ChildListe;

			int klammerebene = 0;
			StringBuilder aktElement = new StringBuilder() ;

			// Solange noch Inhalt da ist, nach weiteren Elementen suchen
			while (code.Length>0) 
			{
				string nextChar=code.Substring(0,1); // nächstes Zeichen holen
				code = code.Remove(0,1); // Das verarbeitete Zeichen entfernen

				// Bei Klammerung wird der geklammerte Bereich komplett als ein 
				// Child gewertet. Es wird daher als Block rekursiv zu ChildErkennung 
				// weitergegeben und hier nicht analyisert.
				switch (nextChar) 
				{
					case "(": 
						klammerebene++;
						break;
					case ")": 
						klammerebene--;
						break;
				}
				if (IstOperator(nextChar)) // Das aktuelle Child wurde beendet
				{
					if (klammerebene==0) // wir befinden uns nicht innerhalb einer ChildKaspselung
					{
						// Bestimmen, welches der Operator zwischen den ChildElementen ist
						this._operator = GetOperatorFromChar(nextChar); 

						string fertig= aktElement.ToString().Trim();

						if  (fertig.Length==0) 
						{
							throw new ApplicationException("Leerer ChildCode gefunden in '" + rohcode + "'");
						}
						else 
						{
							// Den bisher gesammelten Element-String als Child speichern
							SaveChildElement(fertig);
						}
						// Neues Element beginnen
						aktElement = new StringBuilder();
					} 
					else // Operator gehört zum innen eingeschlossenen Childblock
					{
						aktElement.Append(nextChar);
					}
				} 
				else // Zeichen ist kein Operator
				{
					aktElement.Append(nextChar);
				}
			}

			// Wenn zum Schluss noch ein begonnenes Child-Element übrig ist, dieses 
			// auch abschließen und speichern
			if (aktElement.Length > 0) 
			{
				SaveChildElement(aktElement.ToString());
			}
		}

		/// <summary>
		/// Reiht ein Child-Element in die Liste ein
		/// </summary>
		/// <param name="code"></param>
		private void SaveChildElement(string code) 
		{
			code = code.Trim();  
			DTDChildElemente child = new DTDChildElemente (code); // Aus dem code neues Child oder Childliste erzeugen
			//if (child.Art ==DTDChildElementArten.EinzelChild) 
			//{
			//	Trace.WriteLine(code + child._defAnzahl.ToString());
			//}
			this._children.Add(child); // child in Liste speichern
			
		}

		/// <summary>
		/// Ist der angebene String ein Operator?
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		private bool IstOperator(string code) 
		{
			switch (code) 
			{
				case "|":
				case ",":	return true;
				default:	return false;
			}
		}


		/// <summary>
		/// Was für ein Opertator ist der angegebene String?
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		private DTDChildElementOperatoren GetOperatorFromChar(string code) 
		{
			switch (code) 
			{
				case "|":	return DTDChildElementOperatoren.Oder;
				case ",":	return DTDChildElementOperatoren.GefolgtVon;
			}

            // "Der angegebene String '" + code + "' ist kein Operator!"
            throw new ApplicationException(String.Format(ResReader.Reader.GetString("StringIstKeinOperator"), code));
		}

		
	}
}
