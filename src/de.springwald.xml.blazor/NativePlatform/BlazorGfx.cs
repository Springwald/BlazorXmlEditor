﻿﻿using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorGfx : IGraphics
    {
        private List<IGfxJob> jobs = new List<IGfxJob>();
        private Canvas2DContext contextCache;
        private BECanvasComponent canvas;
        private bool isInBatch = false;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public BlazorGfx(BECanvasComponent canvas)
        {
            this.canvas = canvas;
        }

        private async Task<Canvas2DContext> GetContext()
        {
            if (this.contextCache == null)
            {
                this.contextCache = await this.canvas.CreateCanvas2DAsync();
            }
            return this.contextCache;
        }

        public Task SetSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            return Task.CompletedTask;
        }

        public async Task StartBatch()
        {
            if (this.isInBatch) await this.EndBatch();
            this.contextCache = null;
            this.isInBatch = true;
            await (await this.GetContext()).BeginBatchAsync();
        }

        public async Task EndBatch()
        {
            if (!isInBatch) return;
            this.isInBatch = false;
            await (await this.GetContext()).EndBatchAsync();
            this.contextCache = null;
        }

        public async Task DrawLineAsync(Pen pen, int x1, int y1, int x2, int y2)
        {
            var ctx = await this.GetContext();
            await this.SetStrokeFromPen(pen, ctx);
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(x1, y1);
            await ctx.LineToAsync(x2, y2);
            await ctx.StrokeAsync();
            await ResetStroke(ctx);
        }

        public async Task DrawRectangleAsync(Pen pen, Rectangle rectangle)
        {
            var ctx = await this.GetContext();
            await this.SetStrokeFromPen(pen, ctx);
            await ctx.StrokeRectAsync(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            await ResetStroke(ctx);
        }

        public async Task DrawStringAsync(string text, Font font, Color color, int x, int y)
        {
            var ctx = await this.GetContext();
            await ctx.SetFillStyleAsync(color.AsHtml);
            await this.SetFontFormat(ctx, font);
            await ctx.FillTextAsync(text, x, y);
            await ResetStroke(ctx);
        }

        public async Task<float> MeasureDisplayStringWidthAsync(string text, Font font)
        {
            var ctx = await this.GetContext();
            await this.SetFontFormat(ctx, font);
            //await Task.Delay(1000);
            var size = await ctx.MeasureTextAsync(text);
            return (float)size.Width;
        }

        private string FontHtmlString(Font font)
        {
            return string.Join(", ", font.Names.Select(n => CreateHtmlFontName(n)));
        }

        private string CreateHtmlFontName(string fontname)
        {
            fontname = fontname.Trim(new char[] { ' ', '\"' }); 
            if (fontname.Contains(" ")) fontname  = $"\"{fontname}\"";
            return fontname;
        }

        private async Task SetFontFormat(Canvas2DContext ctx, Font font)
        {
            var targetFont = string.Empty;
            switch (font.Unit)
            {
                case Font.GraphicsUnit.Pixel:
                    targetFont = $"{font.Height}px {FontHtmlString(font)}"; // e.g. '48px serif';
                    break;
                default: throw new ArgumentOutOfRangeException($"{nameof(font.Unit)}:{font.Unit.ToString()}");
            }
            await ctx.SetTextAlignAsync(TextAlign.Left);
            await ctx.SetTextBaselineAsync(TextBaseline.Top);
            if (!targetFont.Equals(ctx.Font))
            {
                await ctx.SetFontAsync(targetFont);
            }
        }

        public async Task FillPolygonAsync(Color color, Point[] points)
        {
            if (points.Length == 0) return;

            var ctx = await this.GetContext();
            await ctx.SetFillStyleAsync(color.AsHtml);
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(points[0].X, points[0].Y);
            for (int i = 1; i < points.Length; i++)
            {
                await ctx.LineToAsync(points[i].X, points[i].Y);
            }
            await ctx.LineToAsync(points[0].X, points[0].Y);
            await ctx.FillAsync();
        }

        public async Task DrawPolygonAsync(Pen pen, Point[] points)
        {
            if (points.Length == 0) return;

            var ctx = await this.GetContext();
            await this.SetStrokeFromPen(pen, ctx);
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(points[0].X, points[0].Y);
            for (int i = 1; i < points.Length; i++)
            {
                await ctx.LineToAsync(points[i].X, points[i].Y);
            }
            await ctx.LineToAsync(points[0].X, points[0].Y);
            await ctx.StrokeAsync();
        }

        public async Task FillRectangleAsync(Color color, Rectangle rechteck)
        {
            await Task.CompletedTask; // to prevent warning because of empty async method
        }

        public async Task ClearAsync(Color color)
        {
            var ctx = await this.GetContext();
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

        // ######### private helpers ##########


        private async Task SetStrokeFromPen(Pen pen, Canvas2DContext ctx)
        {
            var col = pen.Color.AsHtml;
            if (!col.Equals(ctx.StrokeStyle))   await ctx.SetStrokeStyleAsync(col);
            if (ctx.LineWidth != pen.Width)   await ctx.SetLineWidthAsync(pen.Width);
        }

        private async Task ResetStroke(Canvas2DContext ctx)
        {
            // reset line dashs and other parameters
            await ctx.SetLineDashAsync(new float[] { });
            await ctx.SetLineCapAsync(LineCap.Butt);
        }


        public void AddJob(IGfxJob job)
        {
            this.jobs.Add(job);
        }

        public async Task PaintJobs()
        {
            var sorted = this.jobs.OrderBy(j => j.Layer).ThenBy(j => j.Batchable);
            var batching = false;
            foreach(var job in sorted)
            {
                if (job.Batchable)
                {
                    if (!batching) {
                        await this.StartBatch();
                        batching = true;
                    }
                }
                else
                {
                    if (batching)
                    {
                        await this.EndBatch();
                        batching = false;
                    }
                }
                await job.Paint(this);
            }
            this.jobs.Clear();
            if (batching) await this.EndBatch();
        }

    }
}