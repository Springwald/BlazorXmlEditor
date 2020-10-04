using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.NativePlatform
{
    internal class BlazorGfxContext
    {
        private Font actualFont = null;
        private Color actualStrokeColor = null;
        private Color actualFillColor = null;
        private float actualLineWidth = 0;

        private Canvas2DContext ctx;
        private BECanvasComponent canvas;

        public bool IsInBatch { get; private set; }

        public BlazorGfxContext(BECanvasComponent canvas, Canvas2DContext context2d)
        {
            this.canvas = canvas;
            this.ctx = context2d;
        }

        public async Task StartBatch()
        {
            this.IsInBatch = true;
            await this.ctx.BeginBatchAsync();
        }

        public async Task EndBatch()
        {
            if (!this.IsInBatch) return;
            this.IsInBatch = false;
            await this.ctx.EndBatchAsync();
        }

        public async Task SetStrokeColor(Color color)
        {
            if (color != this.actualStrokeColor)
            {
                await ctx.SetStrokeStyleAsync(color.AsHtml);
                this.actualStrokeColor = color;
            }
        }

        public async Task SetFillColor(Color color)
        {
            if (color != this.actualFillColor)
            {
                await ctx.SetFillStyleAsync(color.AsHtml);
                this.actualFillColor = color;
            }
        }

        public async Task SetLineWidth(float width)
        {
            if (!this.actualLineWidth.Equals(width))
            {
                await ctx.SetLineWidthAsync(width);
                this.actualLineWidth = width;
            }
        }

        public async Task SetFont(Font font)
        {
            if (this.actualFont == null)
            {
                await this.ctx.SetTextAlignAsync(TextAlign.Left);
                await this.ctx.SetTextBaselineAsync(TextBaseline.Top);
            }
            if (font != this.actualFont) await SetFontFormat(font);
            this.actualFont = font;
        }

        private async Task SetFontFormat(Font font)
        {
            switch (font.Unit)
            {
                case Font.GraphicsUnit.Pixel:
                    await ctx.SetFontAsync($"{font.Height}px {FontHtmlString(font)}"); // e.g. '48px serif';
                    break;
                default: throw new ArgumentOutOfRangeException($"{nameof(font.Unit)}:{font.Unit.ToString()}");
            }
        }

        private string FontHtmlString(Font font)
        {
            return string.Join(", ", font.Names.Select(n => CreateHtmlFontName(n)));
        }

        private string CreateHtmlFontName(string fontname)
        {
            fontname = fontname.Trim(new char[] { ' ', '\"' });
            if (fontname.Contains(" ")) fontname = $"\"{fontname}\"";
            return fontname;
        }

        public async Task DrawLineAsync(Pen pen, int x1, int y1, int x2, int y2)
        {
            await this.SetStrokeColor(pen.Color);
            await this.SetLineWidth(pen.Width);
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x1, y1);
            await ctx.LineToAsync(x2, y2);
            await ctx.StrokeAsync();
        }

        public async Task DrawRectangleAsync(Color fillColor, Color borderColor, float borderWidth, Rectangle rectangle)
        {
            if (fillColor != null)
            {
                await this.SetFillColor(fillColor);
                await ctx.FillRectAsync(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            }

            if (borderColor != null)
            {
                await this.SetStrokeColor(borderColor);
                await this.SetLineWidth(borderWidth);
                await ctx.StrokeRectAsync(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            }
        }

        public async Task DrawStringAsync(string text, Font font, Color color, int x, int y)
        {
            await ctx.SetFillStyleAsync(color.AsHtml);
            await this.SetFont(font);
            await ctx.FillTextAsync(text, x, y);
        }

        public async Task<float> MeasureDisplayStringWidthAsync(string text, Font font)
        {
            await this.SetFont(font);
            var size = await ctx.MeasureTextAsync(text);
            return (float)size.Width;
        }

        public async Task DrawPolygonAsync(Color fillColor, Color borderColor, float borderWidth , Point[] points)
        {
            if (points.Length == 0) return;

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(points[0].X, points[0].Y);
            for (int i = 1; i < points.Length; i++)
            {
                await ctx.LineToAsync(points[i].X, points[i].Y);
            }
            await ctx.LineToAsync(points[0].X, points[0].Y);

            if (fillColor != null)
            {
                await this.SetFillColor(fillColor);
                await ctx.FillAsync();
            }
            if (borderColor != null)
            {
                await this.SetStrokeColor(borderColor);
                await this.SetLineWidth(borderWidth);
                await ctx.StrokeAsync();
            }
        }


        public async Task ClearAsync(Color color)
        {
            if (color != Color.White)
            {
                await ctx.SetFillStyleAsync(color.AsHtml);
                await ctx.FillRectAsync(0, 0, this.canvas.Width, this.canvas.Height);
            }
            else
            {
                await ctx.ClearRectAsync(0, 0, this.canvas.Width, this.canvas.Height);
            }
        }


    }
}
