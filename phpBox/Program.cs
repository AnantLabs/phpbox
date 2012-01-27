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

                if (System.IO.File.Exists(MainForm.Executer.ClearScriptFile))
                {
                    ScriptExecuter sClean = new ScriptExecuter(MainForm.PHPFile, MainForm.Executer.ClearScriptFile, "");
                    sClean.Start();
                    while (sClean.IsExecuting)
                    {
                        Application.DoEvents();
                    }
                }
            }
            catch (Exception ex)
            {
                Call.Error(ex);
            }
        }
    }
}
