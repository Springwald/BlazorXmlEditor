// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform
{
    public interface IClipboard
    {
        Task<bool> ContainsText();
        Task Clear();
        Task<string> GetText();
        Task SetText(string text);
    }
}
