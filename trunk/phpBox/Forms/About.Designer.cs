namespace phpBox
{
    partial class About
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.AboutView = new System.Windows.Forms.RichTextBox();
            this.viewAbout = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // AboutView
            // 
            this.AboutView.Location = new System.Drawing.Point(0, 0);
            this.AboutView.Name = "AboutView";
            this.AboutView.Size = new System.Drawing.Size(100, 96);
            this.AboutView.TabIndex = 2;
            this.AboutView.Text = "";
            // 
            // viewAbout
            // 
            this.viewAbout.AllowWebBrowserDrop = false;
            this.viewAbout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewAbout.Location = new System.Drawing.Point(0, 0);
            this.viewAbout.Margin = new System.Windows.Forms.Padding(0);
            this.viewAbout.MinimumSize = new System.Drawing.Size(20, 20);
            this.viewAbout.Name = "viewAbout";
            this.viewAbout.ScriptErrorsSuppressed = true;
            this.viewAbout.Size = new System.Drawing.Size(501, 382);
            this.viewAbout.TabIndex = 1;
            this.viewAbout.Url = new System.Uri("http://phpbox.googlecode.com/svn/extras/about.html", System.UriKind.Absolute);
            this.viewAbout.WebBrowserShortcutsEnabled = false;
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 382);
            this.Controls.Add(this.viewAbout);
            this.Controls.Add(this.AboutView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.About_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.About_KeyPress);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox AboutView;
        private System.Windows.Forms.WebBrowser viewAbout;


    }
}
