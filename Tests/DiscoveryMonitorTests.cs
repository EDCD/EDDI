using EddiCore;
using EddiDataDefinitions;
using EddiDiscoveryMonitor;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Tests.Properties;

namespace UnitTests
{
    [TestClass]
    public class DiscoveryMonitorTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [ DataTestMethod ]
        [DataRow( .005, 0, true, false, null )]  // (No event) Test at ~150 meters and if we were previously near a sample. Should return false, null. 
        [DataRow( .005, 0, false, true, true )]  // (Entered minimum distance) Test at ~150 meters and if we were NOT previously near a sample. Should return true, true. 
        [DataRow( .050, 0, true, true, false )]  // (Exited minimum distance) Test at ~1500 meters and if we were previously near a sample. Should return true, false. 
        [DataRow( .050, 0, false, false, null )] // (No event) Test at ~1500 meters and if we were NOT previously near a sample. Should return false, null. 
        public void TestScanDistance ( double inputLatitude, double inputLongitude, bool inputNearPriorSample, bool expectedEvent, bool? expectedNearPriorSample )
        {
            var privateObject = new PrivateObject( EDDI.Instance );
            var _currentSystem = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem6);
            var _currentBody = _currentSystem.bodies.FirstOrDefault( b => b.bodyname == "Moon" );
            Assert.IsNotNull( _currentBody );

            privateObject.SetFieldOrProperty( "CurrentStarSystem", _currentSystem );
            var discoveryMonitor = (DiscoveryMonitor)privateObject.Invoke( "ObtainMonitor", "Discovery Monitor" );
            Assert.IsNotNull( discoveryMonitor );
            discoveryMonitor._currentBodyId = _currentBody.bodyId ?? 0;
            discoveryMonitor._currentGenus = OrganicGenus.Bacterial;

            _currentBody.surfaceSignals.AddBioFromGenus( OrganicGenus.Bacterial );
            _currentBody.surfaceSignals.biosignals.First().sampleCoords.Add( new Tuple<decimal?, decimal?>( 0M, 0M ) );
            _currentBody.surfaceSignals.biosignals.First().nearPriorSample = inputNearPriorSample;
            _currentBody.surfaceSignals.biosignals.First().scanState = Exobiology.State.SampleStarted;

            var status = new Status
            {
                altitude = 0,
                bodyname = "Moon",
                latitude = Convert.ToDecimal( inputLatitude ),
                longitude = Convert.ToDecimal( inputLongitude )
            };

            Assert.AreEqual( expectedEvent, discoveryMonitor.TryCheckScanDistance( status, out var actualResult ) );
            Assert.AreEqual( expectedNearPriorSample, actualResult?.nearPriorSample );
        }

        [TestMethod]
        public void handleSurfaceSignalsEventTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void handleScanOrganicEventTest()
        {
            var privateObject = new PrivateObject( EDDI.Instance );
            var _currentSystem = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem6);
            privateObject.SetFieldOrProperty( "CurrentStarSystem", _currentSystem );
            var discoveryMonitor = (DiscoveryMonitor)privateObject.Invoke( "ObtainMonitor", "Discovery Monitor" );
            Assert.IsNotNull( discoveryMonitor );

            var line = ScanOrganicEvent.SAMPLE;
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual( 1, events.Count );
            var @event = (ScanOrganicEvent)events[0];

            var body = _currentSystem.bodies.FirstOrDefault(b => b.bodyId == 42);
            Assert.IsNotNull( body );

            // Set up an initial prediction for the 1st organic that we will test (but not for the 2nd)
            var bio = body.surfaceSignals.AddBioFromGenus( @event.genus, true );

            // Simulate the 2nd scan on the 1st organic, setting a prior sample to jump to the correct number of samples
            bio.sampleCoords.Add( new Tuple<decimal?, decimal?>( 0M, 0M ) );
            discoveryMonitor.handleScanOrganicEvent(@event);
            Assert.AreEqual( body.bodyId, discoveryMonitor._currentBodyId);
            Assert.AreEqual(OrganicGenus.Shrubs, discoveryMonitor._currentGenus);
            Assert.AreEqual(@event.timestamp, body.surfaceSignals?.lastUpdated);
            Assert.IsNotNull( body.surfaceSignals?.biosignals?.Last() );
            Assert.AreEqual( OrganicVariant.Shrubs_05_F, body.surfaceSignals.biosignals.Last().variant );
            Assert.AreEqual( 2, body.surfaceSignals.biosignals.Last().samples );
            Assert.AreEqual( Exobiology.State.SampleInProgress, body.surfaceSignals.biosignals.Last().scanState );
            Assert.AreEqual( 1, @event.numTotal );
            Assert.AreEqual( 1, @event.listRemaining?.Count );
            Assert.AreEqual( body.surfaceSignals.biosignals.Last(), @event.bio );

            // Simulate the 3rd scan on the 1st organic with a fresh copy of the same event
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual( 1, events.Count );
            @event = (ScanOrganicEvent)events[0];
            discoveryMonitor.handleScanOrganicEvent( @event );
            Assert.AreEqual( Exobiology.State.SampleComplete, body.surfaceSignals.biosignals.Last().scanState );
            Assert.AreEqual( 0, @event.listRemaining?.Count );
            Assert.AreEqual( body.surfaceSignals.biosignals.Last(), @event.bio );

            // Scan a 2nd organic which has not been created on the body and test that it is handled gracefully
            var line2 = @"{ ""timestamp"":""2023-05-14T04:22:20Z"", ""event"":""ScanOrganic"", ""ScanType"":""Log"", ""Genus"":""$Codex_Ent_Fonticulus_Genus_Name;"", ""Genus_Localised"":""Fonticulua"", ""Species"":""$Codex_Ent_Fonticulus_02_Name;"", ""Species_Localised"":""Fonticulua Campestris"", ""Variant"":""$Codex_Ent_Fonticulus_02_TTS_Name;"", ""Variant_Localised"":""Fonticulua Campestris - Red"", ""SystemAddress"":10477373803, ""Body"":42 }";
            events = JournalMonitor.ParseJournalEntry( line2 );
            Assert.AreEqual( 1, events.Count );
            @event = (ScanOrganicEvent)events[ 0 ];
            discoveryMonitor.handleScanOrganicEvent( @event );
            Assert.AreEqual( body.bodyId, discoveryMonitor._currentBodyId );
            Assert.AreEqual( OrganicGenus.Fonticulus, discoveryMonitor._currentGenus );
            Assert.IsNotNull( body.surfaceSignals?.biosignals?.Last() );
            Assert.AreEqual( OrganicVariant.Fonticulus_02_TTS, body.surfaceSignals.biosignals.Last().variant );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Last().samples );
            Assert.AreEqual( Exobiology.State.SampleStarted, body.surfaceSignals.biosignals.Last().scanState );
            Assert.AreEqual( 2, @event.numTotal );
            Assert.AreEqual( 1, @event.listRemaining?.Count );
            Assert.AreEqual( body.surfaceSignals.biosignals.Last(), @event.bio );
        }

        [TestMethod]
        public void handleBodyScannedEventTest ()
        {
            Assert.Fail();
        }
    }
}
