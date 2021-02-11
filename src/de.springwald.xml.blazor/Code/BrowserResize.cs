// A platform independent tag-view-style graphical XML editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2021 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.Code
{
    public class BrowserResize
    {
        public static XmlAsyncEvent<EventArgs> OnResize = new XmlAsyncEvent<EventArgs>();

        [JSInvokable]
        public static async Task OnBrowserResize()
        {
            await OnResize.Trigger(EventArgs.Empty);
        }
    }
}
