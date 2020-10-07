// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Linq;

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
            if (invertLength < 0) throw new ArgumentOutOfRangeException(nameof(invertLength) + ":" + invertLength);
            if (invertStart < -1) throw new ArgumentOutOfRangeException(nameof(invertStart) + ":" + invertStart);
            if (invertStart > text.Length) throw new ArgumentOutOfRangeException(nameof(invertStart) + ":" + invertStart);
            if (invertStart + invertLength > text.Length) throw new ArgumentOutOfRangeException(nameof(invertStart) + "+" + nameof(invertLength) + ":" + invertStart + "+" + invertLength + ">" + text.Length);

            var usedChars = 0;
            var watchOutPos = 0;

            var invertedEnd = invertStart + invertLength;
            var inverted = false;

            var maxLengthThisLine = maxLengthFirstLine;
            var lineNo = 0;
            var lastPossibleSplitPos = 0;

            while (watchOutPos < text.Length)
            {
                if (text[watchOutPos] == ' ') lastPossibleSplitPos = watchOutPos;

                var lineTooLong = watchOutPos - usedChars >= maxLengthThisLine;
                var validCutPosAvailable = lastPossibleSplitPos - usedChars > 0;
                var endOfText = watchOutPos == text.Length - 1;

                if ((lineTooLong && validCutPosAvailable) || endOfText)
                {
                    var partText = endOfText ? text.Substring(usedChars, 1 + watchOutPos - usedChars) : text.Substring(usedChars, lastPossibleSplitPos - usedChars);
                    var invertedParts = PartToInvertedParts(partText, lineNo, invertStart - usedChars, invertedEnd - usedChars, inverted).ToArray();
                    inverted = invertedParts.Last().Inverted;
                    foreach (var invertedPart in invertedParts) if (invertedPart.Text != null) yield return invertedPart;
                    lineNo++;
                    usedChars += partText.Length;
                    maxLengthThisLine = maxLength;
                }
                watchOutPos++;
            }
        }

        private static IEnumerable<TextPart> PartToInvertedParts(string partText, int lineNo, int invertStart, int invertEnd, bool isInverted)
        {
            var runPos = 0;

            if (invertStart == 0)
            {
                if (isInverted) throw new ApplicationException("is inverted, but invertstart=0?");
                isInverted = true;
            }

            if (invertStart > 0 && invertStart != invertEnd && invertStart < partText.Length)
            {
                yield return new TextPart // the not inverted part before the inverstart
                {
                    LineNo = lineNo,
                    Inverted = false,
                    Text = partText.Substring(0, invertStart)
                };
                isInverted = true;
                runPos = invertStart;
            }

            if (invertEnd >= 0 && invertEnd != invertStart && invertEnd < partText.Length)
            {
                if (!isInverted) throw new ApplicationException("is not inverted, but invertEnd set?");
                var text = partText.Substring(runPos, invertEnd - runPos);
                if (text.Length != 0)
                {
                    yield return new TextPart // close the inverted part
                    {
                        LineNo = lineNo,
                        Inverted = true,
                        Text = partText.Substring(runPos, invertEnd - runPos)
                    };
                }
                isInverted = false;
                runPos = invertEnd;
                if (runPos == partText.Length - 1)
                {
                    yield return new TextPart // dummy return part to announce the inverted info after last part
                    {
                        LineNo = lineNo,
                        Inverted = false,
                        Text = null
                    };
                }
            }

            if (runPos < partText.Length)
            {
                yield return new TextPart // close the inverted part
                {
                    LineNo = lineNo,
                    Inverted = isInverted,
                    Text = partText.Substring(runPos)
                };
            }

        }

        public static IEnumerable<TextPart> SplitTextOld(string text, int invertStart, int invertLength, int maxLength, int maxLengthFirstLine)
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

                if (watchOutPos == text.Length - 1)
                {
                    splitHere = true; // end of text
                }
                else
                {
                    if (text[watchOutPos + 1] == ' ') lastWordSpacePos = watchOutPos;
                }

                if (lengthActualLine + watchOutPos - usedChars > maxLengthThisLine)
                {
                    if (lastWordSpacePos > usedChars) splitHere = true;
                }

                if (splitHere || inverted != wasInverted)
                {
                    var cutPos = lastWordSpacePos;
                    if (inverted != wasInverted)
                    {
                        cutPos = watchOutPos;
                    }
                    var rememberLastWordSpacePos = true;
                    var newLine = (lengthActualLine + cutPos - usedChars > maxLengthThisLine);
                    if (cutPos - usedChars > maxLengthThisLine && lastWordSpacePos > usedChars)
                    {
                        cutPos = lastWordSpacePos;
                        // lastWordSpacePos = watchOutPos;
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
                    }
                    else
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
