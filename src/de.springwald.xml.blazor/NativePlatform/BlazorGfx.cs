
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Extensions;
using de.springwald.xml.editor.nativeplatform.gfx.Jobs;
using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorGfx : IGraphics
    {
        private BECanvasComponent canvas;
        private BlazorGfxContext contextCache;

        private List<GfxJob> jobs = new List<GfxJob>();

        public int Width { get; private set; }
        public int Height { get; private set; }

        public BlazorGfx(BECanvasComponent canvas)
        {
            this.canvas = canvas;
        }

        public Task SetSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            return Task.CompletedTask;
        }

        public async Task StartBatch()
        {
            if (contextCache?.IsInBatch == true) await this.contextCache.EndBatch();
            this.contextCache = null;
            await (await this.GetContext()).StartBatch();
        }

        public async Task EndBatch()
        {
            if (this.contextCache != null)
            {
                await this.contextCache.EndBatch();
            }
        }

        public async Task<double> MeasureDisplayStringWidthAsync(string text, Font font)
        {
            await this.EndBatch();
            return await (await this.GetContext()).MeasureDisplayStringWidthAsync(text, font);
        }

        public void UnPaintRectangle(Rectangle rectangle)
        {
            if (rectangle != null) this.AddJob(new JobUnpaintRectangle { Rectangle = rectangle });
        }

        public void AddJob(GfxJob job)
        {
            this.jobs.Add(job);
        }

        public async Task PaintJobs(Color backgroundColor)
        {
            var sorted = this.jobs.OrderBy(j => j.Layer).ThenBy(j => j.Batchable).ThenBy(j => j.SortKey);
            var batching = false;
            foreach (var job in sorted)
            {
                if (job.Batchable)
                {
                    if (!batching)
                    {
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

                await BlazorGfxJobPainter.PaintJob(job, await this.GetContext(), backgroundColor);
            }
            this.jobs.Clear();
            if (batching) await this.EndBatch();
        }

        private async Task<BlazorGfxContext> GetContext()
        {
            if (this.contextCache == null)
            {
                var context2d = await this.canvas.CreateCanvas2DAsync();
                this.contextCache = new BlazorGfxContext(canvas, context2d);
            }
            return this.contextCache;
        }

      
    }
}