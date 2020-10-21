using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor.xmlelements.TextNode
{
    internal class TextPart
    {
        public string Text { get; set; }
        public Rectangle Rectangle { get; set; }
        public bool Inverted { get; set; }
        public int CursorPos { get; set; } = -1;

        public bool Equals(TextPart second)
        {
            if (second == null) return false;
            if (this.Inverted != second.Inverted) return false;
            if (this.Text != second.Text) return false;
            if (this.CursorPos != second.CursorPos) return false;
            if (!this.Rectangle.Equals(second.Rectangle)) return false;
            return true;
        }
    }
}
