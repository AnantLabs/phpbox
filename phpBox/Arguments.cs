using System;
using System.Text.RegularExpressions;

namespace phpBox
{
    public class Arguments
    {

        protected static string[] args;

        public static void Initialize()
        {
            args = Environment.GetCommandLineArgs();
        }

        public static string GetValue(string Parameter, string optionalParameter = null)
        {

            if (!String.IsNullOrWhiteSpace(optionalParameter)) optionalParameter = "|" + optionalParameter;

            Regex mt = new Regex("-?[-/](" + Parameter + optionalParameter + ")", RegexOptions.IgnoreCase);

            for (int i = 1; i < args.Length; i++)
            {
                if (mt.IsMatch(args[i]))
                {
                    try
                    {
                        return args[i + 1];
                    }
                    catch
                    {
                        return Convert.ToString(true);
                    }
                }
            }

            return null;
        }

        public static string GetValueOf(int index)
        {
            try
            {
                return Environment.GetCommandLineArgs()[index];
            }
            catch
            {
                return null;
            }
        }


    }
}
