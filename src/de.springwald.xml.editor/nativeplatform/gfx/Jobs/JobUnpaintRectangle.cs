// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.editor.nativeplatform.gfx.Jobs
{
    public class JobUnpaintRectangle : GfxJob
    {
        public Rectangle Rectangle { get; set; }
        public override JobTypes JobType => JobTypes.UnPaintRectangle;
        public override string SortKey => $"";
    }
}
