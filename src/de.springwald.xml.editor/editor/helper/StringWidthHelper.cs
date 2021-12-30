// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.nativeplatform.gfx;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace de.springwald.xml.editor.helper
{
    public static class StringWidthHelper
    {
        private static Dictionary<string, int> buffer = new Dictionary<string, int>();

        public static async Task<int> MeasureStringWidth(IGraphics gfx, string text, Font drawFont)
        {
            var key = $"{text}";
            if (buffer.ContainsKey(key)) return buffer[key];
            var width = (int)await gfx.MeasureDisplayStringWidthAsync(text, drawFont);
            buffer[key] = width;
            return width;
        }
    }
}
