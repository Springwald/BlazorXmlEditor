using de.springwald.toolbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace de.springwald.xml.dtd
{
	/// <summary>
	/// Ein einzelnes DTD-Element aus einer DTD
	/// </summary>
	/// <remarks>
	/// (C)2006 Daniel Springwald, Herne Germany
	/// Springwald Software  - www.springwald.de
	/// daniel@springwald.de -   0700-SPRINGWALD
	/// all rights reserved
	/// </remarks>
	public class DTDElement
	{

		#region PRIVATE ATTRIBUTES

		private string _name;						// Der eindeutige Name dieses Elementes
		private DTDChildElemente _children;			// Child-Elemente dieses Elementes
        private Regex _childrenRegExObjekt;         // Liefert ein RegEx-Objekt, mit welchem man Childfolgen darauf hin prüfen kann, ob sie für dieses Element gültig sind
        private StringCollection _alleElementNamenWelcheAlsDirektesChildZulaessigSind; // Diese DTD-Elemente dürfen innerhalb dieses Elementes vorkommen
		
		#endregion

		#region PUBLIC ATTRIBUTES

		/// <summary>
		/// Der eindeutige Name dieses Elementes
		/// </summary>
		public string Name 
		{
			get { return _name; }
            set { _name = value; }
		}

 		/// <summary>
		/// Die Child-Elemente dieses Elementes
		/// </summary>
		public DTDChildElemente ChildElemente 
		{
			get { return _children; }
			set{ _children = value; }
		}

        /// <summary>
        /// Diese DTD-Elemente dürfen innerhalb dieses Elementes vorkommen.
        /// </summary>
        public StringCollection AlleElementNamenWelcheAlsDirektesChildZulaessigSind
        {
            get {
                if (_alleElementNamenWelcheAlsDirektesChildZulaessigSind == null)
                {
                    _alleElementNamenWelcheAlsDirektesChildZulaessigSind = GetDTDElementeNamenAusChildElementen_(_children);
                    // Das Kommentar-Tag hinzufügen, da dieses immer zulässig ist
                    _alleElementNamenWelcheAlsDirektesChildZulaessigSind.Add("#COMMENT");
                }
                return _alleElementNamenWelcheAlsDirektesChildZulaessigSind;
            }
        }

		/// <summary>
		/// Die für dieses Element bekannten Attribute
		/// </summary>
		public List<DTDAttribut> Attribute { get; set;}

        /// <summary>
        /// Liefert ein RegEx-Objekt, mit welchem man Childfolgen darauf hin prüfen kann, ob sie für dieses
        /// Element gültig sind
        /// </summary>
        public Regex ChildrenRegExObjekt
        {
            get
            {
                if (_childrenRegExObjekt == null)
                {
                    StringBuilder ausdruck = new StringBuilder();
                    ausdruck.Append(">");
                    ausdruck.Append(_children.RegExAusdruck);
                    ausdruck.Append("<");
                    _childrenRegExObjekt = new Regex(ausdruck.ToString());// RegexOptions.Compiled);
                }
                return _childrenRegExObjekt;
            }
        }

		#endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Erzeugt ein DTDElement auf Basis des übergebenen DTD-Element-Quellcodes
        /// </summary>
        public DTDElement()
        {
        }

        #endregion

        #region PUBLIC METHODS



        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Liefert die Liste aller in den Children erwähnten Elemente
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        private StringCollection GetDTDElementeNamenAusChildElementen_(DTDChildElemente children) {
            StringCollection liste = new StringCollection();

            switch (children.Art)
            {
                case DTDChildElemente.DTDChildElementArten.EinzelChild:
                    // Ist ein einzelnes ChildElement und noch nicht in der Liste: Hinzufügen
                    if (!liste.Contains(children.ElementName)) {
                        liste.Add(children.ElementName);
                    }
                    break;

                case DTDChildElemente.DTDChildElementArten.ChildListe:
                    for (int i = 0; i < children.AnzahlChildren; i++)
                    {
                        foreach (string childElementName in GetDTDElementeNamenAusChildElementen_(children.Child(i)))
                        {
                            if (!liste.Contains(childElementName))
                            {
                                liste.Add(childElementName);
                            }
                        }
                    }
                    break;

                case DTDChildElemente.DTDChildElementArten.Leer:
                    break;

                default:
                    // "Unbekannte DTDChildElementArt {0}"
                    throw new ApplicationException(String.Format(ResReader.Reader.GetString("UnbekannteDTDChildElementArt"), children.Art));
            }

            return liste ;
        }

		#endregion

		
	}
}
