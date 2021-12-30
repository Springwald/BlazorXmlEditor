// A platform independent tag-view-style graphical xml editor
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
        //int DesiredMaxWidth { get;  }
        // int ActualWidth { get; }
        // int ActualHeight { get;  }

        // Task SetActualSize(int actualWidth, int actualHeight);

        // Task SetDesiredSize(int desiredMaxWidth);

        void AddJob(GfxJob job);

        void UnPaintRectangle(Rectangle rectangle);
        
        Task PaintJobs(Color backgroundColor);

        void DeleteAllPaintJobs();

        Task<double> MeasureDisplayStringWidthAsync(string text, Font drawFont);
    }
}

