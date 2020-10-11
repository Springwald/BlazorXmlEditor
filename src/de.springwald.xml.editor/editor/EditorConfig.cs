// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Text;
using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor
{
    public interface IEditorConfig
    {
        Font NodeNameFont { get; set; }

        Font NodeAttributeFont { get; set; }

        Font TextNodeFont { get; set; }
    }
}
