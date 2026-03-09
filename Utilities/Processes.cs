using System;
using System.Runtime.InteropServices;

namespace Utilities
{
    public class Processes
    {
        private const string eliteDangerousProcessName = "EliteDangerous";

        public static bool IsProcessRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return false;
            }

            var snapshot = CreateToolhelp32Snapshot(SnapshotFlags.TH32CS_SNAPPROCESS, 0);
            if (snapshot == InvalidHandleValue)
            {
                return false;
            }

            try
            {
                var entry = new ProcessEntry32
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(ProcessEntry32))
                };

                if (!Process32First(snapshot, ref entry))
                {
                    return false;
                }

                do
                {
                    if ( !string.IsNullOrEmpty( entry.szExeFile ) &&
                         entry.szExeFile.Contains( processName, StringComparison.OrdinalIgnoreCase ) )
                    {
                        return true;
                    }
                }
                while (Process32Next(snapshot, ref entry));

                return false;
            }
            finally
            {
                CloseHandle(snapshot);
            }
        }

        public static bool IsEliteRunning()
        {
            return IsProcessRunning(eliteDangerousProcessName);
        }

        private static readonly IntPtr InvalidHandleValue = new(-1);

        [Flags]
        private enum SnapshotFlags : uint
        {
            TH32CS_SNAPPROCESS = 0x00000002
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct ProcessEntry32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool Process32First(IntPtr hSnapshot, ref ProcessEntry32 lppe);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool Process32Next(IntPtr hSnapshot, ref ProcessEntry32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
