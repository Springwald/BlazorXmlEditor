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
        public async Task<TextSplitPart[]> SplitText(IGraphics gfx, string text, int invertiertStart, int invertiertLaenge, PaintContext paintContext, Font drawFont)
        {
            var result = new List<TextSplitPart>();

            var parts = text.Split(new char[] { ' ' }, StringSplitOptions.None);

            var lineContent = new StringBuilder();
            var lineWidth = 0;
            int fontHeight = drawFont.Height;
            int lineStartX = paintContext.PaintPosX;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = i == 0 ? parts[i] : $" {parts[i]}";
                var partWidth = await StringWidthHelper.MeasureStringWidth(gfx, part, drawFont);
                if (paintContext.PaintPosX + partWidth > paintContext.LimitRight && lineWidth != 0)
                {
                    // need to start next line
                    result.Add(GetSplitPart(lineContent.ToString(), lineStartX, lineWidth, fontHeight, paintContext));
                    lineWidth = 0;
                    lineContent.Clear();
                    paintContext.PaintPosX = paintContext.LimitLeft;
                    lineStartX = paintContext.PaintPosX;
                    paintContext.PaintPosY += paintContext.HoeheAktZeile;
                }

                lineWidth += partWidth;
                paintContext.PaintPosX += partWidth;
                paintContext.HoeheAktZeile = Math.Max(paintContext.HoeheAktZeile, fontHeight);
                lineContent.Append(part);
            }
            var rest = GetSplitPart(lineContent.ToString(), lineStartX, lineWidth, fontHeight, paintContext);
            if (rest != null)
            {
                result.Add(rest);
                paintContext.PaintPosX += rest.Rectangle.Width;
            }
            return result.ToArray();
        }

        private TextSplitPart GetSplitPart(string content, int left, int width, int lineHeight, PaintContext paintContext)
        {
            if (string.IsNullOrEmpty(content)) return null;
            return new TextSplitPart
            {
                Rectangle = new Rectangle(left, paintContext.PaintPosY, width, lineHeight),
                Text = content
            };
        }

    }
}
