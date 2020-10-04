namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public class JobDrawString : GfxJob
    {
        public override JobTypes JobType => JobTypes.DrawString;
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
        public Font Font { get; set; }
        public Color Color { get; set; }

        public override string SortKey => $"{this.Color}-{this.Font}";
    }
}

