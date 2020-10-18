using de.springwald.xml.cursor;
using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor.editor.xmlelements.Caching
{
    internal class LastPaintingDataText
    {
        public int LastPaintPosX { get; set; }
        public int LastPaintPosY { get; set; }
        public int LastPaintLimitRight { get; set; }
        public string LastPaintContent { get; set; }
        public int LastPaintTextFontHeight { get; set; }

        public int SelectionStart { get; set; }
        public int SelectionLength { get; set; }
        public bool Equals(LastPaintingDataText second) {
            if (second == null) return false;

            if (LastPaintPosX != second.LastPaintPosX) return false;
            if (LastPaintPosY != second.LastPaintPosY) return false;
            if (LastPaintLimitRight != second.LastPaintLimitRight) return false;
            if (LastPaintContent != second.LastPaintContent) return false;
            if (LastPaintTextFontHeight != second.LastPaintTextFontHeight) return false;
            if (SelectionStart != second.SelectionStart) return false;
            if (SelectionLength != second.SelectionLength) return false;
            return true;
        }

    }
}

