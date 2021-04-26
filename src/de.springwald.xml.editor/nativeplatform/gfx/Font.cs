// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

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
