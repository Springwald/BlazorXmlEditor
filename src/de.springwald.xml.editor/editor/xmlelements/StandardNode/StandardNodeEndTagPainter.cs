using de.springwald.xml.cursor;
using de.springwald.xml.editor.editor.xmlelements.StandardNode;
using de.springwald.xml.editor.nativeplatform.gfx;
using System.Threading.Tasks;
using System.Xml;

namespace de.springwald.xml.editor.editor.xmlelements
{
    internal class StandardNodeEndTagPainter
    {
        private XMLEditor editor;
        private EditorConfig config;
        private StandardNodeDimensionsAndColor dimensions;
        private XmlNode node;

        public Rectangle AreaTag { get; private set; }
        public Rectangle AreaArrow { get; private set; }

        public StandardNodeEndTagPainter(XMLEditor editor, StandardNodeDimensionsAndColor dimensions, System.Xml.XmlNode node)
        {
            this.editor = editor;
            this.config = editor.EditorConfig;
            this.dimensions = dimensions;
            this.node = node;
        }

        public async Task<PaintContext> Paint(PaintContext paintContext, XMLCursor cursor, bool isSelected, IGraphics gfx)
        {
            // Falls der Cursor hinter dem letzten Child dieses Nodes steht, dann
            // den Cursor auch dahin zeichnen
            //if ((_xmlEditor.Cursor.AktNode == _xmlNode) && (_xmlEditor.Cursor.PosInNode == (int)XMLCursorPositionen.CursorHinterLetztemChild)) 
            //{
            //	e.Graphics.DrawLine (new Pen(Color.Black,1),_paintPos.PosX, _paintPos.PosY+2 ,_paintPos.PosX, _paintPos.PosY + 2 +_drawFontNodeName.Height);
            //}

            // Die Breite vorausberechnen
            int schriftBreite = (int)(await gfx.MeasureDisplayStringWidthAsync(this.node.Name, this.config.FontNodeName));

            var esteemedWidth = schriftBreite + this.dimensions.InnerMarginX * 3;
            if (paintContext.PaintPosX + esteemedWidth > paintContext.LimitRight && paintContext.PaintPosX != paintContext.LimitLeft)
            {
                paintContext.PaintPosX = paintContext.LimitLeft + this.config.ChildEinrueckungX;
                paintContext.PaintPosY += paintContext.HoeheAktZeile;
            }

            int startX = paintContext.PaintPosX;

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
                schriftBreite + this.dimensions.InnerMarginX * 2, this.config.TagHeight, this.dimensions.CornerRadius,
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

            paintContext.PaintPosX += schriftBreite + this.dimensions.InnerMarginX; // Distance between font and frame

            // One pixel to the right, because otherwise we draw on the frame line and the cursor flashes on the frame
            paintContext.PaintPosX++;

            // Remember where the mouse areas are
            this.AreaTag = new Rectangle(startX, paintContext.PaintPosY, paintContext.PaintPosX - startX, this.config.TagHeight);
            //this._klickBereiche = this._klickBereiche.Append(_tagBereichRechts).ToArray(); // original:  _klickBereiche.Add(_tagBereichRechts);


            paintContext.BisherMaxX = System.Math.Max(paintContext.BisherMaxX, paintContext.PaintPosX);
            return paintContext;
        }



        public void Unpaint(IGraphics gfx)
        {
            gfx.UnPaintRectangle(this.AreaArrow);
            gfx.UnPaintRectangle(this.AreaTag);
        }
    }
}
