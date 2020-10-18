using de.springwald.xml.editor.editor.xmlelements.Caching;
using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor
{
    public partial class XMLElement_TextNode
    {
        private LastPaintingDataText lastPaintData;
        private PaintContext lastPaintContextResult;

        private LastPaintingDataText CalculateActualPaintData(PaintContext paintContext, Point cursorPaintPos, int selectionStart, int selectionLength)
        {
            return new LastPaintingDataText
            {
                LastPaintPosY = paintContext.PaintPosY,
                LastPaintPosX = paintContext.PaintPosX,
                LastPaintLimitRight = paintContext.LimitRight,
                LastPaintContent = this.AktuellerInhalt,
                LastPaintTextFontHeight = this.Config.TextNodeFont.Height,
                SelectionStart = selectionStart,
                SelectionLength = selectionLength,
            };
        }
    }
}
