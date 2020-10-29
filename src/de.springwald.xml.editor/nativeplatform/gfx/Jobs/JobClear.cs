// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor.nativeplatform.gfxobs
{
    public class JobClear : GfxJob
    {
        public override JobTypes JobType => JobTypes.Clear;
        public Color FillColor { get; set; }

        public override string SortKey => $"{this.FillColor.AsHtml}";
    }
}