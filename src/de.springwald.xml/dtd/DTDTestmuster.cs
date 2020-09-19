using System;
using System.Collections.Specialized;
using System.Text;

namespace de.springwald.xml.dtd
{
	/// <summary>
	/// Ein XML-Block, wie er nach der angestrebten Veränderung aussehen könnte.
	/// Dieses Muster wird durch den Prüfer gejagt. Alle Muster, welche anschließend 
	/// als "bestätigt" geflagt wurden, sind lt. DTD zulässig
	/// </summary>
	/// <remarks>
	/// (C)2005 Daniel Springwald, Herne Germany
	/// Springwald Software  - www.springwald.de
	/// daniel@springwald.de -   0700-SPRINGWALD
	/// all rights reserved
	/// </remarks>
	public class DTDTestmuster
	{
		#region PRIVATE ATTRIBUTES

		private string _elementName;	        // Das zum Test eingefügte Element zur Rückgabe nach Erledigung des Tests. Ist es NULL, bedeutet das, dass statt Einfügen das Löschen geprüft wurde
		private string _parentElementName;	    // Dieses Element liegt über der zu testenden Cursor Pos (Zeichnung:C)
		private bool _erfolgreich;		        // Ist das Muster erfolgreich anwendbar gewesen?

        private string _vergleichsStringFuerRegEx;

        private StringBuilder _elementNamenListe;

		#endregion

		#region PUBLIC ATTRIBUTES

		/// <summary>
		/// Das zum Test eingefügte Element. Ist es NULL, bedeutet das, dass statt Einfügen das Löschen geprüft wurde
		/// </summary>
		public string ElementName 
		{
			get { return _elementName; }
		}

        public string VergleichStringFuerRegEx
        {
            get
            {
                if (_vergleichsStringFuerRegEx == null)
                {
                    _elementNamenListe.Append("<");
                    _vergleichsStringFuerRegEx = _elementNamenListe.ToString();
                }
                return _vergleichsStringFuerRegEx;
            }
        }


		/// <summary>
		/// Eine schriftliche Zusammenfassung dieses Musters
		/// </summary>
		public string Zusammenfassung
		{
			get 
			{
				StringBuilder ergebnis = new StringBuilder();

				// Erfolgreich getestet?
				if (_erfolgreich) 
				{
					ergebnis.Append("+ ");
				}
				else 
				{
					ergebnis.Append("- ");
				}

				 // Der Name des ParentNodes
                ergebnis.Append(this._parentElementName);
				ergebnis.Append(" (");
                ergebnis.Append(VergleichStringFuerRegEx);
				ergebnis.Append(")");

				// Was wurde getestet?
				if (_elementName == null) 
				{
					ergebnis.Append(" [getestet: löschen]");
				}
				else 
				{
					ergebnis.AppendFormat("[getestet: {0}]", this._elementName );
				}

				return ergebnis.ToString();
			}
		}

		/// <summary>
		/// Ist das Muster erfolgreich anwendbar gewesen?
		/// </summary>
		public bool Erfolgreich {
			get { return _erfolgreich; }
			set { _erfolgreich = value; }
		}

		#endregion

        #region CONSTRUCTOR

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element">Das zum Test eingefügte Element. Ist es NULL, bedeutet das, dass statt Einfügen das Löschen geprüft wurde</param>
        /// <param name="parentElementName">Dieses Element liegt über der zu testenden Cursor Pos (Zeichnung:C)</param>
        public DTDTestmuster(string elementName, string parentElementName)
        {

            _elementNamenListe = new StringBuilder();
            _elementNamenListe.Append(">");

            this._elementName = elementName;
            this._parentElementName = parentElementName;
            this._erfolgreich = false; // Bisher nicht bestätigt
        }

        #endregion

        #region PUBLIC METHODS

        public void AddElement(string elementName)
        {
            _elementNamenListe.AppendFormat("-{0}", elementName);
        }

		#endregion

		#region PRIVATE METHODS

		#endregion

		
	}
}
