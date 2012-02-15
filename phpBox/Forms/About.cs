using System;
using System.Reflection;
using System.Windows.Forms;

namespace phpBox
{
    partial class About : Form
    {
        public About()
        {
            InitializeComponent();

            if (!viewAbout.IsOffline)
            {
                viewAbout.Refresh();
            }

            this.Text = String.Format("Info über {0}", AssemblyTitle);
            while (viewAbout.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }
            
            viewAbout.DocumentText = viewAbout.DocumentText.Replace("{NAME}", AssemblyTitle).
                                                            Replace("{VERSION}", AssemblyVersion.Remove(AssemblyVersion.Length - 2, 2)).
                                                            Replace("{COPYRIGHT}", AssemblyCopyright).
                                                            Replace("{COMPANY}", AssemblyCompany);

            while (viewAbout.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }
            viewAbout.Navigating += new WebBrowserNavigatingEventHandler(viewAbout_Navigating);

            viewAbout.Document.GetElementById("closeAbout").Click += new HtmlElementEventHandler(Close_Click);
        }

        #region Assemblyattributaccessoren

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void About_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape) this.Close();
        }

        private void viewAbout_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            e.Cancel = true;

            System.Diagnostics.Process.Start(e.Url.AbsoluteUri);
        }

        private void About_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
        }

        private void Close_Click(object sender, HtmlElementEventArgs e)
        {
            this.Close();
        }
    }
}
