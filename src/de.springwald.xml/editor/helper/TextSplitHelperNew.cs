using System.Collections.Generic;

namespace de.springwald.xml.editor.helper
{
    public class TextSplitHelperNew
    {
        public class TextPart
        {
            public string Text { get; set; }
            public int LineNo { get; set; }
            public bool Inverted { get; set; }
        }

        public static IEnumerable<TextPart> SplitText(string text, int invertiertStart, int invertiertLaenge, int maxLength, int maxLengthFirstLine)
        {
            var invertiertEnd = invertiertStart + invertiertLaenge - 1;
            var usedChars = 0;
            var lastWordSpacePos = 0;
            var watchOutPos = 0;
            var lineNo = 0;
            var inverted = invertiertStart == 0;
            var wasInverted = inverted;
            var maxLengthThisLine = maxLengthFirstLine;
            bool splitHere;

            while (watchOutPos < text.Length)
            {
                splitHere = false;
                wasInverted = inverted;

                if (inverted)
                {
                    if (watchOutPos == invertiertEnd)
                    {
                        //end of inverting
                        splitHere = true;
                        inverted = false;
                    }
                }
                else
                {
                    if (watchOutPos == invertiertStart - 1)
                    {
                        // start of inverting
                        inverted = true;
                        splitHere = true;
                    }
                }

                if (watchOutPos+1 < text.Length &&  text[watchOutPos+1] == ' ')
                {
                    if (watchOutPos - usedChars > maxLengthThisLine)
                    {
                        splitHere = true;
                    }
                    else
                    {
                        lastWordSpacePos = watchOutPos;
                    }
                }

                if (watchOutPos == text.Length - 1) splitHere = true;

                if (splitHere)
                {
                    var cutPos = watchOutPos;
                    if (cutPos - usedChars > maxLengthThisLine && lastWordSpacePos > usedChars) cutPos = lastWordSpacePos;
                    var partText = text.Substring(usedChars, 1 + cutPos - usedChars);
                    yield return new TextPart
                    {
                        Inverted = wasInverted,
                        Text = partText,
                        LineNo = lineNo
                    };
                    if (cutPos - usedChars > maxLengthThisLine) lineNo++; // start new line when needed
                    usedChars += partText.Length;
                    lastWordSpacePos = usedChars;
                }
                watchOutPos++;
            }

            if (usedChars < text.Length) // is there unused text left?
            {
                var partText = text.Substring(usedChars, text.Length - usedChars);
                yield return new TextPart
                {
                    Inverted = wasInverted,
                    Text = partText,
                    LineNo = lineNo
                };
            }
        }
    }
}
