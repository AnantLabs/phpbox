﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace phpBox
{
    public enum StopReason
    {
        Executed,
        Canceled,
        Error
    }

    public enum ExecuteType
    {
        File
        ,Code
    }

    public enum ExitType
    {
        None = 0xff,
        Finished = 0,
        Killed = -1
    }

    public class ScriptExecuter
    {
        protected Thread myThread { get; set; }
        protected ProcessStartInfo myStartInfo{ get; set; }
        protected Process myProcess { get; set; }

        public bool IsRunning
        {
            get
            {
                try
                {
                    return !myProcess.HasExited;
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool IsStoppable { get; set; }
        public bool IsStartable { get; set; }
        public bool EditableParameter { get; set; }
        public bool ChangableScript { get; set; }
        public bool Exit { get; set; }

        private Encoding _ScriptFileEncoding = Encoding.UTF8;
        public Encoding ScriptFileEncoding
        {
            get
            {
                return _ScriptFileEncoding;
            }
            set
            {
                _ScriptFileEncoding = value;
            }
        }

        public string PHPFile { get; set; }

        public ExecuteType Execute { get; set; }

        private string _ScriptFile = "";
        public string ScriptFile
        {
            get
            {
                return _ScriptFile;
            }
            set
            {
                if (ChangableScript)
                {
                    _ScriptFile = value;
                }
            }
        }

        private string _StartParameter = "";
        public string StartParameter
        {
            get
            {
                string ret = _StartParameter.Replace("%s", ScriptFile);
                ret = ret.Replace("%f", PHPFile);
                ret = ret.Replace("%p", ScriptArguments);
                return ret;
            }
            set
            {
                _StartParameter = value;
            }
        }

        /*private string _ScriptCode = null;
        //private Regex _ScriptFilter = new Regex(@"<\?(?:php)?(.*)\?>", RegexOptions.Compiled | RegexOptions.Singleline);
        public string ScriptCode
        {
            set
            {
                Match mt = _ScriptFilter.Match(value);
                if(mt.Success){
                    _ScriptCode = mt.Groups[1].Value;
                }else{
                    _ScriptCode = value;
                }
            }

            protected get
            {
                return _ScriptCode;
            }
        }*/

        public string ClearScriptFile { get; set; }

        private string _ScriptArguments = "";
        public string ScriptArguments
        {
            get
            {
                return _ScriptArguments;
            }
            set
            {
                if (EditableParameter)
                {
                    _ScriptArguments = value;
                }
            }
        }

        public Regex MatchCommandOld { get; set; }
        public Regex MatchCommand { get; set; }

        private DateTime _StartTime = DateTime.Today;
        public DateTime StartTime { 
            get 
            {
                if (IsRunning)
                    return myProcess.StartTime;
                else
                    return _StartTime; 
            }
            protected set
            {
                _StartTime = value;
            }
        }

        public DateTime StopTime
        {
            get
            {
                try
                {
                    return myProcess.ExitTime;
                }
                catch
                {
                    return DateTime.Now;
                }
            }
        }

        public TimeSpan RunTime
        {
            get
            {
                return StopTime.Subtract(StartTime);
            }
        }

        public ExitType ExitReason
        {
            get
            {
                try
                {
                    return (ExitType)myProcess.ExitCode;
                }
                catch
                {
                    return ExitType.None;
                }
            }
        }

        public ScriptExecuter(string PHPFile, string ScriptFile, string ScriptArguments)
        {
            IsStoppable = true;
            IsStartable = true;
            EditableParameter = true;
            ChangableScript = true;
            Exit = false;

            MatchCommandOld = new Regex(@"^\{(.+)\}$", RegexOptions.Compiled | RegexOptions.Singleline);
            //At time not in use
            //MatchCommand = new Regex(@"^phpBox\.([a-zA-Z_0-9]+)\((.+)\)\;$", RegexOptions.Compiled | RegexOptions.Singleline);

            this.PHPFile = PHPFile;
            this.ScriptFile = ScriptFile;
            this.ScriptArguments = ScriptArguments;

            this.Execute = ExecuteType.File;
        }


        public void Start()
        {
            if (!IsStartable) return;
            myThread = new Thread(runProcess);
            myThread.Name = "phpBox - Script Executer";
            myThread.IsBackground = true;
            myThread.Priority = ThreadPriority.Highest;
            myThread.Start();
        }

        public bool Stop(StopReason reason = StopReason.Canceled)
        {
            if (!IsStoppable) return false;
            try
            {
                if (!myProcess.HasExited)
                {
                    myProcess.Kill();
                    myProcess.Dispose();
                    while (IsRunning)
                    {
                    }
                    return true;
                }
            }
            finally
            {
                try
                {
                    myThread.Abort();
                }
                finally
                {
                    try
                    {
                        ScriptStopped(reason);
                    }
                    finally
                    {
                    }
                }
            }
            return true;
        }

        protected void OnProcessDataRecived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (e.Data != null)
                {
                    Match old_cmd = MatchCommandOld.Match(e.Data);
                    //Match cmd = MatchCommand.Match(e.Data);

                    if (old_cmd.Success)
                    {
                        Thread cmdExecuter = new Thread(new ParameterizedThreadStart(raiseOldCommand));
                        cmdExecuter.IsBackground = true;

                        string[] arg = old_cmd.Groups[1].Value.Split('|');
                        cmdExecuter.Start(new object[2] { arg[0], (arg.Length >= 2 ? arg[1] : null) });
                    }
                    /*else if (cmd.Success)
                    {
                        At time not in use...
                    }*/
                    else
                        DataRecived(sender, new ScriptData(e.Data, ScriptDataType.Output));
                }
            }
            catch (Exception ex)
            {
                Call.Error(ex);
            }
        }

        protected void OnProcessErrorRecived(object sender, DataReceivedEventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(e.Data))
                DataRecived(sender, new ScriptData(e.Data, ScriptDataType.Error));
        }

        protected void runProcess()
        {
            try
            {
                myStartInfo = new ProcessStartInfo();
                myStartInfo.FileName = PHPFile;
                //if (Execute == ExecuteType.File)
                    myStartInfo.Arguments = StartParameter;
                /*else if (Execute == ExecuteType.Code)
                //    myStartInfo.Arguments = "-r \"" + ScriptCode.Replace(@"\", @"\\").Replace("\"", "\\\"") + "\""; //At time not in use...

                if (!String.IsNullOrWhiteSpace(ScriptArguments))
                {
                    myStartInfo.Arguments += " -- \"" + ScriptArguments + "\"";
                }
                */
                myStartInfo.WorkingDirectory = Path.GetDirectoryName(ScriptFile);
                myStartInfo.CreateNoWindow = true;
                myStartInfo.RedirectStandardOutput = true;
                myStartInfo.RedirectStandardError = true;
                myStartInfo.UseShellExecute = false;
                myStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myStartInfo.StandardOutputEncoding = ScriptFileEncoding;
                
                myProcess = new Process();
                myProcess.StartInfo = myStartInfo;
                myProcess.EnableRaisingEvents = true;

                myProcess.OutputDataReceived += new DataReceivedEventHandler(OnProcessDataRecived);
                myProcess.ErrorDataReceived += new DataReceivedEventHandler(OnProcessErrorRecived);

                myProcess.Start();
                try
                {
                    ScriptStarted();
                }
                catch {}
                StartTime = myProcess.StartTime;
                if (!myProcess.HasExited)
                {
                    try
                    {
                        myProcess.BeginErrorReadLine();
                        myProcess.BeginOutputReadLine();
                        myProcess.WaitForExit();
                    }
                    catch// (Exception ex)
                    {
                        //System.Windows.Forms.MessageBox.Show(ex.Message);
                    }
                }

                try
                {
                    if(myProcess.ExitCode == (int)ExitType.Finished)
                        ScriptStopped(StopReason.Executed);
                }
                catch
                {
                }
            }
            finally
            {
            }
        }

        protected void raiseOldCommand(object arg)
        {
            try
            {
                object[] oarg = arg as object[];
                string Command = oarg[0] as string, Value = oarg[1] as string;
                switch (Command.ToUpper())
                {
                    case "STATUS": ChangeStatus_Old(Value); break;
                    case "CAPTION": ChangeCaption_Old(Value); break;

                    case "PROGRESS":
                        Value = Value.Replace('.', ',');
                        if (Value.Contains(','))
                        {
                            try
                            {
                                double val = Convert.ToDouble(Value);
                                val *= 100;
                                Value = Convert.ToString(val);
                            }
                            catch
                            {
                                Value = "0";
                            }
                        }
                        ReportProgress_Old(Value);
                        break;

                    case "LINES": SetLines_Old(Value); break;

                    case "NOTICE": 
                        ShowNotice_Old(Value); 
                        break;
                    case "ERROR":
                        IsStoppable = true;
                        Exit = false;
                        Stop(StopReason.Error);
                        ShowError_Old(Value);
                        break;

                    case "DISABLE_STOP": IsStoppable = false; break;
                    case "ENABLE_STOP": IsStoppable = true; break;

                    case "DISABLE_START": IsStartable = false; break;
                    case "ENABLE_START": IsStartable = true; break;

                    case "DISABLE_PARAMETER": EditableParameter = false; break;
                    case "ENABLE_PARAMETER": EditableParameter = true; break;

                    case "DISABLE_FILE": ChangableScript = false; break;
                    case "ENABLE_FILE": ChangableScript = true; break;

                    case "STOP_SCRIPT": ClearScriptFile = Value; break;

                    case "DO_EXIT": Exit = true; break;

                    default:
                        Value = String.IsNullOrEmpty(Value) ? "" : "|" + Value;
                        DataRecived(this, new ScriptData("{" + Command + Value + "}", ScriptDataType.Output)); 
                        break;
                }
            }
            catch
            {
            }
        }

        public event DataRecivedEventHandler DataRecived;
        public delegate void DataRecivedEventHandler(object sender, ScriptData e);

        public event ScriptStartedEventHandler ScriptStarted;
        public delegate void ScriptStartedEventHandler();

        public event ScriptStoppedEventHandler ScriptStopped;
        public delegate void ScriptStoppedEventHandler(StopReason reason);



        public delegate void CommandEventHandler_Old(string Status);
        public event CommandEventHandler_Old ChangeStatus_Old;
        public event CommandEventHandler_Old ChangeCaption_Old;

        public event CommandEventHandler_Old ReportProgress_Old;

        public event CommandEventHandler_Old SetLines_Old;

        public event CommandEventHandler_Old ShowNotice_Old;
        public event CommandEventHandler_Old ShowError_Old; 
    }
}
