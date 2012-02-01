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
    public class Updater
    {

        protected Thread myThread { get; set; }
        protected WebClient myRequest { get; set; }
        protected WebClient myDownloader { get; set; }
        protected Regex checkUpdate { get; set; }

        public string URL { get; protected set; }
        public string LastUpdate { get; protected set; }
        public bool NewUpdate { get; set; }

        public Updater(string url)
        {
            checkUpdate = new Regex(
                @"<entry>.*?<updated>([^<]*?)</updated>.*?Labels:.*?Auto-Update.*?(http.*?\.exe).*?</entry>",
                RegexOptions.Compiled | RegexOptions.Singleline
            );
            this.URL = url;
            this.NewUpdate = false;
            this.LastUpdate = "";
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
                    string updPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + 
                        Path.GetFileName(mt.Groups[2].Value);

                    myDownloader = new WebClient();
                    myDownloader.DownloadFile(mt.Groups[2].Value, updPath);

                    string appPath = Environment.GetCommandLineArgs()[0];

                    string[] cmd = new string[5];
                    cmd[0] = "set title \"phpBox updater\"";
                    cmd[1] = "ping localhost -n 2 -w 3000 > nul";               //Wait 2 seconds
                    cmd[2] = "del \"" + appPath + "\"";                         //Delete old file
                    cmd[3] = "move \"" + updPath + "\" \"" + appPath + "\"";    //Move updated file
                    cmd[4] = "del update.bat";                                  //Delete update batch file

                    File.WriteAllLines("update.bat", cmd);

                    NewUpdate = true;
                    LastUpdate = mt.Groups[1].Value;
                }
            }
        }


        public void StartUpdate()
        {
            Process.Start("update.bat");
        }



    }
}
