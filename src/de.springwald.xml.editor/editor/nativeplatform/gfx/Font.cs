
using System.Linq;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class Font
    {
        public enum GraphicsUnit
        {
            Pixel
        }

        public string[] Names { get; set; }

        public int Height { get; set; }
        public GraphicsUnit Unit { get; set; }

        /// <summary>
        /// All monospace characters have the same width
        /// </summary>
        public bool Monospace { get; set; }

        public Font(string[] names, int height, GraphicsUnit graphicsUnit, bool monospace)
        {
            this.Names = names;
            this.Height = height;
            this.Unit = graphicsUnit;
            this.Monospace = monospace;
        }
    }
}
