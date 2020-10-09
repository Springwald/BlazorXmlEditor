using System;
using System.Collections;
using System.Collections.Generic;

namespace de.springwald.xml.dtd
{
	/// <summary>
	/// Der Inhalt einer DTD
	/// </summary>
	/// <remarks>
	/// (C)2005 Daniel Springwald, Herne Germany
	/// Springwald Software  - www.springwald.de
	/// daniel@springwald.de -   0700-SPRINGWALD
	/// all rights reserved
	/// </remarks>
	public class DTD
	{

		/// <summary>
		/// Diese Ausnahme wird geworfen, wenn ein Element erfragt wurde, welches in der DTD nicht definiert ist
		/// </summary>
		public class XMLUnknownElementException : Exception 
		{
			private string _elementname;
			public XMLUnknownElementException (string elementname) { _elementname = elementname; }

			public string ElementName 
			{
				get { return _elementname; }
			}

		}

		#region PRIVATE ATTRIBUTES

        private List<DTDElement> _elemente;		// Die in dieser DTD verfügbaren Elemente

        private Hashtable _elementeNachNamen;

        private List<DTDEntity> _entities;		// Die bekannten Entity-Einträge dieser DTD

		#endregion

		#region PUBLIC ATTRIBUTES


		/// <summary>
		/// Die in dieser DTD verfügbaren Elemente
		/// </summary>
        public List<DTDElement> Elemente 
		{
			get { return _elemente; }
		}

		/// <summary>
		/// Die in dieser DTD verfügbaren Entities
		/// </summary>
        public List<DTDEntity> Entities 
		{
			get { return _entities; }
		}

		#endregion

		#region CONSTRUCTOR

        public DTD(List<DTDElement> elemente, List<DTDEntity> entities)
		{
			_elemente = elemente;
			_entities = entities;
            _elementeNachNamen = new Hashtable();
		}

		public DTD()
		{
		}

		#endregion 

		#region PUBLIC METHODS

		/// <summary>
		/// Findet heraus, ob ein Element in dieser DTD bekannt ist
		/// </summary>
		/// <param name="elementName"></param>
		/// <returns></returns>
		public bool IstDTDElementBekannt(string elementName) 
		{
            return (DTDElementByNameIntern_(elementName,false) != null);
		}


		/// <summary>
		/// Findet das dem angegebenen Node entsprechende DTD-Element
		/// </summary>
		/// <param name="elementName"></param>
		public DTDElement DTDElementByNode_(System.Xml.XmlNode node, bool fehlerWennNichtVorhanden) 
		{
			return DTDElementByNameIntern_(GetElementNameFromNode(node),fehlerWennNichtVorhanden);
		}

		/// <summary>
		/// Findet das dem angegebenen Namen entsprechende DTD-Element
		/// </summary>
		/// <param name="elementName"></param>
        public DTDElement DTDElementByName(string elementName, bool fehlerWennNichtVorhanden) 
		{
            return DTDElementByNameIntern_(elementName, fehlerWennNichtVorhanden);
		}

		/// <summary>
		/// Ermittelt für die Vergleichsmuster den Namen des angegebenen Nodes
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static string GetElementNameFromNode(System.Xml.XmlNode node) 
		{
            if (node == null) return "";

			//if (node.Name == "#text") 
            if (node is System.Xml.XmlText) 
			{
				return "#PCDATA";
			} 
			else 
			{
                //if (node.Name == "#comment")
                if (node is System.Xml.XmlComment)
                {
                    return "#COMMENT";
                }
                else
                {
                    //if (node.Name == "#whitespace")
                    if (node is System.Xml.XmlWhitespace)
                    {
                        return "#WHITESPACE";
                    }
                    else
                    {
                        return node.Name;
                    }
                }
			}
		}

		#endregion

		#region PRIVATE METHODS


        /// <summary>
        /// Findet das dem angegebenen Namen entsprechende DTD-Element
        /// </summary>
        /// <param name="elementName"></param>
        public DTDElement DTDElementByNameIntern_(string elementName, bool fehlerWennNichtVorhanden)
        {

            DTDElement elementInBuffer = (DTDElement)_elementeNachNamen[elementName];

            if (elementInBuffer != null)
            {
                return elementInBuffer;
            }
            else
            {
                foreach (DTDElement element in this._elemente)
                {
                    if (elementName == element.Name)
                    {
                        _elementeNachNamen.Add(elementName, element);
                        return element;
                    }
                }
                if (fehlerWennNichtVorhanden)
                {
                    // Das gesuchte DTD-Element mit diesem Namen existiert nicht in dieser DTD.
                    throw new XMLUnknownElementException(elementName);
                }
                else
                {
                    return null;
                }
            }
        }

		#endregion

	}
}
