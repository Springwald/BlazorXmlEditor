// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using de.springwald.xml.events;

namespace de.springwald.xml.editor.nativeplatform.events
{
    public interface IInputEvents
    {
        XmlAsyncEvent<MouseEventArgs> MouseDown { get; }
        XmlAsyncEvent<MouseEventArgs> MouseUp { get; }
        XmlAsyncEvent<MouseEventArgs> MouseMove { get; }

        XmlAsyncEvent<KeyEventArgs> KeyPress { get; }
        XmlAsyncEvent<KeyEventArgs> PreviewKey { get; }
    }
}
