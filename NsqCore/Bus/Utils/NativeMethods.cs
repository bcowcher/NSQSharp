using System;
using System.Runtime.InteropServices;

namespace NsqSharp.Bus.Utils
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutineCallback handler, bool add);
    }

    public delegate bool HandlerRoutineCallback(CtrlType dwCtrlType);

    public enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT,
        CTRL_CLOSE_EVENT,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT
    }
}
