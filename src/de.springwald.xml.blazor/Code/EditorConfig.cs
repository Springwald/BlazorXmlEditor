using de.springwald.xml.editor;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.blazor
{
    public class EditorConfig : IEditorConfig
    {
        private Font textNodeFont;

        public Font NodeNameFont { get; set; }
        public Font NodeAttributeFont { get; set; }
        public Font TextNodeFont
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
