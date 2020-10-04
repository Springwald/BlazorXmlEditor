using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
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

    public class TextSplitHelper
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

            var words = this.GetWords(text, invertiertStart, invertiertLaenge);

            foreach (var word in words)
            {
                var partWidth = (int)(word.Text.Length * fontWidth);
                actualInverted = word.Inverted;
                if (actualX + partWidth > paintContext.LimitRight && lineWidth != 0)
                {
                    // need to start next line
                    result.Add(GetSplitPart(lineContent.ToString(), lineStartX, actualY, lineWidth, paintContext.HoeheAktZeile, actualInverted));
                    lineWidth = 0;
                    lineContent.Clear();
                    lineStartX = paintContext.LimitLeft;
                    actualX = lineStartX;
                    actualY += paintContext.HoeheAktZeile + lineSpaceY;
                }
                lineWidth += partWidth;
                actualX += partWidth;
                paintContext.HoeheAktZeile = Math.Max(paintContext.HoeheAktZeile, fontHeight);
                lineContent.Append(word.Text);
            }
            var rest = GetSplitPart(lineContent.ToString(), lineStartX, actualY, lineWidth, paintContext.HoeheAktZeile, actualInverted);
            if (rest != null) result.Add(rest);
            return result.ToArray();
        }

        private IEnumerable<SplitWort> GetWords(string text, int invertiertStart, int invertiertLaenge)
        {
            bool actualInverted = invertiertStart == 0;
            var splits = text.Split(new char[] { ' ' }, StringSplitOptions.None);
            bool firstWord = true;
            foreach (var splitRaw in splits)
            {
                var split = firstWord ? splitRaw : $" {splitRaw}";
                firstWord = false;
                if (actualInverted)
                {
                    if (split.Length < invertiertLaenge) // selection ends here
                    {
                        yield return new SplitWort { Inverted = true, Text = split.Substring(0, invertiertLaenge) };
                        yield return new SplitWort { Inverted = false, Text = split.Substring(invertiertLaenge) };
                        actualInverted = false;
                    }
                    else
                    {
                        yield return new SplitWort { Inverted = true, Text = split };
                    }
                }
                else
                {
                    if (split.Length < invertiertStart) // selection starts here
                    {
                        yield return new SplitWort { Inverted = false, Text = split.Substring(0, invertiertStart) };
                        yield return new SplitWort { Inverted = true, Text = split.Substring(invertiertStart) };
                    }
                    else // no selection change in this node
                    {
                        yield return new SplitWort { Inverted = false, Text = split };
                    }
                }
                invertiertLaenge -= split.Length;
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
