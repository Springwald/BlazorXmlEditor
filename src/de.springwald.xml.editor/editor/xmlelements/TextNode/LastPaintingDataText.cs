// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Xml;
using de.springwald.xml.cursor;

namespace de.springwald.xml.editor.xmlelements.Caching
{
    internal class LastPaintingDataText
    {
        public int LastPaintPosX { get; set; }
        public int LastPaintPosY { get; set; }
        public int LastPaintLimitRight { get; set; }
        public string LastPaintContent { get; set; }
        public int LastPaintTextFontHeight { get; set; }
        public int SelectionStart { get; set; }
        public int SelectionLength { get; set; }
        public bool CursorInNode { get; private set; }
        public int CursorPosInNode { get; private set; }
        public bool CursorBlinkOn { get; private set; }

        public bool Equals(LastPaintingDataText second)
        {
            if (second == null) return false;
            if (LastPaintPosX != second.LastPaintPosX) return false;
            if (LastPaintPosY != second.LastPaintPosY) return false;
            if (LastPaintLimitRight != second.LastPaintLimitRight) return false;
            if (LastPaintContent != second.LastPaintContent) return false;
            if (LastPaintTextFontHeight != second.LastPaintTextFontHeight) return false;
            if (SelectionStart != second.SelectionStart) return false;
            if (SelectionLength != second.SelectionLength) return false;
            if (CursorInNode == true)
            {
                if (second.CursorInNode == false || second.CursorPosInNode != this.CursorPosInNode) return false;
                if (second.CursorBlinkOn != this.CursorBlinkOn) return false;
            }
            else
            {
                if (second.CursorInNode) return false;
            }
            return true;
        }

        public static LastPaintingDataText CalculateActualPaintData(PaintContext paintContext, bool cursorBlinkOn, XmlNode node, string actualText, int fontHeight, XmlCursor cursor, int selectionStart, int selectionLength)
        {
            return new LastPaintingDataText
            {
                LastPaintPosY = paintContext.PaintPosY,
                LastPaintPosX = paintContext.PaintPosX,
                LastPaintLimitRight = paintContext.LimitRight,
                LastPaintContent = actualText,
                LastPaintTextFontHeight = fontHeight,
                SelectionStart = selectionStart,
                SelectionLength = selectionLength,
                CursorInNode = cursor.StartPos.ActualNode == node,
                CursorPosInNode = cursor.StartPos.PosInTextNode,
                CursorBlinkOn = cursorBlinkOn
            };
        }
    }
}

