using Cottle;
using EddiCore;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiEvents;
using EddiScriptResolverService;
using EddiSpeechResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class DeferredSpeechTests : TestBase
    {
        private static readonly string[] StateKeys =
        [ "immediate", "genera", "report", "reports", "matched", "timedout", "included", "standalone", "pending",
          "speechresponder_quiet", "eddi_context_last_speech" ];
        private Dictionary<string, object> savedState;
        private SpeechResponderConfiguration savedConfiguration;
        private string savedPersonality;
        private readonly List<DeferredSpeechScheduler> schedulers = new();

        [TestInitialize]
        public void Initialize ()
        {
            MakeSafe();
            savedState = EDDI.Instance.State.Where( p => StateKeys.Contains( p.Key ) )
                .ToDictionary( p => p.Key, p => p.Value );
            savedConfiguration = ConfigService.Instance.speechResponderConfiguration;
            savedPersonality = savedConfiguration.Personality;
        }

        [TestCleanup]
        public void Cleanup ()
        {
            foreach ( var scheduler in schedulers ) { scheduler.Clear(); }
            foreach ( var key in StateKeys ) { EDDI.Instance.State.Remove( key ); }
            foreach ( var entry in savedState ) { EDDI.Instance.State[ entry.Key ] = entry.Value; }
            savedConfiguration.Personality = savedPersonality;
            ConfigService.Instance.speechResponderConfiguration = savedConfiguration;
        }

        private static readonly DateTime Timestamp = new( 2026, 10, 22, 20, 33, 0, DateTimeKind.Utc );
        private const ulong Address = 2869440619937;
        private const string BodyName = "Piscium Sector PI-T b3-1 2 a";

        private static BodyMappedEvent Mapping ( long? id = 4, ulong address = Address ) =>
            new( Timestamp, BodyName, address, 3, 6 ) { JournalBodyId = id };
        private static SurfaceSignalsEvent Signals ( long id = 4, ulong? address = Address,
            string type = "SAA", int seconds = 0 ) =>
            new( Timestamp.AddSeconds( seconds ), type, address, BodyName, id, null, null );

        private sealed class ManualTimer : IDisposable
        {
            internal TimeSpan Delay;
            internal Func<Task> Callback;
            internal bool Disposed;
            public void Dispose () => Disposed = true;
            // Can model a timer callback that was already queued when Dispose was called.
            internal Task FireAsync () => Callback();
        }

        private sealed class Fixture
        {
            internal readonly List<ManualTimer> Timers = new();
            internal readonly List<bool> Reports = new();
            internal readonly DeferredSpeechScheduler Scheduler;
            internal Fixture ()
            {
                Scheduler = new DeferredSpeechScheduler( ( delay, callback ) =>
                {
                    var timer = new ManualTimer { Delay = delay, Callback = callback };
                    Timers.Add( timer );
                    return timer;
                } );
            }
            internal void Schedule ( Event origin, string waitFor = SurfaceSignalsEvent.NAME,
                Func<bool, Task> render = null ) => Scheduler.Schedule(
                new DeferredSpeechRequest( "Report", 200, waitFor, default ), origin, Scheduler.Generation,
                render ?? ( matched => { Reports.Add( matched ); return Task.CompletedTask; } ) );
            internal Task Complete ( Event e, string script = SurfaceSignalsEvent.NAME ) =>
                Scheduler.RunAsync( () => Scheduler.ScriptCompletedAsync( script, e ) );
        }

        [TestMethod]
        public async Task MatchingSaaReleasesReportAfterScriptFinishes ()
        {
            var f = new Fixture();
            var order = new List<string>();
            await f.Scheduler.RunAsync( () =>
            {
                f.Schedule( Mapping(), render: matched =>
                {
                    Assert.IsTrue( matched ); order.Add( "report" ); return Task.CompletedTask;
                } );
                order.Add( "mapped" ); return Task.CompletedTask;
            } );
            Assert.AreEqual( TimeSpan.FromMilliseconds( 200 ), f.Timers.Single().Delay );
            CollectionAssert.AreEqual( new[] { "mapped" }, order );
            await f.Scheduler.RunAsync( async () =>
            {
                await Task.Yield();
                order.Add( "signals" );
                await f.Scheduler.ScriptCompletedAsync( SurfaceSignalsEvent.NAME, Signals() );
            } );
            CollectionAssert.AreEqual( new[] { "mapped", "signals", "report" }, order );
            Assert.IsTrue( f.Timers.Single().Disposed );
        }

        [TestMethod]
        public async Task UnrelatedFssOlderReplayAndMissingIdentityDoNotRelease ()
        {
            var replay = Signals(); replay.fromLoad = true;
            foreach ( var signals in new[] { Signals( id: 5 ), Signals( address: Address + 1 ),
                         Signals( address: null ), Signals( type: "FSS" ), Signals( seconds: -1 ), replay } )
            {
                var f = new Fixture(); f.Schedule( Mapping() ); await f.Complete( signals );
                Assert.AreEqual( 0, f.Reports.Count );
                await f.Timers.Single().FireAsync();
                CollectionAssert.AreEqual( new[] { false }, f.Reports );
            }
            var missing = new Fixture(); missing.Schedule( Mapping( id: null ) );
            await missing.Complete( Signals() ); Assert.AreEqual( 0, missing.Reports.Count );
            await missing.Timers.Single().FireAsync(); Assert.AreEqual( 1, missing.Reports.Count );
        }

        [TestMethod]
        public async Task CallingSurfaceScriptWithoutSignalEventDoesNotReleaseMapping ()
        {
            var f = new Fixture(); f.Schedule( Mapping() );
            await f.Complete( null ); Assert.AreEqual( 0, f.Reports.Count );
            await f.Complete( Mapping() ); Assert.AreEqual( 0, f.Reports.Count );
        }

        [TestMethod]
        public async Task WrongScriptDoesNotReleaseEvenWithMatchingEvent ()
        {
            var f = new Fixture(); f.Schedule( Mapping() );
            await f.Complete( Signals(), "Other script" ); Assert.AreEqual( 0, f.Reports.Count );
        }

        [TestMethod]
        public async Task TimeoutAndLateOrRepeatedSignalsRenderExactlyOnce ()
        {
            var f = new Fixture(); f.Schedule( Mapping() );
            await f.Timers.Single().FireAsync();
            await f.Complete( Signals() ); await f.Complete( Signals( seconds: 2 ) );
            await f.Timers.Single().FireAsync();
            CollectionAssert.AreEqual( new[] { false }, f.Reports );
        }

        [TestMethod]
        public async Task MatchingSignalsAndAlreadyQueuedTimerRenderExactlyOnce ()
        {
            var f = new Fixture(); f.Schedule( Mapping() );
            await f.Complete( Signals() ); await f.Timers.Single().FireAsync();
            await f.Complete( Signals() );
            CollectionAssert.AreEqual( new[] { true }, f.Reports );
        }

        [TestMethod]
        public async Task ConcurrentTimeoutAndSignalsRenderExactlyOnce ()
        {
            for ( var i = 0; i < 100; i++ )
            {
                var f = new Fixture(); f.Schedule( Mapping() );
                await Task.WhenAll( Task.Run( f.Timers.Single().FireAsync ), Task.Run( () => f.Complete( Signals() ) ) );
                Assert.AreEqual( 1, f.Reports.Count );
            }
        }

        [TestMethod]
        public async Task MultipleBodiesAreReleasedIndependently ()
        {
            var f = new Fixture(); var reports = new List<long>();
            foreach ( var id in new long[] { 4, 5 } )
            {
                f.Schedule( Mapping( id ), render: _ => { reports.Add( id ); return Task.CompletedTask; } );
            }
            await f.Complete( Signals( id: 5 ) );
            CollectionAssert.AreEqual( new long[] { 5 }, reports );
            await f.Complete( Signals() );
            CollectionAssert.AreEqual( new long[] { 5, 4 }, reports );
        }

        [TestMethod]
        public async Task TimerOnlyReportIgnoresScriptCompletions ()
        {
            var f = new Fixture(); f.Schedule( Mapping(), "" );
            await f.Complete( Signals() ); Assert.AreEqual( 0, f.Reports.Count );
            await f.Timers.Single().FireAsync(); CollectionAssert.AreEqual( new[] { false }, f.Reports );
        }

        [TestMethod]
        public async Task SchedulingScriptDoesNotReleaseItsOwnRequest ()
        {
            var f = new Fixture(); var origin = Mapping();
            f.Schedule( origin, BodyMappedEvent.NAME );
            await f.Complete( origin, BodyMappedEvent.NAME ); Assert.AreEqual( 0, f.Reports.Count );
        }

        [TestMethod]
        public async Task ClearCancelsEvenAlreadyQueuedTimersAndRejectsOldGeneration ()
        {
            var f = new Fixture(); f.Schedule( Mapping() ); var generation = f.Scheduler.Generation;
            f.Scheduler.Clear();
            await f.Timers.Single().FireAsync(); await f.Complete( Signals() );
            Assert.AreEqual( 0, f.Reports.Count ); Assert.IsTrue( f.Timers.Single().Disposed );
            f.Scheduler.Schedule( new( "Report", 200, "", default ), Mapping(), generation,
                _ => throw new Exception( "Stale render ran" ) );
            Assert.AreEqual( 1, f.Timers.Count );
        }

        [TestMethod]
        public void ReplayCannotScheduleSpeech ()
        {
            var f = new Fixture(); var origin = Mapping(); origin.fromLoad = true; f.Schedule( origin );
            Assert.AreEqual( 0, f.Timers.Count );
        }

        [TestMethod]
        public async Task TimerCannotEvaluateConcurrentlyWithOrdinarySpeech ()
        {
            var f = new Fixture();
            var entered = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            var release = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            var ordinary = f.Scheduler.RunAsync( async () =>
            {
                f.Schedule( Mapping() ); entered.SetResult(); await release.Task;
            } );
            await entered.Task;
            var timer = f.Timers.Single().FireAsync();
            Assert.AreEqual( 0, f.Reports.Count ); Assert.IsFalse( timer.IsCompleted );
            release.SetResult(); await ordinary; await timer;
            Assert.AreEqual( 1, f.Reports.Count );
        }

        private SpeechResponder Responder ( Fixture f, string mappingScript = null )
        {
            foreach ( var key in StateKeys ) { EDDI.Instance.State.Remove( key ); }
            schedulers.Add( f.Scheduler );
            var responder = new SpeechResponder( f.Scheduler );
            responder.CurrentPersonality = new Personality( "Deferred speech test", "", new Dictionary<string, Script>
            {
                [BodyMappedEvent.NAME] = new( BodyMappedEvent.NAME, null, true, mappingScript ??
                    "{SetState('immediate', event.bodyname)}{InvokeScript('Schedule helper', ['mode': 'nested'])}" ),
                ["Schedule helper"] = new( "Schedule helper", null, false,
                    "{SpeakAfter('Report', 200, 'Surface signals detected', args)}" ),
                [SurfaceSignalsEvent.NAME] = new( SurfaceSignalsEvent.NAME, null, true,
                    "{SetState('genera', 'Fonticulua')}" ),
                ["Report"] = new( "Report", null, false,
                    "{SetState('report', cat(event.bodyname, '|', state.genera, '|', args.mode))}" +
                    "{SetState('matched', speakafter.matched)}{SetState('timedout', speakafter.timedout)}" )
            } );
            return responder;
        }

        [TestMethod]
        public async Task CottleDefersEvaluationAndRetainsOriginalEventAndNestedArgs ()
        {
            var f = new Fixture(); var responder = Responder( f );
            await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
            Assert.AreEqual( BodyName, EDDI.Instance.State[ "immediate" ] );
            Assert.IsFalse( EDDI.Instance.State.ContainsKey( "report" ) );
            // Simulates a later journal read. The original event must not become the signals event.
            await responder.SayAsync( null, SurfaceSignalsEvent.NAME, Signals(), sayOutLoud: false );
            Assert.AreEqual( BodyName + "|Fonticulua|nested", EDDI.Instance.State[ "report" ] );
            Assert.AreEqual( true, EDDI.Instance.State[ "matched" ] );
            Assert.AreEqual( false, EDDI.Instance.State[ "timedout" ] );
        }

        [TestMethod]
        public async Task CottleTimeoutRunsWithoutAnyFurtherJournalEvent ()
        {
            var f = new Fixture(); var responder = Responder( f );
            await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
            await f.Timers.Single().FireAsync();
            Assert.AreEqual( BodyName + "||nested", EDDI.Instance.State[ "report" ] );
            Assert.AreEqual( false, EDDI.Instance.State[ "matched" ] );
            Assert.AreEqual( true, EDDI.Instance.State[ "timedout" ] );
        }

        [TestMethod]
        public async Task ExplicitArgsAndZeroDelayStillDefer ()
        {
            var f = new Fixture(); var responder = Responder( f,
                "{SpeakAfter('Report', 0, '', ['mode': 'explicit'])}{SetState('immediate', 'finished')}" );
            await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
            Assert.IsFalse( EDDI.Instance.State.ContainsKey( "report" ) );
            Assert.AreEqual( TimeSpan.Zero, f.Timers.Single().Delay );
            await f.Timers.Single().FireAsync();
            Assert.AreEqual( BodyName + "||explicit", EDDI.Instance.State[ "report" ] );
            Assert.AreEqual( "finished", EDDI.Instance.State[ "immediate" ] );
        }

        [TestMethod]
        public async Task PersonalityChangeCancelsPendingReport ()
        {
            var f = new Fixture(); var responder = Responder( f );
            await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
            responder.CurrentPersonality = Personality.Default();
            await f.Timers.Single().FireAsync();
            Assert.IsFalse( EDDI.Instance.State.ContainsKey( "report" ) );
        }

        [TestMethod]
        public async Task FileHeaderCancelsBeforeReplayEarlyReturn ()
        {
            var f = new Fixture(); var responder = Responder( f );
            await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
            await responder.HandleAsync( new FileHeaderEvent( Timestamp, "next.log", "4.0", "" ) { fromLoad = true } );
            await f.Timers.Single().FireAsync();
            Assert.IsFalse( EDDI.Instance.State.ContainsKey( "report" ) );
        }

        [TestMethod]
        public async Task DisabledDeferredScriptDoesNotRun ()
        {
            var f = new Fixture(); var responder = Responder( f );
            await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
            responder.CurrentPersonality.Scripts[ "Report" ].Enabled = false;
            await f.Timers.Single().FireAsync();
            Assert.IsFalse( EDDI.Instance.State.ContainsKey( "report" ) );
        }

        [TestMethod]
        public void InvalidFunctionArgumentsAndNonSpeechRendersCannotSchedule ()
        {
            var scripts = new Dictionary<string, Script>
            {
                ["Report"] = new( "Report", null, false, "Report" ),
                ["Test"] = new( "Test", null, false, "" )
            };
            var resolver = new ScriptResolver( scripts );
            foreach ( var expression in new[]
            {
                "SpeakAfter('Missing', 200)", "SpeakAfter('Report', -1)", "SpeakAfter('Report', 30001)",
                "SpeakAfter('Report', 0.5)", "SpeakAfter('Report', '200')", "SpeakAfter('Report', 200, 'Missing')",
                "SpeakAfter('Report', 200, '', 3)"
            } )
            {
                scripts[ "Test" ].Value = "{" + expression + "}";
                var called = false;
                var output = resolver.resolveFromName( "Test", new(), true, _ => called = true );
                Assert.IsFalse( called ); Assert.IsTrue( output?.Contains( "Error" ) == true );
            }
            scripts[ "Test" ].Value = "{SpeakAfter('Report', 200)}";
            Assert.IsTrue( resolver.resolveFromName( "Test", new(), true ).Contains( "only available" ) );
        }

        [TestMethod]
        public async Task DeferredScriptsCannotCreateAnInfiniteSchedulingChain ()
        {
            var f = new Fixture(); var responder = Responder( f );
            responder.CurrentPersonality.Scripts[ "Report" ].Value = "{SpeakAfter('Report', 0)}";
            await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
            await f.Timers.Single().FireAsync();
            Assert.AreEqual( 1, f.Timers.Count );
        }

        [TestMethod]
        public async Task RealTimerReleasesWithoutBlockingTheSchedulingCall ()
        {
            var scheduler = new DeferredSpeechScheduler();
            var completed = new TaskCompletionSource<bool>( TaskCreationOptions.RunContinuationsAsynchronously );
            await scheduler.RunAsync( () =>
            {
                scheduler.Schedule( new( "Report", 20, "", default ), Mapping(), scheduler.Generation,
                    matched => { completed.TrySetResult( matched ); return Task.CompletedTask; } );
                Assert.IsFalse( completed.Task.IsCompleted );
                return Task.CompletedTask;
            } );
            Assert.IsFalse( await completed.Task.WaitAsync( TimeSpan.FromSeconds( 5 ) ) );
            scheduler.Clear();
        }

        [TestMethod]
        public async Task StopAndReloadCancelPendingSpeech ()
        {
            foreach ( var stop in new[] { false, true } )
            {
                var f = new Fixture(); var responder = Responder( f );
                await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
                if ( stop ) { responder.Stop(); } else { responder.Reload(); }
                await f.Timers.Single().FireAsync();
                Assert.IsFalse( EDDI.Instance.State.ContainsKey( "report" ) );
            }
        }

        [TestMethod]
        public async Task SilentSurfaceScriptStillReleasesReport ()
        {
            var f = new Fixture(); var responder = Responder( f );
            responder.CurrentPersonality.Scripts[ SurfaceSignalsEvent.NAME ].Value = "";
            await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
            await responder.SayAsync( null, SurfaceSignalsEvent.NAME, Signals(), sayOutLoud: false );
            Assert.AreEqual( true, EDDI.Instance.State[ "matched" ] );
        }

        [TestMethod]
        public async Task SameTimestampWithoutNewSignalsStillFallsBack ()
        {
            var f = new Fixture();
            await f.Complete( Signals() );
            f.Schedule( Mapping() );
            Assert.AreEqual( 0, f.Reports.Count );
            await f.Timers.Single().FireAsync();
            Assert.AreEqual( false, f.Reports.Single() );
        }

        [TestMethod]
        public async Task CombinedReportPatternSupportsEarlyLateAndUnpairedSignals ()
        {
            foreach ( var scenario in new[] { "early", "late", "unpaired", "no-signals" } )
            {
                var f = new Fixture(); var responder = Responder( f,
                    "{SetState('pending', true)}{SpeakAfter('Report', 200, 'Surface signals detected')}" );
                responder.CurrentPersonality.Scripts[ SurfaceSignalsEvent.NAME ].Value =
                    "{SetState('genera', 'Fonticulua')}{if !GetState('pending'):" +
                    "{SetState('standalone', GetState('standalone') + 1)}}";
                responder.CurrentPersonality.Scripts[ "Report" ].Value =
                    "{SetState('reports', GetState('reports') + 1)}" +
                    "{SetState('included', state.genera)}{SetState('pending', void)}";
                if ( scenario != "unpaired" )
                {
                    await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
                }
                if ( scenario == "late" || scenario == "no-signals" ) { await f.Timers.Single().FireAsync(); }
                if ( scenario != "no-signals" )
                {
                    await responder.SayAsync( null, SurfaceSignalsEvent.NAME, Signals(), sayOutLoud: false );
                }
                if ( scenario == "early" ) { await f.Timers.Single().FireAsync(); }
                Assert.AreEqual( scenario == "unpaired" ? null : (object)1M,
                    EDDI.Instance.State.TryGetValue( "reports", out var reports ) ? reports : null );
                Assert.AreEqual( scenario == "early" ? "Fonticulua" : null,
                    EDDI.Instance.State.TryGetValue( "included", out var included ) ? included : null );
                Assert.AreEqual( scenario == "late" || scenario == "unpaired" ? (object)1M : null,
                    EDDI.Instance.State.TryGetValue( "standalone", out var standalone ) ? standalone : null );
            }
        }

        [TestMethod]
        public async Task GenericWaitWorksWithoutJournalEventsButNotOnItsSchedulingCall ()
        {
            var f = new Fixture(); var responder = Responder( f,
                "{SpeakAfter('Report', 200, 'Body mapped')}" );
            await responder.SayAsync( null, BodyMappedEvent.NAME, sayOutLoud: false );
            Assert.IsFalse( EDDI.Instance.State.ContainsKey( "report" ) );
            responder.CurrentPersonality.Scripts[ BodyMappedEvent.NAME ].Value = "";
            await responder.SayAsync( null, BodyMappedEvent.NAME, sayOutLoud: false );
            Assert.AreEqual( true, EDDI.Instance.State[ "matched" ] );
            Assert.AreEqual( 1, f.Timers.Count );
        }

        [TestMethod]
        public async Task ChangedCoreSystemCancelsBeforeSpeechReceivesLocationEvent ()
        {
            var f = new Fixture(); var responder = Responder( f );
            var originalSystem = EDDI.Instance.GameState.CurrentStarSystem;
            try
            {
                EDDI.Instance.GameStateMutator.CurrentStarSystem = new StarSystem { systemAddress = Address };
                await responder.SayAsync( null, BodyMappedEvent.NAME, Mapping(), sayOutLoud: false );
                EDDI.Instance.GameStateMutator.CurrentStarSystem = new StarSystem { systemAddress = Address + 1 };
                await f.Timers.Single().FireAsync();
                Assert.IsFalse( EDDI.Instance.State.ContainsKey( "report" ) );
            }
            finally { EDDI.Instance.GameStateMutator.CurrentStarSystem = originalSystem; }
        }
    }
}
