
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.xmlelements.StandardNode;

namespace de.springwald.xml.editor.xmlelements
{
    internal abstract class TagPainter
    {
        protected EditorConfig config;
        protected StandardNodeDimensionsAndColor dimensions;
        protected XmlNode node;
        protected bool isClosingTagVisible;

        protected string lastAttributeString;
        protected PaintContext lastPaintContextResult;
        protected bool lastPaintWasSelected;
        protected int lastPaintY;
        protected int lastPaintX;
        protected bool lastCursorBlinkOn;
        protected bool lastCursorOnThisNode;
        protected int lastTextWidthFontHeight;

        protected int nodeNameTextWidth;
        public Rectangle AreaTag { get; protected set; }
        public Rectangle AreaArrow { get; protected set; }

        public TagPainter(EditorConfig config, StandardNodeDimensionsAndColor dimensions, XmlNode node, bool isClosingTagVisible)
        {
            this.config = config;
            this.dimensions = dimensions;
            this.node = node;
            this.isClosingTagVisible = isClosingTagVisible;
        }

        public async Task<PaintContext> Paint(PaintContext paintContext, bool cursorIsOnThisNode, bool cursorBlinkOn, bool alreadyUnpainted, bool isSelected, IGraphics gfx)
        {
            paintContext.PaintPosX += 3;

            var startX = paintContext.PaintPosX;
            var startY = paintContext.PaintPosY;
            var attributesString = this.GetAttributesString();

            if (alreadyUnpainted) this.lastPaintContextResult = null;

            if (this.lastTextWidthFontHeight != this.config.FontNodeName.Height)
            {
                this.nodeNameTextWidth = (int)(await gfx.MeasureDisplayStringWidthAsync(this.node.Name, this.config.FontNodeName));
                this.lastTextWidthFontHeight = this.config.FontNodeName.Height;
                lastPaintContextResult = null;
            }

            if (lastPaintContextResult != null &&
                this.lastPaintX == startX &&
                this.lastPaintY == startY &&
                this.lastPaintWasSelected == isSelected &&
                this.lastAttributeString == attributesString &&
                this.lastCursorBlinkOn == cursorBlinkOn)
            {
                return lastPaintContextResult.Clone();
            }

            if (!alreadyUnpainted) this.Unpaint(gfx);

            this.lastPaintX = startX;
            this.lastPaintY = startY;
            this.lastPaintWasSelected = isSelected;
            this.lastAttributeString = attributesString;
            this.lastCursorBlinkOn = cursorBlinkOn;

            paintContext.PaintPosX += this.dimensions.InnerMarginX;  // margin to left border

            paintContext = await this.PaintInternal(paintContext, attributesString, isSelected, gfx);

            paintContext.HeightActualRow = Math.Max(paintContext.HeightActualRow, this.config.MinLineHeight); // See how high the current line is
            paintContext.FoundMaxX = Math.Max(paintContext.FoundMaxX, paintContext.PaintPosX);

            this.lastPaintContextResult = paintContext.Clone();
            return paintContext.Clone();
        }

        public void Unpaint(IGraphics gfx)
        {
            gfx.UnPaintRectangle(this.AreaArrow);
            gfx.UnPaintRectangle(this.AreaTag);
            this.lastPaintContextResult = null;
        }

        protected abstract Task<PaintContext> PaintInternal(PaintContext paintContext, string attributesString, bool isSelected, IGraphics gfx);

        protected abstract string GetAttributesString();
    }
}
