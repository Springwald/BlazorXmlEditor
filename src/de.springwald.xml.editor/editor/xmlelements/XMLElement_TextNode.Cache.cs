namespace de.springwald.xml.editor
{
    public partial class XMLElement_TextNode
    {
        private int lastPaintPosY;
        private int lastPaintPosX;
        private int lastPaintLimitRight;
        private string lastPaintContent;
        private int lastPaintTextFontHeight;

        protected override bool LastPaintStillUpToDate(PaintContext paintContext)
        {
            if (paintContext.PaintPosY != this.lastPaintPosY) return false;
            if (paintContext.PaintPosX != this.lastPaintPosX) return false;
            if (paintContext.LimitRight != this.lastPaintLimitRight) return false;
            if (this.AktuellerInhalt != this.lastPaintContent) return false;
            if (this.Config.TextNodeFont.Height != this.lastPaintTextFontHeight) return false;
            return true;
        }

        private void SaveLastPaintPosCacheAttributes(PaintContext paintContext)
        {
            this.lastPaintPosY =  paintContext.PaintPosY;
            this.lastPaintPosX = paintContext.PaintPosX;
            this.lastPaintLimitRight = paintContext.LimitRight;
            this.lastPaintContent = this.AktuellerInhalt;
            this.lastPaintTextFontHeight = this.Config.TextNodeFont.Height;
        }

    }
}
