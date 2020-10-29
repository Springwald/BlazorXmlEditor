
using System;
using System.Threading.Tasks;
using de.springwald.xml.editor.nativeplatform.gfx.Jobs;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.nativeplatform.gfxobs;

namespace de.springwald.xml.blazor.NativePlatform
{
    internal static class BlazorGfxJobPainter
    {
        private const bool DebugUnPaint = false;

        private static Color[] unPaintColors = new[] { Color.Blue, Color.DarkBlue, Color.Gray, Color.Red, Color.White };
        private static int unPaintColor = 0;

        internal static async Task PaintJob(GfxJob job, BlazorGfxContext gfx, Color backgroundColor)
        {
            switch (job.JobType)
            {
                case GfxJob.JobTypes.Clear:
                    await gfx.ClearAsync((job as JobClear).FillColor);
                    break;

                case GfxJob.JobTypes.DrawLine:
                    var drawLineJob = job as JobDrawLine;
                    await gfx.DrawLineAsync(drawLineJob.Color, drawLineJob.LineWidth, drawLineJob.X1, drawLineJob.Y1, drawLineJob.X2, drawLineJob.Y2);
                    break;

                case GfxJob.JobTypes.DrawPolygon:
                    var drawDrawPolygonJob = job as JobDrawPolygon;
                    await gfx.DrawPolygonAsync(drawDrawPolygonJob.FillColor, drawDrawPolygonJob.BorderColor, drawDrawPolygonJob.BorderWidth, drawDrawPolygonJob.Points);
                    break;

                case GfxJob.JobTypes.DrawRectangle:
                    var drawDrawRectangleJob = job as JobDrawRectangle;
                    await gfx.DrawRectangleAsync(drawDrawRectangleJob.FillColor, drawDrawRectangleJob.BorderColor, drawDrawRectangleJob.BorderWidth, drawDrawRectangleJob.Rectangle);
                    break;

                case GfxJob.JobTypes.DrawString:
                    var drawStringJob = job as JobDrawString;
                    await gfx.DrawStringAsync(drawStringJob.Text, drawStringJob.Font, drawStringJob.Color, drawStringJob.X, drawStringJob.Y);
                    break;

                case GfxJob.JobTypes.UnPaintRectangle:
                    var unpaintJob = job as JobUnpaintRectangle;
                    const int margin = 1;
                    var rectangle = new Rectangle(unpaintJob.Rectangle.X - margin, unpaintJob.Rectangle.Y - margin, unpaintJob.Rectangle.Width + margin + margin, unpaintJob.Rectangle.Height + margin + margin);
                    if (DebugUnPaint)
                    {
                        unPaintColor++;
                        if (unPaintColor >= unPaintColors.Length) unPaintColor = 0;
                        var fillColor = unPaintColors[unPaintColor]; // this.xmlEditor.NativePlatform.ControlElement.BackColor,
                        await gfx.DrawRectangleAsync(fillColor, null, 0, rectangle);
                    } else
                    {
                        await gfx.DrawRectangleAsync(backgroundColor, null, 0, rectangle);
                    }
                    break;

                default: throw new ArgumentOutOfRangeException($"{nameof(job.JobType)}:{job.JobType.ToString()}");
            }
        }
    }
}
