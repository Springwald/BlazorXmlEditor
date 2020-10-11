using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.events
{
    public enum Keys
    {
        undefined,
        Enter,
        S,
        Left,
        A,
        X,
        C,
        V,
        Home,
        Z,
        Delete,
        Escape,
        Right,
        Tab,
        Back,
    }

    public class PreviewKeyDownEventArgs
    {
        public Keys KeyData { get; set; }

    }
}
