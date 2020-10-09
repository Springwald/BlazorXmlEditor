using System;
using System.Reflection;
using de.springwald.toolbox;

namespace de.springwald.xml
{
    /// <summary>
    /// Dieser Ressourcenreader liefert den schnellen Zugriff auf die lokalisierten Ressourcen der Assembly
    /// </summary>
    /// <remarks>
    /// (C)2006 Daniel Springwald, Herne Germany
    /// Springwald Software  - www.springwald.de
    /// daniel@springwald.de -   0700-SPRINGWALD
    /// all rights reserved
    /// </remarks>
	public class ResReader
	{
        private static string _ressourcenDatei = "de.springwald.xml.editor. esources.xml";


		private  static RessourcenReader _reader = new de.springwald.toolbox.RessourcenReader(_ressourcenDatei,  Assembly.GetExecutingAssembly());

        /// <summary>
        /// Das statische Reader-Objekt für diese Assembly
        /// </summary>
		public static RessourcenReader Reader 
		{
			get { return _reader; }
		}

		public ResReader()
		{
		}
	}
}
