using System;
using System.Windows.Forms;

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

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
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
