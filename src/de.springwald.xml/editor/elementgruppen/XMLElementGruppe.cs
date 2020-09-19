using System.Collections;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Eine Gruppierung für XML-Elemente, damit diese in der Liste der angebotenen Elemente
    /// zum Einfügen gruppiert dargestellt werden können
    /// </summary>
    public class XMLElementGruppe
    {
        #region SYSTEM
        #endregion

        #region PRIVATE ATTRIBUTES

        /// <summary>Diese Elementnamen sind in der Gruppe zulässig</summary>
        private Hashtable _elemente;

        /// <summary>Der anzeigbare Titel dieser Gruppe</summary>
        private string _titel;

        /// <summary>Ist diese Gruppe beim Start erstmal zusammen geklappt?</summary>
        private bool _standardMaessigZusammengeklappt;

        #endregion

        #region PUBLIC ATTRIBUTES

        /// <summary>Der anzeigbare Titel dieser Gruppe</summary>
        public string Titel
        {
            get { return _titel; }
        }

        /// <summary>Ist diese Gruppe beim Start erstmal zusammen geklappt?</summary>
        public bool StandardMaessigZusammengeklappt
        {
            get { return _standardMaessigZusammengeklappt; }
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Stellt eine neue Instanz einer Sortierungs-Gruppe bereit
        /// </summary>
        /// <param name="name"></param>
        /// <param name="standardMaessigZusammengeklappt"> Ist diese Gruppe beim Start erstmal zusammen geklappt?</param>
        public XMLElementGruppe(string titel, bool standardMaessigZusammengeklappt)
        {
            _titel = titel;
            _elemente = new Hashtable();
            _standardMaessigZusammengeklappt = standardMaessigZusammengeklappt;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Nimmt ein Element in die Liste der in dieser Gruppe verfügbaren Elemente auf
        /// </summary>
        /// <param name="name"></param>
        public void AddElementName(string name)
        {
            _elemente.Add(name.ToLower(), null);
        }

        /// <summary>
        /// Ist ein Element in dieser Gruppe vorhanden?
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsElement(string name)
        {
            return _elemente.ContainsKey(name.ToLower());
        }

        #endregion

        #region PRIVATE METHODS
        #endregion
    }
}
