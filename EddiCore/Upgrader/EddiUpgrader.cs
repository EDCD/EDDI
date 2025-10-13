using EddiConfigService;
using EddiSpeechService;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;

namespace EddiCore.Upgrader
{
    public abstract class EddiUpgrader
    {

        // Upgrade information
        public static bool UpgradeAvailable => !string.IsNullOrEmpty(UpgradeLocation);
        public static string UpgradeVersion;
        private static string UpgradeLocation;

        /// <summary>
        /// Check to see if an upgrade is available and populate relevant variables
        /// </summary>

        public static async Task CheckUpgradeAsync ()
        {
            // Clear the old values
            UpgradeLocation = null;
            UpgradeVersion = null;

            const string apiUrl = "https://api.github.com/repos/EDCD/EDDI/releases";
            const int maxRetries = 3;

            try
            {
                using ( var client = new HttpClient() )
                {
                    // Set the User-Agent header as required by GitHub API
                    client.DefaultRequestHeaders.UserAgent.ParseAdd( $"{Constants.EDDI_NAME}/{Constants.EDDI_VERSION} - Upgrader" );
                    client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );

                    string response = null;

                    // Retry logic with exponential backoff
                    for ( var i = 0; i < maxRetries; i++ )
                    {
                        try
                        {
                            response = await client.GetStringAsync( apiUrl ).ConfigureAwait(false);
                            break; // Exit loop if successful
                        }
                        catch ( HttpRequestException )
                        {
                            if ( i == ( maxRetries - 1 ) )
                            {
                                throw; // Rethrow if max retries reached
                            }

                            await Task.Delay( TimeSpan.FromSeconds( Math.Pow( 2, i ) ) ).ConfigureAwait(false); // Exponential backoff
                        }
                    }

                    // Check if the response is null
                    if ( response == null ) { throw new NullReferenceException("The Github API response is null");}

                    // Parse the response
                    var releases = JArray.Parse( response );

                    // Determine the latest release
                    var configuration = ConfigService.Instance.eddiConfiguration;
                    foreach ( var release in releases )
                    {
                        var isPreRelease = release.Value<bool>( "prerelease" );
                        if ( ( isPreRelease && configuration.AcceptsBetaReleases ) || !isPreRelease )
                        {
                            var latestRelease = (JObject)release;
                            await ProcessReleaseAsync( latestRelease ).ConfigureAwait(false);
                            break;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                await SpeechService.Instance.SayAsync( null, Properties.Resources.update_server_unreachable, 0 ).ConfigureAwait(false);
                Logging.Warn( "Failed to access GitHub API for releases", ex );
            }
        }

        private static async Task ProcessReleaseAsync ( JObject release )
        {
            // Get the version information, removing any prefixing description and separator
            var version = release.Value<string>("tag_name");
            version = Regex.Replace( version, @"(^\w+[\\\/:_\-\|+=#@&%!~^*])+", "" );

            if ( release["assets"] is JArray assets )
            {
                foreach ( var asset in assets )
                {
                    var name = asset.Value<string>("name");
                    var contentType = asset.Value<string>("content_type");
                    var downloadUrl = asset.Value<string>("browser_download_url");

                    if ( name.StartsWith( "EDDI" ) && contentType == "application/x-msdownload" )
                    {
                        var latestVersion = new Utilities.Version(version);

                        if ( latestVersion > Constants.EDDI_VERSION )
                        {
                            UpgradeLocation = downloadUrl;
                            UpgradeVersion = version;

                            var spokenVersion = version.Replace(".", $" {Properties.Resources.point} ");
                            var message = String.Format(Properties.Resources.update_available, spokenVersion);
                            await SpeechService.Instance.SayAsync( null, message, 0 ).ConfigureAwait(false);
                        }

                        break; // Exit loop once the correct asset is found
                    }
                }
            }
        }

        public static async Task UpgradeAsync()
        {
            try
            {
                if (UpgradeLocation != null)
                {
                    Logging.Info( $"Downloading upgrade from {UpgradeLocation}" );
                    await SpeechService.Instance.SayAsync(null, Properties.Resources.downloading_upgrade, 0).ConfigureAwait(false);
                    var updateFile = await Net.DownloadFileAsync(UpgradeLocation, @"EDDI-update.exe").ConfigureAwait(false);
                    if (updateFile == null)
                    {
                        await SpeechService.Instance.SayAsync(null, Properties.Resources.download_failed, 0).ConfigureAwait(false);
                    }
                    else
                    {
                        // Inno setup will attempt to restart this application so register it
                        EDDI.NativeMethods.RegisterApplicationRestart(null, EDDI.RestartFlags.NONE);

                        Logging.Info( $"Downloaded update to {updateFile}" );
                        Logging.Info( $"Path is {Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )}" );
                        File.SetAttributes(updateFile, FileAttributes.Normal);
                        await SpeechService.Instance.SayAsync(null, Properties.Resources.starting_upgrade, 0).ConfigureAwait(false);
                        Logging.Info("Starting upgrade.");

                        Process.Start(updateFile, $@"/closeapplications /restartapplications /silent /log /nocancel /noicon /dir=""{Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location )}""" );
                    }
                }
            }
            catch (Exception ex)
            {
                await SpeechService.Instance.SayAsync(null, Properties.Resources.upgrade_failed, 0).ConfigureAwait(false);
                Logging.Error("Upgrade failed", ex);
            }
        }
    }
}