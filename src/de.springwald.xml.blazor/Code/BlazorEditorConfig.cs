// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor;
using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.blazor
{
    public class BlazorEditorConfig : EditorConfig
    {
        private Font textNodeFont;

        public static BlazorEditorConfig StandardConfig
        {
            get
            {
                return new BlazorEditorConfig
                {
                    FontNodeName = new Font(names: new[] { "Verdana", "Geneva", "sans-serif" }, height: 13, graphicsUnit: Font.GraphicsUnit.Pixel, monospace: false),
                    FontNodeAttribute = new Font(names: new[] { "Verdana", "Geneva", "sans-serif" }, height: 12, graphicsUnit: Font.GraphicsUnit.Pixel, monospace: false),
                    FontTextNode = new Font(names: new[] { "Lucida Console", "Monaco", "monospace" }, height: 14, graphicsUnit: Font.GraphicsUnit.Pixel, monospace: true)
                };
            }
        }

        public override Font FontNodeName { get; set; }
        public override Font FontNodeAttribute { get; set; }
        public override Font FontTextNode
        {
            get => this.textNodeFont;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(this.textNodeFont));
                if (!value.Monospace) throw new ArgumentException("TextNodeFont has to be monospace font!");
                this.textNodeFont = value;
            }
        }
    }
}
