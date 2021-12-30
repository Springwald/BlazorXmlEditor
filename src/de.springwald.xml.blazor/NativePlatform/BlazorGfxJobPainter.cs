// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.nativeplatform.gfx.Jobs;

namespace de.springwald.xml.blazor.NativePlatform
{
    internal static class BlazorGfxJobPainter
    {
        private static bool DebugUnPaint = false;

        private static Color[] unPaintColors = new[] { Color.LightBlue, Color.LightGray, Color.Yellow, Color.LightGreen };
        private static int unPaintColor = 0;

        private static Color UnPaintColor
        {
            get
            {
                unPaintColor++;
                if (unPaintColor >= unPaintColors.Length) unPaintColor = 0;
                return unPaintColors[unPaintColor];
            }
        }

        internal static async Task PaintJob(GfxJob job, BlazorGfxContext gfx, Color backgroundColor)
        {
            switch (job.JobType)
            {
                case GfxJob.JobTypes.Clear:
                    if (DebugUnPaint)
                    {
                        await gfx.ClearAsync(UnPaintColor);
                    }
                    else
                    {

                        await gfx.ClearAsync(backgroundColor);
                    }
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
                        await gfx.DrawRectangleAsync(UnPaintColor, null!, 0, rectangle);
                    }
                    else
                    {
                        await gfx.DrawRectangleAsync(backgroundColor, null!, 0, rectangle);
                    }
                    break;

                default: throw new ArgumentOutOfRangeException($"{nameof(job.JobType)}:{job.JobType.ToString()}");
            }
        }
    }
}
