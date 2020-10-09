namespace de.springwald.xml.editor.nativeplatform
{
    public interface IClipboard
    {
        bool ContainsText { get; }

        string GetText();
        void SetText(string inhalt);
        void Clear();
    }
}
