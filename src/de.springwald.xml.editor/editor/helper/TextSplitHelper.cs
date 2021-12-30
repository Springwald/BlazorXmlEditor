// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Collections.Generic;

namespace de.springwald.xml.editor.helper
{
    public class TextSplitHelper
    {
        public class TextPartRaw
        {
            public string Text { get; set; }
            public int LineNo { get; set; }
            public bool Inverted { get; set; }
        }

        public static IEnumerable<TextPartRaw> SplitText(string text, int invertStart, int invertLength, int maxLength, int maxLengthFirstLine)
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

            var lineTooLong = false;
            var validCutPosAvailable = false;
            var endOfText = false;

            while (watchOutPos < text.Length)
            {
                if (text[watchOutPos] == ' ') lastPossibleSplitPos = watchOutPos;

                lineTooLong = watchOutPos - usedChars >= maxLengthThisLine;
                validCutPosAvailable = lastPossibleSplitPos - usedChars > 0;
                endOfText = watchOutPos == text.Length - 1;

                if ((lineTooLong && validCutPosAvailable) || endOfText)
                {
                    var partTextLocal = endOfText ? text.Substring(usedChars, 1 + watchOutPos - usedChars) : text.Substring(usedChars, lastPossibleSplitPos - usedChars);

                    // >>>>> sub split the text part for invertion if needed

                    var localRunPos = 0;
                    var invertStartLocal = invertStart - usedChars;
                    var invertEndLocal = invertedEnd - usedChars;

                    if (invertStartLocal == 0 && invertLength > 0)
                    {
                        if (inverted) throw new ApplicationException("is inverted, but invertstart=0?");
                        inverted = true;
                    }

                    if (invertStartLocal > 0 && invertStartLocal != invertEndLocal && invertStartLocal < partTextLocal.Length)
                    {
                        yield return new TextPartRaw // the not inverted part before the invertstart
                        {
                            LineNo = lineNo,
                            Inverted = false,
                            Text = partTextLocal.Substring(0, invertStartLocal)
                        };
                        inverted = true;
                        localRunPos = invertStartLocal;
                    }

                    if (invertEndLocal >= 0 && invertEndLocal != invertStartLocal && invertEndLocal < partTextLocal.Length)
                    {
                        if (!inverted) throw new ApplicationException("is not inverted, but invertEnd set?");
                        var textInverted = partTextLocal.Substring(localRunPos, invertEndLocal - localRunPos);
                        if (textInverted.Length != 0)
                        {
                            yield return new TextPartRaw // close the inverted part
                            {
                                LineNo = lineNo,
                                Inverted = true,
                                Text = textInverted
                            };
                        }
                        inverted = false;
                        localRunPos = invertEndLocal;
                    }

                    if (localRunPos < partTextLocal.Length)
                    {
                        yield return new TextPartRaw // close the inverted part
                        {
                            LineNo = lineNo,
                            Inverted = inverted,
                            Text = partTextLocal.Substring(localRunPos)
                        };
                    }

                    // <<<< sub split the text part for invertion if needed

                    lineNo++;
                    usedChars += partTextLocal.Length;
                    maxLengthThisLine = maxLength;
                }
                watchOutPos++;
            }
        }
    }
}
