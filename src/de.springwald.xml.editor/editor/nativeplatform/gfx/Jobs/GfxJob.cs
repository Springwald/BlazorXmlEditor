// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public abstract class GfxJob
    {
        public enum JobTypes
        {
            Clear,
            DrawLine,
            DrawPolygon,
            DrawRectangle,
            DrawString
        }

        public int Layer { get; set; }
        public bool Batchable { get; set; }
        public abstract JobTypes JobType { get; }
        public abstract string SortKey { get; }
    }
}
