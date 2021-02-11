// A platform independent tag-view-style graphical XML editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2021 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.blazor.NativePlatform
{
    internal class BlazorGfxContext
    {
        private Font actualFont = null;
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
            this.actualFont = null;
            this.IsInBatch = true;
            await this.ctx.BeginBatchAsync();
        }

        public async Task EndBatch()
        {
            if (!this.IsInBatch) return;
            this.IsInBatch = false;
            await this.ctx.EndBatchAsync();
            this.actualFont = null;
        }

        internal async Task SetStrokeColor(Color color)
        {
            await ctx.SetStrokeStyleAsync(color.AsHtml);
        }

        internal async Task SetFillColor(Color color)
        {
            await ctx.SetFillStyleAsync(color.AsHtml);
        }

        internal async Task SetLineWidth(float width)
        {
            if (!this.actualLineWidth.Equals(width))
            {
                await ctx.SetLineWidthAsync(width);
                this.actualLineWidth = width;
            }
        }

        internal async Task DrawLineAsync(Color color, float lineWidth, int x1, int y1, int x2, int y2)
        {
            await this.SetStrokeColor(color);
            await this.SetLineWidth(lineWidth);
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x1, y1);
            await ctx.LineToAsync(x2, y2);
            await ctx.StrokeAsync();
        }

        internal async Task DrawRectangleAsync(Color fillColor, Color borderColor, float borderWidth, Rectangle rectangle)
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

        internal async Task DrawStringAsync(string text, Font font, Color color, int x, int y)
        {
            await ctx.SetFillStyleAsync(color.AsHtml);
            await this.SetFont(font);
            await ctx.FillTextAsync(text, x, y);
        }

        internal async Task<double> MeasureDisplayStringWidthAsync(string text, Font font)
        {
            await this.SetFont(font);
            var size = await ctx.MeasureTextAsync(text);
            return (double)size.Width;
        }

        internal async Task DrawPolygonAsync(Color fillColor, Color borderColor, float borderWidth , Point[] points)
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

        internal async Task ClearAsync(Color color)
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

        internal async Task SetFont(Font font)
        {
            if (this.actualFont == null)
            {
                await this.ctx.SetTextAlignAsync(TextAlign.Left);
                await this.ctx.SetTextBaselineAsync(TextBaseline.Top);
            }
            await this.SetFontFormat(font);
        }

        private async Task SetFontFormat(Font font)
        {
            var fontString = GetFontString(font);
            await ctx.SetFontAsync(fontString); 
        }

        /// <summary>
        /// e.g. '48px serif';
        /// </summary>
        private string GetFontString(Font font)
        {
            switch (font.Unit)
            {
                case Font.GraphicsUnit.Pixel:
                    return  $"normal normal {font.Height}px {FontHtmlString(font)}";
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
    }
}
