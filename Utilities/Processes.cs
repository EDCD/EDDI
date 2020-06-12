using System.Diagnostics;

namespace Utilities
{
    public class Processes
    {
        private const string eliteDangerousProcessName = "EliteDangerous";

        public static bool IsProcessRunning(string processName)
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.Contains(processName))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsEliteRunning()
        {
            return IsProcessRunning(eliteDangerousProcessName);
        }
    }
}
