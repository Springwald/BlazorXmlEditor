using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorGfx : IGraphics
    {
        private Canvas2DContext context;
        private BECanvasComponent canvas;

        public BlazorGfx(Canvas2DContext context, Blazor.Extensions.BECanvasComponent canvas)
        {
            this.context = context;
            this.canvas = canvas;
        }

        public async Task StartBatch()
        {
            var ctx = this.context;
            await ctx.BeginBatchAsync();
        }

        public async Task EndBatch()
        {
            var ctx = this.context;
            await ctx.EndBatchAsync();
        }

        public async Task DrawLineAsync(Pen pen, int x1, int y1, int x2, int y2)
        {
            var ctx = this.context;
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

            var ctx = this.context;
            await this.SetStrokeFromPen(pen, ctx);
            await LinePath(gp, ctx);
            await ctx.StrokeAsync();

            await ResetStroke(ctx);
        }

        public async Task DrawRectangleAsync(Pen pen, Rectangle rectangle)
        {
            
            var ctx = this.context;
            await this.SetStrokeFromPen(pen, ctx);
            await ctx.StrokeRectAsync(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            await ResetStroke(ctx);
        }

        public async Task DrawStringAsync(string text, Font font, SolidBrush brush, int x, int y, StringFormat drawFormat)
        {
            var ctx = this.context;
            await this.SetFontFormat(font);
            await ctx.SetFillStyleAsync(brush.Color.AsHtml);
            await ctx.SetFontAsync(font.Name);
            await ctx.FillTextAsync(text, x, y);
            await ResetStroke(ctx);
        }

        //public async Task<SizeF> MeasureString(string text, Font font, int maxWidth, StringFormat drawFormat)
        //{
        //    var ctx = this.context;
        //    await this.SetFontFormat(font);
        //    var size = await ctx.MeasureTextAsync(text);
        //    return new SizeF { Width = (float)size.Width, Height = size.;
        //}

        public async Task<float> MeasureDisplayStringWidthAsync(string text, Font font, StringFormat drawFormat)
        {
            var ctx = this.context;
            await this.SetFontFormat(font);
            var size = await ctx.MeasureTextAsync(text);
            return (float)size.Width;
        }

        public async Task FillPathAsync(SolidBrush brush, GraphicsPath gp)
        {
            if (gp.Lines.Count == 0) return;

            var ctx = this.context;
            await ctx.SetFillStyleAsync(brush.Color.AsHtml);
            await ctx.BeginPathAsync();
            await LinePath(gp, ctx);
            await ctx.FillAsync();

            await ResetStroke(ctx);
        }

        public async Task FillPolygonAsync(SolidBrush brush, Point[] points)
        {
            if (points.Length == 0) return;

            var ctx = this.context;
            await ctx.SetFillStyleAsync(brush.Color.AsHtml);

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(points[0].X, points[0].Y);
            for (int i = 1; i < points.Length; i++)
            {
                await ctx.LineToAsync(points[i].X, points[i].Y);
            }
            await ctx.LineToAsync(points[0].X, points[0].Y);
            await ctx.FillAsync();

            await ResetStroke(ctx);
        }

        public async Task FillRectangleAsync(SolidBrush newBrush, Rectangle rechteck)
        {
            await Task.CompletedTask; // to prevent warning because of empty async method
        }

        public async Task ClearAsync(Color color)
        {
            var ctx = this.context;
            await ctx.SetFillStyleAsync(color.AsHtml);
#warning todo clear real canvas size
            await ctx.ClearRectAsync(0, 0, this.canvas.Width, this.canvas.Height);
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

        private async Task SetFontFormat(Font font)
        {
            await this.context.SetTextAlignAsync(TextAlign.Left);
            await this.context.SetTextBaselineAsync(TextBaseline.Top);
            await this.context.SetFontAsync($"{font.Height}px {font.Name}"); // e.g. '48px serif';
            await this.context.SetLineWidthAsync(1);
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
            await ctx.SetStrokeStyleAsync(pen.Color.AsHtml);
            await ctx.SetLineWidthAsync(pen.Width);
            await ctx.SetLineDashAsync(this.GetDashStyle(pen.DashStyle));
        }

        private async Task ResetStroke(Canvas2DContext ctx)
        {
            // reset line dashs and other parameters
            await ctx.SetLineDashAsync(new float[] { });
            await ctx.SetLineCapAsync(LineCap.Butt);
        }


    }
}
