using System.Collections;

namespace de.springwald.xml.editor
{
    /// <summary>
    /// Eine Liste von XML-Element-Gruppen zur Gruppierung der Anzeige
    /// </summary>
    public class XMLElementGruppenListe
    {
        private ArrayList _gruppen;

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



        public XMLElementGruppenListe()
        {
            _gruppen = new ArrayList();
        }




        public void Dispose()
        {
            _gruppen.Clear();
            _gruppen = null;
        }


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


    }
}
