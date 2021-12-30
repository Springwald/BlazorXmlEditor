// A platform independent tag-view-style graphical XML editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2021 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using Blazor.Extensions;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.nativeplatform.gfx.Jobs;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorGfx : IGraphics
    {
        private readonly BECanvasComponent canvas;
        private readonly List<GfxJob> jobs = new();
        private BlazorGfxContext contextCache;

        // public int DesiredMaxWidth { get; private set; }

        public BlazorGfx(BECanvasComponent canvas)
        {
            this.canvas = canvas;
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

        public void DeleteAllPaintJobs()
        {
            this.jobs.Clear();
        }
    }
}