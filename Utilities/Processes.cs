using System.Diagnostics;
using System.Linq;

namespace Utilities
{
    public static class Processes
    {
        private const string eliteDangerousProcessName = "EliteDangerous";

        private static Process[] ProcessesStartingWith ( string processName )
        {
            return Process.GetProcesses().Where( p => p.ProcessName.StartsWith( processName ) ).ToArray();
        }

        public static Process[] ByName ( string processName )
        {
            return Process.GetProcessesByName(processName);
        }

        public static bool IsEliteRunning ()
        {
            return ProcessesStartingWith( eliteDangerousProcessName ).Length > 0;
        }
    }
}
