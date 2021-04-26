// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
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

        protected PaintContext lastPaintContextResult;
        protected string lastAttributeString;
        protected int lastPaintY;
        protected int lastPaintX;
        protected bool lastPaintWasSelected;
        protected int lastTextWidthFontHeight;

        protected bool lastCursorBlinkOn;
        protected bool lastCursorWasOnThisNode;

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
            paintContext.PaintPosX += 1;

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

            var cursorChanged = false;
            if (cursorIsOnThisNode)
            {
                cursorChanged = cursorBlinkOn != this.lastCursorBlinkOn;
            }
            else
            {
                cursorChanged = cursorIsOnThisNode != this.lastCursorWasOnThisNode;
            }

            if (lastPaintContextResult != null &&
                this.lastPaintX == startX &&
                this.lastPaintY == startY &&
                this.lastPaintWasSelected == isSelected &&
                this.lastAttributeString == attributesString &&
                cursorChanged == false)
            {
                return lastPaintContextResult.Clone();
            }

            if (!alreadyUnpainted) this.Unpaint(gfx);

            this.lastPaintX = startX;
            this.lastPaintY = startY;
            this.lastPaintWasSelected = isSelected;
            this.lastAttributeString = attributesString;
            this.lastCursorBlinkOn = cursorBlinkOn;
            this.lastCursorWasOnThisNode = cursorIsOnThisNode;

            paintContext = await this.PaintInternal(paintContext, attributesString, isSelected, gfx);

            paintContext.HeightActualRow = Math.Max(paintContext.HeightActualRow, this.config.MinLineHeight); // See how high the current line is
            paintContext.FoundMaxX = Math.Max(paintContext.FoundMaxX, paintContext.PaintPosX);

            this.lastPaintContextResult = paintContext.Clone();
            return paintContext.Clone();
        }

        public void Unpaint(IGraphics gfx)
        {
            gfx.UnPaintRectangle(this.AreaTag); // (includes the arrow area)
            this.lastPaintContextResult = null;
        }

        protected abstract Task<PaintContext> PaintInternal(PaintContext paintContext, string attributesString, bool isSelected, IGraphics gfx);

        protected abstract string GetAttributesString();
    }
}
