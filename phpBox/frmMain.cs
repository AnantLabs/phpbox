#region usings
using System;
using System.IO;
using System.Windows.Forms;
#endregion
namespace phpBox
{
    public partial class frmMain : Form
    {
        #region WinAPI
        
        public static bool IsKeyPushedDown(Keys vKey)
        {
            return 0 != (Win32.GetAsyncKeyState((int)vKey) & 0x8000);
        }
        #endregion

        #region Fields

        public string ScriptPath
        {
            set
            {
                
                txtFilePath.Text = value;
            }
            get
            {
                return txtFilePath.Text;
            }
        }
        public string PHPFile
        {
            set;
            get;
        }
        public string ScriptArguments
        {
            set
            {
                txtGetParameter.Text = value;
            }
            get
            {
                return txtGetParameter.Text;
            }
        }
        public string Status
        {
            set
            {
                lblStatus.Text = value;
            }

            get
            {
                return lblStatus.Text;
            }
        }

        public ScriptExecuter Executer { get; set; }
        public Updater AutoUpdater { get; set; }

        public frmMain()
        {
            InitializeComponent();
            this.Text = Application.ProductName;
            PHPFile = @"php.exe";

            Arguments.Initialize();
            string r, s, p;

            r = Arguments.GetValue("r", "runtime");
            s = Arguments.GetValue("s", "script");
            p = Arguments.GetValue("p", "parameter");

            if (!String.IsNullOrEmpty(Arguments.GetValue("h", "help"))) writeHelpView();

            if (!String.IsNullOrEmpty(r)) PHPFile = r;
            if (!String.IsNullOrEmpty(p)) ScriptArguments = p;
            if (!String.IsNullOrEmpty(s)) ScriptPath = s;

            Executer = new ScriptExecuter(PHPFile, ScriptPath, ScriptArguments);
            Executer.DataRecived += new ScriptExecuter.DataRecivedEventHandler(Executer_DataRecived);
            Executer.ScriptStopped += new ScriptExecuter.ScriptStoppedEventHandler(ScriptEnd);
            //Commands
            Executer.ChangeStatus +=new ScriptExecuter.CommandEventHandler(InvokeSetStatus);
            Executer.ChangeCaption += new ScriptExecuter.CommandEventHandler(InvokeSetCaption);

            Executer.ReportProgress += new ScriptExecuter.CommandEventHandler(InvokeSetProgress);

            Executer.SetLines += new ScriptExecuter.CommandEventHandler(InvokeSetHeight);

            Executer.ShowNotice += new ScriptExecuter.CommandEventHandler(InvokeShowNotice);
            Executer.ShowError += new ScriptExecuter.CommandEventHandler(InvokeShowError);

            AutoUpdater = new Updater(@"http://code.google.com/feeds/p/phpbox/downloads/basic/");
        }
        #endregion

        
        private void writeHelpView()
        {
            txtOutput.Text += "Usage: phpBox [-r <file>] [-s <file>] [-p \"<parameter>\"]\n";
            txtOutput.Text += "  -r <file>\t\tPHP runtime file with path (php.exe)\n";
            txtOutput.Text += "  -runtime <file>\n\n";

            txtOutput.Text += "  -s <file>\t\tPHP script file with path (*.php)\n";
            txtOutput.Text += "  -script <file>\n\n";

            txtOutput.Text += "  -p \"<parameter>\"\tPHP script parameter (name=value&namen=valuen)\n";
            txtOutput.Text += "  -parameter \"<parameter>\"\n\n";
        }

        private void btnTopMost_Click(object sender, EventArgs e)
        {
            if (this.TopMost)
            {
                btnTopMost.Checked = false;
                this.TopMost = false;
            }
            else
            {
                btnTopMost.Checked = true;
                this.TopMost = true;
            }
        }

        private void getFile(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = false;
                ofd.Title = Application.ProductName + " - File search";
                ofd.Filter = "PHP Files (*.php)|*.php|All Files (*.*)|*.*";
                ofd.FilterIndex = 0;
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ScriptPath = ofd.FileName;
                }
            }
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (btnExecute.Text == "Start")
            {
                StartScript();
            }
            else
            {
                StopScript();
            }
        }

        private void ExecuteScript()
        {

        }

        private void StartScript()
        {
            #region check_inizialized
            if (!File.Exists(ScriptPath))
            {
                if (!String.IsNullOrWhiteSpace(ScriptPath))
                    Call.Warning("Script file not found!");
                return;
            }

            if (!File.Exists(PHPFile))
            {
                Call.Warning("php.exe not found!");
                return;
            }
            #endregion

            SetCaption(Path.GetFileName(ScriptPath));

            Executer.ScriptFile = ScriptPath;
            Executer.ScriptArguments = ScriptArguments;
            setExecuteBtn();
            SetLogReadPropery(true);
            writeHeader();
            SetProgress("0");
            Executer.Start();
        }

        private void ScriptEnd(StopReason reason)
        {
            switch (reason)
            {
                case StopReason.Executed:
                case StopReason.Canceled:
                    writeFooter();
                    resetExecuteBtn();
                    InvokeSetLogReadPropery(false);
                    InvokeSetProgress("100");
                    if (Executer.Exit) Application.Exit();
                    break;
                case StopReason.Error:
                    resetExecuteBtn();
                    InvokeSetLogReadPropery(false);
                    InvokeSetProgress("100");
                    break;
            }
        }

        private void StopScript()
        {
            Executer.Stop();
            ScriptEnd(StopReason.Canceled);
        }

        private void writeHeader()
        {
            ClearLog();
            InvokeWriteLogLine("Runtime:\t" + PHPFile);
            InvokeWriteLogLine("Script:\t" + ScriptPath);
            InvokeWriteLogLine("Parameter:\t" + (ScriptArguments.Length > 0 ? ScriptArguments : "-"));
            InvokeWriteLogLine("Starttime:\t" + getTimeOfDay(DateTime.Now));
            InvokeWriteLogLine("────────── Shell Output ──────────");
        }

        private void writeFooter()
        {
            InvokeWriteLogLine("──────────────────────────────────");
            InvokeWriteLogLine("Endtime:\t" + getTimeOfDay(DateTime.Now));
        }

        private void setExecuteBtn()
        {
            btnExecute.Image = Icons.Stop;
            btnExecute.Text = "Stop";
        }

        private void resetExecuteBtn()
        {
            btnExecute.Image = Icons.Start;
            btnExecute.Text = "Start";
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About dlg = new About();
            dlg.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private string getTimeOfDay(DateTime tm)
        {
            string h = tm.TimeOfDay.Hours < 10 ? "0" + Convert.ToString(tm.TimeOfDay.Hours) :
                                                           Convert.ToString(tm.TimeOfDay.Hours);

            string m = tm.TimeOfDay.Minutes < 10 ? "0" + Convert.ToString(tm.TimeOfDay.Minutes) :
                                                           Convert.ToString(tm.TimeOfDay.Minutes);

            string s = tm.TimeOfDay.Seconds < 10 ? "0" + Convert.ToString(tm.TimeOfDay.Seconds) :
                                                           Convert.ToString(tm.TimeOfDay.Seconds);

            string ms;

            if (tm.TimeOfDay.Milliseconds < 100)
            {
                ms = tm.TimeOfDay.Milliseconds < 10 ? "00" + Convert.ToString(tm.TimeOfDay.Milliseconds) :
                                                                "0" + Convert.ToString(tm.TimeOfDay.Milliseconds);
            }
            else
            {
                ms = Convert.ToString(tm.TimeOfDay.Milliseconds);
            }

            return h + ":" + m + ":" + s + ":" + ms;
        }
        private string getTimeOfDay(TimeSpan tm)
        {
            string h = tm.Hours < 10 ? "0" + Convert.ToString(tm.Hours) :
                                                           Convert.ToString(tm.Hours);

            string m = tm.Minutes < 10 ? "0" + Convert.ToString(tm.Minutes) :
                                                           Convert.ToString(tm.Minutes);

            string s = tm.Seconds < 10 ? "0" + Convert.ToString(tm.Seconds) :
                                                           Convert.ToString(tm.Seconds);

            string ms;

            if (tm.Milliseconds < 100)
            {
                ms = tm.Milliseconds < 10 ? "00" + Convert.ToString(tm.Milliseconds) :
                                                                "0" + Convert.ToString(tm.Milliseconds);
            }
            else
            {
                ms = Convert.ToString(tm.Milliseconds);
            }

            return h + ":" + m + ":" + s + ":" + ms;
        }
        public string ByteLen2Str(long bytes)
        {
            double s = bytes;
            string[] format = new string[]
                  {
                      "{0} Bytes", "{0} KBytes",  
                      "{0} MBytes", "{0} GBytes", "{0} TBytes", "{0} PBytes", "{0} EBytes"
                  };

            int i = 0;

            while (i < format.Length && s >= 1024)
            {
                s = (long)(100 * s / 1024) / 100.0;
                i++;
            }
            return string.Format(format[i], s);
        }

        private void viewUpdater_Tick(object sender, EventArgs e)
        {
            if (Executer.IsExecuting)
            {
                lblExecTime.Text = getTimeOfDay(DateTime.Now.Subtract(Executer.StartTime));
            }

            if ((!Executer.IsStartable && btnExecute.Text == "Start") || (!Executer.IsStoppable && btnExecute.Text != "Start"))
            {
                btnExecute.Enabled = false;
            }
            else
            {
                btnExecute.Enabled = true;
            }

            if (!Executer.EditableParameter)
            {
                txtGetParameter.Enabled = false;
            }
            else
            {
                txtGetParameter.Enabled = true;
            }

            if (!Executer.ChangableScript)
            {
                txtFilePath.Enabled = false;
                btnGetFile.Enabled = false;
            }
            else
            {
                txtFilePath.Enabled = true;
                btnGetFile.Enabled = true;
            }



            try
            {
                if (Win32.GetForegroundWindow() == this.Handle)
                {
                    if (IsKeyPushedDown(Keys.F5))
                    {
                        if (Executer.IsExecuting) Executer.Stop();
                        setExecuteBtn();
                        StartScript();
                    }
                }
            }
            catch (Exception ex)
            {
                Call.Error(ex);
            }
        }

        private void frmMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                if (Executer.IsExecuting)
                {
                    Executer.Stop();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {
            if (txtOutput.ReadOnly || Executer.IsExecuting)
            {
                txtOutput.SelectionStart = txtOutput.TextLength;
                txtOutput.ScrollToCaret();
            }
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            btnFile.ShowDropDown();
        }

        private void Executer_DataRecived(object sender, ScriptData e)
        {
            if (e.Type == ScriptDataType.Error)
            {
                InvokeWriteLogLine("[ScriptError] " + e.Message);
            }
            else
            {
                InvokeWriteLogLine(e.Message);
            }
        }

        #region Delegates

        private void InvokeWriteLogLine(string Message)
        {
            this.BeginInvoke(new WriteLogLineHandle(WriteLogLine), Message);
        }
        public delegate void WriteLogLineHandle(string Message);
        private void WriteLogLine(string Message)
        {
            txtOutput.AppendText(Message + "\n");
        }

        private void ClearLog()
        {
            txtOutput.Text = "";
        }


        public delegate void ChangeBoolValue(bool value);
        public delegate void ChangeStringValue(string value);

        private void SetLogReadPropery(bool value)
        {
            txtOutput.ReadOnly = value;
        }
        private void InvokeSetLogReadPropery(bool value)
        {
            this.Invoke(new ChangeBoolValue(SetLogReadPropery), value);
        }

        
        private void SetStatus(string text)
        {
            Status = text;
        }
        private void InvokeSetStatus(string text)
        {
            this.Invoke(new ChangeStringValue(SetStatus), text);
        }

        private void ShowNotice(string msg)
        {
            WriteLogLine("[Notice] " + msg);
            MessageBox.Show(msg, "Notice", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
        private void InvokeShowNotice(string msg)
        {
            this.Invoke(new ChangeStringValue(ShowNotice), msg);  
        }

        private void ShowError(string msg)
        {
            WriteLogLine("[Error] " + msg);
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void InvokeShowError(string msg)
        {
            this.Invoke(new ChangeStringValue(ShowError), msg);
        }

        private void SetCaption(string text)
        {
            this.Text = text + " - " + Application.ProductName;
        }
        private void InvokeSetCaption(string text)
        {
            this.Invoke(new ChangeStringValue(SetCaption), text);
        }

        private void SetProgress(string value)
        {
            try
            {
                int val = Convert.ToInt32(value);
                pbProgress.Value = val;
                lblPercent.Text = value + "%";
            }
            catch (Exception ex)
            {
                Call.Error(ex);
            }
        }
        private void InvokeSetProgress(string value)
        {
            this.Invoke(new ChangeStringValue(SetProgress), value);
        }

        private void SetHeight(string lines)
        {
            this.Height = 300 + (Convert.ToInt32(lines) * 5);
        }
        private void InvokeSetHeight(string lines)
        {
            this.Invoke(new ChangeStringValue(SetHeight), lines);
        }

        #endregion

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Executer.IsStoppable && Executer.IsExecuting)
            {
                e.Cancel = true;
            }

            if (e.CloseReason == CloseReason.UserClosing)
            {

                if (Executer.IsExecuting)
                {
                    Executer.Stop();
                    e.Cancel = true;
                }
            }

            this.Hide();

            if (System.IO.File.Exists(Executer.ClearScriptFile))
            {
                ScriptExecuter sClean = new ScriptExecuter(PHPFile, Executer.ClearScriptFile, "");
                sClean.Start();
                while (sClean.IsExecuting)
                {
                    Application.DoEvents();
                }
            }

            if (AutoUpdater.NewUpdate)
            {
                AutoUpdater.StartUpdate();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ScriptPath))
            {
                StartScript();
            }

            AutoUpdater.Update();
        }

        private void txtFilePath_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            txtFilePath.Text = files[0];
        }

        private void txtFilePath_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Link;
        }
    }
}
