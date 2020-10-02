using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public abstract class GfxJob
    {
        public int Layer { get; set; }
        public bool Batchable { get; set; }
    }
}
