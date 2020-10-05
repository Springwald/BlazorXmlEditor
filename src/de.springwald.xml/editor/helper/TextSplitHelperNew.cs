﻿using de.springwald.xml.editor.nativeplatform.gfx;
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
            var invertiertEnd = invertiertStart + invertiertLaenge;
            var pos = 0;
            var watchOutPos = 0;
            char charAtPos;
            var inverted = invertiertStart == 0;
            var wasInverted = inverted;
            var splitHere = false;
            var startNewLine = false;

            while (watchOutPos < text.Length)
            {
                splitHere = watchOutPos == text.Length-1;
                wasInverted = inverted;
                startNewLine = false;
                watchOutPos++;
                charAtPos = text[pos];
                if (charAtPos == ' ') // next chance to split
                {
                    startNewLine = true;
                    splitHere = true;
                }
                if (inverted)
                {
                    if (watchOutPos == invertiertEnd) 
                    {
                        //end of inverting
                        splitHere = true;
                    }
                } else
                {
                    if (watchOutPos == invertiertStart)
                    {
                        // start of inverting
                        inverted = true;
                        splitHere = false;
                    }
                }
                if (splitHere)
                {
                    yield return new TextPart
                    {
                        Inverted = wasInverted,
                        Text = text.Substring(pos, watchOutPos - pos)
                    };
                    pos = watchOutPos ;
                }
            }
        }

    }
}
