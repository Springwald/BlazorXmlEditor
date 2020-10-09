using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.editor
{
    public interface IEditorConfig
    {
        Font NodeNameFont { get; set; }

        Font NodeAttributeFont { get; set; }

        Font TextNodeFont { get; set; }
    }
}
