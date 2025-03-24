using EddiDataDefinitions;
using EddiEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class MetaVariablesTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestGalnetNewsPublishedEvent()
        {
            var entry = new KeyValuePair<string, Type>("Galnet news published", typeof(GalnetNewsPublishedEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(7, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].category"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].content"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].id"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].published"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].read"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"items[\<index\>].title"));
            Assert.IsNotNull(cottleVars.TrueForAll(v => v.value == null));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(7, vaVars.Count);
            var category = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> category" );
            Assert.IsNotNull( category );
            Assert.AreEqual(typeof(string), category.variableType);
            var content = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> content" );
            Assert.IsNotNull( content );
            Assert.AreEqual(typeof(string), content.variableType);
            var id = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> id" );
            Assert.IsNotNull( id );
            Assert.AreEqual(typeof(string), id.variableType);
            var publishedDate = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> published" );
            Assert.IsNotNull( publishedDate );
            Assert.AreEqual(typeof(DateTime), publishedDate.variableType);
            var read = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> read" );
            Assert.IsNotNull( read );
            Assert.AreEqual(typeof(bool), read.variableType);
            var title = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items \<index\> title" );
            Assert.IsNotNull( title );
            Assert.AreEqual(typeof(string), title.variableType);
            var items = vaVars.FirstOrDefault( k => k.key == @"EDDI galnet news published items" );
            Assert.IsNotNull( items );
            Assert.AreEqual(typeof( int ), items.variableType);
            Assert.IsTrue(vaVars.TrueForAll(v => v.value == null));
        }

        [TestMethod]
        public void TestSRVTurretDeployableEvent()
        {
            var entry = new KeyValuePair<string, Type>("SRV turret deployable", typeof(SRVTurretDeployableEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(1, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"deployable")?.key);
            Assert.IsTrue(cottleVars.TrueForAll(v => v.value == null));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(1, vaVars.Count);
            var var = vaVars.FirstOrDefault( k => k.key == @"EDDI srv turret deployable" );
            Assert.IsNotNull(var);
            Assert.AreEqual(typeof(bool), var.variableType);
            Assert.IsTrue(vaVars.TrueForAll(v => v.value == null));
        }

        [TestMethod]
        public void TestExplorationDataSoldEvent()
        {
            var entry = new KeyValuePair<string, Type>("Exploration data sold", typeof(ExplorationDataSoldEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(5, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"systems"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == @"systems[\<index\>]"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "reward"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "bonus"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "total"));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(5, vaVars.Count);
            var index = vaVars.FirstOrDefault( k => k.key == "EDDI exploration data sold systems \\<index\\>" );
            Assert.IsNotNull( index );
            Assert.AreEqual(typeof(string), index.variableType);
            var systems = vaVars.FirstOrDefault( k => k.key == "EDDI exploration data sold systems" );
            Assert.IsNotNull( systems );
            Assert.AreEqual(typeof( int ), systems.variableType);
            var reward = vaVars.FirstOrDefault( k => k.key == "EDDI exploration data sold reward" );
            Assert.IsNotNull( reward );
            Assert.AreEqual(typeof(decimal), reward.variableType);
            var bonus = vaVars.FirstOrDefault( k => k.key == "EDDI exploration data sold bonus" );
            Assert.IsNotNull( bonus );
            Assert.AreEqual(typeof(decimal), bonus.variableType);
            var total = vaVars.FirstOrDefault( k => k.key == "EDDI exploration data sold total" );
            Assert.IsNotNull( total );
            Assert.AreEqual(typeof(decimal), total.variableType);
        }

        [TestMethod]
        public void TestDiscoveryScanEvent()
        {
            var entry = new KeyValuePair<string, Type>("Discovery scan", typeof(DiscoveryScanEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(2, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "totalbodies"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "nonbodies"));
            Assert.IsNull(cottleVars.FirstOrDefault(k => k.key == "progress"));
            Assert.IsTrue(cottleVars.TrueForAll(v => v.value == null));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(2, vaVars.Count);
            var totalbodies = vaVars.FirstOrDefault( k => k.key == "EDDI discovery scan totalbodies" );
            Assert.IsNotNull( totalbodies );
            Assert.AreEqual(typeof(int), totalbodies.variableType);
            var nonbodies = vaVars.FirstOrDefault( k => k.key == "EDDI discovery scan nonbodies" );
            Assert.IsNotNull( nonbodies );
            Assert.AreEqual(typeof(int), nonbodies.variableType);
            Assert.IsNull(vaVars.FirstOrDefault(k => k.key == "EDDI discovery scan progress")?.variableType);
        }

        [TestMethod]
        public void TestAsteroidProspectedEvent()
        {
            var entry = new KeyValuePair<string, Type>("Asteroid prospected", typeof(AsteroidProspectedEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(6, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodities"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodities[\\<index\\>].commodity"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodities[\\<index\\>].percentage"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "materialcontent"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "remaining"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "motherlode"));

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(6, vaVars.Count);
            var commodity = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected commodities \\<index\\> commodity" );
            Assert.IsNotNull( commodity );
            Assert.AreEqual(typeof(string), commodity.variableType);
            var percentage = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected commodities \\<index\\> percentage" );
            Assert.IsNotNull( percentage );
            Assert.AreEqual(typeof(decimal), percentage.variableType);
            var commodities = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected commodities" );
            Assert.IsNotNull( commodities );
            Assert.AreEqual(typeof(int), commodities.variableType);
            var materialcontent = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected materialcontent" );
            Assert.IsNotNull( materialcontent );
            Assert.AreEqual(typeof(string), materialcontent.variableType);
            var remaining = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected remaining" );
            Assert.IsNotNull( remaining );
            Assert.AreEqual(typeof(decimal), remaining.variableType);
            var motherlode = vaVars.FirstOrDefault( k => k.key == "EDDI asteroid prospected motherlode" );
            Assert.IsNotNull( motherlode );
            Assert.AreEqual(typeof(string), motherlode.variableType);
        }

        [TestMethod]
        public void TestCommodityEjectedEvent()
        {
            var entry = new KeyValuePair<string, Type>("Commodity ejected", typeof(CommodityEjectedEvent));
            var vars = new MetaVariables(entry.Value, null).Results;

            Assert.AreEqual(4, vars.Count);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "commodity")?.description);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "amount")?.description);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "missionid")?.description);
            Assert.IsNotNull(vars.FirstOrDefault(k => k.keysPath.Last() == "abandoned")?.description);

            var cottleVars = vars.AsCottleVariables();
            Assert.AreEqual(4, cottleVars.Count);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodity"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "amount"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "missionid"));
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "abandoned"));

            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "commodity")?.description);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "amount")?.description);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "missionid")?.description);
            Assert.IsNotNull(cottleVars.FirstOrDefault(k => k.key == "abandoned")?.description);

            var vaVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);
            Assert.AreEqual(4, vaVars.Count);
            var commodity = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected commodity" );
            Assert.IsNotNull( commodity );
            Assert.AreEqual(typeof(string), commodity.variableType);
            var amount = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected amount" );
            Assert.IsNotNull( amount );
            Assert.AreEqual(typeof(int), amount.variableType);
            var missionid = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected missionid" );
            Assert.IsNotNull( missionid );
            Assert.AreEqual(typeof(decimal), missionid.variableType);
            var abandoned = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected abandoned" );
            Assert.IsNotNull( abandoned );
            Assert.AreEqual(typeof(bool), abandoned.variableType);

            var ejectedcommodity = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected commodity" );
            Assert.IsNotNull( ejectedcommodity );
            Assert.AreEqual("The name of the commodity ejected", ejectedcommodity.description);
            var ejectedamount = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected amount" );
            Assert.IsNotNull( ejectedamount );
            Assert.AreEqual("The amount of commodity ejected", ejectedamount.description);
            var ejectedmissionid = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected missionid" );
            Assert.IsNotNull( ejectedmissionid );
            Assert.AreEqual("ID of the mission-related commodity, if applicable", ejectedmissionid.description);
            var ejectedabandoned = vaVars.FirstOrDefault( k => k.key == "EDDI commodity ejected abandoned" );
            Assert.IsNotNull( ejectedabandoned );
            Assert.AreEqual("True if the cargo has been abandoned", ejectedabandoned.description);
        }

        [ TestMethod ]
        public void TestRouteDetailsEvent ()
        {
            dynamic mockVaProxy = new MockVAProxy();
            var entry = new KeyValuePair<string, Type>( "Route details", typeof(RouteDetailsEvent) );
            var vars = new MetaVariables( entry.Value, new RouteDetailsEvent(DateTime.MinValue, "set", "Shinrarta Dezhra", 3932277478106U, "Jameson Memorial", 128666762, new NavWaypointCollection(), 0, null ) ).Results;
            var vaVars = vars.AsVoiceAttackVariables( string.Empty, entry.Key );
            try
            {
                vaVars.ForEach( v => v.Set( mockVaProxy ) );
            }
            catch ( Exception e )
            {
                Assert.Fail($"{e.Message}: {JsonConvert.SerializeObject(e)}");
                throw;
            }
        }
    }
}
