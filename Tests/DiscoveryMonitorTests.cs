using EddiConfigService.Configurations;
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

            privateObject.SetFieldOrProperty( nameof(EDDI.CurrentStarSystem), _currentSystem );
            var discoveryMonitor = (DiscoveryMonitor)privateObject.Invoke( nameof(EDDI.ObtainMonitor), "Discovery Monitor" );
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

        [DataTestMethod]
        [DataRow( true, false )]
        [DataRow( true, true )]
        [DataRow( false, true )]
        [DataRow( false, false )]
        public void TestSurfaceSignalPredictions(bool starFirst, bool enableVariantPredictions)
        {
            var fsdJump = @"{ ""timestamp"":""2022-12-08T06:26:43Z"", ""event"":""FSDJump"", ""Taxi"":false, ""Multicrew"":false, ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""StarPos"":[43.59375,42.34375,-74.37500], ""SystemAllegiance"":""Federation"", ""SystemEconomy"":""$economy_Refinery;"", ""SystemEconomy_Localised"":""Refinery"", ""SystemSecondEconomy"":""$economy_Military;"", ""SystemSecondEconomy_Localised"":""Military"", ""SystemGovernment"":""$government_Corporate;"", ""SystemGovernment_Localised"":""Corporate"", ""SystemSecurity"":""$SYSTEM_SECURITY_medium;"", ""SystemSecurity_Localised"":""Medium Security"", ""Population"":94434, ""Body"":""TestStar"", ""BodyID"":0, ""BodyType"":""Star"", ""JumpDist"":61.825, ""FuelUsed"":3.428739, ""FuelLevel"":23.546732, ""Factions"":[ { ""Name"":""Morinbath Progressive Party"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.093596, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Nobles of Morinbath"", ""FactionState"":""None"", ""Government"":""Feudal"", ""Influence"":0.009852, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Silver United Systems"", ""FactionState"":""None"", ""Government"":""Corporate"", ""Influence"":0.464039, ""Allegiance"":""Federation"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Morinbath Silver Organisation"", ""FactionState"":""None"", ""Government"":""Anarchy"", ""Influence"":0.043350, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Aristocrats of Morinbath"", ""FactionState"":""Boom"", ""Government"":""Feudal"", ""Influence"":0.081773, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000, ""ActiveStates"":[ { ""State"":""Boom"" } ] }, { ""Name"":""League of Free Commanders"", ""FactionState"":""None"", ""Government"":""Confederacy"", ""Influence"":0.162562, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":46.959999 }, { ""Name"":""Aesir Heavy Industries"", ""FactionState"":""None"", ""Government"":""Corporate"", ""Influence"":0.144828, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 } ], ""SystemFaction"":{ ""Name"":""Silver United Systems"" } }";
            var fssScanStar = @"{ ""timestamp"":""2022-12-08T06:26:49Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""BodyName"":""TestSystem"", ""BodyID"":0, ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""F"", ""Subclass"":9, ""StellarMass"":0.964844, ""Radius"":732452608.000000, ""AbsoluteMagnitude"":4.518463, ""Age_MY"":1066, ""SurfaceTemperature"":6049.000000, ""Luminosity"":""Vb"", ""RotationPeriod"":275396.820636, ""AxialTilt"":0.000000, ""WasDiscovered"":true, ""WasMapped"":false }";
            var fssbodySignal = @"{ ""timestamp"":""2022-12-08T06:28:49Z"", ""event"":""FSSBodySignals"", ""BodyName"":""TestSystem 4"", ""BodyID"":5, ""SystemAddress"":9999999999999, ""Signals"":[ { ""Type"":""$SAA_SignalType_Biological;"", ""Type_Localised"":""Biological"", ""Count"":7 } ] }";
            var fssScanBody = @"{ ""timestamp"":""2022-12-08T06:28:50Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""BodyName"":""TestSystem 4"", ""BodyID"":5, ""Parents"":[ {""Null"":4}, {""Star"":0} ], ""StarSystem"":""TestSystem"", ""SystemAddress"":9999999999999, ""DistanceFromArrivalLS"":1681.751167, ""TidalLock"":false, ""TerraformState"":"""", ""PlanetClass"":""High metal content body"", ""Atmosphere"":""thin carbon dioxide atmosphere"", ""AtmosphereType"":""CarbonDioxide"", ""AtmosphereComposition"":[ { ""Name"":""CarbonDioxide"", ""Percent"":99.009911 }, { ""Name"":""SulphurDioxide"", ""Percent"":0.990099 } ], ""Volcanism"":"""", ""MassEM"":0.015033, ""Radius"":1614210.750000, ""SurfaceGravity"":2.299467, ""SurfaceTemperature"":191.011368, ""SurfacePressure"":7667.134766, ""Landable"":true, ""Materials"":[ { ""Name"":""iron"", ""Percent"":23.308260 }, { ""Name"":""nickel"", ""Percent"":17.629391 }, { ""Name"":""sulphur"", ""Percent"":16.632193 }, { ""Name"":""carbon"", ""Percent"":13.985950 }, { ""Name"":""manganese"", ""Percent"":9.626074 }, { ""Name"":""phosphorus"", ""Percent"":8.954046 }, { ""Name"":""zirconium"", ""Percent"":2.706569 }, { ""Name"":""selenium"", ""Percent"":2.603079 }, { ""Name"":""niobium"", ""Percent"":1.592995 }, { ""Name"":""molybdenum"", ""Percent"":1.522016 }, { ""Name"":""ruthenium"", ""Percent"":1.439434 } ], ""Composition"":{ ""Ice"":0.000000, ""Rock"":0.672532, ""Metal"":0.327468 }, ""SemiMajorAxis"":186101865.768433, ""Eccentricity"":0.108020, ""OrbitalInclination"":-4.563992, ""Periapsis"":250.185041, ""OrbitalPeriod"":18410457.372665, ""AscendingNode"":-128.926722, ""MeanAnomaly"":144.565168, ""RotationPeriod"":123456.822256, ""AxialTilt"":-0.160430, ""WasDiscovered"":true, ""WasMapped"":true }";
            var saaScan = @"{ ""timestamp"":""2022-12-08T06:32:56Z"", ""event"":""SAASignalsFound"", ""BodyName"":""TestSystem 4"", ""SystemAddress"":9999999999999, ""BodyID"":5, ""Signals"":[ { ""Type"":""$SAA_SignalType_Biological;"", ""Type_Localised"":""Biological"", ""Count"":7 }, { ""Type"":""$SAA_SignalType_Human;"", ""Type_Localised"":""Human"", ""Count"":4 } ], ""Genuses"":[ { ""Genus"":""$Codex_Ent_Bacterial_Genus_Name;"", ""Genus_Localised"":""Bacterium"" }, { ""Genus"":""$Codex_Ent_Cactoid_Genus_Name;"", ""Genus_Localised"":""Cactoida"" }, { ""Genus"":""$Codex_Ent_Clypeus_Genus_Name;"", ""Genus_Localised"":""Clypeus"" }, { ""Genus"":""$Codex_Ent_Osseus_Genus_Name;"", ""Genus_Localised"":""Osseus"" }, { ""Genus"":""$Codex_Ent_Stratum_Genus_Name;"", ""Genus_Localised"":""Stratum"" }, { ""Genus"":""$Codex_Ent_Shrubs_Genus_Name;"", ""Genus_Localised"":""Frutexa"" }, { ""Genus"":""$Codex_Ent_Tussocks_Genus_Name;"", ""Genus_Localised"":""Tussock"" } ] }";

            var privateObject = new PrivateObject( EDDI.Instance );
            var discoveryMonitor = (DiscoveryMonitor)privateObject.Invoke( nameof(EDDI.ObtainMonitor), "Discovery Monitor" );
            Assert.IsNotNull( discoveryMonitor );
            discoveryMonitor.configuration = new DiscoveryMonitorConfiguration { enableVariantPredictions = enableVariantPredictions };

            // Enable the monitor so that it can handle passed events
            privateObject.Invoke( nameof(EDDI.EnableMonitor), discoveryMonitor.MonitorName() );

            // Simulate a `FSDJump` event to set our current star system
            var events = JournalMonitor.ParseJournalEntry( fsdJump );
            Assert.AreEqual( 1, events.Count );
            privateObject.Invoke( "eventHandler", events[ 0 ] );
            Assert.IsNotNull( discoveryMonitor._currentSystem );
            Assert.AreEqual( "TestSystem", discoveryMonitor._currentSystem.systemname );

            void StarScan ()
            {
                // Simulate a FSS `Scan` event of the star
                events = JournalMonitor.ParseJournalEntry( fssScanStar );
                Assert.AreEqual( 1, events.Count );
                privateObject.Invoke( "eventHandler", events[ 0 ] );
            }

            if ( starFirst )
            {
                StarScan();
            }

            // Simulate a `FSSBodySignals` event (preceeding an FSS Body `Scan`).
            // This should be handled by the Discovery Monitor to capture and hold the surface signal data
            events = JournalMonitor.ParseJournalEntry( fssbodySignal );
            Assert.AreEqual( 1, events.Count );
            privateObject.Invoke( "eventHandler", events[ 0 ] );
            Assert.AreEqual( 1, discoveryMonitor.fssSignalsLibrary?.Count );
            Assert.AreEqual( 9999999999999UL, discoveryMonitor.fssSignalsLibrary?.Last().systemAddress );
            Assert.AreEqual( 5, discoveryMonitor.fssSignalsLibrary?.Last().bodyId );
            Assert.AreEqual( 0, discoveryMonitor.fssSignalsLibrary?.Last().geoCount );
            Assert.AreEqual( 7, discoveryMonitor.fssSignalsLibrary?.Last().bioCount );

            // Simulate a FSS `Scan` event of the body
            // This should be handled by the Discovery Monitor to generate and populate bio predictions if the star HAS already been scanned
            events = JournalMonitor.ParseJournalEntry( fssScanBody );
            Assert.AreEqual( 1, events.Count );
            privateObject.Invoke( "eventHandler", events[ 0 ] );
            Assert.AreEqual( 5, discoveryMonitor._currentBodyId );
            Assert.AreEqual( 1, discoveryMonitor.fssSignalsLibrary?.Count );
            Assert.AreEqual( 9999999999999UL, discoveryMonitor.fssSignalsLibrary?.Last().systemAddress );
            Assert.AreEqual( 5, discoveryMonitor.fssSignalsLibrary?.Last().bodyId );
            Assert.AreEqual( 0, discoveryMonitor.fssSignalsLibrary?.Last().geoCount );
            Assert.AreEqual( 7, discoveryMonitor.fssSignalsLibrary?.Last().bioCount );
            var body = discoveryMonitor._currentSystem.bodies.FirstOrDefault( b => b.bodyId == 5 );
            Assert.IsNotNull( body );

            if ( !starFirst )
            {
                // This should be handled by the Discovery Monitor to generate and populate bio predictions if the star HAS NOT already been scanned
                StarScan();
            }

            Assert.AreEqual( 7, body.surfaceSignals.reportedBiologicalCount );
            Assert.AreEqual( 0, body.surfaceSignals.reportedGeologicalCount );
            Assert.IsNotNull( body.surfaceSignals.biosignals );
            Assert.IsTrue( body.surfaceSignals.biosignals.Count >= 7 );
            Assert.IsTrue( body.surfaceSignals.biosignals.Distinct().Count() == body.surfaceSignals.biosignals.Count );
            Assert.IsTrue( body.surfaceSignals.biosignals.All( s => s.scanState == Exobiology.State.Predicted ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Bacterial ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Cactoid ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Clypeus ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Osseus ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Stratum ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Shrubs ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Tussocks ) );
            Assert.AreEqual( OrganicGenus.Bacterial.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Bacterial )?.value );
            Assert.AreEqual( OrganicGenus.Cactoid.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Cactoid )?.value );
            Assert.AreEqual( OrganicGenus.Clypeus.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Clypeus )?.value );
            Assert.AreEqual( OrganicGenus.Osseus.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Osseus )?.value );
            Assert.AreEqual( OrganicGenus.Stratum.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Stratum )?.value );
            Assert.AreEqual( OrganicGenus.Shrubs.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Shrubs )?.value );
            Assert.AreEqual( OrganicGenus.Tussocks.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Tussocks )?.value );
            Assert.AreEqual( events[ 0 ].timestamp, body.surfaceSignals.lastUpdated );

            // Simulate a `SAASignalsFound` event
            // This should be handled by the Discovery Monitor to replace and confirm bio predictions
            events = JournalMonitor.ParseJournalEntry( saaScan );
            Assert.AreEqual( 1, events.Count );
            privateObject.Invoke( "eventHandler", events[ 0 ] );
            Assert.AreEqual( 5, discoveryMonitor._currentBodyId );
            Assert.AreEqual( 1, discoveryMonitor.fssSignalsLibrary?.Count );
            Assert.AreEqual( 7, body.surfaceSignals.reportedBiologicalCount );
            Assert.AreEqual( 0, body.surfaceSignals.reportedGeologicalCount );
            Assert.IsNotNull( body.surfaceSignals.biosignals );
            Assert.AreEqual( 7, body.surfaceSignals.biosignals.Count );
            Assert.IsTrue( body.surfaceSignals.biosignals.All( s => s.scanState == Exobiology.State.Confirmed ) );
            Assert.IsTrue( body.surfaceSignals.biosignals.All( s => s.samples == 0 ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Bacterial ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Cactoid ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Clypeus ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Osseus ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Stratum ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Shrubs ) );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Count( b => b.genus == OrganicGenus.Tussocks ) );
            Assert.AreEqual( OrganicGenus.Bacterial.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Bacterial )?.value );
            Assert.AreEqual( OrganicGenus.Cactoid.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Cactoid )?.value );
            Assert.AreEqual( OrganicGenus.Clypeus.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Clypeus )?.value );
            Assert.AreEqual( OrganicGenus.Osseus.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Osseus )?.value );
            Assert.AreEqual( OrganicGenus.Stratum.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Stratum )?.value );
            Assert.AreEqual( OrganicGenus.Shrubs.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Shrubs )?.value );
            Assert.AreEqual( OrganicGenus.Tussocks.maximumValue, body.surfaceSignals.biosignals.FirstOrDefault( b => b.genus == OrganicGenus.Tussocks )?.value );
            Assert.AreEqual( events[ 0 ].timestamp, body.surfaceSignals.lastUpdated );

            // // Reset
            // ReSharper disable once AssignNullToNotNullAttribute
            privateObject.SetFieldOrProperty( nameof( EDDI.CurrentStarSystem ), null );
        }

        [TestMethod]
        public void handleScanOrganicEventTest()
        {
            var privateObject = new PrivateObject( EDDI.Instance );
            var _currentSystem = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem6);
            privateObject.SetFieldOrProperty( nameof(EDDI.CurrentStarSystem), _currentSystem );
            var discoveryMonitor = (DiscoveryMonitor)privateObject.Invoke( nameof(EDDI.ObtainMonitor), "Discovery Monitor" );
            Assert.IsNotNull( discoveryMonitor );

            var line = ScanOrganicEvent.SAMPLE;
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual( 1, events.Count );
            var @event = (ScanOrganicEvent)events[0];

            var body = _currentSystem.bodies.FirstOrDefault(b => b.bodyId == 42);
            Assert.IsNotNull( body );

            // Set up an initial prediction for the 1st organic that we will test (but not for the 2nd)
            var bio = body.surfaceSignals.AddBioFromGenus( @event.genus, true );
            Assert.IsTrue( body.surfaceSignals.predicted );

            // Simulate the 2nd scan on the 1st organic, setting a prior sample to jump to the correct number of samples
            bio.sampleCoords.Add( new Tuple<decimal?, decimal?>( 0M, 0M ) );
            discoveryMonitor.PreHandle( @event);
            Assert.AreEqual( body.bodyId, discoveryMonitor._currentBodyId);
            Assert.AreEqual(OrganicGenus.Shrubs, discoveryMonitor._currentGenus);
            Assert.AreEqual(@event.timestamp, body.surfaceSignals?.lastUpdated);
            Assert.IsNotNull( body.surfaceSignals?.biosignals?.Last() );
            Assert.AreEqual( OrganicVariant.Shrubs_05_F, body.surfaceSignals.biosignals.Last().variant );
            Assert.AreEqual( 2, body.surfaceSignals.biosignals.Last().samples );
            Assert.AreEqual( Exobiology.State.SampleInProgress, body.surfaceSignals.biosignals.Last().scanState );
            Assert.AreEqual( OrganicVariant.Shrubs_05_F.species.value , body.surfaceSignals.biosignals.Last().value );
            Assert.AreEqual( 1, @event.numTotal );
            Assert.AreEqual( 1, @event.listRemaining?.Count );
            Assert.AreEqual( body.surfaceSignals.biosignals.Last(), @event.bio );
            Assert.IsFalse( body.surfaceSignals.predicted );

            // Simulate the 3rd scan on the 1st organic with a fresh copy of the same event
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual( 1, events.Count );
            @event = (ScanOrganicEvent)events[0];
            discoveryMonitor.PreHandle( @event );
            Assert.AreEqual( Exobiology.State.SampleComplete, body.surfaceSignals.biosignals.Last().scanState );
            Assert.AreEqual( 0, @event.listRemaining?.Count );
            Assert.AreEqual( body.surfaceSignals.biosignals.Last(), @event.bio );
            Assert.AreEqual( 0, body.surfaceSignals.biosignalsremaining().Count );

            // Scan a 2nd organic which has not been created on the body and test that it is handled gracefully
            var line2 = @"{ ""timestamp"":""2023-05-14T04:22:20Z"", ""event"":""ScanOrganic"", ""ScanType"":""Log"", ""Genus"":""$Codex_Ent_Fonticulus_Genus_Name;"", ""Genus_Localised"":""Fonticulua"", ""Species"":""$Codex_Ent_Fonticulus_02_Name;"", ""Species_Localised"":""Fonticulua Campestris"", ""Variant"":""$Codex_Ent_Fonticulus_02_TTS_Name;"", ""Variant_Localised"":""Fonticulua Campestris - Red"", ""SystemAddress"":10477373803, ""Body"":42 }";
            events = JournalMonitor.ParseJournalEntry( line2 );
            Assert.AreEqual( 1, events.Count );
            @event = (ScanOrganicEvent)events[ 0 ];
            discoveryMonitor.PreHandle( @event );
            Assert.AreEqual( body.bodyId, discoveryMonitor._currentBodyId );
            Assert.AreEqual( OrganicGenus.Fonticulus, discoveryMonitor._currentGenus );
            Assert.IsNotNull( body.surfaceSignals?.biosignals?.Last() );
            Assert.AreEqual( OrganicVariant.Fonticulus_02_TTS, body.surfaceSignals.biosignals.Last().variant );
            Assert.AreEqual( 1, body.surfaceSignals.biosignals.Last().samples );
            Assert.AreEqual( Exobiology.State.SampleStarted, body.surfaceSignals.biosignals.Last().scanState );
            Assert.AreEqual( OrganicVariant.Fonticulus_02_TTS.species.value, body.surfaceSignals.biosignals.Last().value );
            Assert.AreEqual( 2, @event.numTotal );
            Assert.AreEqual( 1, @event.listRemaining?.Count );
            Assert.AreEqual( body.surfaceSignals.biosignals.Last(), @event.bio );
            Assert.AreEqual( 1, body.surfaceSignals.biosignalsremaining().Count );
        }
    }
}
