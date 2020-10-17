//namespace de.springwald.xml.editor.editor.xmlelemente
//{
//    public class XmlElementPaintCacheData
//    {
//        public int PaintPosX { get; set; }

//        public int PaintPosY { get; set; }

//        public string Attributes { get; set; }

//        public object Value { get; set; }

//        public bool Changed(XmlElementPaintCacheData secondData)
//        {
//            if (secondData == null) return true;

//            if (!PaintPosY.Equals(secondData.PaintPosY)) return true;
//            if (!PaintPosX.Equals(secondData.PaintPosX)) return true;

//            if (Attributes == null)
//            {
//                if (secondData.Attributes != null) return true;
//            }
//            else
//            {
//                if (!Attributes.Equals(secondData.Attributes)) return true;
//            }

//            if (Value == null)
//            {
//                if (secondData.Value != null) return true;
//            }
//            else
//            {
//                if (!Value.Equals(secondData.Value)) return true;
//            }
//            return false;
//        }
//    }
//}
