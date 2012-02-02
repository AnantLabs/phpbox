#region usings
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
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

        private string file = Program.AppDirectory + @"\settings.ini";

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
        private string _PHPFile = @"php.exe";
        public string PHPFile
        {
            set
            {
                _PHPFile = value;
                IniFile.SetValue("PHPFile", value, "Last known php path...");
            }
            get
            {
                if (!File.Exists(_PHPFile))
                    _PHPFile = IniFile.GetValue("PHPFile");
                return _PHPFile;
            }
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
        public string RuntimeVersion 
        { 
            get 
            {
                Match mt = Regex.Match(File.ReadAllText(PHPFile), @"([0-9]+\.[0-9]+\.[0-9]+)");
                if (mt.Success)
                {
                    return mt.Groups[1].Value;
                }
                else return null;
            } 
        }


        public ScriptExecuter Executer { get; set; }
        public Updater AutoUpdater { get; set; }
        public IniParser IniFile { get; set; }

        public frmMain()
        {
            InitializeComponent();
            this.Text = Application.ProductName;

            IniFile = new IniParser(file);

            Arguments.Initialize();
            string r, s, p, d;

            r = Arguments.GetValue("r", "runtime");
            s = Arguments.GetValue("s", "script");
            p = Arguments.GetValue("p", "parameter");

            d = Arguments.GetValueOf(1);
            if (!String.IsNullOrEmpty(d) && !d.StartsWith("-") && String.IsNullOrEmpty(s)) s = d;
            

            if (!String.IsNullOrEmpty(Arguments.GetValue("h", "help"))) writeHelpView();

            if (!String.IsNullOrEmpty(r)) PHPFile = r;
            if (!String.IsNullOrEmpty(p)) ScriptArguments = p;
            if (!String.IsNullOrEmpty(s)) ScriptPath = s;

            Executer = new ScriptExecuter(PHPFile, ScriptPath, ScriptArguments);
            Executer.DataRecived += new ScriptExecuter.DataRecivedEventHandler(Executer_DataRecived);
            Executer.ScriptStopped += new ScriptExecuter.ScriptStoppedEventHandler(ScriptEnd);
            //Commands
            Executer.ChangeStatus_Old +=new ScriptExecuter.CommandEventHandler_Old(InvokeSetStatus_Old);
            Executer.ChangeCaption_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeSetCaption_Old);

            Executer.ReportProgress_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeSetProgress_Old);

            Executer.SetLines_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeSetHeight_Old);

            Executer.ShowNotice_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeShowNotice_Old);
            Executer.ShowError_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeShowError_Old);

            AutoUpdater = new Updater(@"http://code.google.com/feeds/p/phpbox/downloads/basic/");
            AutoUpdater.UpdateReport += new Updater.UpdateReportHandler(updateStatusChanged);
        }

        ~frmMain()
        {
        }
        #endregion

        
        private void writeHelpView()
        {
            txtOutput.Text += "Usage: phpBox [-r <file>] [-s <file>] [-p \"<parameter>\"]\n";
            txtOutput.Text += "Usage: phpBox <script_file>\n";
            txtOutput.Text += "  -r <file>\t\tPHP runtime file with path (php.exe)\n";
            txtOutput.Text += "  -runtime <file>\n\n";

            txtOutput.Text += "  -s <file>\t\tPHP script file with path (*.php)\n";
            txtOutput.Text += "  -script <file>\n\n";

            txtOutput.Text += "  -p \"<parameter>\"\tPHP script parameter (name=value&namen=valuen)\n";
            txtOutput.Text += "  -parameter \"<parameter>\"\n\n";
        }

        private void updateStatusChanged(UpdateStatus status)
        {
            switch (status)
            {
                case UpdateStatus.None: btnFile.Image = Icons.File; break;
                case UpdateStatus.Downloading: btnFile.Image = Icons.FileDownloading; break;
                case UpdateStatus.Ready: btnFile.Image = Icons.FileReady; break;
                case UpdateStatus.Failed: btnFile.Image = Icons.FileFailed; break;
            }
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
                if(String.IsNullOrWhiteSpace(ScriptPath))
                    getFile(this, new EventArgs());
                if (String.IsNullOrWhiteSpace(ScriptPath))
                    return;
            }

            if (!File.Exists(PHPFile))
            {
                if (String.IsNullOrWhiteSpace(PHPFile) || PHPFile == @"php.exe")
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Multiselect = false;
                        ofd.Title = Application.ProductName + " - php.exe search";
                        ofd.Filter = "PHP Executable (php.exe)|php.exe|All Files (*.*)|*.*";
                        ofd.FilterIndex = 0;
                        ofd.RestoreDirectory = true;
                        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            PHPFile = ofd.FileName;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            #endregion

            SetCaption_Old(Path.GetFileName(ScriptPath));

            Executer.PHPFile = PHPFile;
            Executer.ScriptFile = ScriptPath;
            Executer.ScriptArguments = ScriptArguments;
            setExecuteBtn();
            SetLogReadPropery_Old(true);
            writeHeader();
            SetProgress_Old("0");
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
                    InvokeSetLogReadPropery_Old(false);
                    InvokeSetProgress_Old("100");
                    if (Executer.Exit) Application.Exit();
                    break;
                case StopReason.Error:
                    resetExecuteBtn();
                    InvokeSetLogReadPropery_Old(false);
                    InvokeSetProgress_Old("100");
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
            Color clr = Color.Gray;
            InvokeWriteLogLine_Old("PHP Runtime:\t" + RuntimeVersion, clr);
            InvokeWriteLogLine_Old("Script:\t" + Path.GetFileName(ScriptPath), clr);
            InvokeWriteLogLine_Old("Parameter:\t" + (ScriptArguments.Length > 0 ? ScriptArguments : "-"), clr);
            InvokeWriteLogLine_Old("Starttime:\t" + getTimeOfDay(DateTime.Now), clr);
            InvokeWriteLogLine_Old("--------- Shell Output ---------", clr);
        }

        private void writeFooter()
        {
            Color clr = Color.Gray;
            InvokeWriteLogLine_Old("--------------------------------", clr);
            InvokeWriteLogLine_Old("Endtime:\t" + getTimeOfDay(DateTime.Now), clr);
            InvokeWriteLogLine_Old("\t\t——————————————————", clr);
            InvokeWriteLogLine_Old("\t\t" + lblExecTime.Text, clr);
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
                InvokeWriteLogLine_Old("[ScriptError] " + e.Message, Color.Red);
            }
            else
            {
                InvokeWriteLogLine_Old(e.Message, Color.Black);
            }
        }

        #region Delegates
        private void ClearLog()
        {
            txtOutput.Text = "";
        }

        #region Old
        private void InvokeWriteLogLine_Old(string Message, Color clr)
        {
            this.BeginInvoke(new WriteLogLineHandle_Old(WriteLogLine_Old), Message, clr);
        }
        public delegate void WriteLogLineHandle_Old(string Message, Color clr);
        private void WriteLogLine_Old(string Message, Color clr)
        {
            Color oclr = txtOutput.SelectionColor;
            txtOutput.SelectionColor = clr;
            txtOutput.AppendText(Message + "\n");
            txtOutput.SelectionColor = oclr;
        }

        


        public delegate void ChangeBoolValue_Old(bool value);
        public delegate void ChangeStringValue_Old(string value);

        private void SetLogReadPropery_Old(bool value)
        {
            txtOutput.ReadOnly = value;
        }
        private void InvokeSetLogReadPropery_Old(bool value)
        {
            this.Invoke(new ChangeBoolValue_Old(SetLogReadPropery_Old), value);
        }

        
        private void SetStatus_Old(string text)
        {
            Status = text;
        }
        private void InvokeSetStatus_Old(string text)
        {
            this.Invoke(new ChangeStringValue_Old(SetStatus_Old), text);
        }

        private void ShowNotice_Old(string msg)
        {
            WriteLogLine_Old("[Notice] " + msg, Color.Black);
            MessageBox.Show(msg, "Notice", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
        private void InvokeShowNotice_Old(string msg)
        {
            this.Invoke(new ChangeStringValue_Old(ShowNotice_Old), msg);  
        }

        private void ShowError_Old(string msg)
        {
            WriteLogLine_Old("[Error] " + msg, Color.Red);
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void InvokeShowError_Old(string msg)
        {
            this.Invoke(new ChangeStringValue_Old(ShowError_Old), msg);
        }

        private void SetCaption_Old(string text)
        {
            this.Text = text + " - " + Application.ProductName;
        }
        private void InvokeSetCaption_Old(string text)
        {
            this.Invoke(new ChangeStringValue_Old(SetCaption_Old), text);
        }

        private void SetProgress_Old(string value)
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
        private void InvokeSetProgress_Old(string value)
        {
            this.Invoke(new ChangeStringValue_Old(SetProgress_Old), value);
        }

        private void SetHeight_Old(string lines)
        {
            this.Height = 300 + (Convert.ToInt32(lines) * 5);
        }
        private void InvokeSetHeight_Old(string lines)
        {
            this.Invoke(new ChangeStringValue_Old(SetHeight_Old), lines);
        }
        #endregion
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

            IniFile.Save(file);

            if (AutoUpdater.NewUpdate)
            {
                while (AutoUpdater.Status == UpdateStatus.Downloading)
                {
                    Application.DoEvents();
                }
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

        private void txtOutput_SelectionChanged(object sender, EventArgs e)
        {
            lblSelCount.Text = "Sel: " + txtOutput.SelectedText.Length;
        }
    }
}
