using System;
using System.Threading.Tasks;

namespace de.springwald.xml.cursor
{
    /// <summary>
    /// Zusammenfassung für XmlCursorPos.
    /// </summary>
    public partial class XMLCursorPos
    {
        /// <summary>
        /// Event definieren, wenn sich der Cursor geändert hat
        /// </summary>
        public XmlAsyncEvent<EventArgs> PosChangedEvent { get; } = new XmlAsyncEvent<EventArgs>();


        private System.Xml.XmlNode _aktNode;        // Der XMLNode, welcher aktuell den Fokus hat
        private XMLCursorPositionen _posAmNode;     // Dort befindet sich der Cursor innerhalb oder außerhalb des fokusierten XMLNodes
        private int _posImTextnode;                 // Dort befindet sich der Cursor im Fließtext, wenn die Pos CursorInnerhalbDesTextNodes ist


        /// <summary>
        /// Auf diesem XML-Node liegt gerade der Fokus des XMLEditors
        /// </summary>
        public System.Xml.XmlNode AktNode
        {
            get { return _aktNode; }
            /*set 
			{ 
				if (_aktNode != value) 
				{
					_posAmNode = XMLCursorPositionen.CursorAufNodeSelbst;
					_aktNode = value;
					Changed (EventArgs.Empty); // Bescheid geben, dass nun der Cursor auf einen anderern Node zeigt
				}
			}*/
        }

        /// <summary>
        /// Dort befindet sich der Cursor im Fließtext, wenn dei Pos CursorInnerhalbDesTextNodes ist
        /// </summary>
        public int PosImTextnode
        {
            get
            {
                if (this._posAmNode != XMLCursorPositionen.CursorInnerhalbDesTextNodes)
                {
                    //throw new ApplicationException("PosImTextnode kann nicht abgefragt werden, wenn posAmNode = " + _posAmNode + " statt XMLCursorPositionen.CursorInnerhalbDesTextNodes!");
                }
                return _posImTextnode;
            }
            /*set 
			{ 
				if (_aktNode != null) // es ist ein AktNodeXML zugewiesen
				{
					if (this._posAmNode != XMLCursorPositionen.CursorInnerhalbDesTextNodes) 
					{
						//throw new ApplicationException("PosImTextnode kann nicht gesetzt werden, wenn posAmNode != XMLCursorPositionen.CursorInnerhalbDesTextNodes!");
					}
					if (_posImTextnode != value) 
					{

						if (value==0) // wenn der Cursor vor dem ersten Zeichen ist, dann ist der Cursor eigentlich auch vor dem Node
						{
							_posImTextnode = 0;
							this._posAmNode = XMLCursorPositionen.CursorVorDemNode;
						} 
						else 
						{
							if (value>=_aktNode.InnerText.Length ) // wenn der Cursor hinter dem letzten Zeichen ist, dann ist der Cursor eigentlich auch hinter dem Node
							{
								_posImTextnode = 0;
								this._posAmNode = XMLCursorPositionen.CursorHinterDemNode;
							} 
							else 
							{
								_posImTextnode = value; 
							}
						}
						Changed(EventArgs.Empty); // Bescheid geben, dass nun der Cursor auf einen anderen Teil des Nodes zeigt
						
					}
				} 
				else // Noch kein AktNode zugewiesen 
				{
					throw (new ApplicationException("AktNode=null; muss vor Zuweisung von PosInNode gesetzt sein. " + this.ToString() + ".set PosInNode"));
				}
			}*/
        }


        /// <summary>
        /// Dort befindet sich der Cursor innerhalb oder außerhalb des fokusierten XMLNodes
        /// </summary>
        public XMLCursorPositionen PosAmNode
        {
            get { return _posAmNode; }
            /*set 
			{ 
				if (_aktNode != null) // es ist ein AktNodeXML zugewiesen
				{
					if (_posAmNode != value) 
					{
						_posAmNode = value; 
						Changed(EventArgs.Empty); // Bescheid geben, dass nun der Cursor auf einen anderen Teil des Nodes zeigt
					}
				} 
				else // Noch kein AktNode zugewiesen 
				{
					throw (new ApplicationException("AktNode=null; muss vor Zuweisung von PosInNode gesetzt sein. " + this.ToString() + ".set PosInNode"));
				}
			}*/
        }


        public XMLCursorPos()
        {
            _aktNode = null;  // Kein Node angewählt
            _posAmNode = XMLCursorPositionen.CursorAufNodeSelbstVorderesTag;
            _posImTextnode = 0;
        }

        /// <summary>
        /// Prüft ob diese Position mit einer zweiten inhaltsgleich ist
        /// </summary>
        /// <param name="zweitePos"></param>
        public bool Equals(XMLCursorPos zweitePos)
        {
            if (this.AktNode != zweitePos.AktNode) return false;
            if (this.PosAmNode != zweitePos.PosAmNode) return false;
            if (this._posImTextnode != zweitePos._posImTextnode) return false;
            return true;
        }

        /// <summary>
        /// Erstellt eine Kopie des Cursors
        /// </summary>
        /// <returns></returns>
        public XMLCursorPos Clone()
        {
            XMLCursorPos klon = new XMLCursorPos();
            klon.CursorSetzenOhneChangeEvent(this._aktNode, this._posAmNode, this._posImTextnode);
            return klon;
        }

        /// <summary>
        /// Prüft, ob der angegebene Node hinter dieser CursorPosition liegt
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool LiegtNodeHinterDieserPos(System.Xml.XmlNode node)
        {
            return ToolboxXML.Node1LiegtVorNode2(_aktNode, node);
        }

        /// <summary>
        /// Prüft, ob der angegebene Node vor dieser CursorPosition liegt
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool LiegtNodeVorDieserPos(System.Xml.XmlNode node)
        {
            return ToolboxXML.Node1LiegtVorNode2(node, _aktNode);
        }

        /// <summary>
        /// Löst den Cursor-Changed-Event manuell aus
        /// </summary>
        public async Task ErzwingeChanged()
        {
            await this.PosChangedEvent.Trigger(EventArgs.Empty);
        }

    }
}
