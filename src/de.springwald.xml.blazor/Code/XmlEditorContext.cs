using System.Threading.Tasks;
using System.Xml;

namespace de.springwald.xml.blazor
{
    public class XmlEditorContext
    {
        private XmlNode rootNode;

        public XMLRegelwerk XmlRules { get; set; }

        public XmlNode RootNode
        {
            get => this.rootNode;
        }

        public async Task SetRootNode(XmlNode rootNode)
        {
            if (this.rootNode != rootNode)
            {
                this.rootNode = rootNode;
                await this.RootNodeChanged.Trigger(rootNode);
            }
        }
        public XmlAsyncEvent<XmlNode> RootNodeChanged { get; set; } = new XmlAsyncEvent<XmlNode>();
    }
}
