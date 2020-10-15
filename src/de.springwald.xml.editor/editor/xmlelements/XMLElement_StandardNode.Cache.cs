using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.editor
{
    public partial class XMLElement_StandardNode
    {
        private int lastPaintPosY;
        private int lastPaintPosX;
        private int lastPaintLimitRight;
        private string lastPaintContent;
        private int lastPaintTextFontHeight;
        private string lastAttributeString;


        protected override bool LastPaintStillUpToDate(PaintContext paintContext)
        {
            if (paintContext.PaintPosY != this.lastPaintPosY) return false;
            if (paintContext.PaintPosX != this.lastPaintPosX) return false;
            if (paintContext.LimitRight != this.lastPaintLimitRight) return false;
            if (this.lastAttributeString != this.GetAttributeString()) return false;
            if (this.Config.TextNodeFont.Height != this.lastPaintTextFontHeight) return false;
            return true;
        }

        private void SaveLastPaintPosCacheAttributes(PaintContext paintContext)
        {
            this.lastPaintPosY = paintContext.PaintPosY;
            this.lastPaintPosX = paintContext.PaintPosX;
            this.lastPaintLimitRight = paintContext.LimitRight;
            this.lastAttributeString = this.GetAttributeString();
            this.lastPaintTextFontHeight = this.Config.TextNodeFont.Height;
        }
    }
}
