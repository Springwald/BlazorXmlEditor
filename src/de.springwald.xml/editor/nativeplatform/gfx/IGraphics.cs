using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public interface IGraphics
    {
        int Height { get; }
        int Width { get; }

        Task SetSize(int width, int height);
        //Task StartBatch();
        //Task EndBatch();

        void AddJob(IGfxJob job);
        
        Task PaintJobs();
        Task<float> MeasureDisplayStringWidthAsync(string text, Font drawFont);

        Task DrawLineAsync(Pen pen, int x1, int y1, int x2, int y2);

        Task DrawPathAsync(Pen pen, GraphicsPath gp);
        Task FillPathAsync(Color color, GraphicsPath gp);
        Task FillPolygonAsync(Color color, Point[] points);

        Task DrawPolygonAsync(Pen pen, Point[] points);

        Task DrawRectangleAsync(Pen newPen, Rectangle rechteck);
        Task FillRectangleAsync(Color color, Rectangle rechteck);

        Task ClearAsync(Color color);

        Task DrawStringAsync(string name, Font font, Color color, int x, int y);
        // Task  SizeF MeasureString(string text, Font drawFont, int maxWidth, StringFormat drawFormat);



    }
}

