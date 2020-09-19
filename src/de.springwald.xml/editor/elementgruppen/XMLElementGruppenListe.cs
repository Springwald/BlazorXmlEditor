using System.Collections;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Eine Liste von XML-Element-Gruppen zur Gruppierung der Anzeige
    /// </summary>
    public class XMLElementGruppenListe
    {
        #region SYSTEM

        #endregion

        #region PRIVATE ATTRIBUTES

        private ArrayList _gruppen;

        #endregion

        #region PUBLIC ATTRIBUTES

        /// <summary>
        /// Die Anzahl Gruppen
        /// </summary>
        public int Count
        {
            get { return _gruppen.Count; }
        }

        /// <summary>
        /// Ein bestimmtes Suchattribut dieser Liste
        /// </summary>
        public XMLElementGruppe this[int index]
        {
            get
            {
                return (XMLElementGruppe)_gruppen[index];
            }
            set
            {
                _gruppen[index] = value;
            }
        }

        #endregion

        #region CONSTRUCTOR

        public XMLElementGruppenListe()
        {
            _gruppen = new ArrayList();
        }

        #endregion

        #region PUBLIC METHODS

        #region IDisposable Member

        public void Dispose()
        {
            _gruppen.Clear();
            _gruppen = null;
        }

        #endregion

        public void Add(XMLElementGruppe gruppe)
        {
            _gruppen.Add(gruppe);
        }

        /// <summary>
        /// Entfernt ein Attribut aus der Liste
        /// </summary>
        public void Remove(XMLElementGruppe gruppe)
        {
            _gruppen.Remove(gruppe);
        }

        #endregion

        #region PRIVATE METHODS

        #endregion
    }
}
