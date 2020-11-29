// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Threading.Tasks;
using CurrieTechnologies.Razor.Clipboard;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorClipboard : editor.nativeplatform.IClipboard
    {
        private ClipboardService clipboard;

        public BlazorClipboard(ClipboardService clipboard)
        {
            this.clipboard = clipboard;
        }

        public async Task<bool> ContainsText()
        {
            return !string.IsNullOrEmpty(await this.GetText());
        }

        public async Task Clear()
        {
            await this.SetText(string.Empty);
        }

        public async Task<string> GetText()
        {
            try
            {
                return await this.clipboard.ReadTextAsync();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task SetText(string text)
        {
            try
            {
                await this.clipboard.WriteTextAsync(text);
            }
            catch (Exception)
            {
            }
        }
    }
}
