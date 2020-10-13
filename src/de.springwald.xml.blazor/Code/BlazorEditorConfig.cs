using de.springwald.xml.editor;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.blazor
{
    public class BlazorEditorConfig : EditorConfig
    {
        private Font textNodeFont;

        public override Font NodeNameFont { get; set; }
        public override Font NodeAttributeFont { get; set; }
        public override Font TextNodeFont
        {
            get => this.textNodeFont; set
            {
                if (value == null) throw new ArgumentNullException(nameof(this.textNodeFont));
                if (!value.Monospace) throw new ArgumentException("TextNodeFont has to be monospace font!");
                this.textNodeFont = value;
            }
        }
    }
}
