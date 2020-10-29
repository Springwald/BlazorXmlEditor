// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Reflection;
using de.springwald.toolbox;

namespace de.springwald.xml
{
    /// <summary>
    /// Dieser Ressourcenreader liefert den schnellen Zugriff auf die lokalisierten Ressourcen der Assembly
    /// </summary>
	public class ResReader
	{
        private static string _ressourcenDatei = "de.springwald.xml.editor.resources.xml";
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
