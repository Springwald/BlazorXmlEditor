using System.Threading.Tasks;

namespace de.springwald.xml.editor.nativeplatform.gfx
{
    public interface IGfxJob
    {
        int Layer { get; set; }
        bool Batchable { get; set; }

        Task Paint(IGraphics gfx);
    }
}
