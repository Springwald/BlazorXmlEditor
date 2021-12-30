// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor.xmlelements.TextNode
{
    internal class TextPart
    {
        public string Text { get; set; }
        public Rectangle Rectangle { get; set; }
        public bool Inverted { get; set; }
        public int CursorPos { get; set; } = -1;
        public bool CursorBlink { get; set; }

        public bool Equals(TextPart second)
        {
            if (second == null) return false;
            if (this.Inverted != second.Inverted) return false;
            if (this.Text != second.Text) return false;
            if (this.CursorPos != second.CursorPos) return false;
            if (this.CursorPos >= 0 && this.CursorBlink != second.CursorBlink) return false;
            if (!this.Rectangle.Equals(second.Rectangle)) return false;
            return true;
        }
    }
}
