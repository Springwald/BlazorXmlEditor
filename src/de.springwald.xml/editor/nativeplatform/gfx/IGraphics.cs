using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public interface IGraphics
    {
        Task DrawLineAsync(Pen myPen, int x1, int y1, int x2, int y2);

        Task DrawPathAsync(Pen newPen, GraphicsPath gp);
        Task FillPathAsync(SolidBrush newBrush, GraphicsPath gp);
        Task FillPolygonAsync(SolidBrush brush, Point[] points);

        Task DrawRectangleAsync(Pen newPen, Rectangle rechteck);
        Task FillRectangleAsync(SolidBrush newBrush, Rectangle rechteck);

        Task ClearAsync(Color white);

        Task DrawStringAsync(string name, Font drawFontNodeName, SolidBrush drawBrush, int posX, int v, StringFormat drawFormat);
        // Task  SizeF MeasureString(string text, Font drawFont, int maxWidth, StringFormat drawFormat);

        Task<float> MeasureDisplayStringWidthAsync(string text, Font drawFont, StringFormat drawFormat);
        
    }
}
