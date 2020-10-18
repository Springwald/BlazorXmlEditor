using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor.editor.xmlelements.StandardNode
{
    internal class StandardNodeTagPaint
    {

        internal static void DrawNodeBodyBySize(GfxJob.Layers layer, int x, int y, int width, int height, int cornerRadius, Color fillColor, Color borderColor, IGraphics gfx)
        {
            DrawNodeBodyByCoordinates(layer, x, y, x + width, y + height, cornerRadius, fillColor, borderColor, gfx);
        }

        internal static void DrawNodeBodyByCoordinates(GfxJob.Layers layer, int x1, int y1, int x2, int y2, int cornerRadius, Color fillColor, Color borderColor, IGraphics gfx)
        {
            gfx.AddJob(new JobDrawPolygon
            {
                Batchable = true,
                Layer = layer,
                FillColor = fillColor == Color.Transparent ? null : fillColor,
                BorderColor = borderColor == Color.Transparent ? null : borderColor,
                BorderWidth = 1,
                Points = new[] {
                new Point(x1 + cornerRadius, y1),
                new Point(x2 - cornerRadius, y1),
                new Point(x2, y1 + cornerRadius),
                new Point(x2, y2 - cornerRadius),
                new Point(x2 - cornerRadius, y2),
                new Point(x1 + cornerRadius, y2),
                new Point(x1, y2 - cornerRadius),
                new Point(x1, y1 + cornerRadius)}
            });
        }
    }
}
