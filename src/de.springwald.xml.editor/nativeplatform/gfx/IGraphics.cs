// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public interface IGraphics
    {
        void AddJob(GfxJob job);

        void UnPaintRectangle(Rectangle rectangle);
        
        Task PaintJobs(Color backgroundColor);

        void DeleteAllPaintJobs();

        Task<double> MeasureDisplayStringWidthAsync(string text, Font drawFont);
    }
}

