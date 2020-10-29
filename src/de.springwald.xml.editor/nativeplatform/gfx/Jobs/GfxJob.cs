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
        public enum Layers
        {
            ClearBackground = 1,
            TagBackground = 5,
            AttributeBackground = 10,
            TagBorder = 15,
            ClickAreas = 20,
            Cursor = 25,
            Text = 50
        }

        public enum JobTypes
        {
            Clear,
            UnPaintRectangle,
            DrawLine,
            DrawPolygon,
            DrawRectangle,
            DrawString
        }

        public Layers Layer { get; set; }
        public bool Batchable { get; set; }
        public abstract JobTypes JobType { get; }
        public abstract string SortKey { get; }
    }
}
