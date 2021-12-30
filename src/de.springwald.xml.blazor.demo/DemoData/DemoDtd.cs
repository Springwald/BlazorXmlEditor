// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.rules.dtd;
using System.Reflection;

namespace de.springwald.xml.blazor.demo.DemoData
{
    /// <summary>
    /// delivers a demo dtd
    /// </summary>
    public static class DemoDtd
    {
        private static string dtdContent; // the dtd content

        /// <summary>
        /// the dtd content
        /// </summary>
        private static string DtdContent
        {
            get
            {
                if (dtdContent == null)
                {
                    // First read in the DTD file. This is compiled as a resource into the DLL
                    var myAssembly = Assembly.GetExecutingAssembly();
                    using (var fs = myAssembly.GetManifestResourceStream("de.springwald.xml.blazor.demo.DemoData.Resources.demo.dtd"))
                    {
                        using (var sr = new StreamReader(fs))
                        {
                            dtdContent = sr.ReadToEnd();
                            sr.Close();
                        }
                    }
                }
                return dtdContent;
            }
        }

        public static Dtd LoadDemoDtd()
        {
            // now generate the DTD from it and return it
            return new DtdReaderDtd().GetDtdFromString(DtdContent);
        }

    }
}
