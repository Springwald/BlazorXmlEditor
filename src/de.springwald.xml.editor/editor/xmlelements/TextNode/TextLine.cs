using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.editor.editor.xmlelements.TextNode
{
    internal class TextLine
    {
        public string Text { get; set; }
        public Rectangle Rectangle { get; set; }
        public bool Inverted { get; set; }
    }
}
