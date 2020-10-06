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
            var saveFallbackForActualLine = 0;
            var watchOutPos = 0;

            var lineNo = 0;
            var inverted = invertiertStart == 0;
            var wasInverted = inverted;
            var splitHere = false;
            var startNewLine = false;
            var endOfText = false;
            var maxLengthThisLine = maxLengthFirstLine;

            while (watchOutPos < text.Length)
            {
                splitHere = false;
                wasInverted = inverted;
                startNewLine = false;

                if (watchOutPos == text.Length - 1)
                {
                    endOfText = true;
                    splitHere = true;
                }

                if (text[watchOutPos] == ' ') // next chance to split
                {
                    var lineLengthAtWatchOutPos = watchOutPos - usedChars;
                    if (lineLengthAtWatchOutPos <= maxLengthThisLine)
                    {
                        saveFallbackForActualLine = watchOutPos; // actual word fits into line
                    }
                    else
                    {
                        splitHere = true; // actual word does not fit into line
                    }
                }

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

                if (splitHere)
                {
                    startNewLine = watchOutPos - usedChars >= maxLengthThisLine;
                    if (startNewLine && saveFallbackForActualLine - usedChars == 0) startNewLine = false;
                    if (startNewLine)
                    {
                        var partText = text.Substring(usedChars, saveFallbackForActualLine - usedChars);
                        yield return new TextPart
                        {
                            Inverted = wasInverted,
                            Text = partText,
                            LineNo = lineNo
                        };
                        lineNo++;
                        usedChars += partText.Length;
                        saveFallbackForActualLine = watchOutPos;
                    }

                    if (startNewLine == false || endOfText)
                    {
                        var partText = text.Substring(usedChars, 1 + watchOutPos - usedChars);
                        if (partText.Length != 0)
                        {
                            yield return new TextPart
                            {
                                Inverted = wasInverted,
                                Text = partText,
                                LineNo = lineNo
                            };
                            saveFallbackForActualLine = watchOutPos;
                            usedChars += partText.Length;
                        }
                    }
                }
                watchOutPos++;
            }
        }
    }
}
