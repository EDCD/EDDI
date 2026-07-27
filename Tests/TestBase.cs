using EddiCompanionAppService;
using EddiConfigService;
using EddiCore;
using EddiCore.EventHandling;
using EddiCore.GameState;
using EddiDataProviderService;
using EddiEvents;
using EddiSpanshService;
using EddiStarMapService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Number of worker threads is automatic because `Workers` is set to 0.
// There are 3 scopes of parallelization:
// (1) ClassLevel – each thread executes a TestClass worth of tests. Within the TestClass, the test methods execute serially.
//     This is the default – tests within a class might have interdependency, and we don’t want to be too aggressive.
// (2) MethodLevel – each thread executes a TestMethod.
// (3) Custom – the user can provide a plugin implementing the required execution semantics. 
// source: https://devblogs.microsoft.com/devops/mstest-v2-in-assembly-parallel-test-execution/
[assembly: Parallelize( Workers = 0, Scope = ExecutionScope.ClassLevel )]

namespace Tests
{
    public class TestBase
    {
        internal static readonly FakeSpanshHttpClient FakeSpanshHttpClient = new();
        internal static readonly SpanshService fakeSpanshService = new( FakeSpanshHttpClient );

        internal static readonly FakeEdsmHttpClient FakeEdsmHttpClient = new();
        internal static readonly StarMapService fakeEdsmService = new( FakeEdsmHttpClient );

        private static readonly StarSystemSqLiteRepository fakeStarSystemRepository = StarSystemSqLiteRepository.Create( true );

        internal static void MakeSafe()
        {
            // Prevent telemetry data from being reported based on test results
            Utilities.TelemetryService.Telemetry.TelemetryEnabled = false;

            // Don't write to permanent storage (do this before we initialize our EDDI instance)
            Utilities.Files.unitTesting = true;
            ConfigService.unitTesting = true;
            CompanionAppService.Instance.unitTesting = true;

            // Set ourselves as in a beta game session to stop automatic sending of data to remote systems
            EDDI.Instance.GameStateMutator.gameIsBeta = true;
        }

        internal static DataProviderService CreateTestDataProvider ()
        {
            return DataProviderService.Create( 
                fakeEdsmService, 
                fakeSpanshService,
                fakeStarSystemRepository, 
                true
                );
        }

        internal static DataProviderService CreateIsolatedTestDataProvider (
            out FakeSpanshHttpClient fakeSpanshHttpClient,
            out FakeEdsmHttpClient fakeEdsmHttpClient )
        {
            fakeSpanshHttpClient = new FakeSpanshHttpClient();
            fakeEdsmHttpClient = new FakeEdsmHttpClient();
            return DataProviderService.Create(
                new StarMapService( fakeEdsmHttpClient ),
                new SpanshService( fakeSpanshHttpClient ),
                StarSystemSqLiteRepository.Create( true ),
                true );
        }

        internal static IsolatedEddiEventProcessorContext CreateEventProcessorContext (
            DataProviderService dataProvider = null )
        {
            return new IsolatedEddiEventProcessorContext
            {
                DataProvider = dataProvider ?? CreateIsolatedTestDataProvider( out _, out _ )
            };
        }

        internal sealed class IsolatedEddiEventProcessorContext : IEddiEventProcessorContext
        {
            private readonly EddiGameStateService gameStateService;

            internal IsolatedEddiEventProcessorContext ()
            {
                GameStateOwner = new EddiGameState();
                gameStateService = new EddiGameStateService(
                    GameStateOwner,
                    () => ( null, null, null ),
                    null,
                    null,
                    null,
                    new Version( 4, 0 ) );
                EventPipeline = new EddiEventPipeline(
                    _ => Task.FromResult( true ),
                    () => [ ],
                    () => [ ],
                    _ => null,
                    () => true,
                    () => GameStateOwner.GameVersion,
                    new Version( 4, 0 ),
                    CancellationToken.None );
            }

            internal EddiGameState GameStateOwner { get; }
            public IEddiGameState GameState => GameStateOwner;
            public IEddiGameStateMutator GameStateMutator => gameStateService;
            public DataProviderService DataProvider { get; init; }
            public EddiEventPipeline EventPipeline { get; }
            public ConcurrentDictionary<string, Event> lastEventOfType => EventPipeline.LastEventOfType;
            public Dictionary<string, IEddiMonitor> Monitors { get; } = [ ];

            public IEddiMonitor ObtainMonitor ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
            {
                return Monitors.FirstOrDefault( kvp => kvp.Key.Equals( invariantName, stringComparison ) ).Value;
            }

            public Task conditionallyRefreshStationProfileAsync (
                string expectedSystemName,
                long expectedLastMarketID,
                bool forceUpdate = false,
                JObject profileJson = null ) => Task.CompletedTask;

            public Task updateDestinationSystemAsync ( ulong? destinationSystemAddress, string destinationSystem = null )
            {
                GameStateMutator.DestinationStarSystem = null;
                return Task.CompletedTask;
            }
        }

        public static T DeserializeJsonResource<T>(byte[] data, JsonSerializerSettings settings = null) where T : class
        {
            using (var stream = new MemoryStream(data))
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    var jsonSerializer = settings is null 
                        ? JsonSerializer.Create() 
                        : JsonSerializer.Create(settings);
                    if (typeof(T) == typeof(string))
                    {
                        return jsonSerializer.Deserialize(reader, typeof(JObject))?.ToString() as T;
                    }
                    else
                    {
                        return jsonSerializer.Deserialize(reader, typeof(T)) as T;
                    }
                }
            }
        }
    }
}
