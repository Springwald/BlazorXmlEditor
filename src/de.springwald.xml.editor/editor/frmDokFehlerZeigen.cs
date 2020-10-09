//namespace de.springwald.xml.editor
//{
//    /// <summary>
//    /// Zusammenfassung für frmDokFehlerZeigen.
//    /// </summary>
//    public class frmDokFehlerZeigen : System.Windows.Forms.Form
//    {

//        #region SYSTEM

//        /// <summary>
//        /// Erforderliche Designervariable.
//        /// </summary>
//        private System.ComponentModel.Container components = null;

//        public frmDokFehlerZeigen()
//        {
//            InitializeComponent();
//        }

//        /// <summary>
//        /// Die verwendeten Ressourcen bereinigen.
//        /// </summary>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                if (components != null)
//                {
//                    components.Dispose();
//                }
//            }
//            base.Dispose(disposing);
//        }

//        #region Vom Windows Form-Designer generierter Code
//        /// <summary>
//        /// Erforderliche Methode für die Designerunterstützung. 
//        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDokFehlerZeigen));
//            this.ucXMLQuellCodeFehlerliste = new de.springwald.xml.editor.ucXMLQuellCodeFehlerliste();
//            this.ucXMLQuellcodeViewer = new de.springwald.xml.editor.ucXMLQuellcodeViewer();
//            this.splitter1 = new System.Windows.Forms.Splitter();
//            this.SuspendLayout();
//            // 
//            // ucXMLQuellCodeFehlerliste
//            // 
//            this.ucXMLQuellCodeFehlerliste.Dock = System.Windows.Forms.DockStyle.Left;
//            this.ucXMLQuellCodeFehlerliste.Location = new System.Drawing.Point(0, 0);
//            this.ucXMLQuellCodeFehlerliste.Name = "ucXMLQuellCodeFehlerliste";
//            this.ucXMLQuellCodeFehlerliste.Size = new System.Drawing.Size(352, 357);
//            this.ucXMLQuellCodeFehlerliste.TabIndex = 0;
//            // 
//            // ucXMLQuellcodeViewer
//            // 
//            this.ucXMLQuellcodeViewer.BackColor = System.Drawing.SystemColors.Control;
//            this.ucXMLQuellcodeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.ucXMLQuellcodeViewer.Location = new System.Drawing.Point(352, 0);
//            this.ucXMLQuellcodeViewer.Name = "ucXMLQuellcodeViewer";
//            this.ucXMLQuellcodeViewer.Size = new System.Drawing.Size(240, 357);
//            this.ucXMLQuellcodeViewer.TabIndex = 1;
//            // 
//            // splitter1
//            // 
//            this.splitter1.Location = new System.Drawing.Point(352, 0);
//            this.splitter1.Name = "splitter1";
//            this.splitter1.Size = new System.Drawing.Size(3, 357);
//            this.splitter1.TabIndex = 2;
//            this.splitter1.TabStop = false;
//            // 
//            // frmDokFehlerZeigen
//            // 
//            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
//            this.ClientSize = new System.Drawing.Size(592, 357);
//            this.Controls.Add(this.splitter1);
//            this.Controls.Add(this.ucXMLQuellcodeViewer);
//            this.Controls.Add(this.ucXMLQuellCodeFehlerliste);
//            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
//            this.Name = "frmDokFehlerZeigen";
//            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
//            this.Text = "frmDokFehlerZeigen";
//            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
//            this.Load += new System.EventHandler(this.frmDokFehlerZeigen_Load);
//            this.ResumeLayout(false);

//        }
//        #endregion

//        private de.springwald.xml.editor.ucXMLQuellcodeViewer ucXMLQuellcodeViewer;
//        private System.Windows.Forms.Splitter splitter1;
//        private de.springwald.xml.editor.ucXMLQuellCodeFehlerliste ucXMLQuellCodeFehlerliste;

//        #endregion

//        #region PRIVATE ATTRIBUTES

//        #endregion

//        #region PUBLIC ATTRIBUTES

//        #endregion

//        #region CONSTRUCTOR

//        private void frmDokFehlerZeigen_Load(object sender, System.EventArgs e)
//        {
//            this.Text = ResReader.Reader.GetString("QuellcodeFehleranzeige");
//        }
//        #endregion

//        #region PUBLIC METHODS

//        /// <summary>
//        /// Zeigt den Inhalt des Dokumentes und die dazugehörgen Fehlermeldungen an
//        /// </summary>
//        /// <param name="xMLQuellCodeAlsRTF"></param>
//        /// <param name="fehlerProtokollAlsText"></param>
//        public void Anzeigen(string xMLQuellCodeAlsRTF, string fehlerProtokollAlsText)
//        {
//            this.ucXMLQuellcodeViewer.XMLCodeAlsRTF = xMLQuellCodeAlsRTF;
//            this.ucXMLQuellCodeFehlerliste.FehlerProtokollAlsText = fehlerProtokollAlsText;
//        }

//        #endregion

//        #region PRIVATE METHODS

//        #endregion


//    }
//}
