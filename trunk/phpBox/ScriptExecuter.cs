﻿using System;
using System.Diagnostics;
using System.IO;
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

    public class ScriptExecuter
    {

        protected Thread myThread { get; set; }
        protected ProcessStartInfo myStartInfo{ get; set; }
        protected Process myProcess { get; set; }

        public bool IsExecuting { get; set; }
        public bool IsStoppable { get; set; }
        public bool IsStartable { get; set; }
        public bool EditableParameter { get; set; }
        public bool ChangableScript { get; set; }
        public bool Exit { get; set; }

        public string PHPFile { get; set; }

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

        public DateTime StartTime { get; set; }

        public ScriptExecuter(string PHPFile, string ScriptFile, string ScriptArguments)
        {
            IsExecuting = false;
            IsStoppable = true;
            IsStartable = true;
            EditableParameter = true;
            ChangableScript = true;
            Exit = false;

            MatchCommandOld = new Regex(@"^\{(.+)\}$", RegexOptions.Compiled | RegexOptions.Singleline);
            MatchCommand = new Regex(@"^phpBox\.([a-zA-Z_0-9]+)\((.+)\)\;$", RegexOptions.Compiled | RegexOptions.Singleline);

            this.PHPFile = PHPFile;
            this.ScriptFile = ScriptFile;
            this.ScriptArguments = ScriptArguments;

            this.StartTime = DateTime.Now;
        }


        public void Start()
        {
            if (!IsStartable) return;
            myThread = new Thread(runProcess);
            myThread.IsBackground = true;
            myThread.Priority = ThreadPriority.Highest;
            myThread.Start();
        }


        public void Stop(StopReason reason = StopReason.Canceled)
        {
            if (!IsStoppable) return;
            try
            {
                if (!myProcess.HasExited)
                {
                    myProcess.Kill();
                    myProcess.Dispose();
                }

                myThread.Abort();
                IsExecuting = false;
                ScriptStopped(reason);
            }
            catch
            {
            }
        }

        protected void OnProcessDataRecived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Match old_cmd = MatchCommandOld.Match(e.Data);
                Match cmd = MatchCommand.Match(e.Data);

                if (old_cmd.Success)
                {
                    Thread cmdExecuter = new Thread(new ParameterizedThreadStart(raiseOldCommand));
                    cmdExecuter.IsBackground = true;

                    string[] arg = old_cmd.Groups[1].Value.Split('|');
                    cmdExecuter.Start(new object[2] { arg[0], (arg.Length >= 2 ? arg[1] : null)});
                }
                else if (cmd.Success)
                {

                }
                else
                    DataRecived(sender, new ScriptData(e.Data, ScriptDataType.Output));
            }
        }

        protected void OnProcessErrorRecived(object sender, DataReceivedEventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(e.Data))
                DataRecived(sender, new ScriptData(e.Data, ScriptDataType.Error));
        }

        protected void runProcess()
        {
            myStartInfo = new ProcessStartInfo();
            myStartInfo.FileName = PHPFile;
            myStartInfo.Arguments = "-f \"" + ScriptFile + "\"";

            if (!String.IsNullOrWhiteSpace(ScriptArguments))
            {
                myStartInfo.Arguments += " -- \"" + ScriptArguments + "\"";
            }

            myStartInfo.WorkingDirectory = Path.GetDirectoryName(ScriptFile);
            myStartInfo.CreateNoWindow = true;
            myStartInfo.RedirectStandardOutput = true;
            myStartInfo.RedirectStandardError = true;
            myStartInfo.UseShellExecute = false;
            myStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            myProcess = new Process();
            myProcess.StartInfo = myStartInfo;
            myProcess.EnableRaisingEvents = true;

            myProcess.OutputDataReceived += new DataReceivedEventHandler(OnProcessDataRecived);
            myProcess.ErrorDataReceived += new DataReceivedEventHandler(OnProcessErrorRecived);

            myProcess.Start();
            this.StartTime = DateTime.Now;
            IsExecuting = true;
            myProcess.BeginErrorReadLine();
            myProcess.BeginOutputReadLine();
            myProcess.WaitForExit();
            IsExecuting = false;
            try
            {
                ScriptStopped(StopReason.Executed);
            }
            catch
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
                    case "STATUS": ChangeStatus(Value); break;
                    case "CAPTION": ChangeCaption(Value); break;

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
                        ReportProgress(Value);
                        break;

                    case "LINES": SetLines(Value); break;

                    case "NOTICE": 
                        ShowNotice(Value); 
                        break;
                    case "ERROR":
                        IsStoppable = true;
                        Exit = false;
                        Stop(StopReason.Error);
                        ShowError(Value);
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

        public event ScriptStoppedEventHandler ScriptStopped;
        public delegate void ScriptStoppedEventHandler(StopReason reason);



        public delegate void CommandEventHandler(string Status);
        public event CommandEventHandler ChangeStatus;
        public event CommandEventHandler ChangeCaption;

        public event CommandEventHandler ReportProgress;

        public event CommandEventHandler SetLines;

        public event CommandEventHandler ShowNotice;
        public event CommandEventHandler ShowError; 
    }
}