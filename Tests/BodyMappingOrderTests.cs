using EddiCore;
using EddiCore.EventHandling;
using EddiDataDefinitions;
using EddiEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class BodyMappingOrderTests
    {
        // Fresh mapping reported on 2026-10-22: SAA complete, SAA signals, then Scan.
        private static readonly DateTime Timestamp = new( 2026, 10, 22, 20, 33, 0, DateTimeKind.Utc );
        private const ulong SystemAddress = 2869440619937;
        private const string BodyName = "Piscium Sector PI-T b3-1 2 a";

        private sealed class ManualScheduler : IEventScheduler
        {
            internal readonly List<Func<Event>> Callbacks = new();
            internal bool Disposed;
            public void Schedule ( TimeSpan delay, Func<Event> createEvent )
            {
                Assert.AreEqual( TimeSpan.FromSeconds( 1 ), delay );
                Callbacks.Add( createEvent );
            }
            public void Dispose () => Disposed = true;
            internal Event Fire ( int index = 0 ) => Disposed ? null : Callbacks[ index ]();
        }

        private sealed class Recorder : IEddiResponder
        {
            internal readonly List<Event> Seen = new();
            internal Func<Event, Task> BeforeRecord;
            public string ResponderName () => "Mapping order test";
            public string LocalizedResponderName () => ResponderName();
            public string ResponderDescription () => ResponderName();
            public bool Start () => true;
            public void Stop () { }
            public void Reload () { }
            public Task HandleStatusAsync ( Status status ) => Task.CompletedTask;
            public UserControl ConfigurationTabItem () => null;
            public async Task HandleAsync ( Event @event )
            {
                if ( BeforeRecord != null ) { await BeforeRecord( @event ); }
                Seen.Add( @event );
            }
        }

        private sealed class Fixture : IDisposable
        {
            internal readonly Recorder Recorder = new();
            internal readonly ManualScheduler Scheduler = new();
            internal readonly List<Event> Processed = new();
            internal readonly EddiEventPipeline Pipeline;
            internal Fixture ( Func<Event, bool> accepts = null )
            {
                Pipeline = new EddiEventPipeline(
                    e => { Processed.Add( e ); return Task.FromResult( accepts?.Invoke( e ) ?? true ); },
                    () => Array.Empty<IEddiMonitor>(), () => new[] { Recorder }, _ => Recorder,
                    () => true, () => new Version( 4, 0 ), new Version( 4, 0 ),
                    CancellationToken.None, Scheduler );
            }
            public void Dispose () => Pipeline.Stop();
            internal Task Send ( Event e ) => Pipeline.HandleEventAsync( e );
        }

        private static BodyMappedEvent Mapping ( long? id = 4, ulong address = SystemAddress ) =>
            new( Timestamp, BodyName, address, 3, 6 ) { JournalBodyId = id };

        private static SurfaceSignalsEvent Signals (
            long id = 4, ulong? address = SystemAddress, string type = "SAA", int seconds = 0 ) =>
            new( Timestamp.AddSeconds( seconds ), type, address, BodyName, id, null, null );

        [TestMethod]
        public async Task MatchingSaa_FinishesBeforeMapping_WithoutReprocessingCore ()
        {
            using var f = new Fixture();
            var mapping = Mapping();
            var signals = Signals();
            var signalsFinished = false;
            var mappingSawSignals = false;
            f.Recorder.BeforeRecord = async e =>
            {
                if ( e == signals ) { await Task.Yield(); signalsFinished = true; }
                if ( e == mapping ) { mappingSawSignals = signalsFinished; }
            };
            await f.Send( mapping );
            Assert.AreEqual( 0, f.Recorder.Seen.Count );
            CollectionAssert.AreEqual( new Event[] { mapping }, f.Processed.ToArray() );
            // Independent calls model separate journal reads, with no batch-local lookahead.
            await f.Send( signals );
            Assert.IsTrue( mappingSawSignals );
            CollectionAssert.AreEqual( new Event[] { signals, mapping }, f.Recorder.Seen.ToArray() );
            Assert.AreEqual( 1, f.Processed.Count( e => e == mapping ) );
            Assert.AreSame( mapping, f.Pipeline.LastEventOfType[ BodyMappedEvent.NAME ] );
            Assert.IsNull( f.Scheduler.Fire() );
        }

        [TestMethod]
        public async Task SignalsAlreadyProcessed_DoNotDelayMapping ()
        {
            using var f = new Fixture();
            await f.Send( Signals() );
            await f.Send( Mapping() );
            Assert.AreEqual( 2, f.Recorder.Seen.Count );
            Assert.AreEqual( 0, f.Scheduler.Callbacks.Count );
        }

        [TestMethod]
        public async Task UnrelatedFssOlderAndReplaySignals_DoNotReleaseMapping ()
        {
            var replay = Signals(); replay.fromLoad = true;
            foreach ( var signals in new[] {
                         Signals( id: 5 ), Signals( address: SystemAddress + 1 ),
                         Signals( address: null ), Signals( type: "FSS" ),
                         Signals( seconds: -1 ), replay } )
            {
                using var f = new Fixture();
                var mapping = Mapping();
                await f.Send( mapping );
                await f.Send( signals );
                Assert.IsFalse( f.Recorder.Seen.Contains( mapping ) );
                await f.Send( f.Scheduler.Fire() );
                Assert.AreEqual( 1, f.Recorder.Seen.Count( e => e == mapping ) );
            }
        }

        [TestMethod]
        public async Task NoSignals_TimeoutDispatchesWithoutRunningCoreAgain ()
        {
            using var f = new Fixture();
            var mapping = Mapping();
            await f.Send( mapping );
            await f.Send( f.Scheduler.Fire() );
            CollectionAssert.AreEqual( new Event[] { mapping }, f.Recorder.Seen.ToArray() );
            Assert.AreEqual( 1, f.Processed.Count );
        }

        [TestMethod]
        public async Task TimeoutQueuedBeforeSignals_DoesNotDuplicateMapping ()
        {
            using var f = new Fixture();
            var mapping = Mapping();
            await f.Send( mapping );
            var queuedTimeout = f.Scheduler.Fire();
            await f.Send( Signals() );
            await f.Send( queuedTimeout );
            Assert.AreEqual( 1, f.Recorder.Seen.Count( e => e == mapping ) );
            Assert.AreEqual( 1, f.Scheduler.Callbacks.Count );
        }

        [TestMethod]
        public async Task LateSignalsAfterTimeout_DoNotRepeatMapping ()
        {
            using var f = new Fixture();
            var mapping = Mapping();
            await f.Send( mapping );
            await f.Send( f.Scheduler.Fire() );
            await f.Send( Signals( seconds: 2 ) );
            Assert.AreEqual( 1, f.Recorder.Seen.Count( e => e == mapping ) );
        }

        [TestMethod]
        public async Task MultipleBodies_ReleaseOnlyTheMatchingMapping ()
        {
            using var f = new Fixture();
            var first = Mapping();
            var second = Mapping( id: 5 );
            await f.Send( first );
            await f.Send( second );
            await f.Send( Signals( id: 5 ) );
            Assert.IsFalse( f.Recorder.Seen.Contains( first ) );
            Assert.IsTrue( f.Recorder.Seen.Contains( second ) );
            await f.Send( Signals() );
            Assert.AreEqual( 1, f.Recorder.Seen.Count( e => e == first ) );
            Assert.IsNull( f.Scheduler.Fire( 0 ) );
            Assert.IsNull( f.Scheduler.Fire( 1 ) );
        }

        [TestMethod]
        public async Task ReplayAndMissingIdentity_DoNotWait ()
        {
            var replay = Mapping(); replay.fromLoad = true;
            foreach ( var mapping in new[] { replay, Mapping( id: null ) } )
            {
                using var f = new Fixture();
                await f.Send( mapping );
                Assert.AreEqual( 1, f.Recorder.Seen.Count );
                Assert.AreEqual( 0, f.Scheduler.Callbacks.Count );
            }
        }

        [TestMethod]
        public async Task RingMapping_IsNotDeferred ()
        {
            using var f = new Fixture();
            var ring = new RingMappedEvent( Timestamp, BodyName + " A Ring", null, SystemAddress, 1, 0 );
            await f.Send( ring );
            Assert.AreSame( ring, f.Recorder.Seen.Single() );
            Assert.AreEqual( 0, f.Scheduler.Callbacks.Count );
        }

        [TestMethod]
        public async Task FileHeader_FlushesBeforeContextReset_AndIgnoresQueuedTimeout ()
        {
            using var f = new Fixture();
            var mapping = Mapping();
            var header = new FileHeaderEvent( Timestamp, "next.log", "4.0", "" );
            await f.Send( mapping );
            var queuedTimeout = f.Scheduler.Fire();
            await f.Send( header );
            await f.Send( queuedTimeout );
            CollectionAssert.AreEqual( new Event[] { mapping, header }, f.Recorder.Seen.ToArray() );
        }

        [TestMethod]
        public async Task RejectedMapping_IsNotDeferred ()
        {
            using var f = new Fixture( e => e is not BodyMappedEvent );
            await f.Send( Mapping() );
            Assert.AreEqual( 0, f.Recorder.Seen.Count );
            Assert.AreEqual( 0, f.Scheduler.Callbacks.Count );
        }

        [TestMethod]
        public async Task RejectedSignals_DoNotReleaseMappingEarly ()
        {
            using var f = new Fixture( e => e is not SurfaceSignalsEvent );
            await f.Send( Mapping() );
            await f.Send( Signals() );
            Assert.AreEqual( 0, f.Recorder.Seen.Count );
            await f.Send( f.Scheduler.Fire() );
            Assert.AreEqual( 1, f.Recorder.Seen.Count );
        }

        [TestMethod]
        public async Task RejectedPriorSignals_DoNotSatisfyLaterMapping ()
        {
            using var f = new Fixture( e => e is not SurfaceSignalsEvent );
            await f.Send( Signals() );
            await f.Send( Mapping() );
            Assert.AreEqual( 0, f.Recorder.Seen.Count );
            Assert.AreEqual( 1, f.Scheduler.Callbacks.Count );
        }

        [TestMethod]
        public async Task SchedulerRacingWithSignals_StillDispatchesMappingOnce ()
        {
            for ( var i = 0; i < 100; i++ )
            {
                using var f = new Fixture();
                var mapping = Mapping();
                await f.Send( mapping );
                var timeout = Task.Run( () => f.Scheduler.Fire() );
                await f.Send( Signals() );
                await f.Send( await timeout );
                Assert.AreEqual( 1, f.Recorder.Seen.Count( e => e == mapping ) );
                Assert.AreEqual( 1, f.Processed.Count( e => e == mapping ) );
            }
        }

        [TestMethod]
        public async Task Stop_CancelsScheduledFallback ()
        {
            using var f = new Fixture();
            await f.Send( Mapping() );
            f.Pipeline.Stop();
            Assert.IsNull( f.Scheduler.Fire() );
        }
    }
}
