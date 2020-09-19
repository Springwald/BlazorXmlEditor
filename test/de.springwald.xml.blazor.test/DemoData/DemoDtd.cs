using de.springwald.xml.dtd;
using System.IO;
using System.Reflection;

namespace de.springwald.xml.blazor.test.DemoData
{
    /// <summary>
    /// delivers a demo dtd
    /// </summary>
    /// <remarks>
    /// (C)2006 Daniel Springwald, Bochum Germany
    /// Springwald Software  -  www.springwald.de
    /// daniel@springwald.de  -   0234 298 788 47
    /// all rights reserved
    /// </remarks>
    public static class DemoDtd
    {
        private static string _dtdInhalt;       // the dtd content

        /// <summary>
        /// the dtd content
        /// </summary>
        private static string DTDInhalt
        {
            get
            {
                if (_dtdInhalt == null)
                {
                    // Zuerst die DTDDatei einlesen. Diese ist als Ressource in die DLL kompiliert
                    var myAssembly = Assembly.GetExecutingAssembly();
                    using (var fs = myAssembly.GetManifestResourceStream("de.springwald.xml.blazor.test.DemoData.Resources.demo.dtd"))
                    {
                        using (var sr = new StreamReader(fs))
                        {
                            _dtdInhalt = sr.ReadToEnd();
                            sr.Close();
                        }
                    }
                }
                return _dtdInhalt;
            }
        }

        /// <summary>
        /// loads the dtd
        /// </summary>
        /// <returns></returns>
        public static de.springwald.xml.dtd.DTD LoadDemoDtd()
        {
            // nun daraus die DTD erzeugen und zurückgeben
            var reader = new DTDReaderDTD();
            return reader.GetDTDFromString(DTDInhalt);
        }

    }
}
