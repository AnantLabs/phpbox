#region usings
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Taskbar;
using Microsoft.WindowsAPICodePack.Shell;
using System.Collections.Generic;

#endregion
namespace phpBox
{
    public partial class frmMain : Form
    {
        #region Fields

        private string file = Path.Combine(Program.AppDirectory,"settings.ini");
        private string fScripts = Path.Combine(Program.AppDirectory, "scripts.ini");
        private string fParameters = Path.Combine(Program.AppDirectory, "parameters.ini");

        public string ScriptPath
        {
            set
            {
                txtFilePath.Text = Path.GetFullPath(value);
            }
            get
            {
                try
                {
                    return txtFilePath.Text;
                }
                catch 
                {
                    return _script_path_cache;
                }
            }
        }
        private string _script_path_cache = null;
        private void _cache_script_path() 
        { 
            _script_path_cache = txtFilePath.Text; 
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
                if (!File.Exists(_PHPFile)) return "php.exe";
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
                try
                {
                    return txtGetParameter.Text;
                }
                catch
                {
                    return _script_arguments_cache;
                }
            }
        }
        private string _script_arguments_cache = null;
        private void _cache_script_arguments() 
        { 
            _script_arguments_cache = txtGetParameter.Text; 
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
                return FileVersionInfo.GetVersionInfo(PHPFile).FileVersion;
            }
        }
        public string StartParameter
        {
            get
            {
                return txtStartParameter.Text;
            }
            set
            {
                txtStartParameter.Text = value;
            }
        }

        public bool IsShown { get; set; }

        public ScriptExecuter Executer { get; set; }
        public Updater AutoUpdater { get; set; }
        public IniParser IniFile { get; set; }
        public IniParser IniFavorites { get; set; }
        public IniParser IniParameters { get; set; }

        public JumpList myJumpList { get; set; }
        public JumpListCustomCategory myFavorites { get; set; }
        public JumpListCustomCategory myHome { get; set; }

        public frmMain()
        {
            InitializeComponent();

            this.Text = Application.ProductName;

            IniFile = new IniParser(file);
            IniFavorites = new IniParser(fScripts);
            IniParameters = new IniParser(fParameters);


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
            Executer.ScriptStarted += new ScriptExecuter.ScriptStartedEventHandler(writeHeader);
            Executer.ScriptStopped += new ScriptExecuter.ScriptStoppedEventHandler(ScriptEnd);
            //Commands
            Executer.ChangeStatus_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeSetStatus_Old);
            Executer.ChangeCaption_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeSetCaption_Old);

            Executer.ReportProgress_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeSetProgress_Old);

            Executer.SetLines_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeSetHeight_Old);

            Executer.ShowNotice_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeShowNotice_Old);
            Executer.ShowError_Old += new ScriptExecuter.CommandEventHandler_Old(InvokeShowError_Old);

            AutoUpdater = new Updater(@"http://code.google.com/feeds/p/phpbox/downloads/basic/");
            AutoUpdater.UpdateReport += new Updater.UpdateReportHandler(updateStatusChanged);
        }
        #endregion

        #region Stable functions
        private void writeHelpView()
        {
            txtOutput.Text += "Usage: phpBox [-r <file>] [-s <file>] [-p \"<parameter>\"]\n";
            txtOutput.Text += "Usage: phpBox <script_file>\n";
            txtOutput.Text += "  -r <file>\t\tPHP runtime file with path (php.exe)\n";
            txtOutput.Text += "  -runtime <file>\n\n";

            txtOutput.Text += "  -s <file>\t\tPHP home file with path (*.php)\n";
            txtOutput.Text += "  -home <file>\n\n";

            txtOutput.Text += "  -p \"<parameter>\"\tPHP home parameter (name=value&namen=valuen)\n";
            txtOutput.Text += "  -parameter \"<parameter>\"\n\n";

            txtOutput.Text += "  -nostart\tStarts phpBox without starting the script.\n";
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

        private void frmMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                if (Executer.IsRunning)
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
            if (txtOutput.ReadOnly || Executer.IsRunning)
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

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Executer.IsStoppable && Executer.IsRunning)
            {
                e.Cancel = true;
            }

            if (e.CloseReason == CloseReason.UserClosing)
            {

                if (Executer.IsRunning)
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
                while (sClean.IsRunning)
                {
                    Application.DoEvents();
                }
            }

            IniFile.Save(file);
            IniFavorites.Save(fScripts);
            IniParameters.Save(fParameters);

            if (AutoUpdater.NewUpdate)
            {
                while (AutoUpdater.Status == UpdateStatus.Downloading)
                {
                    Application.DoEvents();
                }
                AutoUpdater.StartUpdate();
            }

            if(Executer.IsRunning) StopScript();

            Environment.Exit((int)e.CloseReason);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ScriptPath) && !Convert.ToBoolean(Arguments.GetValue("nostart")))
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

        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            txtOutput.ScrollToCaret();
        }
        #endregion

        private void StartScript()
        {
            #region check_inizialized
            if (!File.Exists(ScriptPath))
            {
                if (String.IsNullOrWhiteSpace(ScriptPath))
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

            _cache_script_path();
            _cache_script_arguments();

            if (txtGetParameter.Text.Length > 0 && !txtGetParameter.Items.Contains(txtGetParameter.Text))
            {
                txtGetParameter.Items.Add(ScriptArguments);
                IniParameters.AddNumericValue("StrNr", ScriptArguments);
            }

            SetCaption_Old(Path.GetFileName(ScriptPath));

            Executer.PHPFile = PHPFile;
            Executer.ScriptFile = ScriptPath;
            Executer.ScriptArguments = ScriptArguments;
            Executer.StartParameter = StartParameter;
            setExecuteBtn();
            SetLogReadPropery_Old(true);
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

        private bool StopScript()
        {
            if (Executer.Stop())
            {
                return true;
            }
            return false;
        }

        private void writeHeader()
        {
            try
            {
                InvokeClearLog_Old();
                Color clr = Color.Gray;
                InvokeWriteLogLine_Old("PHP Runtime:\t" + RuntimeVersion, clr, false);
                InvokeWriteLogLine_Old("Script:\t" + Path.GetFileName(ScriptPath), clr, false);
                InvokeWriteLogLine_Old("Parameter:\t" + (ScriptArguments.Length > 0 ? ScriptArguments : "-"), clr, false);
                InvokeWriteLogLine_Old("Starttime:\t" + getTimeOfDay(Executer.StartTime), clr, false);
                InvokeWriteLogLine_Old("--------- Shell Output ---------", clr, false);
            }
            catch (Exception ex)
            {
                Call.Error(ex);
            }
        }

        private void writeFooter()
        {
            Color clr = Color.Gray;
            InvokeWriteLogLine_Old("--------------------------------", clr, false);
            InvokeWriteLogLine_Old("Endtime:\t" + getTimeOfDay(Executer.StopTime), clr, false);
            InvokeWriteLogLine_Old("\t\t——————————————————", clr, false);
            if (Executer.ExitReason == ExitType.Finished)
            {
                InvokeWriteLogLine_Old("\t\t" + getTimeOfDay(Executer.RunTime), clr, false);
            }
            else
            {
                InvokeWriteLogLine_Old("\t\t" + getTimeOfDay(Executer.RunTime), Color.Red, false);
            }
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

        private void viewUpdater_Tick(object sender, EventArgs e)
        {
            #region Stable part
            if (Executer.IsRunning)
            {
                lblExecTime.Text = getTimeOfDay(Executer.RunTime);
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
            #endregion
        }

        #region Delegates
        public delegate void RaisFunction_Old();
        private void ClearLog_Old()
        {
            txtOutput.Text = "";
            webView.DocumentText = "";
        }
        private void InvokeClearLog_Old()
        {
            this.Invoke(new RaisFunction_Old(ClearLog_Old));
        }

        #region Old
        private void InvokeWriteLogLine_Old(string Message, Color clr, bool showInWebBrowser = true)
        {
            try
            {
                this.BeginInvoke(new WriteLogLineHandle_Old(WriteLogLine_Old), Message, clr, showInWebBrowser);
            }
            catch { }
        }
        public delegate void WriteLogLineHandle_Old(string Message, Color clr, bool showInWebBrowser);
        private void WriteLogLine_Old(string Message, Color clr, bool showInWebBrowser = true)
        {
            try
            {
                Color oclr = txtOutput.SelectionColor;
                txtOutput.SelectionColor = clr;
                txtOutput.AppendText(Message + "\n");
                txtOutput.SelectionColor = oclr;
                if(showInWebBrowser)
                    webView.Document.Write(Message + "\n");
            }
            catch (Exception ex)
            {
                Call.Error(ex);
            }
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
            if (TaskbarManager.IsPlatformSupported)
            {
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);
            }
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
                if (TaskbarManager.IsPlatformSupported && this.IsShown)
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                    TaskbarManager.Instance.SetProgressValue(val, 100);
                }
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

        private void AddFavorite(string path, bool addToIniList = true, bool doRefresh = true)
        {
            if (!File.Exists(path))
            {
                return;
            }

            if(addToIniList) IniFavorites.AddNumericValue("PathNr", path, 8);
            txtFilePath.Items.Add(path);
            _addFav2JmpCat(path, doRefresh);
        }

        private void _addFav2JmpCat(string path, bool doRefresh){
            if (TaskbarManager.IsPlatformSupported)
            {
                JumpListLink script = new JumpListLink(path, Path.GetFileName(path));
                script.IconReference = new IconReference(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), 
                                        "imageres.dll"), 2);
                myFavorites.AddJumpListItems(script);
                if (doRefresh) myJumpList.Refresh();
            }
        }

        private void RemoveFavorite(string path, bool doRefresh = true)
        {
            IniFavorites.RemoveByValue(txtFilePath.Text);
            txtFilePath.Items.Remove(txtFilePath.Text);
            if (TaskbarManager.IsPlatformSupported)
            {
                myFavorites = new JumpListCustomCategory(myFavorites.Name);

                foreach (string pth in txtFilePath.Items)
                {
                    _addFav2JmpCat(pth, false);
                }

                myJumpList.Refresh();
            }
        }


        private void frmMain_CreateJumpList(object sender, EventArgs e)
        {
            myFavorites = new JumpListCustomCategory("Favorites");
            myHome = new JumpListCustomCategory("Home");

            for (int i = 1; i <= 8; i++)
            {
                string val = IniFavorites.GetValue("PathNr" + i);
                if (!String.IsNullOrEmpty(val) && File.Exists(val))
                {
                    AddFavorite(val, false, false);
                }
            }

            if (TaskbarManager.IsPlatformSupported)
            {
                try
                {
                    //Home
                    JumpListLink mainFolder = new JumpListLink(Program.AppDirectory, "AppData");
                    mainFolder.IconReference = new IconReference(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "imageres.dll"), 3);
                    myHome.AddJumpListItems(mainFolder);
                    if(Directory.Exists(Path.GetDirectoryName(PHPFile)))
                    {
                        JumpListLink runtimeFolder = new JumpListLink(Path.GetDirectoryName(PHPFile), "PHP Runtime");
                        runtimeFolder.IconReference = new IconReference(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "imageres.dll"), 3);
                        myHome.AddJumpListItems(runtimeFolder);
                    }

                    myJumpList = JumpList.CreateJumpList();
                    myJumpList.AddCustomCategories(myHome, myFavorites);
                    myJumpList.Refresh();
                }
                catch (Exception ex)
                {
                    Call.Error(ex);
                }
            }
            this.IsShown = true;
            txtFilePath_CheckIsFavorite(sender, e);
        }

        private bool _isPushed = false;
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                    if (e.KeyCode == Keys.F5 && !_isPushed)
                    {
                        if (Executer.IsRunning && StopScript())
                        {
                            setExecuteBtn();
                            StartScript();
                        }
                        else
                        {
                            setExecuteBtn();
                            StartScript();
                        }
                        _isPushed = true;
                    }
            }
            catch (Exception ex)
            {
                Call.Error(ex);
            }
        }
        private void frmMain_KeyUp(object sender, KeyEventArgs e)
        {
            _isPushed = false;
        }

        private void lblSelCount_TextChanged(object sender, EventArgs e)
        {
            if (lblSelCount.Text == "Sel: 0")
            {
                lblSelCount.Visible = false;
                ToolStripStatusSeparator1.Visible = false;
            }
            else
            {
                lblSelCount.Visible = true;
                ToolStripStatusSeparator1.Visible = true;

            }
        }

        private void txtFilePath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (ScriptPath.Length > 0) ScriptPath = ""; 
                else frmMain_KeyPress(sender, new KeyPressEventArgs((char)e.KeyCode));
            }
        }

        private void viewInWebBrowser_Click(object sender, EventArgs e)
        {
            if (webView.Visible)
            {
                webView.Visible = false;
                viewInWebBrowser.Text = "WebBrowser";
                txtOutput.Focus();
            }
            else
            {
                viewInWebBrowser.Text = "Text";
                webView.Visible = true;
                webView.Focus();
            }
        }

        private void setFavBtn()
        {
            btnFav.TabStop = false;
            btnFav.Image = Icons.RemoveFavorite;
        }

        private void resetFavBtn()
        {
            btnFav.TabStop = true;
            btnFav.Image = Icons.AddFavorite;
        }

        private bool isInFavourites()
        {
            return txtFilePath.Items.Contains(txtFilePath.Text);
        }

        private void btnFav_Click(object sender, EventArgs e)
        {
            if (btnFav.TabStop)
            {
                if (isInFavourites())
                {
                    setFavBtn();
                }
                else
                {
                    AddFavorite(txtFilePath.Text);
                    setFavBtn();
                }
            }
            else
            {
                RemoveFavorite(txtFilePath.Text);
                resetFavBtn();
            }
        }

        private void txtFilePath_CheckIsFavorite(object sender, EventArgs e)
        {
            if (!File.Exists(txtFilePath.Text))
            {
                btnFav.Enabled = false;
            }
            else
            {
                if (isInFavourites())
                {
                    setFavBtn();
                    btnFav.Enabled = true;
                }
                else
                {
                    resetFavBtn();
                    if (txtFilePath.Items.Count < 8)
                    {
                        btnFav.Enabled = true;
                    }
                }
            }
        }

        private void txtFilePath_TextChanged(object sender, EventArgs e)
        {
            txtFilePath_CheckIsFavorite(sender, e);
        }

        private void txtGetParameter_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Shift)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    IniParameters.RemoveByValue(txtGetParameter.SelectedText);
                    txtGetParameter.Items.Remove(txtGetParameter.SelectedText);
                }
            }

            if (!txtGetParameter.DroppedDown && e.KeyCode == Keys.Down)
            {
                txtGetParameter.DroppedDown = true;
            }
        }

        private void txtGetParameter_Enter(object sender, EventArgs e)
        {
            if (txtGetParameter.Items.Count <= 0)
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (IniParameters.IsSet("StrNr" + i))
                    {
                        txtGetParameter.Items.Add(IniParameters.GetValue("StrNr" + i));
                    }
                }
            }
        }

        private void txtFilePath_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Shift)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    RemoveFavorite(txtFilePath.Text);
                    resetFavBtn();
                }
            }

            if (!txtFilePath.DroppedDown && e.KeyCode == Keys.Down)
            {
                txtFilePath.DroppedDown = true;
            }
        }
    }
}
