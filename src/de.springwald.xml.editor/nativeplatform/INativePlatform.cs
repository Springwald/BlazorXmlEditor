// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Threading.Tasks;
using de.springwald.xml.editor.nativeplatform.events;
using de.springwald.xml.editor.nativeplatform.gfx;

namespace de.springwald.xml.editor.nativeplatform
{
    public interface INativePlatform
    {
        Task SetSize(int width, int height);
        IClipboard Clipboard { get; }
        IControlElement ControlElement { get; }
        IInputEvents InputEvents { get; }
        IFocus Focus { get; }
        IGraphics Gfx { get; }
        void LogError(string v);
    }
}
