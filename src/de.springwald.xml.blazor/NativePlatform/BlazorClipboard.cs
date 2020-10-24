using System;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorClipboard : de.springwald.xml.editor.nativeplatform.IClipboard
    {
        public bool ContainsText => false;

        public void Clear()
        {
        }

        public string GetText()
        {
            return "";
        }

        public void SetText(string inhalt)
        {
        }
    }
}
