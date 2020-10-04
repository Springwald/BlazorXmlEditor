namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public abstract class GfxJob
    {
        public enum JobTypes
        {
            Clear,
            DrawLine,
            DrawPolygon,
            DrawRectangle,
            FillPolygon,
            FillRectangle,
            DrawString
        }

        public int Layer { get; set; }
        public bool Batchable { get; set; }
        public abstract JobTypes JobType { get; }
        public abstract string SortKey { get; }
    }
}
