using de.springwald.xml.editor.nativeplatform.gfx;
using System;

namespace de.springwald.xml.editor.nativeplatform
{
    public interface IControlElement
    {
        int Width { get; }
        bool Enabled { get; set; }
        bool Focused { get; }
        Color BackColor { get; }

        /// <summary>
        /// Repaint native control
        /// </summary>
        AsyncEvent<EventArgs> Invalidated { get; }

    }
}
