
namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class Font
    {
        public enum GraphicsUnit
        {
            Point
        }

        public string Name { get; set; }
        public int Height { get; set; }
        public GraphicsUnit Unit { get; set; }

        public Font(string name, int height, GraphicsUnit graphicsUnit)
        {
            this.Name = name;
            this.Height = height;
            this.Unit = graphicsUnit;
        }
    }
}
