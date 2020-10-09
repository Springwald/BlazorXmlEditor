/*
using System;

namespace de.springwald.xml
{
	/// <summary>
	/// Zusammenfassung für XMLSonderzeichen.
	/// </summary>
	/// <remarks>
	/// (C)2005 Daniel Springwald, Herne Germany
	/// Springwald Software  - www.springwald.de
	/// daniel@springwald.de -   0700-SPRINGWALD
	/// all rights reserved
	/// </remarks>
	public class XMLSonderzeichen
	{
		public XMLSonderzeichen()
		{
		}

		/// <summary>
		/// Wandelt ASCII in XML-Code um, also z.B. ä nach &#x00E4;
		/// </summary>
		/// <param name="Eingabe">Der ASCII-String</param>
		/// <returns>Den XML-Code-String</returns>
		public static string ASCII2Code (string eingabe) 
		{
			return AllesUmwandeln(eingabe,false);
		}

		/// <summary>
		/// Wandelt XML-Code in ASCII um, also z.B. &#x00E4; nach ä 
		/// </summary>
		/// <param name="Eingabe">Den XML-Code-String</param>
		/// <returns>Der ASCII-String</returns>
		public static string Code2ASCII (string eingabe) 
		{
			return AllesUmwandeln(eingabe,true);
		}

		/// <summary>
		/// Die Abarbeitung aller unterstützter Sonderzeichen
		/// </summary>
		/// <param name="Eingabe">Der umzuwandelnde String</param>
		/// <param name="NachASCII">Wenn True, dann Code2ASCII, sonder ASCII2Code</param>
		/// <returns></returns>
		private static string AllesUmwandeln(string eingabe, bool nachASCII) 
		{
			string ergebnis = eingabe;
			ergebnis = EinenUmwandeln(ergebnis,"ä","&#x00E4;",nachASCII);
			ergebnis = EinenUmwandeln(ergebnis,"Ä","&#x00C4;",nachASCII);
			ergebnis = EinenUmwandeln(ergebnis,"ö","&#x00F6;",nachASCII);
			ergebnis = EinenUmwandeln(ergebnis,"Ö","&#x00D6;",nachASCII);
			ergebnis = EinenUmwandeln(ergebnis,"ü","&#x00FC;",nachASCII);
			ergebnis = EinenUmwandeln(ergebnis,"Ü","&#x00DC;",nachASCII);
			ergebnis = EinenUmwandeln(ergebnis,"ß","&#x00DF;",nachASCII);
			ergebnis = EinenUmwandeln(ergebnis,"§","&#x00A7;",nachASCII);

			if (nachASCII) 
			{
				ergebnis = EinenUmwandeln(ergebnis,"<","&lt;",nachASCII);
				ergebnis = EinenUmwandeln(ergebnis,">","&gt;",nachASCII);
				ergebnis = EinenUmwandeln(ergebnis,"&","&amp;",nachASCII);
			}
			
			return ergebnis;
		}

		/// <summary>
		/// Wandelt ein einzelnes Sonderzeichen um
		/// </summary>
		/// <param name="Eingabe">Der umzuwandelnde String</param>
		/// <param name="NachASCII">Wenn True, dann Code2ASCII, sonder ASCII2Code</param>
		/// <returns></returns>
		private static string EinenUmwandeln(string eingabe, string ascii, string code, bool nachASCII) 
		{
			if (nachASCII) 
			{
				return eingabe.Replace (code,ascii);
			} 
			else 
			{
				return eingabe.Replace(ascii,code);
			}
		}

	}
}
*/