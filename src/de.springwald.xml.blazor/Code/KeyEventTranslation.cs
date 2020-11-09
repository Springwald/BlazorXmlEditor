// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using de.springwald.xml.events;
using Microsoft.AspNetCore.Components.Web;

namespace de.springwald.xml.blazor
{
    internal class KeyEventTranslation
    {
        public static KeyEventArgs Translate(KeyboardEventArgs e)
        {
            var args = new KeyEventArgs
            {
                CtrlKey = e.CtrlKey,
                AltKey = e.AltKey,
                ShiftKey = e.ShiftKey,
                Content = e.Key,
                Key = Keys.undefined
            };

            switch (e.Key)
            {
                // check control keys here to prevent checking ControlLeft vs. ControlRight etc.
                case "Control":
                case "Shift":
                case "Alt":
                case "F1":
                case "F2":
                case "F3":
                case "F4":
                case "F5":
                case "F6":
                case "F7":
                case "F8":
                case "F9":
                case "F10":
                case "F11":
                case "F12":
                case "Insert":
                case "PageUp":
                case "PageDown":
                case "End":
                case "Meta":
                case "AltGraph":
                case "NumLock":
                case "Delete":
                case "Dead":
                case "Escape":
                case "ContextMenu":
                    // skip these keydowns
                    return null;

                case "Home": args.Key = Keys.Home; break;
                case "Enter": args.Key = Keys.Enter; break;

                default:
                    // check other keys in detail by key code
                    switch (e.Code)
                    {
                        case "KeyA": args.Key = Keys.A; break;
                        case "KeyC": args.Key = Keys.C; break;
                        case "KeyS": args.Key = Keys.S; break;
                        case "KeyV": args.Key = Keys.V; break;
                        case "KeyX": args.Key = Keys.X; break;
                        case "KeyY": args.Key = Keys.Y; break;
                        case "KeyZ": args.Key = Keys.Z; break;

                        case "Backspace": args.Key = Keys.Back; break;
                        case "Delete": args.Key = Keys.Delete; break;
                        case "Escape": args.Key = Keys.Escape; break;

                        case "ArrowLeft": args.Key = Keys.Left; break;
                        case "ArrowRight": args.Key = Keys.Down; break;
                        case "ArrowUp": args.Key = Keys.Up; break;
                        case "ArrowDown": args.Key = Keys.Down; break;

                        case "Tab":
                            args.Key = Keys.Tab;
                            break;
                    }
                    break;
            }
            return args;
        }
    }
}
