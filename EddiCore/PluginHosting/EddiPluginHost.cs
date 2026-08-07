using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiSpeechService;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiCore.PluginHosting
{
    internal sealed class EddiPluginHost
    {
        private const int HResultApplicationControlBlocked = unchecked((int)0x800711C7);
        private const int HResultNotSupported = unchecked((int)0x80131515);

        private readonly CancellationToken _appCancellationToken;
        private readonly Func<bool> _isRunning;
        private readonly Func<bool> _isUnitTesting;
        private readonly object _monitorLock = new();
        private readonly object _responderLock = new();
        private readonly Dictionary<string, CancellationTokenSource> _monitorCancellationTokens = [];
        private readonly Func<List<IEddiMonitor>> _findMonitors;
        private readonly Func<List<IEddiResponder>> _findResponders;

        internal EddiPluginHost (            
            Func<bool> isRunning,
            Func<bool> isUnitTesting,
            IEnumerable<IEddiMonitor> monitors = null,
            IEnumerable<IEddiResponder> responders = null,
            Func<List<IEddiMonitor>> findMonitors = null,
            Func<List<IEddiResponder>> findResponders = null, 
            CancellationToken appCancellationToken = default )
        {
            _appCancellationToken = appCancellationToken;
            _isRunning = isRunning;
            _isUnitTesting = isUnitTesting;
            Monitors = monitors?.ToList() ?? [];
            Responders = responders?.ToList() ?? [];
            _findMonitors = findMonitors ?? FindMonitors;
            _findResponders = findResponders ?? FindResponders;
        }

        internal List<IEddiMonitor> Monitors { get; private set; } = [];
        internal List<IEddiResponder> Responders { get; private set; } = [];
        internal ConcurrentBag<IEddiMonitor> ActiveMonitors { get; private set; } = [];
        internal ConcurrentBag<IEddiResponder> ActiveResponders { get; private set; } = [];

        internal Task DiscoverAsync ( CancellationToken cancellationToken )
        {
            var discoveryTasks = new List<Task>
            {
                Task.Run( () =>
                {
                    try
                    {
                        Responders = _findResponders();
                        Logging.Debug( $"Discovered {Responders.Count} responders" );
                    }
                    catch ( Exception ex )
                    {
                        Logging.Error( "Failed to discover responders", ex );
                        Responders = [];
                    }
                }, cancellationToken ),
                Task.Run( () =>
                {
                    try
                    {
                        Monitors = _findMonitors();
                        Logging.Debug( $"Discovered {Monitors.Count} monitors" );
                    }
                    catch ( Exception ex )
                    {
                        Logging.Error( "Failed to discover monitors", ex );
                        Monitors = [];
                    }
                }, cancellationToken )
            };

            return Task.WhenAll( discoveryTasks );
        }

        internal void Start ( EDDIConfiguration configuration )
        {
            foreach ( var monitor in Monitors )
            {
                if ( !configuration.Plugins.TryGetValue( monitor.MonitorName(), out var enabled ) )
                {
                    enabled = true;
                }

                if ( !enabled && !monitor.IsRequired() )
                {
                    Logging.Info( $"{monitor.MonitorName()} is disabled; not starting" );
                }
                else
                {
                    EnableMonitor( monitor );
                }
            }

            foreach ( var responder in Responders )
            {
                if ( !configuration.Plugins.TryGetValue( responder.ResponderName(), out var enabled ) )
                {
                    enabled = true;
                }

                if ( !enabled )
                {
                    Logging.Info( $"{responder.ResponderName()} is disabled; not starting" );
                }
                else if ( ActiveResponders.Any( r => r.ResponderName() == responder.ResponderName() ) )
                {
                    Logging.Warn( $"{responder.ResponderName()} is already running." );
                }
                else
                {
                    try
                    {
                        var responderStarted = responder.Start();
                        if ( responderStarted )
                        {
                            ActiveResponders.Add( responder );
                            Logging.Info( "Started " + responder.ResponderName() );
                        }
                        else
                        {
                            Logging.Warn( "Failed to start " + responder.ResponderName() );
                        }
                    }
                    catch ( Exception ex )
                    {
                        Logging.Error( "Failed to start " + responder.ResponderName(), ex );
                    }
                }
            }
        }

        internal void StopAll ()
        {
            foreach ( var responder in Responders )
            {
                DisableResponder( responder );
            }

            foreach ( var monitor in Monitors )
            {
                DisableMonitor( monitor );
            }
        }

        internal void Reload ()
        {
            foreach ( var responder in Responders )
            {
                responder.Reload();
            }

            foreach ( var monitor in Monitors )
            {
                monitor.Reload();
            }
        }

        internal void Reload ( string name, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            foreach ( var responder in Responders )
            {
                if ( responder.ResponderName().Contains( name, stringComparison ) )
                {
                    responder.Reload();
                    return;
                }
            }

            foreach ( var monitor in Monitors )
            {
                if ( monitor.MonitorName().Contains( name, stringComparison ) )
                {
                    monitor.Reload();
                }
            }
        }

        internal IEddiMonitor ObtainMonitor ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            foreach ( var monitor in Monitors )
            {
                if ( monitor.MonitorName().Equals( invariantName, stringComparison ) )
                {
                    return monitor;
                }
            }

            return null;
        }

        internal IEddiResponder ObtainResponder ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            foreach ( var responder in Responders )
            {
                if ( responder.ResponderName().Equals( invariantName, stringComparison ) )
                {
                    return responder;
                }
            }

            return null;
        }

        internal void DisableResponder ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            DisableResponder( ObtainResponder( invariantName, stringComparison ) );
        }

        internal void DisableResponder ( IEddiResponder responder )
        {
            if ( responder is null ) { return; }

            lock ( _responderLock )
            {
                var newResponders = new ConcurrentBag<IEddiResponder>();
                while ( ActiveResponders.TryTake( out var item ) )
                {
                    if ( item != responder ) { newResponders.Add( item ); }
                }

                ActiveResponders = newResponders;
                responder.Stop();
            }
        }

        internal void EnableResponder ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            EnableResponder( ObtainResponder( invariantName, stringComparison ) );
        }

        internal void EnableResponder ( IEddiResponder responder )
        {
            if ( responder is null ) { return; }

            if ( !ActiveResponders.Contains( responder ) )
            {
                ActiveResponders.Add( responder );
                responder.Start();
            }
        }

        internal void DisableMonitor ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            DisableMonitor( ObtainMonitor( invariantName, stringComparison ) );
        }

        internal void DisableMonitor ( IEddiMonitor monitor )
        {
            if ( monitor is null ) { return; }

            lock ( _monitorLock )
            {
                var monitorName = monitor.MonitorName();
                if ( _monitorCancellationTokens.TryGetValue( monitorName, out var cts ) )
                {
                    cts.Cancel();
                    cts.Dispose();
                    _monitorCancellationTokens.Remove( monitorName );
                }

                var newMonitors = new ConcurrentBag<IEddiMonitor>();
                while ( ActiveMonitors.TryTake( out var item ) )
                {
                    if ( item != monitor )
                    {
                        newMonitors.Add( item );
                    }
                }

                ActiveMonitors = newMonitors;
                monitor.Stop();
                Logging.Info( $"{monitorName} disabled." );
            }
        }

        internal void EnableMonitor ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
        {
            EnableMonitor( ObtainMonitor( invariantName, stringComparison ) );
        }

        internal void EnableMonitor ( IEddiMonitor monitor )
        {
            if ( monitor is null ) { return; }

            if ( !ActiveMonitors.Contains( monitor ) )
            {
                ActiveMonitors.Add( monitor );
                if ( monitor.NeedsStart() )
                {
                    var monitorName = monitor.MonitorName();
                    var cts = new CancellationTokenSource();
                    _monitorCancellationTokens[ monitorName ] = cts;
                    ThreadPool.QueueUserWorkItem( _ => KeepAlive( monitorName, monitor.Start, cts.Token ), null );
                    Logging.Debug( "Queued keepalive for " + monitorName + " to thread pool" );
                }
            }
            else
            {
                Logging.Warn( $"{monitor.MonitorName()} is already running." );
            }
        }

        internal async Task<bool> HandleStatusAsync ( Status status )
        {
            var monitorTasks = new List<Task>();
            foreach ( var monitor in ActiveMonitors )
            {
                var monitorTask = monitor.HandleStatusAsync( status );
                monitorTask.ContinueWith( task =>
                    {
                        if ( task.IsFaulted )
                        {
                            var dict = new Dictionary<string, object>
                            {
                                [ "status" ] = status, [ "exception" ] = task.Exception
                            };
                            Logging.Error( $"{monitor.MonitorName()} failed to handle status", dict );
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously )
                    .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                monitorTasks.Add( monitorTask );
            }

            var responderTasks = new List<Task>();
            foreach ( var responder in ActiveResponders )
            {
                var responderTask = responder.HandleStatusAsync( status );
                responderTask.ContinueWith( task =>
                    {
                        if ( task.IsFaulted )
                        {
                            var dict = new Dictionary<string, object>
                            {
                                [ "status" ] = status, [ "exception" ] = task.Exception
                            };
                            Logging.Error( $"{responder.ResponderName()} failed to handle status", dict );
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously )
                    .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                responderTasks.Add( responderTask );
            }

            try
            {
                await Task.WhenAll( monitorTasks ).ConfigureAwait( false );
                await Task.WhenAll( responderTasks ).ConfigureAwait( false );
                return true;
            }
            catch ( TaskCanceledException )
            {
                return false;
            }
        }

        internal async Task<bool> HandleProfileAsync ( JObject profile )
        {
            var monitorTasks = new List<Task>();
            foreach ( var monitor in ActiveMonitors )
            {
                var monitorTask = monitor.HandleProfileAsync( profile );
                monitorTask.ContinueWith( task =>
                        {
                            if ( task.IsFaulted )
                            {
                                Logging.Warn(
                                    $"Monitor {monitor.MonitorName()} failed to handle Frontier API update",
                                    task.Exception );
                            }
                        },
                        TaskContinuationOptions.OnlyOnFaulted |
                        TaskContinuationOptions.ExecuteSynchronously )
                    .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                monitorTasks.Add( monitorTask );
            }

            try
            {
                await Task.WhenAll( monitorTasks ).ConfigureAwait( false );
                return true;
            }
            catch ( TaskCanceledException tce )
            {
                Logging.Debug( "Task cancelled", tce );
                return false;
            }
        }

        internal static List<IEddiMonitor> FindMonitors ()
        {
            var path = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            if ( string.IsNullOrEmpty( path ) )
            {
                Logging.Warn( "Unable to start EDDI Monitors, application directory path not found." );
                return null;
            }

            var dir = new DirectoryInfo( path );
            List<IEddiMonitor> foundMonitors = [];
            var pluginType = typeof( IEddiMonitor );
            foreach ( var file in dir.GetFiles( "*Monitor.dll", SearchOption.AllDirectories ) )
            {
                try
                {
                    var assembly = Assembly.LoadFrom( file.FullName );
                    foreach ( var type in assembly.GetTypes() )
                    {
                        if ( !type.IsInterface && !type.IsAbstract )
                        {
                            if ( type.GetInterface( pluginType.FullName ) != null )
                            {
                                try
                                {
                                    Logging.Debug( "Instantiating monitor plugin at " + file.FullName );
                                    var monitor = type.InvokeMember( null,
                                        BindingFlags.CreateInstance,
                                        null, null, null ) as IEddiMonitor;
                                    foundMonitors.Add( monitor );
                                }
                                catch ( TargetInvocationException )
                                {
                                    Logging.Warn(
                                        $"Error loading {file.Name}. Failed to load {type.Name} from {type.Assembly}." );
                                }
                            }
                        }
                    }
                }
                catch ( BadImageFormatException )
                {
                    // Ignore this; probably due to CPU architecture mismatch.
                }
                catch ( ReflectionTypeLoadException ex )
                {
                    var sb = new StringBuilder();
                    foreach ( var exSub in ex.LoaderExceptions )
                    {
                        sb.AppendLine( exSub.Message );
                        if ( exSub is FileNotFoundException exFileNotFound )
                        {
                            if ( !string.IsNullOrEmpty( exFileNotFound.FusionLog ) )
                            {
                                sb.AppendLine( "Fusion Log:" );
                                sb.AppendLine( exFileNotFound.FusionLog );
                            }
                        }
                        sb.AppendLine();
                    }
                    Logging.Warn( "Failed to instantiate plugin at " + file.FullName + ":\n" + sb );
                }
                catch ( FileLoadException flex )
                {
                    var logMessage = BuildMonitorFileLoadLogMessage( flex, file.FullName, dir.FullName );
                    var userMessage = BuildMonitorFileLoadUserMessage( flex, file.FullName, dir.FullName );
                    Logging.Error( logMessage, flex );
                    SpeechService.Instance.SayAsync( null, userMessage, 0 )
                        .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                }
                catch ( Exception ex )
                {
                    var logMessage = $"Failed to load monitor {file.Name}. {ex.Message} {ex.InnerException?.Message ?? ""}";
                    var userMessage = string.Format(
                        Properties.Resources.problem_load_monitor,
                        $"{file.Name}.\n{ex.Message}", 
                        ex.InnerException?.ToString() ?? "" );
                    Logging.Error( logMessage, ex );
                    SpeechService.Instance.SayAsync( null, userMessage, 0 )
                        .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                }
            }
            return foundMonitors;
        }

        internal static string BuildMonitorFileLoadLogMessage (
            FileLoadException ex,
            string monitorPath,
            string applicationPath )
        {
            var failedFile = string.IsNullOrEmpty( ex.FileName ) ? monitorPath : ex.FileName;
            if ( ex.HResult == HResultApplicationControlBlocked ||
                 ex.Message.Contains( "Application Control policy", StringComparison.OrdinalIgnoreCase ) )
            {
                return $"Failed to load monitor {failedFile}. Windows Application Control blocked this file. This usually happens on systems with Smart App Control, AppLocker, WDAC, or corporate security policies.";
            }

            if ( ex.HResult == HResultNotSupported ||
                 ex.Message.Contains( "Operation is not supported", StringComparison.OrdinalIgnoreCase ) )
            {
                return $"Failed to load monitor {failedFile}. Windows blocked this file because it came from another computer or an untrusted location.";
            }

            return $"Failed to load monitor {failedFile}. Please ensure that {applicationPath} is not on a network share, or itself shared. Windows reported: {ex.Message}";
        }

        internal static string BuildMonitorFileLoadUserMessage (
            FileLoadException ex,
            string monitorPath,
            string applicationPath )
        {
            var failedFile = string.IsNullOrEmpty( ex.FileName ) ? monitorPath : ex.FileName;
            if ( ex.HResult == HResultApplicationControlBlocked ||
                 ex.Message.Contains( "Application Control policy", StringComparison.OrdinalIgnoreCase ) )
            {
                return $"Failed to load monitor {failedFile}. Windows Application Control blocked this file. This usually happens on systems with Smart App Control, AppLocker, WDAC, or corporate security policies. Please allow EDDI in Windows Security or ask your system administrator to allow EDDI's application files.";
            }

            if ( ex.HResult == HResultNotSupported ||
                 ex.Message.Contains( "Operation is not supported", StringComparison.OrdinalIgnoreCase ) )
            {
                return $"Failed to load monitor {failedFile}. Windows blocked this file because it came from another computer or an untrusted location. Please unblock the file in Windows file properties or reinstall EDDI from a trusted download.";
            }

            return $"Failed to load monitor {failedFile}. Please ensure that {applicationPath} is not on a network share, or itself shared. Windows reported: {ex.Message}";
        }

        internal static List<IEddiResponder> FindResponders ()
        {
            var path = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            if ( string.IsNullOrEmpty( path ) )
            {
                Logging.Warn( "Unable to start EDDI Responders, application directory path not found." );
                return null;
            }

            var dir = new DirectoryInfo( path );
            List<IEddiResponder> foundResponders = [];
            var pluginType = typeof( IEddiResponder );
            foreach ( var file in dir.GetFiles( "*Responder.dll", SearchOption.AllDirectories ) )
            {
                try
                {
                    var assembly = Assembly.LoadFrom( file.FullName );
                    foreach ( var type in assembly.GetTypes() )
                    {
                        if ( !type.IsInterface && !type.IsAbstract && pluginType.FullName is not null )
                        {
                            if ( type.GetInterface( pluginType.FullName ) != null )
                            {
                                Logging.Debug( "Instantiating responder plugin at " + file.FullName );
                                var responder = type.InvokeMember( type.Name,
                                    BindingFlags.CreateInstance,
                                    null, null, null ) as IEddiResponder;
                                foundResponders.Add( responder );
                            }
                        }
                    }
                }
                catch ( BadImageFormatException )
                {
                    // Ignore this; probably due to CPU architecture mismatch.
                }
                catch ( ReflectionTypeLoadException ex )
                {
                    var sb = new StringBuilder();
                    foreach ( var exSub in ex.LoaderExceptions )
                    {
                        if ( exSub is null ) { continue; }
                        sb.AppendLine( exSub.Message );
                        if ( exSub is FileNotFoundException exFileNotFound )
                        {
                            if ( !string.IsNullOrEmpty( exFileNotFound.FusionLog ) )
                            {
                                sb.AppendLine( "Fusion Log:" );
                                sb.AppendLine( exFileNotFound.FusionLog );
                            }
                        }
                        sb.AppendLine();
                    }
                    Logging.Warn( "Failed to instantiate plugin at " + file.FullName + ":\n" + sb );
                }
            }
            return foundResponders;
        }

        private bool IsMonitorActive ( string name ) =>
            ActiveMonitors.Any( m => m.MonitorName().Equals( name, StringComparison.OrdinalIgnoreCase ) );

        private void KeepAlive ( string name, Action start, CancellationToken monitorCancellationToken = default )
        {
            var token = monitorCancellationToken != CancellationToken.None
                ? monitorCancellationToken
                : _appCancellationToken;
            const int maxConsecutiveFailures = 5;
            var stableRunResetsFailures = TimeSpan.FromMinutes( 5 );
            var consecutiveFailures = 0;
            var rng = new Random( unchecked( ( System.Environment.TickCount * 31 ) + System.Environment.CurrentManagedThreadId ) );

            try
            {
                while ( _isRunning() && !token.IsCancellationRequested && IsMonitorActive( name ) )
                {
                    var runStartTs = Stopwatch.GetTimestamp();
                    Exception failure = null;

                    try
                    {
                        Logging.Info( $"Starting {name} (consecutiveFailures={consecutiveFailures})" );
                        start();
                    }
                    catch ( Exception ex ) when ( !token.IsCancellationRequested )
                    {
                        failure = ex;
                    }

                    if ( !_isRunning() || token.IsCancellationRequested || !IsMonitorActive( name ) )
                    {
                        break;
                    }

                    var ranFor = ElapsedSince( runStartTs );
                    if ( ranFor >= stableRunResetsFailures )
                    {
                        consecutiveFailures = 0;
                    }
                    consecutiveFailures++;
                    Logging.Warn( $"{name} exited unexpectedly after {ranFor.TotalMilliseconds} ms. Restarting." );

                    if ( failure != null )
                    {
                        Logging.Error( $"{name} crashed. Restarting. Consecutive failures: {consecutiveFailures}", failure );
                    }
                    else
                    {
                        Logging.Warn( $"{name} exited unexpectedly. Restarting. Consecutive failures: {consecutiveFailures}" );
                    }

                    if ( consecutiveFailures >= maxConsecutiveFailures )
                    {
                        DisableMonitor( name );
                        Logging.Warn( $"{name} disabled after {consecutiveFailures} consecutive failures" );
                        break;
                    }

                    var exponent = Math.Min( Math.Max( 0, consecutiveFailures - 1 ), 5 );
                    var backoffSeconds = Math.Min( 30, 1 << exponent );
                    var jitterMs = rng.Next( 0, 500 );
                    var delay = _isUnitTesting()
                        ? TimeSpan.Zero
                        : TimeSpan.FromSeconds( backoffSeconds ) + TimeSpan.FromMilliseconds( jitterMs );

                    token.WaitHandle.WaitOne( delay );
                }
            }
            catch ( OperationCanceledException )
            {
                Logging.Debug( "Monitor keepAlive cancelled" );
            }
            catch ( ThreadAbortException )
            {
                Logging.Debug( "Thread aborted" );
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"keepAlive for {name} failed", ex );
            }

            return;

            static TimeSpan ElapsedSince ( long startTimestamp )
            {
                var delta = Stopwatch.GetTimestamp() - startTimestamp;
                var seconds = (double)delta / Stopwatch.Frequency;
                return TimeSpan.FromSeconds( seconds );
            }
        }
    }
}
