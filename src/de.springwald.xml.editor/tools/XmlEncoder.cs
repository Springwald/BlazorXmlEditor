// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Text;
using System.Xml.Linq;

namespace de.springwald.xml
{
    public class XmlEncoder
    {
        public Encoding FindSourceEncoding(byte[] contentBytes)
        {
            var encodingsToTry = new Encoding[]
            {
                Encoding.GetEncoding("ISO-8859-1"),
                Encoding.UTF8,
                Encoding.Unicode,
                Encoding.Default
            };

            foreach (var encoding in encodingsToTry)
            {
                string content = encoding.GetString(contentBytes);
                string sourceEncodingName = null;
                try
                {
                    using (var reader = new System.IO.StringReader(content))
                    {
                        var doc = XDocument.Load(reader);
                        sourceEncodingName = doc.Declaration.Encoding;
                        // doc.Declaration.Encoding = "UTF-8";
                        // content = doc.ToString();
                    }
                }
                catch (Exception e)
                {
                    var x = e.Message;
                }
                switch (sourceEncodingName)
                {
                    case "ISO-8859-1": return Encoding.GetEncoding("ISO-8859-1");
                    case "ISO-8859-15": return Encoding.GetEncoding("ISO-8859-1");
                    case "UTF-8": return Encoding.UTF8;
                }
            }

            return Encoding.UTF8;
        }

        public string ContentToUTF8(byte[] sourceBytes) => ContentToUTF8(sourceBytes, Encoding.Default);

        public string ContentToUTF8(byte[] sourceBytes, Encoding sourceEncoding)
        {
            if (sourceEncoding == Encoding.UTF8) return sourceEncoding.GetString(sourceBytes);

            //string sourceEncodingName;
            //try
            //{
            //    var doc = XDocument.Load(content);
            //    sourceEncodingName = doc.Declaration.Encoding;
            //    // doc.Declaration.Encoding = "UTF-8";
            //    // content = doc.ToString();
            //}
            //catch (Exception e)
            //{
            //    return content;
            //}

            //switch (sourceEncodingName)
            //{
            //    case "ISO-8859-1":
            //        sourceEncoding = Encoding.GetEncoding("ISO-8859-1");
            //        //StringBuilder output = new StringBuilder();
            //        //foreach (char ch in content)
            //        //{
            //        //    if (ch > 0x7F)
            //        //        output.AppendFormat("&#{0};", (int)ch);
            //        //    else
            //        //        output.Append(ch);
            //        //}
            //        //content = output.ToString();
            //        break;
            //    default:
            //        return content;
            //}

            // byte[] sourceBytes = sourceEncoding.GetBytes(content);
            byte[] convertedBytes = Encoding.Convert(sourceEncoding, Encoding.UTF8, sourceBytes);
            return Encoding.UTF8.GetString(convertedBytes);
        }
    }
}
