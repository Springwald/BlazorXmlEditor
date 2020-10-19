//#define DenkProtokoll // Soll das getestete Geschehen protokolliert werden?
                     

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using de.springwald.xml.cursor;
using System.Text;
using System.Text.RegularExpressions;
using de.springwald.toolbox;

namespace de.springwald.xml.dtd
{
	/// <summary>
	/// Prüft gegen eine DTD, ob und welche Veränderungen an einem XML-Dom erlaubt sind
	/// </summary>
	/// <remarks>
	/// (C)2006 Daniel Springwald, Herne Germany
	/// Springwald Software  - www.springwald.de
	/// daniel@springwald.de -   0700-SPRINGWALD
	/// all rights reserved
	/// </remarks>
	public class DTDNodeEditCheck
	{
		private DTD _dtd; // Die DTD, gegen die geprüft werden soll

        #if DenkProtokoll
		private StringBuilder _denkProtokoll; // Aufgrund welcher Annahmen wurde das Ergebnis von AnDieserStelleErlaubteTags erzeugt?
        #endif

		/// <summary>
		/// Aufgrund welcher Annahmen wurde das Ergebnis von AnDieserStelleErlaubteTags erzeugt?
		/// </summary>
		public string DenkProtokoll
		{
			get { 
                    #if DenkProtokoll
                        return _denkProtokoll.ToString(); 
                    #else
                        return "DenkProtokoll istper Define deaktiviert (DTDNodeEditCheck.cs)";
                    #endif
            }
		}

		public DTDNodeEditCheck(DTD dtd)
		{
            _dtd = dtd;

            #if DenkProtokoll
			_denkProtokoll= new StringBuilder(); // Aufgrund welcher Annahmen wurde das Ergebnis von AnDieserStelleErlaubteTags erzeugt?
            #endif
		}

		/// <summary>
		/// Welche Nodes sind an dieser Stelle im XML erlaubt?
		/// </summary>
		/// <param name="xmlPfad">Der XMLPfad</param>
		/// <param name="pcDATAMitAuflisten">wenn true, wird PCDATA mit als Node aufgeführt, sofern er erlaubt ist</param>
		/// <returns></returns>
		public string[] AnDieserStelleErlaubteTags_(XMLCursorPos zuTestendeCursorPos, bool pcDATAMitAuflisten, bool kommentareMitAufListen) 
		{
			// Damit nicht aus Versehen etwas an Änderungen zurückgeben wird, erstmal die CursorPos klonen
			XMLCursorPos cursorPos = zuTestendeCursorPos.Clone ();

            #if DenkProtokoll
			_denkProtokoll=new StringBuilder();
            #endif

			List<DTDTestmuster> zuTestendeMuster = GetAlleTestmuster(cursorPos);

			// Elemente der gültigen Testmuster ins das Ergebnis schreiben
			var ergebnis = new List<string>();
			foreach (DTDTestmuster muster in zuTestendeMuster)
			{ 
				if (muster.Erfolgreich) 
				{
					if (muster.ElementName == null) 
					{
						ergebnis.Add (""); // das vorhandene Element darf gelöscht werden
					} 
					else 
					{
                        switch (muster.ElementName.ToLower()) {

                            case "#pcdata":
                                if (pcDATAMitAuflisten) ergebnis.Add (muster.ElementName); // Dieses Element darf eingefügt werden
                                break;

                            case "#comment":
                                if (kommentareMitAufListen) ergebnis.Add (muster.ElementName); // Dieses Element darf eingefügt werden
                                break;

                            default:
                                ergebnis.Add (muster.ElementName); // Dieses Element darf eingefügt werden
                                break;

                        }
					}
				}
                #if DenkProtokoll
				_denkProtokoll.Append(muster.Zusammenfassung + "\r\n");
                #endif
			}

			return (ergebnis.ToArray());
		}


		/// <summary>
		/// Ist das angegeben Element an dieser Stelle im XML erlaubt?
		/// </summary>
		/// <returns></returns>
		public bool IstDerNodeAnDieserStelleErlaubt(System.Xml.XmlNode node) 
		{
			if (node.ParentNode is System.Xml.XmlDocument) 
			{	// Es handelt sich um das root-Element, dieses kann nicht gegen den Parent-Node geprüft
				// werden, sondern muss getrennt verglichen werden. Wenn es das in der DTD erlaubt Root-
				// Element ist, dann ok, sonst nicht

				// Implementierung: TO DO!
				return true;
			} 
			else 
			{
				XMLCursorPos cursorPos = new XMLCursorPos();
				cursorPos.CursorSetzenOhneChangeEvent(node,XMLCursorPositionen.CursorAufNodeSelbstVorderesTag);

                #if DenkProtokoll
				_denkProtokoll=new StringBuilder();
                #endif

				// Die Testmuster zum Einfügen für alle verfügbaren Elemente erstellen
				string elementName = DTD.GetElementNameFromNode(node);
                DTDTestmuster muster = CreateTestMuster(elementName, cursorPos);

				// Zur Prüfung in eine Testmusterliste packen und die Liste zur Prüfung absenden
                List<DTDTestmuster> liste = new List<DTDTestmuster>();
				liste.Add(muster);
				PruefeAlleTestmuster(liste,cursorPos);

				// Das Ergebnis der Prüfung auswerten
				if (muster.Erfolgreich) 
				{
                    #if DenkProtokoll
					_denkProtokoll=new StringBuilder();
					_denkProtokoll.Append(muster.Zusammenfassung + "\r\n");
                    #endif
					return true;
				} 
				else 
				{
					return false;
				} 
			}
		}


		/// <summary>
		/// Erzeugt alle Testmuster inkl. der Ergebnisse, ob diese Zulässig sind
		/// </summary>
        private List<DTDTestmuster> GetAlleTestmuster(XMLCursorPos cursorPos)
		{
            List<DTDTestmuster> zuTestendeMuster = new List<DTDTestmuster>();
			DTDTestmuster einMuster;	

			if (cursorPos.AktNode == null) 
			{
				// Wie soll denn für einen nicht vorhandenen Node geschaut werden, was erlaubt ist???
				throw new ApplicationException("GetAlleTestmuster: cursorPos.AktNode=NULL!");
			}

			// Löschen prüfen (Löschen-Testmuster anmelden)
			switch (cursorPos.PosAmNode) 
			{
				case XMLCursorPositionen.CursorInDemLeeremNode:
				case XMLCursorPositionen.CursorVorDemNode:
				case XMLCursorPositionen.CursorHinterDemNode:
				case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
					// Hier muss kein Löschen getestet werden, da kein Node selektiert ist
					break;
				case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:
                case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
					// Löschen-Muster zum Testen bereitstellen, ob der selektierte Node gelöscht werden kann
					//einMuster = CreateTestMuster(null,cursorPos);
					//zuTestendeMuster.Add(einMuster);
					break;
				default:
					throw new ApplicationException(String.Format("unknown cursorPos.StartPos.PosAmNode '{0}' detected.", cursorPos.PosAmNode ));
			}

            if (cursorPos.AktNode is System.Xml.XmlComment)
            {
                // In einem Kommantar können keine Tags eingefügt werden
            }
            else // Ist kein Kommentar
            {
                string[] anDieserStelleErlaubteChildren;
                if (cursorPos.PosAmNode == XMLCursorPositionen.CursorInDemLeeremNode ) {
                    // Im Node sind alle Children dieses Nodes erlaubt
                    DTDElement element = _dtd.DTDElementByName(cursorPos.AktNode.Name,false);
                    if (element == null)
                    {
                        // Ein Element mit diesem Namen ist nicht bekannt
                        anDieserStelleErlaubteChildren = new string[] { };
                    }
                    else
                    {
                        anDieserStelleErlaubteChildren = element.AlleElementNamenWelcheAlsDirektesChildZulaessigSind;
                    }
                }
                else 
                {
                    // Welche Elemente sind *neben* dem Element erlaubt?   
                    if (cursorPos.AktNode.OwnerDocument == null)
                    {
                        // Der AktNode hängt in keinem Dokument? Hm, sind wir vielleicht gerade mitten in einem
                        // einfüge-Prozess...
#warning Noch eine korrekte Meldung oder Ton einfügen

                        Debug.Assert(false, "Beep!");
                        anDieserStelleErlaubteChildren = new string[] { };
                    }
                    else
                    {
                        if (cursorPos.AktNode == cursorPos.AktNode.OwnerDocument.DocumentElement)
                        {
                            // Bei diesem Node handelt es sich im das Dokument-Tag selbst. Dieses ist auf dem Root 
                            // exklusiv, daher kann es daneben keine anderen Elemente geben
                            anDieserStelleErlaubteChildren = new string[] { };
                        }
                        else
                        {
                            // Neben oder an der Stelle des Nodes sind alle Children des Parent erlaubt
                            // Zuerst herausfinden, welches das Parent-Element des Nodes ist, für den gecheckt werden soll
                            DTDElement parentElement = _dtd.DTDElementByName(cursorPos.AktNode.ParentNode.Name, false);
                            if (parentElement == null)
                            {
                                // Ein Element mit diesem Namen ist nicht bekannt
                                anDieserStelleErlaubteChildren = new string[] { };
                            }
                            else
                            {
                                anDieserStelleErlaubteChildren = parentElement.AlleElementNamenWelcheAlsDirektesChildZulaessigSind;
                            }
                        }
                    }
                }

			    // Die Testmuster zum Einfügen für alle erlaubten Elemente erstellen
                foreach (string elementName in anDieserStelleErlaubteChildren) 
			    {
				    //if (element.Name =="pattern") //  || element.Name =="template") 
				    //if (element.Name == "#PCDATA")
				    //if (element.Name == "meta")
				    {
                        einMuster = CreateTestMuster(elementName, cursorPos);
					    zuTestendeMuster.Add(einMuster);
				    }
			    }
            }

            
			// alle gesammelten Testmuster auf Gültigkeit prüfen
			PruefeAlleTestmuster(zuTestendeMuster,cursorPos);

			return zuTestendeMuster;
		}



		/// <summary>
		/// Prüft alle Testmuster darauf hin, sie es im Rahmen der eingelesenen DTD gültig sind
		/// </summary>
        private void PruefeAlleTestmuster(List<DTDTestmuster> alleMuster, XMLCursorPos cursorPos) 
		{

			System.Xml.XmlNode node = cursorPos.AktNode;

			DTDElement element_;

			if (cursorPos.PosAmNode == XMLCursorPositionen.CursorInDemLeeremNode ) 
			{
				// Das DTDElement für den Node des Cursors holen 
				element_ = _dtd.DTDElementByName (DTD.GetElementNameFromNode(node),false);
			} 
			else 
			{
                if ((node.OwnerDocument == null) || (node.OwnerDocument.DocumentElement == null))
                {
                    Debug.Assert(false, "Beep!");
                    return;
                }
                else
                {
                    if (node == node.OwnerDocument.DocumentElement) // Der Node ist das Root-Element
                    {
                        // An der Stelle des Root-Elementes ist nur das Root-Element erlaubt
                        foreach (DTDTestmuster muster in alleMuster)  // alle zu testenden Muster durchlaufen
                        {
                            if (muster.ElementName == node.Name) // wenn es das Root-Element ist
                            {
                                muster.Erfolgreich = true; // Nur das Root-Element ist an der Stelle des Root-Elementes erlaubt 
                            }
                        }
                        return;
                    }
                    else // Der node ist nicht das Root-Element
                    {
                        // Das DTDElement für den Parent-Node des Cursors holen 
                        element_ = _dtd.DTDElementByName(DTD.GetElementNameFromNode(node.ParentNode), false);
                    }
                }
			}

            // Prüfen, ob der aktuelle DTD-Durchlauf zu einem der gesuchten Testmuster geführt hat
            foreach (DTDTestmuster muster in alleMuster)  // alle zu testenden Muster durchlaufen
            {

                if (element_ == null)
                {
                    // Dieses Element ist gar nicht bekannt
                    muster.Erfolgreich = false;
                }
                else
                {
                    if (!muster.Erfolgreich)
                    {

#if DEBUGTRACE
				    Trace.WriteLine(String.Format("Check für neues Ziel-Muster {0} > {1}",  ElementName(muster.Element) ,  muster.Zusammenfassung_ ));
#endif

                        muster.Erfolgreich = PasstMusterInElement(muster, element_);
                    }
                }
            }
		}

        private bool PasstMusterInElement(DTDTestmuster muster, DTDElement element)
        {
            Match match = element.ChildrenRegExObjekt.Match(muster.VergleichStringFuerRegEx);
            return match.Success;
        }

		/// <summary>
		/// Fügt ein Testmuster hinzu
		/// </summary>
		private DTDTestmuster CreateTestMuster(string elementName, XMLCursorPos cursorPos) 
		{
			DTDTestmuster testMuster;
			System.Xml.XmlNode node = cursorPos.AktNode;

			// Alle verfügbaren Elemente zum Testen bereitstellen
			System.Xml.XmlNode bruder;
			
			switch(cursorPos.PosAmNode) 
			{
				case XMLCursorPositionen.CursorInDemLeeremNode:
					// Der Parentnode ist leer, also müssen wir nur auf die erlaubten Elemente darin testen 
					// und keine Bruder-Elemente auf gleicher Ebene erwarten
                    testMuster = new DTDTestmuster(elementName, DTD.GetElementNameFromNode(node));
                    testMuster.AddElement(elementName);
					break;

				default:

					// Wenn der Parent-Node das XML.Dokument selbst ist, dann hier abbrechen
					if (node.ParentNode is System.Xml.XmlDocument) 
					{
                         // "Für das Root-Element kann kein Testmuster erstellt werden. Seine Gültigkeit muss durch Vergleich mit dem DTD-Root-Element gewährleistet werden."
						throw new ApplicationException(ResReader.Reader.GetString("FuerRootElementKeinTestmuster"));
					}

                    testMuster = new DTDTestmuster(elementName, DTD.GetElementNameFromNode(node.ParentNode));

					// Alle Elemente innerhalb des Parent-Elementes durchlaufen
					bruder = node.ParentNode.FirstChild;
					while (bruder !=null) 
					{
                        if (bruder is System.Xml.XmlWhitespace)
						{
							// Whitespace-Tags können bei der Prüfung ignoriert werden
						} 
                        else 
                        {
                            if (bruder == node) // an dieser Stelle muss der Node eingefügt werden
                            {
                                if (bruder is System.Xml.XmlComment)
                                {
                                    testMuster.AddElement("#COMMENT");
                                } 
                                else 
                                {
                                    if (this._dtd.DTDElementByName(DTD.GetElementNameFromNode(node), false) == null)
                                    {
                                        // Dieses Element ist gar nicht bekannt, daher wird das 
                                        // Element mal nicht aufgenommen
                                        //throw new ApplicationException(String.Format("unknown Node-Element '{0}'", DTD.GetElementNameFromNode(node)));
                                    }
                                    else
                                    {
                                        switch (cursorPos.PosAmNode)
                                        {

                                            case XMLCursorPositionen.CursorAufNodeSelbstVorderesTag:	// Wenn der Node selbst ausgewählt ist und somit ersetzt werden soll
                                            case XMLCursorPositionen.CursorAufNodeSelbstHinteresTag:
                                                if (elementName == null) // Das Löschen wird geprüft
                                                {
                                                    // Element weglassen
                                                }
                                                else // Einfügen/Ersetzen wird geprüft
                                                {
                                                    // Statt des an dieser Stelle vorhandenen Elementes wird hier das
                                                    // zu testende Element eingesetzt
                                                    testMuster.AddElement(elementName);
                                                }
                                                break;

                                            case XMLCursorPositionen.CursorHinterDemNode:
                                                if (elementName == null) // Das Löschen wird geprüft
                                                {
                                                    throw new ApplicationException("CreateTestMuster: Löschen darf bei XMLCursorPositionen.CursorHinterDemNode nicht geprüft werden!");
                                                }
                                                else
                                                {
                                                    // Hinter Stelle vorhandenen Elementes wird hinter das
                                                    // zu testende Element eingesetzt

                                                    testMuster.AddElement(DTD.GetElementNameFromNode(node));
                                                    testMuster.AddElement(elementName);
                                                }
                                                break;

                                            case XMLCursorPositionen.CursorInDemLeeremNode:
                                                if (elementName == null) // Das Löschen wird geprüft
                                                {
                                                    throw new ApplicationException("CreateTestMuster: Löschen darf bei XMLCursorPositionen.CursorHinterDemNode nicht geprüft werden!");
                                                }
                                                else
                                                {
                                                    throw new ApplicationException("CreateTestMuster: CursorInDemLeeremNode can´t be handled at this place!");
                                                }


                                            case XMLCursorPositionen.CursorVorDemNode:
                                                if (elementName == null) // Das Löschen wird geprüft
                                                {
                                                    throw new ApplicationException("CreateTestMuster: Löschen darf bei XMLCursorPositionen.CursorVorDemNode nicht geprüft werden!");
                                                }
                                                else
                                                {
                                                    // Hinter Stelle vorhandenen Elementes wird vor das
                                                    // zu testende Element eingesetzt
                                                    testMuster.AddElement(elementName);
                                                    testMuster.AddElement(DTD.GetElementNameFromNode(node));
                                                }
                                                break;

                                            case XMLCursorPositionen.CursorInnerhalbDesTextNodes:
                                                if (elementName == null) // Das Löschen wird geprüft
                                                {
                                                    throw new ApplicationException("CreateTestMuster: Löschen darf bei XMLCursorPositionen.CursorInnerhalbDesTextNodes nicht geprüft werden!");
                                                }
                                                else
                                                {
                                                    if (DTD.GetElementNameFromNode(node) != "#PCDATA")
                                                    {
                                                        throw new ApplicationException("CreateTestMuster: CursorInnerhalbDesTextNodes angegeben, aber node.name=" + de.springwald.xml.dtd.DTD.GetElementNameFromNode(node));
                                                    }
                                                    else
                                                    {
                                                        // Das zu testende Element wird zwischen zwei Textnodes gesetzt
                                                        testMuster.AddElement("#PCDATA");
                                                        testMuster.AddElement(elementName);
                                                        testMuster.AddElement("#PCDATA");
                                                    }
                                                }
                                                break;

                                            default:
                                                throw new ApplicationException("Unknown XMLCursorPositionen value:" + cursorPos.PosAmNode);
                                        }
                                    }
                                }
                            }
                            else // einfach normal weiter die Elemente aufzählen
                            {
                                testMuster.AddElement(DTD.GetElementNameFromNode(bruder));
                            }
                        }
						bruder = bruder.NextSibling; // zum nächsten Element
					}
					break;
			}
			return testMuster;
		}

		/// <summary>
		/// Ermittelt den Namen des in einem Testmuster hinterlegten Elementes
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		private string ElementName(DTDElement element) 
		{
			if (element==null) 
			{
				return "[null]";
			} 
			else 
			{
				return element.Name;
			}
		}
	}
}
