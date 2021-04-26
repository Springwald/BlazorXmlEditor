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
    public class JobDrawPolygon : GfxJob
    {
        public override JobTypes JobType => JobTypes.DrawPolygon;
        public Color BorderColor { get; set; }
        public float BorderWidth { get; set; } = 1;
        public Color FillColor { get; set; }
        public Point[] Points { get; set; }
        public override string SortKey => $"{this.BorderColor}-{this.FillColor}";
    }
}
