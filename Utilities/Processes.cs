using System.Linq;

namespace Utilities
{
    public static class Processes
    {
        private const string eliteDangerousProcessName = "EliteDangerous";

        private static bool IsProcessRunning ( string processName )
        {
            return System.Diagnostics.Process.GetProcesses().Any( p => p.ProcessName.StartsWith( processName ) );
        }

        public static bool IsEliteRunning ()
        {
            return IsProcessRunning( eliteDangerousProcessName );
        }
    }
}
