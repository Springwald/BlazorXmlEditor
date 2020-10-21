﻿// A platform indepentend tag-view-style graphical xml editor
// https://github.com/Springwald/BlazorXmlEditor
//
// (C) 2020 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using de.springwald.xml.editor.cursor;

namespace de.springwald.xml
{
    public interface IXmlAsyncEvent<T>
    {
        void Add(XmlAsyncEvent<T>.Handler handler);
        void Remove(XmlAsyncEvent<T>.Handler handler);
    }

    public class XmlAsyncEvent<T> : IXmlAsyncEvent<T>
    {
        public delegate Task Handler(T data);

        private List<Handler> handlers = new List<Handler>();

        public void Add(Handler handler)
        {
            this.handlers.Add(handler);
        }

        public void Remove(Handler handler)
        {
            this.handlers.Remove(handler);
        }

        public async Task Trigger(T data)
        {
            var handlersArray = this.handlers.ToArray();
            foreach (var handler in handlersArray)
            {
                if (handler != null)
                {
                    await handler(data);
                }
            }
        }

        internal void Add(CursorBlink cursorBlink)
        {
            throw new NotImplementedException();
        }


        //public void expose() : IXmlAsyncEvent<T> {
        //    return this;
        //}
    }
}