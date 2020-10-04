using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.nativeplatform.gfxobs;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.NativePlatform
{
    internal static class BlazorGfxJobPainter
    {
        internal static async Task PaintJob(GfxJob job, BlazorGfxContext gfx)
        {
            switch (job.JobType)
            {
                case GfxJob.JobTypes.Clear:
                    await gfx.ClearAsync((job as JobClear).Color);
                    break;

                case GfxJob.JobTypes.DrawLine:
                    var drawLineJob = job as JobDrawLine;
                    await gfx.DrawLineAsync(drawLineJob.Pen, drawLineJob.X1, drawLineJob.Y1, drawLineJob.X2, drawLineJob.Y2);
                    break;

                case GfxJob.JobTypes.DrawPolygon:
                    var drawDrawPolygonJob = job as JobDrawPolygon;
                    await gfx.DrawPolygonAsync(drawDrawPolygonJob.Pen, drawDrawPolygonJob.Points);
                    break;

                case GfxJob.JobTypes.FillPolygon:
                    var drawFillPolygonJob = job as JobFillPolygon;
                    await gfx.FillPolygonAsync(drawFillPolygonJob.Color, drawFillPolygonJob.Points);
                    break;

                case GfxJob.JobTypes.DrawRectangle:
                    var drawDrawRectangleJob = job as JobDrawRectangle;
                    await gfx.DrawRectangleAsync(drawDrawRectangleJob.Pen, drawDrawRectangleJob.Rectangle);
                    break;

                case GfxJob.JobTypes.FillRectangle:
                    var drawFillRectangleJob = job as JobFillRectangle;
                    await gfx.FillRectangleAsync(drawFillRectangleJob.Color, drawFillRectangleJob.Rectangle);
                    break;

                case GfxJob.JobTypes.DrawString:
                    var drawStringJob = job as JobDrawString;
                    await gfx.DrawStringAsync(drawStringJob.Text, drawStringJob.Font, drawStringJob.Color, drawStringJob.X, drawStringJob.Y);
                    break;

                default: throw new ArgumentOutOfRangeException($"{nameof(job.JobType)}:{job.JobType.ToString()}");
            }
        }
    }
}
