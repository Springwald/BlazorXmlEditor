// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Collections.Generic;

namespace de.springwald.xml.editor.helper
{
    public class TextSplitHelper
    {
        public class TextPart
        {
            public string Text { get; set; }
            public int LineNo { get; set; }
            public bool Inverted { get; set; }
        }

        public static IEnumerable<TextPart> SplitText(string text, int invertStart, int invertLength, int maxLength, int maxLengthFirstLine)
        {
            var usedChars = 0;
            var lastWordSpacePos = 0;
            var watchOutPos = 0;
            bool splitHere;

            var invertedEnd = invertStart + invertLength - 1;
            var inverted = invertStart == 0;
            var wasInverted = inverted;

            var maxLengthThisLine = maxLengthFirstLine;
            var lineNo = 0;
            var lengthActualLine = 0;

            while (watchOutPos < text.Length)
            {
                splitHere = false;
                wasInverted = inverted;

                if (inverted)
                {
                    if (watchOutPos == invertedEnd) inverted = false;  //end of inverting
                }
                else
                {
                    if (watchOutPos == invertStart - 1) inverted = true; // start of inverting
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

                if (splitHere || inverted != wasInverted)
                {
                    var cutPos = watchOutPos;
                    var rememberLastWordSpacePos = true;
                    var newLine =  (lengthActualLine +  cutPos - usedChars > maxLengthThisLine);
                    if (cutPos - usedChars > maxLengthThisLine && lastWordSpacePos > usedChars)
                    {
                        cutPos = lastWordSpacePos;
                        lastWordSpacePos = watchOutPos;
                        rememberLastWordSpacePos = false;
                    } 
                    var partText = text.Substring(usedChars, 1 + cutPos - usedChars);
                    yield return new TextPart
                    {
                        Inverted = wasInverted,
                        Text = partText,
                        LineNo = lineNo
                    };
                    if (newLine)
                    {
                        lineNo++; // start new line when needed
                        maxLengthThisLine = maxLength;
                        lengthActualLine = 0;
                    } else
                    {
                        lengthActualLine += partText.Length;
                    }
                    usedChars += partText.Length;
                    if (rememberLastWordSpacePos) lastWordSpacePos = usedChars;
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
