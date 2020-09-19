using System.Collections.Generic;
using System.Threading.Tasks;

namespace de.springwald.xml
{
    public interface IAsyncEvent<T>
    {
        void Add(AsyncEvent<T>.Handler handler);
        void Remove(AsyncEvent<T>.Handler handler);
    }

    public class AsyncEvent<T> : IAsyncEvent<T>
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

        //public void expose() : IAsyncEvent<T> {
        //    return this;
        //}
    }
}