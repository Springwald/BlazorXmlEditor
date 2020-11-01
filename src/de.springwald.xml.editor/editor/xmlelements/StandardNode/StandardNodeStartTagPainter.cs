// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Text;
using System.Threading.Tasks;
using System.Xml;
using de.springwald.xml.editor.xmlelements.StandardNode;
using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor.xmlelements
{
    internal class StandardNodeStartTagPainter : TagPainter
    {
        public StandardNodeStartTagPainter(EditorConfig config, StandardNodeDimensionsAndColor dimensions, XmlNode node, bool isClosingTagVisible): base(config, dimensions, node, isClosingTagVisible)
        {
        }

        protected override async Task<PaintContext> PaintInternal(PaintContext paintContext, string attributesString,  bool isSelected, IGraphics gfx)
        {
            var startX = paintContext.PaintPosX;
            var startY = paintContext.PaintPosY;

            paintContext.PaintPosX += this.dimensions.InnerMarginX;  // margin to left border

            // draw node name
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

            paintContext.PaintPosX += nodeNameTextWidth + this.dimensions.InnerMarginX;

            // draw the attributes
            paintContext = await this.PaintAttributes(paintContext, attributesString, isSelected, gfx);

            // standard distance + one pixel to the right, otherwise we draw on the frame line
            paintContext.PaintPosX += 1;

            var borderWidth = paintContext.PaintPosX - startX;

            StandardNodeTagPaint.DrawNodeBodyBySize(
                GfxJob.Layers.TagBackground,
                startX, paintContext.PaintPosY,
                borderWidth, this.config.TagHeight,
                this.dimensions.CornerRadius,
                isSelected ? this.dimensions.BackgroundColor.InvertedColor : this.dimensions.BackgroundColor,
                isSelected ? this.config.ColorNodeTagBorder.InvertedColor : this.config.ColorNodeTagBorder,
                gfx);

            // if necessary draw the continuing arrow at the end of the frame 
            if (this.isClosingTagVisible)
            {
                var point1 = new Point(paintContext.PaintPosX, paintContext.PaintPosY + this.config.InnerMarginY);
                var point2 = new Point(paintContext.PaintPosX + this.dimensions.InnerMarginX, paintContext.PaintPosY + this.config.TagHeight / 2);
                var point3 = new Point(paintContext.PaintPosX, paintContext.PaintPosY + this.config.TagHeight - this.config.InnerMarginY);
                gfx.AddJob(new JobDrawPolygon
                {
                    Batchable = true,
                    Layer = GfxJob.Layers.TagBorder,
                    FillColor = isSelected ? this.config.ColorNodeTagBorder.InvertedColor : this.config.ColorNodeTagBorder,
                    Points = new[] { point1, point2, point3 }
                });

                // Remember the right arrow area
                this.AreaArrow = new Rectangle(paintContext.PaintPosX, paintContext.PaintPosY, this.dimensions.InnerMarginX, this.config.TagHeight);
                paintContext.PaintPosX += this.dimensions.InnerMarginX;
            }
            else
            {
                this.AreaArrow = null;
            }

            paintContext.HeightActualRow = System.Math.Max(paintContext.HeightActualRow, this.config.MinLineHeight); // See how high the current line is

            // Remember where the mouse areas are
            this.AreaTag = new Rectangle(startX-2, startY, paintContext.PaintPosX + 2 - (startX), this.config.TagHeight);

            paintContext.FoundMaxX = System.Math.Max(paintContext.FoundMaxX, paintContext.PaintPosX);

            this.lastPaintContextResult = paintContext.Clone();
            return paintContext.Clone();
        }

        protected override string GetAttributesString()
        {
            var attributes = this.node.Attributes; 
            if (attributes == null || attributes.Count == 0) return null;
            var attributeString = new StringBuilder();
            for (int i = 0; i < attributes.Count; i++)
            {
                attributeString.AppendFormat($" {attributes[i].Name}=\"{attributes[i].Value}\"");
            }
            return attributeString.ToString();
        }

      

        private async Task<int> GetAttributeTextWidth(string attributesString, IGraphics gfx)
        {
            if (string.IsNullOrEmpty(attributesString)) return 0;
            return (int)await gfx.MeasureDisplayStringWidthAsync(attributesString, this.config.FontNodeAttribute);
        }

        /// <summary>
        /// Draws the attributes 
        /// </summary>
        private async Task<PaintContext> PaintAttributes(PaintContext paintContext, string attributesString, bool isSelected, IGraphics gfx)
        {
            paintContext = paintContext.Clone();

            if (string.IsNullOrWhiteSpace(attributesString)) return paintContext;

            var attributeWidth = await this.GetAttributeTextWidth(attributesString, gfx);

            // draw a frame around the attributes
            StandardNodeTagPaint.DrawNodeBodyBySize(GfxJob.Layers.AttributeBackground,
                paintContext.PaintPosX, paintContext.PaintPosY + this.dimensions.AttributeMarginY,
                attributeWidth, this.dimensions.AttributeHeight, 2,
                isSelected ? this.config.ColorNodeAttributeBackground.InvertedColor : this.config.ColorNodeAttributeBackground,
                isSelected ? this.config.ColorNodeTagBorder.InvertedColor : this.config.ColorNodeTagBorder,
                gfx);

            // Draw attributes
            gfx.AddJob(new JobDrawString
            {
                Batchable = false,
                Layer = GfxJob.Layers.Text,
                Text = attributesString.ToString(),
                Color = isSelected ? this.config.ColorText.InvertedColor : this.config.ColorText,
                X = paintContext.PaintPosX,
                Y = paintContext.PaintPosY + this.dimensions.AttributeMarginY + this.dimensions.AttributeInnerMarginY + 1,
                Font = config.FontNodeAttribute
            });

            // Set character cursor behind the attributes
            paintContext.PaintPosX += attributeWidth + this.dimensions.InnerMarginX;

            return paintContext;
        }
    }
}
