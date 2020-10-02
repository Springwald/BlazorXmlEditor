using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorGfx : IGraphics
    {
        // public Canvas2DContext context;

        private Canvas2DContext contextCache;

        private async Task<Canvas2DContext> GetContext()
        {
            if (this.contextCache == null)
            {
                this.contextCache = await this.canvas.CreateCanvas2DAsync();
            }
            return this.contextCache;
        }

        private BECanvasComponent canvas;
        private bool isInBatch = false;

        public BlazorGfx(BECanvasComponent canvas)
        {
            this.canvas = canvas;
        }

        public async Task StartBatch()
        {
            if (this.isInBatch) await this.EndBatch();
            this.contextCache = null;
            this.isInBatch = true;
            //await (await this.GetContext()).BeginBatchAsync();
        }

        public async Task EndBatch()
        {
            if (!isInBatch) return;
            this.isInBatch = false;
           // await (await this.GetContext()).EndBatchAsync();
            this.contextCache = null;
        }

        public async Task DrawLineAsync(Pen pen, int x1, int y1, int x2, int y2)
        {
            var ctx = await this.GetContext();
            await this.SetStrokeFromPen(pen, ctx);

            await ctx.BeginPathAsync();
            await ctx.SetLineDashAsync(this.GetDashStyle(pen.DashStyle));
            await ctx.SetLineCapAsync(this.GetLineCap(pen.StartCap));
            await ctx.MoveToAsync(x1, y1);

            await ctx.SetLineCapAsync(this.GetLineCap(pen.EndCap));
            await ctx.LineToAsync(x2, y2);
            await ctx.StrokeAsync();

            await ResetStroke(ctx);
        }

        public async Task DrawPathAsync(Pen pen, GraphicsPath gp)
        {
            if (gp.Lines.Count == 0) return;

            var ctx = await this.GetContext();

            await this.SetStrokeFromPen(pen, ctx);
            // await ctx.SetLineCapAsync(this.GetLineCap(Pen.LineCap.NoAnchor));
            //  await ctx.SetLineJoinAsync(LineJoin.Round);
            //  await ctx.SetStrokeStyleAsync("");
            await LinePath(gp, ctx);
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

        public async Task DrawStringAsync(string text, Font font, SolidBrush brush, int x, int y)
        {
            var ctx = await this.GetContext();
            await ctx.SetFillStyleAsync(brush.Color.AsHtml);
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

        private async Task SetFontFormat(Canvas2DContext ctx, Font font)
        {
            var targetFont = string.Empty;
            switch (font.Unit)
            {
                case Font.GraphicsUnit.Pixel:
                    targetFont = $"{font.Height}px {font.Name}"; // e.g. '48px serif';
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

        public async Task FillPathAsync(SolidBrush brush, GraphicsPath gp)
        {
            if (gp.Lines.Count == 0) return;

            var ctx = await this.GetContext();
            await ctx.SetFillStyleAsync(brush.Color.AsHtml);
            await ctx.BeginPathAsync();
            await LinePath(gp, ctx);
            await ctx.FillAsync();
            await ResetStroke(ctx);
        }

        public async Task FillPolygonAsync(SolidBrush brush, Point[] points)
        {
            if (points.Length == 0) return;

            var ctx = await this.GetContext();
            await ctx.SetFillStyleAsync(brush.Color.AsHtml);
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(points[0].X, points[0].Y);
            for (int i = 1; i < points.Length; i++)
            {
                await ctx.LineToAsync(points[i].X, points[i].Y);
            }
            await ctx.LineToAsync(points[0].X, points[0].Y);
            await ctx.FillAsync();
        }

        public async Task FillRectangleAsync(SolidBrush newBrush, Rectangle rechteck)
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

        private static async Task LinePath(GraphicsPath gp, Canvas2DContext ctx)
        {
            await ctx.MoveToAsync(gp.Lines[0].X1, gp.Lines[0].Y1);
            await ctx.LineToAsync(gp.Lines[0].X2, gp.Lines[0].Y2);
            for (int i = 1; i < gp.Lines.Count; i++)
            {
                if (gp.Lines[i].X1 != gp.Lines[i - 1].X2 || gp.Lines[i].Y1 != gp.Lines[i - 1].Y2)
                {
                    await ctx.LineToAsync(gp.Lines[i].X1, gp.Lines[i].Y1);
                }
                await ctx.LineToAsync(gp.Lines[i].X2, gp.Lines[i].Y2);
            }
        }

        private LineCap GetLineCap(Pen.LineCap cap)
        {
            switch (cap)
            {
                case Pen.LineCap.NoAnchor: return LineCap.Butt;
                case Pen.LineCap.SquareAnchor: return LineCap.Square;
                case Pen.LineCap.RoundAnchor: return LineCap.Round;
                default: throw new ArgumentOutOfRangeException($"{nameof(cap)}: {cap.ToString()}");
            }
        }

        private float[] GetDashStyle(Pen.DashStyles dashStyle)
        {
            switch (dashStyle)
            {
                case Pen.DashStyles.Solid:
                    return new float[] { };

                case Pen.DashStyles.Dash:
                    return new float[] { 1, 4 };

                default: throw new ArgumentOutOfRangeException($"{nameof(dashStyle)}: {dashStyle.ToString()}");
            }
        }

        private async Task SetStrokeFromPen(Pen pen, Canvas2DContext ctx)
        {
            var col = pen.Color.AsHtml;
            // if (!col.Equals(ctx.StrokeStyle)) 
            await ctx.SetStrokeStyleAsync(col);

            //if (ctx.LineWidth != pen.Width) 
            //     await ctx.SetLineWidthAsync(pen.Width);

            // await ctx.SetLineDashAsync(this.GetDashStyle(pen.DashStyle));
        }

        private async Task ResetStroke(Canvas2DContext ctx)
        {
            // reset line dashs and other parameters
            await ctx.SetLineDashAsync(new float[] { });
            await ctx.SetLineCapAsync(LineCap.Butt);
        }


    }
}
