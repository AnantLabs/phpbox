using System;
using System.Runtime.InteropServices;

namespace phpBox
{
        public class Win32
        {
            [DllImport("user32.dll")]
            public static extern ushort GetAsyncKeyState(int vKey);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr GetForegroundWindow();
        }
}
