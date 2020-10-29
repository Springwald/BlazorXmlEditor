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
    public class JobDrawString : GfxJob
    {
        public override JobTypes JobType => JobTypes.DrawString;
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
        public Font Font { get; set; }
        public Color Color { get; set; }
        public override string SortKey => $"{this.Color}-{this.Font}";
    }
}

