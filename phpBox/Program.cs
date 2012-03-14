using System;
using System.Windows.Forms;
using System.IO;

namespace phpBox
{
    static class Program
    {

        private static frmMain _MainForm;

        public static frmMain MainForm
        {
            get
            {
                return _MainForm;
            }
        }

        private static AppDomain myDomain = AppDomain.CurrentDomain;

        public static string AppDirectory { get; private set; }

        private static System.Reflection.Assembly myDomain_AssemblyResolve(object sender, ResolveEventArgs e)
        {
            if (e.Name.Contains("Microsoft.WindowsAPICodePack.Shell"))
            {
                return System.Reflection.Assembly.Load(Libs.Microsoft_WindowsAPICodePack_Shell);
            }

            if (e.Name.Contains("Microsoft.WindowsAPICodePack"))
            {
                return System.Reflection.Assembly.Load(Libs.Microsoft_WindowsAPICodePack);
            }
            
            return null;
        }

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            myDomain.AssemblyResolve += new ResolveEventHandler(myDomain_AssemblyResolve);

                AppDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\phpBox";
                if (!Directory.Exists(AppDirectory))
                {
                    Directory.CreateDirectory(AppDirectory);
                }

                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    _MainForm = new frmMain();
                    Application.Run(MainForm);
                }
                catch (Exception ex)
                {
                    Call.Error(ex);
                }
        }
    }
}
