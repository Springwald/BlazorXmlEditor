using de.springwald.xml.cursor;
using de.springwald.xml.editor.editor.xmlelements.StandardNode;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace de.springwald.xml.editor.editor.xmlelements
{
    internal class StandardNodeStartTagPainter
    {
        private XMLEditor editor;
        private EditorConfig config;
        private StandardNodeDimensionsAndColor dimensions;
        private XmlNode node;
        private bool isClosingTagVisible;
        private string lastAttributeString;

        public Rectangle AreaTag { get; private set; }
        public Rectangle AreaArrow { get; private set; }

        public StandardNodeStartTagPainter(XMLEditor editor, StandardNodeDimensionsAndColor dimensions, System.Xml.XmlNode node, bool isClosingTagVisible)
        {
            this.editor = editor;
            this.config = editor.EditorConfig;
            this.dimensions = dimensions;
            this.node = node;
            this.isClosingTagVisible = isClosingTagVisible;
          
        }

        public async Task<PaintContext> Paint(PaintContext paintContext, XMLCursor cursor,  bool isSelected, IGraphics gfx)
        {
            var startX = paintContext.PaintPosX;
            var startY = paintContext.PaintPosY;

            // ### Write the name of the node ###

            // Pre-calculate the width of the node name

            int nodeNameTextWidth = (int)(await gfx.MeasureDisplayStringWidthAsync(this.node.Name, this.config.FontNodeName));
            //
            // Console.WriteLine(">>>>>>>>>>>>>>" + this.XMLNode.Name + ":" + xmlEditor.EditorConfig.NodeNameFont.Height + "=" + nodeNameTextWidth);

            // Pre-calculate the width of the attribute string
            var attributeString = this.GetAttributeString();
            this.lastAttributeString = attributeString;

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
            paintContext = await AttributeZeichnen(paintContext, attributeString, isSelected, gfx);

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



            paintContext.HoeheAktZeile = System.Math.Max(paintContext.HoeheAktZeile, this.config.MinLineHeight); // See how high the current line is

            // Remember where the mouse areas are
            this.AreaTag = new Rectangle(startX, startY, paintContext.PaintPosX - startX, this.config.TagHeight);

            // this._klickBereiche = this._klickBereiche.Append(_tagBereichLinks).ToArray();

 
            paintContext.BisherMaxX = System.Math.Max(paintContext.BisherMaxX, paintContext.PaintPosX);

            return paintContext;
        }

        public void Unpaint(IGraphics gfx )
        {
            gfx.UnPaintRectangle(this.AreaArrow);
            gfx.UnPaintRectangle(this.AreaTag);
        }

        /// <summary>
        /// Draws the attributes 
        /// </summary>
        private async Task<PaintContext> AttributeZeichnen(PaintContext paintContext, string attributeString, bool isSelected, IGraphics gfx)
        {
            paintContext = paintContext.Clone();

            if (string.IsNullOrWhiteSpace(attributeString)) return paintContext;

            var attributeBreite = await this.GetAttributeTextWidth(attributeString, gfx);

            // draw a frame around the attributes
            StandardNodeTagPaint.DrawNodeBodyBySize(GfxJob.Layers.AttributeBackground,
                paintContext.PaintPosX, paintContext.PaintPosY + this.dimensions.AttributeMarginY,
                attributeBreite, this.dimensions.AttributeHeight, 2,
                isSelected ? this.config.ColorNodeAttributeBackground.InvertedColor : this.config.ColorNodeAttributeBackground,
                isSelected ? this.config.ColorNodeTagBorder.InvertedColor : this.config.ColorNodeTagBorder,
                gfx);

            // Draw attributes
            gfx.AddJob(new JobDrawString
            {
                Batchable = false,
                Layer = GfxJob.Layers.Text,
                Text = attributeString.ToString(),
                Color = isSelected ? this.config.ColorText.InvertedColor : this.config.ColorText,
                X = paintContext.PaintPosX,
                Y = paintContext.PaintPosY + this.dimensions.AttributeMarginY + this.dimensions.AttributeInnerMarginY + 1,
                Font = config.FontNodeAttribute
            });

            // Set character cursor behind the attributes
            paintContext.PaintPosX += attributeBreite + this.dimensions.InnerMarginX;
            return paintContext;
        }

        private async Task<int> GetAttributeTextWidth(string attributeString, IGraphics gfx)
        {
            if (string.IsNullOrEmpty(attributeString)) return 0;
            return (int)await gfx.MeasureDisplayStringWidthAsync(attributeString, this.config.FontNodeAttribute);
        }

        private string GetAttributeString()
        {
            System.Xml.XmlAttributeCollection attribute = this.node.Attributes; // Attribs auf Kurznamen umlegen
            if (attribute == null) return null;
            if (attribute.Count == 0) return null;

            StringBuilder attributeString = new StringBuilder();
            for (int attribLauf = 0; attribLauf < attribute.Count; attribLauf++)
            {
                attributeString.AppendFormat(" {0}=\"{1}\"", attribute[attribLauf].Name, attribute[attribLauf].Value);
            }
            return attributeString.ToString();
        }
    }
}
