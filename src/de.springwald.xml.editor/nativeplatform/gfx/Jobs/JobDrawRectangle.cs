﻿// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobDrawRectangle : GfxJob
    {
        public override JobTypes JobType => JobTypes.DrawRectangle;
        public Color FillColor { get; set; }
        public Color BorderColor { get; set; }
        public float BorderWidth { get; set; } = 1;
        public Rectangle Rectangle { get; set; }
        public override string SortKey => $"{this.BorderColor}-{this.FillColor}";
    }
}