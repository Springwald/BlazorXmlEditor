using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace de.springwald.xml.editor.helper
{
    public class TextSplitPart
    {
        public string Text { get; set; }
        public Rectangle Rectangle { get; set; }
        public bool Inverted { get; set; }
    }

    public class TextSplitHelper
    {
        public TextSplitPart[] SplitText(string text, int invertiertStart, int invertiertLaenge, PaintContext paintContext, int lineSpaceY, int fontHeight, float fontWidth)
        {
            var result = new List<TextSplitPart>();

            var parts = text.Split(new char[] { ' ' }, StringSplitOptions.None);

            var lineContent = new StringBuilder();
            var lineWidth = 0;
            int lineStartX = paintContext.PaintPosX;
            int actualX = paintContext.PaintPosX;
            int actualY = paintContext.PaintPosY;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = i == 0 ? parts[i] : $" {parts[i]}";
                var partWidth = (int)(part.Length * fontWidth);
                if (actualX + partWidth > paintContext.LimitRight && lineWidth != 0)
                {
                    // need to start next line
                    result.Add(GetSplitPart(lineContent.ToString(), lineStartX, actualY, lineWidth, paintContext.HoeheAktZeile));
                    lineWidth = 0;
                    lineContent.Clear();
                    lineStartX = paintContext.LimitLeft;
                    actualX = lineStartX;
                    actualY += paintContext.HoeheAktZeile + lineSpaceY;
                }

                lineWidth += partWidth;
                actualX += partWidth;
                paintContext.HoeheAktZeile = Math.Max(paintContext.HoeheAktZeile, fontHeight);
                lineContent.Append(part);
            }
            var rest = GetSplitPart(lineContent.ToString(), lineStartX, actualY, lineWidth, paintContext.HoeheAktZeile);
            if (rest != null) result.Add(rest);
            return result.ToArray();
        }

        private TextSplitPart GetSplitPart(string content, int x, int y, int width, int height)
        {
            if (string.IsNullOrEmpty(content)) return null;
            return new TextSplitPart
            {
                Rectangle = new Rectangle(x, y, width + 5, height),
                Text = content
            };
        }

    }
}
