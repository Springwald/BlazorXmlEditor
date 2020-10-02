using de.springwald.xml.editor;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.blazor
{
    public class EditorConfig : IEditorConfig
    {
        public Font NodeNameFont { get; set; }
        public Font NodeAttributeFont { get; set; }
        public Font TextNodeFont { get; set; }
    }
}
