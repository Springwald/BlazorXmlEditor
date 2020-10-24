using CurrieTechnologies.Razor.Clipboard;
using System;
using System.Threading.Tasks;


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
            catch (Exception e)
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
            catch (Exception e)
            {
            }
        }
    }
}
