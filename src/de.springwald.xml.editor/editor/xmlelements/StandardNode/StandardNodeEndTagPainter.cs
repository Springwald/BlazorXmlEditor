using de.springwald.xml.editor.editor.xmlelements.StandardNode;
using de.springwald.xml.editor.nativeplatform.gfx;
using System.Threading.Tasks;
using System.Xml;

namespace de.springwald.xml.editor.editor.xmlelements
{
    internal class StandardNodeEndTagPainter
    {
        private EditorConfig config;
        private StandardNodeDimensionsAndColor dimensions;
        private XmlNode node;

        private int lastPaintX;
        private int lastPaintY;
        private bool lastPaintWasSelected;
        private PaintContext lastPaintContextResult;

        private int nodeNameTextWidth;
        private int lastTextWidthFontHeight;

        public Rectangle AreaTag { get; private set; }
        public Rectangle AreaArrow { get; private set; }

        public StandardNodeEndTagPainter(EditorConfig config, StandardNodeDimensionsAndColor dimensions, XmlNode node)
        {
            this.config = config;
            this.dimensions = dimensions;
            this.node = node;
        }

        public async Task<PaintContext> Paint(PaintContext paintContext, bool alreadyUnpainted, bool isSelected, IGraphics gfx)
        {
            var startX = paintContext.PaintPosX;
            var startY = paintContext.PaintPosY;

            if (alreadyUnpainted) this.lastPaintContextResult = null;

            // Die Breite vorausberechnen
            if (this.lastTextWidthFontHeight != this.config.FontNodeName.Height)
            {
                this.nodeNameTextWidth = (int)(await gfx.MeasureDisplayStringWidthAsync(this.node.Name, this.config.FontNodeName));
                this.lastTextWidthFontHeight = this.config.FontNodeName.Height;
                this.lastPaintContextResult = null;
            }

            if (lastPaintContextResult != null &&
                this.lastPaintX == startX &&
                this.lastPaintY == startY &&
                this.lastPaintWasSelected == isSelected)
            {
                return lastPaintContextResult.Clone();
            }

            this.lastPaintX = startX;
            this.lastPaintY = startY;
            this.lastPaintWasSelected = isSelected;

            if (!alreadyUnpainted) this.Unpaint(gfx);

            var esteemedWidth = this.nodeNameTextWidth + this.dimensions.InnerMarginX * 3;
            if (paintContext.PaintPosX + esteemedWidth > paintContext.LimitRight && paintContext.PaintPosX != paintContext.LimitLeft)
            {
                paintContext.PaintPosX = paintContext.LimitLeft + this.config.ChildEinrueckungX;
                paintContext.PaintPosY += paintContext.HoeheAktZeile;
            }

            // vor dem Noderahmen einen Pfeil nach links zeichnen
            // Pfeil nach links
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

            // Den rechten Pfeilbereich merken
            this.AreaArrow = new Rectangle(paintContext.PaintPosX, paintContext.PaintPosY, this.dimensions.InnerMarginX, this.config.TagHeight);
            paintContext.PaintPosX += this.dimensions.InnerMarginX + 1; // Zeichnungscursor hinter den Pfeil setzen

            // ## RAHMEN für schließenden Node  zeichnen ###
            StandardNodeTagPaint.DrawNodeBodyBySize(GfxJob.Layers.TagBackground,
                paintContext.PaintPosX, paintContext.PaintPosY,
                this.nodeNameTextWidth + this.dimensions.InnerMarginX * 2, this.config.TagHeight, this.dimensions.CornerRadius,
                isSelected ? this.dimensions.BackgroundColor.InvertedColor : this.dimensions.BackgroundColor,
                isSelected ? this.config.ColorNodeTagBorder.InvertedColor : this.config.ColorNodeTagBorder,
                gfx);

            paintContext.PaintPosX += this.dimensions.InnerMarginX; // Abstand zwischen Rahmen und Schrift

            // ## Name für schließenden Node zeichnen ###
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
            //this._klickBereiche = this._klickBereiche.Append(_tagBereichRechts).ToArray(); // original:  _klickBereiche.Add(_tagBereichRechts);

            paintContext.BisherMaxX = System.Math.Max(paintContext.BisherMaxX, paintContext.PaintPosX);

            this.lastPaintContextResult = paintContext.Clone();
            return paintContext;
        }



        public void Unpaint(IGraphics gfx)
        {
            gfx.UnPaintRectangle(this.AreaArrow);
            gfx.UnPaintRectangle(this.AreaTag);
            this.lastPaintContextResult = null;
        }
    }
}
