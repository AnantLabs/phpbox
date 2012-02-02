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

        public static string AppDirectory { get; private set; }

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
                AppDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\phpBox";
                if (!Directory.Exists(AppDirectory))
                {
                    Directory.CreateDirectory(AppDirectory);
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                _MainForm = new frmMain();
                Application.Run(MainForm);
        }
    }
}
