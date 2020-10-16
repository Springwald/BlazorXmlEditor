using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.Code
{
    public class BrowserResize
    {
        public static event Func<Task> OnResize;

        [JSInvokable]
        public static async Task OnBrowserResize()
        {
            await OnResize?.Invoke();
        }
    }
}
