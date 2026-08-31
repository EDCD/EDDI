using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Net
    {
        private const int MaxStringDownloadAttempts = 3;

        private static readonly HttpClient httpClient = new(
            new HttpClientHandler
            {
                AutomaticDecompression =
                    System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip
            } );

        public static async Task<string> DownloadStringAsync ( string uri )
        {
            return await DownloadStringAsync( uri, httpClient, MaxStringDownloadAttempts, DelayBeforeRetryAsync )
                .ConfigureAwait(false);
        }

        internal static async Task<string> DownloadStringAsync (
            string uri,
            HttpClient client,
            int maxAttempts,
            Func<int, Task> delayBeforeRetryAsync )
        {
            if ( maxAttempts < 1 )
            {
                throw new ArgumentOutOfRangeException( nameof(maxAttempts), maxAttempts, "At least one attempt is required." );
            }

            try
            {
                for ( var attempt = 1; attempt <= maxAttempts; attempt++ )
                {
                    try
                    {
                        Logging.Debug( "Requesting " + uri );
                        using ( var response = await client.GetAsync( uri, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait(false) )
                        {
                            if ( !response.IsSuccessStatusCode )
                            {
                                Logging.Error( $"Error obtaining string response from {uri}: {response.StatusCode}" );
                                return null;
                            }

                            var encoding = GetEncodingFromResponse( response );
                            Logging.Debug( "Reading response from " + uri );
                            return await ReadResponseStringAsync( response, encoding ).ConfigureAwait(false);
                        }
                    }
                    catch ( Exception e ) when ( attempt < maxAttempts && IsTransientDownloadException( e ) )
                    {
                        Logging.Warn( $"Error obtaining string response from {uri}: {e.Message}. Retrying ({attempt + 1}/{maxAttempts}).", e );
                        await delayBeforeRetryAsync( attempt ).ConfigureAwait(false);
                    }
                }
            }
            catch ( HttpRequestException hre ) when (hre.InnerException is WebException we)
            {
                Logging.Warn( $"Error obtaining string response from {uri}: {we.Message}", we );
                return null;
            }
            catch ( Exception e )
            {
                Logging.Error( $"Error obtaining string response from {uri}: {e.Message}", e );
                return null;
            }

            return null;
        }

        private static Task DelayBeforeRetryAsync ( int attempt )
        {
            return Task.Delay( 250 * attempt );
        }

        private static bool IsTransientDownloadException ( Exception exception )
        {
            return exception is HttpRequestException or
                                IOException or
                                TaskCanceledException or
                                WebException;
        }

        private static Encoding GetEncodingFromResponse ( HttpResponseMessage response )
        {
            if ( response.Content.Headers.ContentType?.CharSet != null )
            {
                try
                {
                    return Encoding.GetEncoding( response.Content.Headers.ContentType.CharSet );
                }
                catch
                {
                    // Fallback to UTF8 if encoding is not supported
                }
            }

            return Encoding.UTF8;
        }

        private static async Task<string> ReadResponseStringAsync ( HttpResponseMessage response, Encoding encoding )
        {
            string data = null;
            var attempts = 0;
            Exception ex = null;

            while ( data is null && attempts < 10 )
            {
                try
                {
                    await using ( var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false) )
                    {
                        if ( stream != null )
                        {
                            using ( var reader = new StreamReader( stream, encoding ) )
                            {
                                data = await reader.ReadToEndAsync().ConfigureAwait(false);
                                return data;
                            }
                        }

                        return null;
                    }
                }
                catch ( Exception e )
                {
                    attempts++;
                    await Task.Delay( 50 * attempts ).ConfigureAwait(false);
                    ex = e;
                }
            }

            if ( attempts >= 10 && ex != null )
            {
                Logging.Warn( ex.Message, ex );
            }

            return data;
        }

        public static async Task<string> DownloadFileAsync ( string uri, string name )
        {
            try
            {
                var fileName = Path.Combine( Path.GetTempPath(), name );
                using ( var response = await httpClient.GetAsync( uri, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait(false) )
                {
                    if ( !response.IsSuccessStatusCode )
                    {
                        Logging.Error( $"Failed to download update file. Status code: {response.StatusCode}" );
                        return null;
                    }

                    await using ( var fs = new FileStream( fileName, FileMode.OpenOrCreate, FileAccess.Write ) )
                    {
                        await response.Content.CopyToAsync( fs ).ConfigureAwait( true );
                    }
                }

                return fileName;
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to download file " + uri, ex );
                return null;
            }
        }

        public static string GetDefaultBrowserPath ()
        {
            var urlAssociation = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http";
            var browserPathKey = @"$BROWSER$\shell\open\command";

            try
            {
                // Read default browser path from userChoiceLKey
                var userChoiceKey = Registry.CurrentUser.OpenSubKey( urlAssociation + @"\UserChoice", false );

                // If user choice was not found, try machine default
                if ( userChoiceKey == null )
                {
                    // Read default browser path from Win XP registry key
                    // If browser path wasn’t found, try Win Vista (and newer) registry key
                    var browserKey = Registry.ClassesRoot.OpenSubKey( @"HTTP\shell\open\command", false ) ??
                                     Registry.CurrentUser.OpenSubKey(
                                         urlAssociation, false );

                    var path = CleanifyBrowserPath( browserKey?.GetValue( null ) as string );
                    browserKey?.Close();
                    Logging.Debug( "Browser path (1) is " + path );
                    return path;
                }
                else
                {
                    // user defined browser choice was found
                    var progId = userChoiceKey.GetValue( "ProgId" )?.ToString();
                    userChoiceKey.Close();

                    // now look up the path of the executable
                    var concreteBrowserKey = browserPathKey.Replace( "$BROWSER$", progId );
                    var kp = Registry.ClassesRoot.OpenSubKey( concreteBrowserKey, false );
                    var browserPath = CleanifyBrowserPath( kp?.GetValue( null ) as string );
                    kp?.Close();
                    Logging.Debug( "Browser path (2) is " + browserPath );
                    return browserPath;
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to find default browser: ", ex );
                return "explorer.exe";
            }
        }

        private static string CleanifyBrowserPath ( string p )
        {
            var url = p.Split( '"' );
            var clean = url[ 1 ];
            return clean;
        }
    }
}
