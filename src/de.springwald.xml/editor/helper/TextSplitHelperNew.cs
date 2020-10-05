using de.springwald.xml.editor.nativeplatform.gfx;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.editor.helper
{
    public class TextSplitHelperNew
    {
        public class TextPart
        {
            public string Text { get; set; }
            public Rectangle Rectangle { get; set; }
            public bool Inverted { get; set; }
        }

        public static IEnumerable<TextPart> SplitText(string text, int invertiertStart, int invertiertLaenge, PaintContext paintContext, int lineSpaceY, int fontHeight, float fontWidth)
        {
            var pos = 0;
            var watchOutPos = 0;
            char charAtPos;
            while (watchOutPos < text.Length)
            {
                watchOutPos++;
                charAtPos = text[pos];
                if (charAtPos == ' ')
                {

                }
            }
            yield break;
        }
    }
}
