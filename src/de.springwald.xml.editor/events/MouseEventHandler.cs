using System;
using System.Collections.Generic;
using System.Text;

namespace de.springwald.xml.events
{
    public class MouseEventHandler
    {
        private Action<object, MouseEventArgs> xmlEditor_MouseDownEvent;

        public MouseEventHandler(Action<object, MouseEventArgs> xmlEditor_MouseDownEvent)
        {
            this.xmlEditor_MouseDownEvent = xmlEditor_MouseDownEvent;
        }
    }
}
