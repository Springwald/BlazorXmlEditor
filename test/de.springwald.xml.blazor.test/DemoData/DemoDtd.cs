

using System.IO;
using System.Reflection;
using de.springwald.xml.rules.dtd;

namespace de.springwald.xml.blazor.test.DemoData
{
    /// <summary>
    /// delivers a demo dtd
    /// </summary>
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
        public static Dtd LoadDemoDtd()
        {
            // nun daraus die DTD erzeugen und zurückgeben
            return new DtdReaderDtd().GetDtdFromString(DTDInhalt);
        }

    }
}
