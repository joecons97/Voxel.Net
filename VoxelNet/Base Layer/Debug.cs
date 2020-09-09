using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet
{
    public enum DebugLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Detail = 3
    }

    public static class Debug
    {
        public static DebugLevel DebugLevel { get; set; } = DebugLevel.Detail;

        public static void Assert(string msg = null)
        {
            Program.Window.CursorGrabbed = false;
            Program.Window.CursorVisible = true;

            if (string.IsNullOrEmpty(msg))
                System.Diagnostics.Debug.Assert(false);
            else
                System.Diagnostics.Debug.Assert(false, msg);
        }

        public static void Log(string message, DebugLevel level = DebugLevel.Detail)
        {
            string prefix = level.ToString();
            if (level == DebugLevel.Detail)
                prefix = "";
            else
                prefix += ": ";

            if(DebugLevel >= level)
                Console.WriteLine(prefix + message);
        }
        public static void Log(object message, DebugLevel level = DebugLevel.Detail)
        {
            Log(message.ToString(), level);
        }
    }
}
