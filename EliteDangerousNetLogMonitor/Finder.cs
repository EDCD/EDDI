using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

// Based on MagicMau's code at https://gist.github.com/MagicMau/38ff1db3fc77e58025d1
namespace EliteDangerousNetLogMonitor
{
    public class Finder
    {
        public static string DefLauncherPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Frontier", "EDLaunch");
        public static string DefProductsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Frontier_Developments", "Products");

        public delegate void EliteStartedEventHandler(object sender, EliteStartedEventArgs e);
        public event EliteStartedEventHandler OnEliteStarted;

        private Timer timer;

        public string[] FindInstallationPaths()
        {
            // if Elite is running, use the process to find the installation path
            string[] installationPaths = GetPathFromProcess();
            if (installationPaths != null)
                return installationPaths;

            // if not found in running process, try another way
            string productsPath = FindProductsPath();

            if (productsPath != null)
            {
                return Directory.GetDirectories(productsPath).OrderByDescending(p => new DirectoryInfo(p).LastWriteTime).ToArray();
            }

            return new string[0];
        }

        private string[] GetPathFromProcess()
        {
            var wmiQueryString = "SELECT ExecutablePath FROM Win32_Process WHERE Name LIKE 'elitedangerous%.exe'";

            List<string> paths = new List<string>();
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                foreach (ManagementObject mo in results)
                {
                    var path = (string)mo["ExecutablePath"];
                    if (!string.IsNullOrEmpty(path))
                    {
                        var fi = new FileInfo(path);
                        if (fi != null && fi.Directory != null)
                        {
                            path = new FileInfo(path).Directory.FullName;
                            paths.Add(path);
                        }
                    }
                }
            }

            if (paths.Count == 0)
            {
                WaitForProcessStart();
            }
            else
            {
                var handler = OnEliteStarted;
                if (handler != null)
                    handler(this, new EliteStartedEventArgs(paths[0]));
            }

            return paths.Count == 0 ? null : paths.ToArray();
        }

        private void WaitForProcessStart()
        {
            timer = new Timer(_ =>
            {
                GetPathFromProcess();
            }, null, 5000, Timeout.Infinite);
        }

        private string FindProductsPath()
        {
            // How to find the Elite installation folder?
            string productsPath = null;

            if (Directory.Exists(DefProductsPath))
                productsPath = DefProductsPath;

            if (productsPath == null)
            {
                string launcherPath = FindLauncherPath();

                if (launcherPath != null)
                {
                    // if we do have the path to the launcher, check if it has a Products subfolder
                    if (Directory.Exists(Path.Combine(launcherPath, "Products")))
                        productsPath = Path.Combine(launcherPath, "Products");
                }
            }

            if (productsPath == null)
                productsPath = FindSteamPath();

            return productsPath;
        }

        private string FindLauncherPath()
        {
            string launcherPath = null;

            // attempt 1. Read the registry
            RegistryKey key = null;
            try
            {
                if (Environment.Is64BitOperatingSystem)
                    key = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Wow6432Node");
                if (key == null)
                    key = Registry.LocalMachine.OpenSubKey("SOFTWARE");
                key = key.OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Uninstall");
                key = key.OpenSubKey("{696F8871-C91D-4CB1-825D-36BE18065575}_is1");

                if (key != null)
                {
                    launcherPath = key.GetValue("InstallLocation") as string;
                }
            }
            catch
            {

            }

            if (launcherPath != null)
                return launcherPath;

            // attempt 2. try to read the shortcut in the program menu
            string menuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs", "Frontier", "Elite Dangerous Launcher");
            string shortcut = Path.Combine(menuPath, "Elite Dangerous Launcher.lnk");

            if (File.Exists(shortcut))
            {
                var shell = new IWshRuntimeLibrary.WshShell();
                var lnk = shell.CreateShortcut(shortcut);

                string launcherExePath = lnk.TargetPath;
                launcherPath = new FileInfo(launcherExePath).DirectoryName;
            }

            // attempt 3. try the default path
            if (launcherPath == null)
            {
                if (File.Exists(Path.Combine(DefLauncherPath, "EDLaunch.exe")))
                    launcherPath = DefLauncherPath;
            }

            return launcherPath;
        }

        private string FindSteamPath()
        {
            // tries to determine if it's a steam install
            string defInstall = @"C:/Program Files (x86)/Steam";

            string installPath = null;
            try
            {
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("Valve").OpenSubKey("Steam");
                installPath = key.GetValue("SteamPath") as string;
            }
            catch
            {
            }

            if (installPath == null)
                installPath = defInstall;

            string testPath = installPath + "/SteamApps/common/Elite Dangerous/Products";

            if (!Directory.Exists(testPath))
                testPath = installPath + "/SteamApps/common/Elite Dangerous Horizons/Products";

            if (!Directory.Exists(testPath))
                return null;

            return testPath;
        }

        public bool? IsVerboseLoggingEnabled(string installPath)
        {
            if (installPath == null)
                return null;

            if (IsVerboseLoggingEnabled(installPath, "AppConfig.xml"))
                return true;

            // Commented out, because we don't want to auto-enable it
            //CreateAppConfigLocal(installPath);

            return IsVerboseLoggingEnabled(installPath, "AppConfigLocal.xml");
        }

        public bool IsVerboseLoggingEnabled(string installPath, string filename)
        {
            string appConfigPath = Path.Combine(installPath, filename);

            if (!File.Exists(appConfigPath))
                return false;

            var doc = XDocument.Load(appConfigPath);
            var networkElt = doc.Root.Element("Network");

            if (networkElt == null)
                return false;

            var loggingAttr = networkElt.Attribute("VerboseLogging");

            if (loggingAttr == null)
                loggingAttr = networkElt.Attribute("verboseLogging");

            if (loggingAttr == null)
                return false;

            return loggingAttr.Value == "1";
        }

        public bool CreateAppConfigLocal(string path)
        {
            string filename = Path.Combine(path, "AppConfigLocal.xml");

            if (!File.Exists(filename))
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(filename))
                    {
                        writer.WriteLine("<AppConfig>");
                        writer.WriteLine(" <Network");
                        writer.WriteLine("  VerboseLogging=\"1\">");
                        writer.WriteLine(" </Network>");
                        writer.WriteLine("</AppConfig>");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("Create AppConfigLocal, exception: " + ex.Message);
                    return false;
                }
            }

            return true;
        }

    }

    public class EliteStartedEventArgs : EventArgs
    {
        public string Path { get; private set; }

        public EliteStartedEventArgs(string path)
        {
            Path = path;
        }
    }

}