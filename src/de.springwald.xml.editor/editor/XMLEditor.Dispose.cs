using System;

namespace de.springwald.xml.editor
{
    public partial class XMLEditor
    {

        #region EVENTS

        /// <summary>
        /// allen XML-Elementen Bescheid sagen, dass Sie sich aufräumen
        /// </summary>
        public event EventHandler xmlElementeAufraeumenEvent;
        protected virtual void xmlElementeAufraeumen()
        {
            if (xmlElementeAufraeumenEvent != null) xmlElementeAufraeumenEvent(this, EventArgs.Empty);
        }

        #endregion	

        #region PRIVATE ATTRIBUTES

        private bool _disposed;

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            if (!_disposed)
            {
                xmlElementeAufraeumen();
                _disposed = true;
            }
        }

        #endregion
    }
}
