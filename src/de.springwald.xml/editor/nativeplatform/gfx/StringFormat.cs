namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public enum StringTrimming
    {
        None
    }

    public enum StringFormatFlags
    {
        MeasureTrailingSpaces
    }

    public class StringFormat
    {
        public static StringFormat GenericTypographic { get; internal set; } = new StringFormat { FormatFlags = StringFormatFlags.MeasureTrailingSpaces, Trimming = StringTrimming.None };

        public StringFormatFlags FormatFlags { get; internal set; }
        public StringTrimming Trimming { get; internal set; }

    public StringFormat Clone()
    {
        return new StringFormat
        {
            FormatFlags = this.FormatFlags,
            Trimming = this.Trimming
        };
    }
}
}
