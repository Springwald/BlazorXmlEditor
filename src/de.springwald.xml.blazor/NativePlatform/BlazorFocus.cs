using de.springwald.xml.editor.nativeplatform;
using System.Threading.Tasks;

namespace de.springwald.xml.blazor.NativePlatform
{
    public class BlazorFocus : de.springwald.xml.editor.nativeplatform.IFocus
    {
        async Task IFocus.FokusAufEingabeFormularSetzen()
        {
            await Task.CompletedTask; // to prevent warning because of empty async method
        }
    }
}
