using de.springwald.xml.editor.nativeplatform.gfx;
//using Excubo.Blazor.Canvas;
//using Excubo.Blazor.Canvas.Contexts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.NativePlatform
{
    //public class BlazorGfx2 : IGraphics
    //{
    //    private Context2D contextCache;

    //    public int Width { get; private set; }
    //    public int Height { get; private set; }

    //    public Task SetSize(int width, int height)
    //    {
    //        this.Width = width;
    //        this.Height = height;
    //        return Task.CompletedTask;
    //    }

    //    private async Task<Context2D> Get2DContext()
    //    {
    //        if (this.contextCache == null)
    //        {
    //            this.contextCache = await this.canvas.GetContext2DAsync();
    //        }
    //        return this.contextCache;
    //    }

    //    private async Task<IContext2DWithoutGetters> GetContext()
    //    {
    //        if (this.batch != null) return this.batch;
    //        if (this.contextCache == null)
    //        {
    //            this.contextCache = await this.canvas.GetContext2DAsync();
    //        }
    //        return this.contextCache;
    //    }

    //    private Canvas canvas;
    //    private Batch2D batch;

    //    public BlazorGfx2(Canvas canvas)
    //    {
    //        this.canvas = canvas;
    //    }

    //    public async Task StartBatch()
    //    {
    //        return;
    //        if (this.batch != null) await this.EndBatch();
    //        this.contextCache = null;
    //        this.batch = await (await this.Get2DContext()).CreateBatchAsync();
    //    }

    //    public async Task EndBatch()
    //    {
    //        if (this.batch == null) return;
    //        await this.batch.DisposeAsync();
    //        this.contextCache = null;
    //    }

    //    public async Task DrawLineAsync(Pen pen, int x1, int y1, int x2, int y2)
    //    {
    //        var ctx = await this.GetContext();
    //        await this.SetStrokeFromPen(pen, ctx);

    //        await ctx.BeginPathAsync();
    //        await ctx.SetLineDashAsync(this.GetDashStyle(pen.DashStyle));
    //        await ctx.LineCapAsync(this.GetLineCap(pen.StartCap));
    //        await ctx.MoveToAsync(x1, y1);

    //        await ctx.LineCapAsync(this.GetLineCap(pen.EndCap));
    //        await ctx.LineToAsync(x2, y2);
    //        await ctx.StrokeAsync();

    //        await ResetStroke(ctx);
    //    }

    //    public async Task DrawPathAsync(Pen pen, GraphicsPath gp)
    //    {
    //        if (gp.Lines.Count == 0) return;

    //        var ctx = await this.GetContext();

    //        await this.SetStrokeFromPen(pen, ctx);
    //        // await ctx.SetLineCapAsync(this.GetLineCap(Pen.LineCap.NoAnchor));
    //        //  await ctx.SetLineJoinAsync(LineJoin.Round);
    //        //  await ctx.SetStrokeStyleAsync("");
    //        await LinePath(gp, ctx);
    //        await ctx.StrokeAsync();

    //        await ResetStroke(ctx);
    //    }

    //    public async Task DrawRectangleAsync(Pen pen, Rectangle rectangle)
    //    {
    //        var ctx = await this.GetContext();
    //        await this.SetStrokeFromPen(pen, ctx);
    //        await ctx.StrokeRectAsync(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    //        await ResetStroke(ctx);
    //    }

    //    public async Task DrawStringAsync(string text, Font font, SolidBrush brush, int x, int y)
    //    {
    //        var ctx = await this.Get2DContext();
    //        await ctx.FillStyleAsync(brush.Color.AsHtml);
    //        await this.SetFontFormat(ctx, font);
    //        await ctx.FillTextAsync(text, x, y);
    //        await ResetStroke(ctx);
    //    }

    //    public async Task<float> MeasureDisplayStringWidthAsync(string text, Font font)
    //    {
    //        var ctx = await this.Get2DContext();
    //        await this.SetFontFormat(ctx, font);
    //        //await Task.Delay(1000);
    //        var size = await ctx.MeasureTextAsync(text);
    //        return (float)size.Width;
    //    }

    //    private async Task SetFontFormat(Context2D ctx, Font font)
    //    {
    //        var targetFont = string.Empty;
    //        switch (font.Unit)
    //        {
    //            case Font.GraphicsUnit.Pixel:
    //                targetFont = $"{font.Height}px {font.Name}"; // e.g. '48px serif';
    //                break;
    //            default: throw new ArgumentOutOfRangeException($"{nameof(font.Unit)}:{font.Unit.ToString()}");
    //        }
    //        await ctx.TextAlignAsync(TextAlign.Left);
    //        await ctx.TextBaseLineAsync(TextBaseLine.Top);
    //        //if (!targetFont.Equals(ctx.Font))
    //        {
    //            await ctx.FontAsync(targetFont);
    //        }
    //    }

    //    public async Task FillPathAsync(SolidBrush brush, GraphicsPath gp)
    //    {
    //        if (gp.Lines.Count == 0) return;

    //        var ctx = await this.GetContext();
    //        await ctx.FillStyleAsync(brush.Color.AsHtml);
    //        await ctx.BeginPathAsync();
    //        await LinePath(gp, ctx);
    //        await ctx.FillAsync(FillRule.NonZero);
    //        await ResetStroke(ctx);
    //    }

    //    public async Task FillPolygonAsync(SolidBrush brush, Point[] points)
    //    {
    //        if (points.Length == 0) return;

    //        var ctx = await this.GetContext();
    //        await ctx.FillStyleAsync(brush.Color.AsHtml);
    //        await ctx.BeginPathAsync();
    //        await ctx.MoveToAsync(points[0].X, points[0].Y);
    //        for (int i = 1; i < points.Length; i++)
    //        {
    //            await ctx.LineToAsync(points[i].X, points[i].Y);
    //        }
    //        await ctx.LineToAsync(points[0].X, points[0].Y);
    //        await ctx.FillAsync(FillRule.NonZero);
    //    }

    //    public async Task FillRectangleAsync(SolidBrush newBrush, Rectangle rechteck)
    //    {
    //        await Task.CompletedTask; // to prevent warning because of empty async method
    //    }

    //    public async Task ClearAsync(Color color)
    //    {
    //        var ctx = await this.GetContext();
    //        if (color != Color.White)
    //        {
    //            await ctx.FillStyleAsync(color.AsHtml);
    //            await ctx.FillRectAsync(0, 0, this.Width, this.Height);
    //        }
    //        else
    //        {
    //            await ctx.ClearRectAsync(0, 0, this.Width, this.Height);
    //        }
    //    }

    //    // ######### private helpers ##########

    //    private static async Task LinePath(GraphicsPath gp, IContext2DWithoutGetters ctx)
    //    {
    //        await ctx.MoveToAsync(gp.Lines[0].X1, gp.Lines[0].Y1);
    //        await ctx.LineToAsync(gp.Lines[0].X2, gp.Lines[0].Y2);
    //        for (int i = 1; i < gp.Lines.Count; i++)
    //        {
    //            if (gp.Lines[i].X1 != gp.Lines[i - 1].X2 || gp.Lines[i].Y1 != gp.Lines[i - 1].Y2)
    //            {
    //                await ctx.LineToAsync(gp.Lines[i].X1, gp.Lines[i].Y1);
    //            }
    //            await ctx.LineToAsync(gp.Lines[i].X2, gp.Lines[i].Y2);
    //        }
    //    }

    //    private LineCap GetLineCap(Pen.LineCap cap)
    //    {
    //        switch (cap)
    //        {
    //            case Pen.LineCap.NoAnchor: return LineCap.Butt;
    //            case Pen.LineCap.SquareAnchor: return LineCap.Square;
    //            case Pen.LineCap.RoundAnchor: return LineCap.Round;
    //            default: throw new ArgumentOutOfRangeException($"{nameof(cap)}: {cap.ToString()}");
    //        }
    //    }

    //    private double[] GetDashStyle(Pen.DashStyles dashStyle)
    //    {
    //        switch (dashStyle)
    //        {
    //            case Pen.DashStyles.Solid:
    //                return new double[] { };

    //            case Pen.DashStyles.Dash:
    //                return new double[] { 1, 4 };

    //            default: throw new ArgumentOutOfRangeException($"{nameof(dashStyle)}: {dashStyle.ToString()}");
    //        }
    //    }

    //    private async Task SetStrokeFromPen(Pen pen, IContext2DWithoutGetters ctx)
    //    {
    //        var col = pen.Color.AsHtml;
    //        // if (!col.Equals(ctx.StrokeStyle)) 
    //        await ctx.StrokeStyleAsync(col);

    //        //if (ctx.LineWidth != pen.Width) 
    //        //     await ctx.SetLineWidthAsync(pen.Width);

    //        // await ctx.SetLineDashAsync(this.GetDashStyle(pen.DashStyle));
    //    }

    //    private async Task ResetStroke(IContext2DWithoutGetters ctx)
    //    {
    //        // reset line dashs and other parameters
    //        await ctx.SetLineDashAsync(new double[] { });
    //        await ctx.LineCapAsync(LineCap.Butt);
    //    }


    //}
}
