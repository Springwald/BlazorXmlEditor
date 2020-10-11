// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace de.springwald.xml.events
{
    public class KeyEventArgs
    {
        public Keys Key { get; set; }
        public string Content { get; set; }

        public bool CtrlKey { get; set; }
        public bool AltKey { get; set; }
        public bool ShiftKey { get; set; }
    }
}

