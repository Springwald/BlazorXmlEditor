// A platform independent tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2022 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.editor.actions;
using de.springwald.xml.editor.nativeplatform;
using System;

namespace de.springwald.xml.editor
{
    public class EditorContext : IDisposable
    {
        public EditorContext(EditorConfig editorConfig)
        {
            this.EditorConfig = editorConfig;
            this.EditorState = new EditorState();
            this.Actions = new EditorActions(this);
        }

        public EditorContext(EditorConfig editorConfig, XmlRules xmlRules) : this(editorConfig)
        {
            this.XmlRules = xmlRules;
        }

        public INativePlatform NativePlatform { get; set; }

        public EditorConfig EditorConfig { get; }

        public EditorState EditorState { get; }

        public EditorActions Actions { get; }

        public XmlRules XmlRules { get; set; }

        public void Dispose()
        {
            this.EditorState.Dispose();
        }
    }
}
