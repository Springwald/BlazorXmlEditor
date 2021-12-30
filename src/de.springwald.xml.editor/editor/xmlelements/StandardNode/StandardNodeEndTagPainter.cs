// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.nativeplatform.gfx;
using de.springwald.xml.editor.xmlelements.StandardNode;
using System;
using System.Threading.Tasks;
using System.Xml;

namespace de.springwald.xml.editor.xmlelements
{
    internal class StandardNodeEndTagPainter : TagPainter
    {
        public StandardNodeEndTagPainter(EditorConfig config, StandardNodeDimensionsAndColor dimensions, XmlNode node, bool isEndTagVisible) : base(config, dimensions, node, isEndTagVisible)
        {
        }

        protected override async Task<PaintContext> PaintInternal(PaintContext paintContext, string attributesString, bool isSelected, IGraphics gfx)
        {
            var startX = paintContext.PaintPosX;

            var esteemedWidth = this.nodeNameTextWidth + this.dimensions.InnerMarginX * 3;
            if (paintContext.PaintPosX + esteemedWidth > paintContext.LimitRight)
            {
                paintContext.HeightActualRow = Math.Max(paintContext.HeightActualRow, config.MinLineHeight);
                paintContext.PaintPosX = paintContext.LimitLeft + this.config.ChildIndentX;
                paintContext.PaintPosY += paintContext.HeightActualRow;
                paintContext.HeightActualRow = config.MinLineHeight;
            }

            // draw an arrow to the left in front of the node frame
            // arrow to left
            Point point1 = new Point(paintContext.PaintPosX + this.dimensions.InnerMarginX, paintContext.PaintPosY + this.config.InnerMarginY);
            Point point2 = new Point(paintContext.PaintPosX + this.dimensions.InnerMarginX, paintContext.PaintPosY + this.config.TagHeight - this.config.InnerMarginY);
            Point point3 = new Point(paintContext.PaintPosX, paintContext.PaintPosY + this.config.TagHeight / 2);
            gfx.AddJob(new JobDrawPolygon
            {
                Batchable = true,
                Layer = GfxJob.Layers.TagBorder,
                FillColor = isSelected ? this.config.ColorNodeTagBorder.InvertedColor : this.config.ColorNodeTagBorder,
                Points = new[] { point1, point2, point3 }
            });

            // Remember the right arrow area
            this.AreaArrow = new Rectangle(paintContext.PaintPosX, paintContext.PaintPosY, this.dimensions.InnerMarginX, this.config.TagHeight);
            paintContext.PaintPosX += this.dimensions.InnerMarginX + 1; // Place drawing cursor behind the arrow

            // ## Draw frame for closing node ###
            StandardNodeTagPaint.DrawNodeBodyBySize(GfxJob.Layers.TagBackground,
                paintContext.PaintPosX, paintContext.PaintPosY,
                this.nodeNameTextWidth + this.dimensions.InnerMarginX * 2, this.config.TagHeight, this.dimensions.CornerRadius,
                isSelected ? this.dimensions.BackgroundColor.InvertedColor : this.dimensions.BackgroundColor,
                isSelected ? this.config.ColorNodeTagBorder.InvertedColor : this.config.ColorNodeTagBorder,
                gfx);

            paintContext.PaintPosX += this.dimensions.InnerMarginX; // Distance between frame and font

            //  ## Draw name for closing node ###
            gfx.AddJob(new JobDrawString
            {
                Batchable = false,
                Layer = GfxJob.Layers.Text,
                Text = this.node.Name,
                Color = isSelected ? this.config.ColorText.InvertedColor : this.config.ColorText,
                X = paintContext.PaintPosX,
                Y = paintContext.PaintPosY + this.config.InnerMarginY,
                Font = this.config.FontNodeName
            });

            paintContext.PaintPosX += this.nodeNameTextWidth + this.dimensions.InnerMarginX; // Distance between font and frame

            // One pixel to the right, because otherwise we draw on the frame line and the cursor flashes on the frame
            paintContext.PaintPosX++;

            // Remember where the mouse areas are
            this.AreaTag = new Rectangle(startX, paintContext.PaintPosY, paintContext.PaintPosX - startX, this.config.TagHeight);

            paintContext.FoundMaxX = System.Math.Max(paintContext.FoundMaxX, paintContext.PaintPosX);

            this.lastPaintContextResult = paintContext.Clone();
            await Task.CompletedTask;
            return paintContext;
        }

        protected override string GetAttributesString() => string.Empty;
    }
}
