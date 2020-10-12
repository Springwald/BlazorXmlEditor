namespace de.springwald.xml.editor.editor.xmlelemente
{
    public class XmlElementPaintCacheData
    {
        public int PaintPosX { get; set; }

        public int PaintPosY { get; set; }

        public string Attributes { get; set; }

        public object Value { get; set; }

        public bool Changed(XmlElementPaintCacheData secondData)
        {
            if (secondData == null) return true;
            if (!secondData.PaintPosY.Equals(secondData.PaintPosY)) return true;
            if (!secondData.PaintPosX.Equals(secondData.PaintPosX)) return true;
            if (!secondData.Attributes.Equals(secondData.Attributes)) return true;
            if (!secondData.Value.Equals(secondData.Value)) return true;
            return false;
        }
    }
}
