using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace de.springwald.xml.editor.helper
{
    public class TextLine
    {
        public string Text { get; set; }
        public Rectangle Rectangle { get; set; }
        public bool Inverted { get; set; }
    }

    public class TextSplitHelperOld
    {
        private class SplitWort
        {
            public string Text;
            public bool Inverted;
        }

        public TextLine[] SplitText(string text, int invertiertStart, int invertiertLaenge, PaintContext paintContext, int lineSpaceY, int fontHeight, float fontWidth)
        {
            var result = new List<TextLine>();

            var lineContent = new StringBuilder();
            var lineWidth = 0;
            int lineStartX = paintContext.PaintPosX;
            int actualX = paintContext.PaintPosX;
            int actualY = paintContext.PaintPosY;
            bool actualInverted = false;
            bool lineIsEmpty = true;

            invertiertStart = 3;
            invertiertLaenge = 2;

            var invertParts = this.GetInvertParts(text, invertiertStart, invertiertLaenge).ToArray();

            foreach (var invertPart in invertParts)
            {
                var splits = invertPart.Text.Split(new char[] { ' ' }, StringSplitOptions.None);
                for (int i = 0; i < splits.Length; i++)
                {
                    var word = i == 0 ? splits[0] : $" {splits[i]}";
                    var partWidth = (int)(word.Length * fontWidth);
                    var doesNotFitInThisLine = actualX + partWidth > paintContext.LimitRight;

                    if (doesNotFitInThisLine || invertPart.Inverted != actualInverted)
                    {
                        // need to close this part because of inverted switch or because the right limit is reached
                        var resultPart = GetSplitPart(lineContent.ToString(), lineStartX, actualY, lineWidth, paintContext.HoeheAktZeile, inverted: actualInverted);
                        if (resultPart != null)
                        {
                            result.Add(resultPart);

                            lineIsEmpty = false;
                            actualInverted = invertPart.Inverted;
                            lineWidth = 0;
                            lineContent.Clear();

                            if (doesNotFitInThisLine && !lineIsEmpty)  // need to start next line
                            {
                                lineStartX = paintContext.LimitLeft;
                                actualX = lineStartX;
                                actualY += paintContext.HoeheAktZeile + lineSpaceY;
                                lineIsEmpty = true;
                            }
                        }
                    }

                    lineWidth += partWidth;
                    actualX += partWidth;
                    paintContext.HoeheAktZeile = Math.Max(paintContext.HoeheAktZeile, fontHeight);
                    lineContent.Append(word);
                }
            }
            var rest = GetSplitPart(lineContent.ToString(), lineStartX, actualY, lineWidth, paintContext.HoeheAktZeile, inverted: actualInverted);
            if (rest != null) result.Add(rest);
            return result.ToArray();
        }

        private IEnumerable<SplitWort> GetInvertParts(string text, int invertiertStart, int invertiertLaenge)
        {
            if (invertiertStart == -1) yield return new SplitWort { Inverted = false, Text = text };

            if (invertiertStart > 0)
            {
                yield return new SplitWort { Inverted = false, Text = text.Substring(0, invertiertStart) };
                text = text.Substring(invertiertStart);
            }

            if (invertiertLaenge < text.Length)
            {
                yield return new SplitWort { Inverted = true, Text = text.Substring(0, invertiertLaenge) };
                text = text.Substring(invertiertLaenge);
            }
            else
            {
                yield return new SplitWort { Inverted = true, Text = text };
                yield break;
            }

            if (!string.IsNullOrEmpty(text))
            {
                yield return new SplitWort { Inverted = false, Text = text };
            }
        }

        private TextLine GetSplitPart(string content, int x, int y, int width, int height, bool inverted)
        {
            if (string.IsNullOrEmpty(content)) return null;
            return new TextLine
            {
                Rectangle = new Rectangle(x, y, width + 5, height),
                Text = content,
                Inverted = inverted
            };
        }

    }
}
