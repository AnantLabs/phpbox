using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace phpBox
{

    public enum UpdateStatus
    {
        None,
        Downloading,
        Ready,
        Failed
    }

    public class Updater
    {
        protected string file = Program.AppDirectory + @"\update.ini";

        protected Thread myThread { get; set; }
        protected WebClient myRequest { get; set; }
        protected WebClient myDownloader { get; set; }
        protected Regex checkUpdate { get; set; }
        protected IniParser myIni { get; set; }

        public string URL { get; protected set; }
        public string LastUpdate { get; protected set; }
        public bool NewUpdate { get; set; }

        protected UpdateStatus _Status = UpdateStatus.None;
        public UpdateStatus Status
        {
            get
            {
                return _Status;
            }

            set
            {
                _Status = value;
                try
                {
                    UpdateReport(value);
                }
                catch
                {
                }
                
            }
        }

        public Updater(string url)
        {
            checkUpdate = new Regex(
                @"<entry>.*?<updated>([^<]*?)</updated>.*?Labels:.*?Auto-Update.*?(http.*?\.exe).*?</entry>",
                RegexOptions.Compiled | RegexOptions.Singleline
            );

            myIni = new IniParser(file);

            this.URL = url;
            this.NewUpdate = false;
            this.LastUpdate = myIni.GetValue("Date");
        }


        public void Update()
        {
            myThread = new Thread(thrd_update);
            myThread.IsBackground = true;
            myThread.Start();
        }

        private void thrd_update()
        {
            myRequest = new WebClient();
            string text = myRequest.DownloadString(URL);

            Match mt = checkUpdate.Match(text);

            if (mt.Success)
            {
                if (mt.Groups[1].Value != LastUpdate)
                {
                    try
                    {
                        Status = UpdateStatus.Downloading;

                        string updPath = Program.AppDirectory + @"\" + Path.GetFileName(mt.Groups[2].Value);
                        string appPath = Environment.GetCommandLineArgs()[0];
                        string ubtPath = Program.AppDirectory + @"\update.bat";

                        myDownloader = new WebClient();
                        myDownloader.DownloadFile(mt.Groups[2].Value, updPath);

                        string[] cmd = new string[5];
                        cmd[0] = "set title \"phpBox updater\"";
                        cmd[1] = "ping localhost -n 2 -w 3000 > nul";               //Wait 2 seconds
                        cmd[2] = "del \"" + appPath + "\"";                         //Delete old file
                        cmd[3] = "move \"" + updPath + "\" \"" + appPath + "\"";    //Move updated file
                        cmd[4] = "del \"" + ubtPath + "\"";                         //Delete update batch file

                        File.WriteAllLines(ubtPath, cmd);

                        Status = UpdateStatus.Ready;

                        myIni.SetValue("Date", mt.Groups[1].Value, "Date of last update");

                        NewUpdate = true;
                        LastUpdate = mt.Groups[1].Value;
                    }
                    catch
                    {
                        Status = UpdateStatus.Failed;
                    }
                }
            }
        }


        public void StartUpdate()
        {
            Process.Start(Program.AppDirectory + @"\update.bat");
        }

        ~Updater()
        {
            myIni.Save(file);
        }

        public delegate void UpdateReportHandler(UpdateStatus status);
        public event UpdateReportHandler UpdateReport;
    }
}
