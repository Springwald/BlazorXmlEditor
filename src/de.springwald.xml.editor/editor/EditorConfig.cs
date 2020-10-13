// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.nativeplatform.gfx;
using System;

namespace de.springwald.xml.editor
{
    public abstract class EditorConfig
    {
        public abstract Font NodeNameFont { get; set; }

        public abstract Font NodeAttributeFont { get; set; }

        public abstract Font TextNodeFont { get; set; }

        public int TagHeight => this.NodeNameFont.Height + this.InnerMarginY * 2;

        public int InnerMarginY => Math.Max(1, this.NodeNameFont.Height / 3);

        public int MinLineHeight => this.TagHeight;
    }
}
