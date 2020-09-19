using System;
using System.Text;
using de.springwald.xml.cursor;
using System.Collections.Specialized;
using System.Collections;
using de.springwald.toolbox;

namespace de.springwald.xml.dtd.pruefer
{

	/// <summary>
	/// Prüft Nodes und Attribute etc. innerhalb eines Dokumentes darauf hin, ob sie erlaubt sind
	/// </summary>
	/// <remarks>
	/// (C)2005 Daniel Springwald, Herne Germany
	/// Springwald Software  - www.springwald.de
	/// daniel@springwald.de -   0700-SPRINGWALD
	/// all rights reserved
	/// </remarks>
	public class DTDPruefer
	{

		#region SYSTEM
		
		#endregion

		#region PRIVATE ATTRIBUTES

        private DTD _dtd; // Die DTD, gegen die geprüft werden soll
		private DTDNodeEditCheck _nodeCheckerintern;
		private StringBuilder _fehlermeldungen;

		
		private DTDNodeEditCheck NodeChecker 
		{
			get 
			{
				if (_nodeCheckerintern==null) 
				{
					_nodeCheckerintern = new DTDNodeEditCheck(_dtd);
				}
				return _nodeCheckerintern;
			}
		}

		#endregion

		#region PUBLIC ATTRIBUTES

		public string Fehlermeldungen 
		{
			get { return this._fehlermeldungen.ToString(); }
		}

		#endregion

		#region CONSTRUCTOR

		/// <summary>
		/// Prüft Nodes und Attribute etc. innerhalb eines Dokumentes darauf hin, ob sie erlaubt sind
		/// </summary>
		/// <param name="dtd">Die DTD, gegen die geprüft werden soll</param>
		public DTDPruefer(DTD dtd)
		{
			_dtd = dtd;
			this.Reset();
		}

		#endregion 

		#region PUBLIC METHODS

		/// <summary>
		/// Prüft, ob das übergebene XML-Objekt lt. der angegebenen DTD ok ist.
		/// </summary>
		/// <returns></returns>
		public bool IstXmlAttributOk(System.Xml.XmlAttribute xmlAttribut) 
		{
			this.Reset();
			return this.PruefeAttribut (xmlAttribut);
		}

		/// <summary>
		/// Prüft, ob das übergebene XML-Objekt lt. der angegebenen DTD ok ist.
		/// </summary>
		/// <param name="posBereitsGeprueft">Ist für diesen Node bereits bekannt, ob er dort stehen darf wo er steht?</param>
		/// <returns></returns>
		public bool IstXmlNodeOk(System.Xml.XmlNode xmlNode,bool posBereitsAlsOKBestaetigt) 
		{
			this.Reset();
			if (posBereitsAlsOKBestaetigt) 
			{
				return true;
			} 
			else 
			{
				return this.PruefeNodePos (xmlNode);
			}

			
		}

		#endregion

		#region PRIVATE METHODS

		/// <summary>
		/// Prüft einen XML-Node gegen die DTD
		/// <param name="node"></param>
		/// <returns></returns>
		private bool PruefeNodePos(System.Xml.XmlNode node) 
		{
			// Kommentar ist immer ok
			//if (node is System.Xml.XmlComment) return true;
            // Whitespace ist immer ok
            if (node is System.Xml.XmlWhitespace) return true;

			if (_dtd.IstDTDElementBekannt(DTD.GetElementNameFromNode(node)))// Das Element dieses Nodes ist in der DTD bekannt 
			{
				try 
				{
					if (this.NodeChecker.IstDerNodeAnDieserStelleErlaubt(node))
					{
						return true;
					} 
					else 
					{
                        // "Tag '{0}' hier nicht erlaubt: "
						_fehlermeldungen.AppendFormat(ResReader.Reader.GetString("TagHierNichtErlaubt"), node.Name);
						XMLCursorPos pos = new XMLCursorPos();
						pos.CursorSetzenOhneChangeEvent(node,XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);
						var erlaubteTags= this.NodeChecker.AnDieserStelleErlaubteTags_ (pos,false,false); // was ist an dieser Stelle erlaubt?
						if (erlaubteTags.Length > 0) 
						{
                            // "An dieser Stelle erlaubte Tags: "
                            _fehlermeldungen.Append(ResReader.Reader.GetString("ErlaubteTags"));
							foreach (string tag in erlaubteTags) 
							{
								_fehlermeldungen.AppendFormat("{0} ",tag );
							}
						} 
						else 
						{
                            //"An dieser Stelle sind keine Tags erlaubt. Wahrscheinlich ist das Parent-Tag bereits defekt."
							_fehlermeldungen.Append(ResReader.Reader.GetString("AnDieserStelleKeineTagsErlaubt"));
						}
						return false;
					}
				}
				catch (de.springwald.xml.dtd.DTD.XMLUnknownElementException e) 
				{
                    // "Unbekanntes Element '{0}'"
					_fehlermeldungen.AppendFormat(ResReader.Reader.GetString("UnbekanntesElement"), e.ElementName);
					return false;
				}
			} 
			else // Das Element dieses Nodes ist in der DTD gar nicht bekannt
			{
                //  "Unbekanntes Element '{0}'"
				_fehlermeldungen.AppendFormat(ResReader.Reader.GetString("UnbekanntesElement"), DTD.GetElementNameFromNode(node));
				return false;
			}
		}

		/// <summary>
		/// Prüft ein Attribut gegen die DTD
		/// </summary>
		/// <param name="attribut"></param>
		/// <returns></returns>
		private bool PruefeAttribut (System.Xml.XmlAttribute attribut) 
		{
			return false;
		}

		/// <summary>
		/// Setzt die Ergebnisse der letzten Prüfung auf Null zurück
		/// </summary>
		private void Reset() 
		{
			_fehlermeldungen = new StringBuilder();
		}

		#endregion
	}
}
