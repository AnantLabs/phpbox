using System;
using System.Windows.Forms;

namespace phpBox
{
    public class Call
    {

        public static void Error(Exception ex)
        {
            string msg = ex.Message;
            if (ex.InnerException != null)
            {
                msg += "\n" + ex.InnerException.Message;
            }

            MessageBox.Show(msg, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void Error(string msg, string source = null)
        {
            if (String.IsNullOrEmpty(source)) source = Application.ProductName;
            MessageBox.Show(msg, source, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void Warning(string msg, string source = null)
        {
            if (String.IsNullOrEmpty(source)) source = Application.ProductName;
            MessageBox.Show(msg, source, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


    }
}
