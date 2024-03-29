﻿// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.nativeplatform.events;
using de.springwald.xml.events;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorInputEvents : IInputEvents
    {
        public XmlAsyncEvent<MouseEventArgs> MouseDown { get; } = new XmlAsyncEvent<events.MouseEventArgs>();

        public XmlAsyncEvent<MouseEventArgs> MouseUp { get; } = new XmlAsyncEvent<events.MouseEventArgs>();

        public XmlAsyncEvent<MouseEventArgs> MouseMove { get; } = new XmlAsyncEvent<events.MouseEventArgs>();

        public XmlAsyncEvent<KeyEventArgs> KeyPress { get; } = new XmlAsyncEvent<KeyEventArgs>();

        public XmlAsyncEvent<KeyEventArgs> PreviewKey { get; } = new XmlAsyncEvent<KeyEventArgs>();
    }
}
