using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace EddiCompanionAppService
{
    public class CustomURLResponder : IDisposable
    {
        public delegate void UrlHandler(string url);
        public delegate void Logger(string message);

        private delegate IntPtr DdeDelegate(
                 uint uType,
                 uint uFmt,
                 IntPtr hconv,
                 IntPtr hsz1,
                 IntPtr hsz2,
                 IntPtr hdata,
                 UIntPtr dwData1,
                 UIntPtr dwData2);

        private DdeDelegate ddeDelegate;
        private uint DdeInstance = 0;
        private IntPtr ServerNameHandle = new IntPtr(0);
        private IntPtr TopicHandle = new IntPtr(0);
        private UrlHandler urlHandler;
        private Logger logger;

        /// <summary>Class that implements a custom local URL protocol and reports and URLs received to the delegate method.</summary>
        /// <param name="name">The name of the custom URL protocol, e.g. "eddi". Typically lower case.</param>
        /// <param name="urlHandler">Callback delegate to which the incoming URL will be passed. NB since a user can send any URL from the Windows Run dialog, treat the URL as untrusted.</param>
        /// <param name="logger">Callback delegate on which any errors will be logged.</param>
        /// <param name="absoluteAppPath">Optional full path to your executable. If null, no registry changes will be made. If supplied, an apprpriate registry key will be created or updated in HKCU.</param>
        /// <remarks>To test this, set a breakpoint on your urlHandler callback, and send a URL from the Windows Run dialog.</remarks>
        public CustomURLResponder(string name, UrlHandler urlHandler, Logger logger, string absoluteAppPath = null)
        {
            this.urlHandler = urlHandler;
            this.logger = logger;
            ddeDelegate = new DdeDelegate(DdeCallback);
            if (absoluteAppPath != null)
            {
                RegisterAppPath(name, absoluteAppPath);
            }

            bool success = Setup(name);
            if (!success)
            {
                CleanUp();
            }
        }

        ~CustomURLResponder()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
            }
            // dispose unmanaged resources
            CleanUp();
        }

        // The installer sets the keys we need in HKLM and HKCU, but maybe we are running from a different app location. In that case, adjust.
        private void RegisterAppPath(string name, string absoluteAppPath)
        {
            try
            {
                RegistryKey currentUser = Registry.CurrentUser;
                RegistryKey baseKey = currentUser.CreateSubKey($"Software\\Classes\\{name}");
                using (baseKey)
                {
                    baseKey.SetValue("", $"URL Protocol {name}");
                    baseKey.SetValue("URL Protocol", "");
                    RegistryKey defaultIcon = baseKey.CreateSubKey("Default Icon");
                    using (defaultIcon)
                    {
                        defaultIcon.SetValue("", $"{absoluteAppPath},0");
                    }
                    RegistryKey open = baseKey.CreateSubKey("shell\\open\\command");
                    using (open)
                    {
                        open.SetValue("", $"\"{absoluteAppPath}\" \"%1\"");
                    }
                    RegistryKey ddeexec = baseKey.CreateSubKey("shell\\open\\ddeexec");
                    using (ddeexec)
                    {
                        ddeexec.SetValue("", "%1");
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.Invoke($"There was an error [{ex.Message}] registering the path to EDDI for the URL protocol [{absoluteAppPath}].");
            }
        }

        private bool Setup(string name)
        {
            uint result = NativeMethods.DdeInitializeW(ref DdeInstance, ddeDelegate, (uint)(CallbackFilters.CBF_SKIP_ALLNOTIFICATIONS | CallbackFilters.CBF_FAIL_REQUESTS | CallbackFilters.CBF_FAIL_POKES), 0);
            if (result != 0)
            {
                logger?.Invoke($"Error {result} initialising DDE.");
                return false;
            }
            ServerNameHandle = NativeMethods.DdeCreateStringHandleW(DdeInstance, name, (int)CodePages.CP_WINUNICODE);
            if (ServerNameHandle == IntPtr.Zero)
            {
                LogDDEErrorIfFalse(false, "creating ServerNameHandle");
                return false;
            }
            TopicHandle = NativeMethods.DdeCreateStringHandleW(DdeInstance, "System", (int)CodePages.CP_WINUNICODE);
            if (TopicHandle == IntPtr.Zero)
            {
                LogDDEErrorIfFalse(false, "creating TopicHandle");
                return false;
            }
            IntPtr nameResult = NativeMethods.DdeNameService(DdeInstance, ServerNameHandle, IntPtr.Zero, (uint)DdeNameServiceCommands.DNS_REGISTER);
            if (nameResult == IntPtr.Zero)
            {
                LogDDEErrorIfFalse(false, "registering DdeNameService");
                return false;
            }
            return true;
        }

        private void CleanUp()
        {
            if (DdeInstance == 0)
            {
                return;
            }
            // best error handling we can do here is to log the results and plough on
            IntPtr nameResult = NativeMethods.DdeNameService(DdeInstance, ServerNameHandle, IntPtr.Zero, (uint)DdeNameServiceCommands.DNS_UNREGISTER);
            if (nameResult != IntPtr.Zero)
            {
                LogDDEErrorIfFalse(false, "unregistering DdeNameService");
            }
            FreeStringHandleIfNeeded(ref TopicHandle, "TopicHandle");
            FreeStringHandleIfNeeded(ref ServerNameHandle, "ServerNameHandle");
            if (DdeInstance != 0)
            {
                bool success = NativeMethods.DdeUninitialize(DdeInstance);
                LogDDEErrorIfFalse(success, "DdeUninitialize");
                DdeInstance = 0;
            }
        }

        private void FreeStringHandleIfNeeded(ref IntPtr stringHandle, string context)
        {
            if (stringHandle != IntPtr.Zero)
            {
                bool success = NativeMethods.DdeFreeStringHandle(DdeInstance, stringHandle);
                LogDDEErrorIfFalse(success, $"DdeFreeStringHandle {context}");
                stringHandle = new IntPtr(0);
            }
        }

        private void LogDDEErrorIfFalse(bool success, string context)
        {
            if (!success)
            {
                uint error = NativeMethods.DdeGetLastError(DdeInstance);
                if (error == 0x4006) // DMLERR_INVALIDPARAMETER -- false positive every time
                {
                    return;
                }
                logger?.Invoke($"DDE error {error} in {context}");
            }
        }

        // https://msdn.microsoft.com/en-us/library/ms648742%28v=VS.85%29.aspx?f=255&MSPPError=-2147217396
        private IntPtr DdeCallback(uint uType, uint uFmt, IntPtr hconv, IntPtr hsz1, IntPtr hsz2, IntPtr hdata, UIntPtr dwData1, UIntPtr dwData2)
        {
            DDEMsgType type = (DDEMsgType)uType;
            switch (type)
            {
                case DDEMsgType.XTYP_CONNECT:
                    bool isValid = (NativeMethods.DdeCmpStringHandles(hsz1, TopicHandle) == 0 
                                 && NativeMethods.DdeCmpStringHandles(hsz2, ServerNameHandle) == 0);
                    return new IntPtr(isValid ? 1 : 0);
                case DDEMsgType.XTYP_EXECUTE:
                    string url = FromDdeStringHandle(hdata);
                    urlHandler?.Invoke(url);
                    return new IntPtr((int)DdeResult.DDE_FACK);
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private string FromDdeStringHandle(IntPtr handle)
        {
            byte[] raw = DataFromDdeHandle(handle);
            char[] trimNulls = {'\0'};
            string s = System.Text.Encoding.Unicode.GetString(raw).TrimEnd(trimNulls);
            return s;
        }

        private byte[] DataFromDdeHandle(IntPtr handle)
        {
            uint size = NativeMethods.DdeGetData(handle, null, 0, 0);
            byte[] buffer = new byte[size];
            size = NativeMethods.DdeGetData(handle, buffer, size, 0);
            return buffer;
        }

        // from Windows Kits\10\Include\10.0.16299.0\um\ddeml.h
        enum DdeNameServiceCommands : uint
        {
            DNS_REGISTER = 0x0001,
            DNS_UNREGISTER = 0x0002,
            DNS_FILTERON = 0x0004,
            DNS_FILTEROFF = 0x0008,
        }

        enum CodePages : int
        {
            CP_WINUNICODE = 1200,
        }

        enum DdeResult : int
        {
            DDE_FACK = 0x8000,
            DDE_FBUSY = 0x4000,
            DDE_FDEFERUPD = 0x4000,
            DDE_FACKREQ = 0x8000,
            DDE_FRELEASE = 0x2000,
            DDE_FREQUESTED = 0x1000,
            DDE_FAPPSTATUS = 0x00ff,
            DDE_FNOTPROCESSED = 0x0000,
        }

        [Flags]
        enum CallbackFilters : uint
        {
            CBF_FAIL_SELFCONNECTIONS = 0x00001000,
            CBF_FAIL_CONNECTIONS = 0x00002000,
            CBF_FAIL_ADVISES = 0x00004000,
            CBF_FAIL_EXECUTES = 0x00008000,
            CBF_FAIL_POKES = 0x00010000,
            CBF_FAIL_REQUESTS = 0x00020000,
            CBF_FAIL_ALLSVRXACTIONS = 0x0003f000,

            CBF_SKIP_CONNECT_CONFIRMS = 0x00040000,
            CBF_SKIP_REGISTRATIONS = 0x00080000,
            CBF_SKIP_UNREGISTRATIONS = 0x00100000,
            CBF_SKIP_DISCONNECTS = 0x00200000,
            CBF_SKIP_ALLNOTIFICATIONS = 0x003c0000,
        }

        enum DDEMsgType : uint
        {
            XTYPF_NOBLOCK = 0x0002,  /* CBR_BLOCK will not work */
            XTYPF_NODATA = 0x0004,  /* DDE_FDEFERUPD */
            XTYPF_ACKREQ = 0x0008,  /* DDE_FACKREQ */

            XCLASS_MASK = 0xFC00,
            XCLASS_BOOL = 0x1000,
            XCLASS_DATA = 0x2000,
            XCLASS_FLAGS = 0x4000,
            XCLASS_NOTIFICATION = 0x8000,

            XTYP_ERROR = (0x0000 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK),
            XTYP_ADVDATA = (0x0010 | XCLASS_FLAGS),
            XTYP_ADVREQ = (0x0020 | XCLASS_DATA | XTYPF_NOBLOCK),
            XTYP_ADVSTART = (0x0030 | XCLASS_BOOL),
            XTYP_ADVSTOP = (0x0040 | XCLASS_NOTIFICATION),
            XTYP_EXECUTE = (0x0050 | XCLASS_FLAGS),
            XTYP_CONNECT = (0x0060 | XCLASS_BOOL | XTYPF_NOBLOCK),
            XTYP_CONNECT_CONFIRM = (0x0070 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK),
            XTYP_XACT_COMPLETE = (0x0080 | XCLASS_NOTIFICATION),
            XTYP_POKE = (0x0090 | XCLASS_FLAGS),
            XTYP_REGISTER = (0x00A0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK),
            XTYP_REQUEST = (0x00B0 | XCLASS_DATA),
            XTYP_DISCONNECT = (0x00C0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK),
            XTYP_UNREGISTER = (0x00D0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK),
            XTYP_WILDCONNECT = (0x00E0 | XCLASS_DATA | XTYPF_NOBLOCK),
        }

        private class NativeMethods
        {
            [DllImport("User32.dll")]
            internal static extern uint DdeInitializeW(ref uint DDEInstance, DdeDelegate pfnCallback, uint afCmd, uint ulRes);

            [DllImport("User32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DdeUninitialize(uint DDEInstance);

            [DllImport("User32.dll")]
            internal static extern uint DdeGetLastError(uint DDEInstance);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr DdeCreateStringHandleW(uint DDEInstance, string text, int codePage);

            [DllImport("User32.dll")]
            internal static extern int DdeCmpStringHandles(IntPtr left, IntPtr right);

            [DllImport("User32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DdeFreeStringHandle(uint DDEInstance, IntPtr stringHandle);

            [DllImport("user32.dll")]
            internal static extern IntPtr DdeNameService(uint DDEInstance, IntPtr serviceStringHandle, IntPtr reservedZero, uint afCmd);

            [DllImport("user32.dll")]
            internal static extern uint DdeGetData(IntPtr hData, [Out] byte[] pDst, uint cbMax, uint cbOff);
        }
    }
}
