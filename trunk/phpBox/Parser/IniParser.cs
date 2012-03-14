using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace phpBox
{
    public class IniParser
    {

        protected List<string> myLines { get; set; }

        public IniParser(string file)
        {
            try
            {
                myLines = new List<string>();
                if (File.Exists(file))
                {
                    string[] myFile = File.ReadAllLines(file);
                    foreach (string line in myFile)
                    {
                        myLines.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Call.Error(ex);
            }
        }


        public void AddValue(string Name, string Value, string Comment = "")
        {
            if (!String.IsNullOrEmpty(Comment) && !Comment.StartsWith(";")) Comment = ";" + Comment;
            myLines.Add(Name + " = " + Value + Comment);
        }

        public void AddNumericValue(string Name, string Value, int MaxLength = 10, string Comment = "")
        {
            for (int i = 1; i <= MaxLength; i++)
            {
                if (!IsSet(Name + i))
                {
                    AddValue(Name + i, Value, Comment);
                    return;
                }
            }
        }

        public bool IsSet(string Name)
        {
            for (int i = 0; i < myLines.Count; i++)
            {
                string pattern = @"^ *(" + Name + ") = ([^;]+)(;.*)?$";
                Match mt = Regex.Match(myLines[i], pattern);
                if (mt.Success)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetValue(string Name, string Value, string Comment = "")
        {
            if (!String.IsNullOrEmpty(Comment) && !Comment.StartsWith(";")) Comment = ";" + Comment;

            for (int i = 0; i < myLines.Count; i++)
            {
                string pattern = @"^ *(" + Name + ") = ([^;]+)(;.*)?$";
                Match mt = Regex.Match(myLines[i], pattern);
                if (mt.Success)
                {
                    if (String.IsNullOrEmpty(Comment)) Comment = mt.Groups[3].Value;
                    myLines[i] = Regex.Replace(myLines[i], pattern, Name + " = " + Value + Comment);
                    return;
                }
            }

            AddValue(Name, Value, Comment);
        }

        public string GetValue(string Name)
        {
            for (int i = 0; i < myLines.Count; i++)
            {
                string pattern = @"^ *(" + Name + ") = ([^;]+)(;.*)?$";
                    Match mt = Regex.Match(myLines[i], pattern);
                    if (mt.Success)
                    {
                        return mt.Groups[2].Value;
                    }
            }
            return null;
        }

        public void Remove(string Name)
        {
            for (int i = 0; i < myLines.Count; i++)
            {
                string pattern = @"^ *(" + Name + ") = ([^;]+)(;.*)?$";
                if (Regex.IsMatch(Name, pattern))
                {
                    myLines.RemoveAt(i);
                }
            }
        }

        public void RemoveByValue(string Value)
        {
            Value = Value.Replace(@"\", @"\\");
            Value = Value.Replace(@".", @"\.");
            Value = Value.Replace(@"[", @"\[");
            Value = Value.Replace(@"]", @"\]");
            Value = Value.Replace(@"(", @"\(");
            Value = Value.Replace(@")", @"\)");

            for (int i = 0; i < myLines.Count; i++)
            {
                string pattern = @"^ *(.*) = ("+Value+")(;.*)?$";
                if (Regex.IsMatch(myLines[i], pattern))
                {
                    myLines.RemoveAt(i);
                }
            }
        }

        public void Save(string file)
        {
            try
            {
                string[] lines = new string[myLines.Count];
                for (int i = 0; i < myLines.Count; i++)
                {
                    lines[i] = myLines[i];
                }

                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                Call.Error(ex);
            }
        }
    }
}
