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
