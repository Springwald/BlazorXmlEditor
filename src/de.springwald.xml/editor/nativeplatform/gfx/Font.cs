
namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class Font
    {
        public enum GraphicsUnit
        {
            Pixel
        }

        public string Name { get; set; }
        public int Height { get; set; }
        public GraphicsUnit Unit { get; set; }

        /// <summary>
        /// All monospace characters have the same width
        /// </summary>
        public bool Monospace { get; set; }

        public Font(string name, int height, GraphicsUnit graphicsUnit)
        {
            this.Name = name;
            this.Height = height;
            this.Unit = graphicsUnit;
        }
    }
}
