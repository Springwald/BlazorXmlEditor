// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace de.springwald.toolbox
{
    /// <summary>
    /// Liest Ressourcen-Informationen aus der angegebenen, eingebundenen Ressourcen
    /// </summary>

    public class RessourcenReader
    {
        private ResourceManager _manager;
        private string _sourceName;

        //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Request.UserLanguages[0]);
        //Thread.CurrentThread.CurrentUICulture = new CultureInfo(Request.UserLanguages[0]);

        /// <summary>
        /// Liefert aus den Ressourcen den String zum angegebenen Key passend 
        /// lokalisiert auf die aktuelle Lokalisierung des Threads zurück
        /// </summary>
        /// <param name="key">Der Key, zu welchem der String gewünscht wird</param>
        /// <returns></returns>
        public string GetString(string key)
        {

            string ergebnis = _manager.GetString(key, Thread.CurrentThread.CurrentUICulture);
            if (ergebnis == null)
            {
                ergebnis = String.Format("ResNotFound:{0}({1})", key, _sourceName);
            }
            ergebnis = ergebnis.Replace("\\n", "\n");
            ergebnis = ergebnis.Replace("\\\"", "\"");

            return ergebnis;
        }


        /// <summary>
        /// Holt den Inhalt einer genannten Ressourcen-Datei
        /// </summary>
        /// <param name="ressourcenname">Der Dateiname der Ressourcen-Datei</param>
        /// <returns></returns>
        public string GetRessourcenDateiInhalt(Assembly assembly, string ressourcenname)
        {

            using (var fs = assembly.GetManifestResourceStream(ressourcenname))
            {
                if (fs == null)
                {
                    return String.Format("ResNotFound:{0}({1})", ressourcenname, _sourceName);
                }
                else
                {
                    using (var sr = new StreamReader(fs, System.Text.Encoding.GetEncoding("ISO-8859-15")))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }


        /// <summary>
        /// Liefert aus den Ressourcen den String zum angegebenen Key passend 
        /// lokalisiert auf die angegebene Lokalisierung zurück
        /// </summary>
        /// <param name="key">Der Key, zu welchem der String gewünscht wird</param>
        /// <returns></returns>
        /// <param name="culture">Die gewünschte Lokalisierung</param>
        /// <returns></returns>
        public string GetString(string key, CultureInfo culture)
        {
            /*CultureInfo vorher = Thread.CurrentThread.CurrentUICulture;
            if (vorher.ToString() != culture.ToString())
            {
                Thread.CurrentThread.CurrentUICulture = culture;
            }*/

            string ergebnis = _manager.GetString(key, culture);
            if (ergebnis == null)
            {
                ergebnis = String.Format("ResNotFound:{0}({1})", key, _sourceName);
            }
            ergebnis = ergebnis.Replace("\\n", "\n");
            ergebnis = ergebnis.Replace("\\\"", "\"");

            /*if (vorher.ToString() != culture.ToString())
            {
                Thread.CurrentThread.CurrentUICulture = vorher;
            }*/

            return ergebnis;
        }

        /// <summary>
        /// Stellt eine neue Instanz eines Ressourcen-Readers bereit
        /// </summary>
        /// <param name="resourceSource"></param>
        /// <param name="assembly"></param>
		public RessourcenReader(string resourceSource, System.Reflection.Assembly assembly)
        {
            _sourceName = resourceSource;
            _manager = new ResourceManager(resourceSource, assembly); // Ressourcenmanager bereitstellen
        }

    }
}
