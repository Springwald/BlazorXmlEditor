using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public interface IGraphics
    {
        int Height { get; }
        int Width { get; }

        Task SetSize(int width, int height);

        void AddJob(GfxJob job);
        
        Task PaintJobs();

        Task<float> MeasureDisplayStringWidthAsync(string text, Font drawFont);
    }
}

