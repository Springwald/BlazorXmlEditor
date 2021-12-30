// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
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
        public abstract Font FontNodeName { get; set; }

        public abstract Font FontNodeAttribute { get; set; }

        public abstract Font FontTextNode { get; set; }

        public Color ColorBackground { get; set; } = Color.White;
        public Color ColorText { get; set; } = Color.Black;
        public Color ColorNodeTagBorder { get; set; } = Color.Gray;
        public Color ColorNodeTagBackground { get; set; } = Color.LightGray;
        public Color ColorNodeAttributeBackground { get; set; } = Color.White;
        public Color ColorCommentTextBackground { get; set; } = Color.LightGray;

        public int TagHeight => this.FontNodeName.Height + this.InnerMarginY * 2;

        public int InnerMarginY => Math.Max(1, this.FontNodeName.Height / 3);

        public int ChildIndentX => (int)(this.FontNodeName.Height * 1.5);

        public int SpaceYBetweenLines => (int)(Math.Max(1, this.FontNodeName.Height * 0.2));

        public int MinLineHeight => this.TagHeight + 2;
    }
}
