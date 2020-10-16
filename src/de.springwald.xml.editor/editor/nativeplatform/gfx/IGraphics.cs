// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public interface IGraphics
    {
        int Height { get; }
        int Width { get; }

        Task SetSize(int width, int height);

        void AddJob(GfxJob job);
        
        Task PaintJobs();

        Task<double> MeasureDisplayStringWidthAsync(string text, Font drawFont);
    }
}

