using EddiCore;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using Utilities;

namespace Eddi
{
    public class EddiUpgrader
    {

        // Upgrade information
        public static bool UpgradeAvailable = false;
        public static bool UpgradeRequired = false;
        public static string UpgradeVersion;
        public static string UpgradeLocation;
        public static string Motd;
        public static List<string> ProductionBuilds = new List<string>() { "r131487/r0" };

        /// <summary>
        /// Check to see if an upgrade is available and populate relevant variables
        /// </summary>
        public static void CheckUpgrade()
        {
            // Clear the old values
            UpgradeRequired = false;
            UpgradeAvailable = false;
            UpgradeLocation = null;
            UpgradeVersion = null;
            Motd = null;

            try
            {
                ServerInfo updateServerInfo = ServerInfo.FromServer(Constants.EDDI_SERVER_URL);
                if (updateServerInfo == null)
                {
                    throw new Exception("Failed to contact update server");
                }
                else
                {
                    EDDIConfiguration configuration = EDDIConfiguration.FromFile();
                    InstanceInfo info = configuration.Beta ? updateServerInfo.beta : updateServerInfo.production;
                    string spokenVersion = info.version.Replace(".", $" {Eddi.Properties.EddiResources.point} ");
                    Motd = info.motd;
                    if (updateServerInfo.productionbuilds != null)
                    {
                        ProductionBuilds = updateServerInfo.productionbuilds;
                    }
                    Utilities.Version minVersion = new Utilities.Version(info.minversion);
                    if (minVersion > Constants.EDDI_VERSION)
                    {
                        // There is a mandatory update available
                        if (!App.FromVA)
                        {
                            string message = String.Format(Eddi.Properties.EddiResources.mandatory_upgrade, spokenVersion);
                            SpeechService.Instance.Say(null, message, 0);
                        }
                        UpgradeRequired = true;
                        UpgradeLocation = info.url;
                        UpgradeVersion = info.version;
                        return;
                    }

                    Utilities.Version latestVersion = new Utilities.Version(info.version);
                    if (latestVersion > Constants.EDDI_VERSION)
                    {
                        // There is an update available
                        if (!App.FromVA)
                        {
                            string message = String.Format(Eddi.Properties.EddiResources.update_available, spokenVersion);
                            SpeechService.Instance.Say(null, message, 0);
                        }
                        UpgradeAvailable = true;
                        UpgradeLocation = info.url;
                        UpgradeVersion = info.version;
                    }
                }
            }
            catch (Exception ex)
            {
                SpeechService.Instance.Say(null, Eddi.Properties.EddiResources.update_server_unreachable, 0);
                Logging.Warn("Failed to access " + Constants.EDDI_SERVER_URL, ex);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void Upgrade()
        {
            try
            {
                if (UpgradeLocation != null)
                {
                    Logging.Info("Downloading upgrade from " + UpgradeLocation);
                    SpeechService.Instance.Say(null, Eddi.Properties.EddiResources.downloading_upgrade, 0);
                    string updateFile = Net.DownloadFile(UpgradeLocation, @"EDDI-update.exe");
                    if (updateFile == null)
                    {
                        SpeechService.Instance.Say(null, Eddi.Properties.EddiResources.download_failed, 0);
                    }
                    else
                    {
                        // Inno setup will attempt to restart this application so register it
                        EDDI.NativeMethods.RegisterApplicationRestart(null, EDDI.RestartFlags.NONE);

                        Logging.Info("Downloaded update to " + updateFile);
                        Logging.Info("Path is " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                        File.SetAttributes(updateFile, FileAttributes.Normal);
                        SpeechService.Instance.Say(null, Eddi.Properties.EddiResources.starting_upgrade, 0);
                        Logging.Info("Starting upgrade.");

                        Process.Start(updateFile, @"/closeapplications /restartapplications /silent /log /nocancel /noicon /dir=""" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"""");
                    }
                }
            }
            catch (Exception ex)
            {
                SpeechService.Instance.Say(null, Eddi.Properties.EddiResources.upgrade_failed, 0);
                Logging.Error("Upgrade failed", ex);
            }
        }
    }
}