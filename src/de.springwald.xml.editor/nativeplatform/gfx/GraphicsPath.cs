// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Collections.Generic;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class GraphicsPath
    {
        public struct Line
        {
            public int X1;
            public int Y1;
            public int X2;
            public int Y2;
        }

        public List<Line> Lines { get; } = new List<Line>();

        public void AddLine(int x1, int y1, int x2, int y2)
        {
            this.Lines.Add(new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2 });
        }

        public void CloseFigure()
        {
            if (this.Lines.Count < 2) return;
            var first = this.Lines[0];
            var last = this.Lines[this.Lines.Count - 1];
            this.AddLine(last.X2, last.Y2, first.X1, first.Y1);
        }
    }
}

