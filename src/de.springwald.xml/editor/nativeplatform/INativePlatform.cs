using de.springwald.xml.editor.nativeplatform.events;
using de.springwald.xml.editor.nativeplatform.gfx;
using System;

namespace de.springwald.xml.editor.nativeplatform
{
    public interface INativePlatform
    {
        IClipboard Clipboard { get; }
        IControlElement ControlElement { get; }
        IInputEvents InputEvents { get; }
        IFocus Focus { get; }
        IGraphics Gfx { get; }

        void ProtokolliereFehler(string v);
    }
}
