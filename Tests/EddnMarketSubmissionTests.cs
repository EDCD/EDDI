using EddiEddnResponder;
using EddiEddnResponder.Schemas;
using EddiEddnResponder.Sender;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using Tests.Properties;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class EddnMarketSubmissionTests : TestBase
    {
        [TestInitialize]
        public void Start () => MakeSafe();

        private static ISchema Schema ( string family ) => family switch
        {
            "commodity" => new CommoditySchema(),
            "outfitting" => new OutfittingSchema(),
            "shipyard" => new ShipyardSchema(),
            "fcmaterials" => new FCMaterialsSchema(),
            _ => throw new ArgumentException( family )
        };

        private static JObject CapiData ( string family ) => DeserializeJsonResource<JObject>( family switch
        {
            "commodity" => Resources.capi_market_Libby_Horizons,
            "outfitting" or "shipyard" => Resources.capi_shipyard_Abasheli_Barracks,
            "fcmaterials" => Resources.capi_market_fleet_carrier,
            _ => throw new ArgumentException( family )
        } );

        [TestMethod]
        [DataRow( "commodity" )]
        [DataRow( "outfitting" )]
        [DataRow( "shipyard" )]
        [DataRow( "fcmaterials" )]
        public void RefreshedOrChangedJournalSnapshotAtSameMarketRemainsEligible ( string family )
        {
            var schema = Schema( family );
            var sender = new EDDNSender { unitTesting = true };
            var state = new EDDNState();
            for ( var observation = 0; observation < 3; observation++ )
            {
                var json = DeserializeJsonResource<string>( family switch
                {
                    "commodity" => Resources.market,
                    "outfitting" => Resources.Outfitting,
                    "shipyard" => Resources.Shipyard,
                    "fcmaterials" => Resources.FCMaterials,
                    _ => throw new ArgumentException( family )
                } );
                var data = Deserializtion.DeserializeData( json );
                if ( observation == 1 ) { data["timestamp"] = JsonParsing.getDateTime( "timestamp", data ).AddSeconds( 1 ); }
                if ( observation == 2 )
                {
                    var nameField = family == "fcmaterials" ? "CarrierName" : "StationName";
                    data[nameField] = data[nameField] + " revised";
                }
                Assert.IsTrue( schema.Handle( schema.edTypes[0], ref data, state, sender ) );
            }
            Assert.HasCount( 3, sender.sentMessages );
        }

        [TestMethod]
        [DataRow( "commodity" )]
        [DataRow( "outfitting" )]
        [DataRow( "shipyard" )]
        [DataRow( "fcmaterials" )]
        public void RefreshedCapiSnapshotAtSameMarketRemainsEligible ( string family )
        {
            var schema = (ICapiSchema)Schema( family );
            var sender = new EDDNSender { unitTesting = true };
            var state = new EDDNState();
            var profile = JObject.Parse( """{"lastSystem":{"name":"Sol"}}""" );
            for ( var seconds = 0; seconds < 2; seconds++ )
            {
                var data = CapiData( family );
                data["timestamp"] = data["timestamp"].ToObject<DateTime>().AddSeconds( seconds );
                var shipyard = family is "outfitting" or "shipyard";
                Assert.IsNotNull( schema.Handle( profile, shipyard ? null : data, shipyard ? data : null, null, state, sender ) );
            }
            Assert.HasCount( 2, sender.sentMessages );
        }

        [TestMethod]
        public void JournalAndCapiCarrierMaterialsDoNotSuppressEachOther ()
        {
            var schema = new FCMaterialsSchema();
            var sender = new EDDNSender { unitTesting = true };
            var state = new EDDNState();
            var capi = CapiData( "fcmaterials" );
            var journal = Deserializtion.DeserializeData( DeserializeJsonResource<string>( Resources.FCMaterials ) );
            Assert.IsNotNull( schema.Handle( null, capi, null, null, state, sender ) );
            Assert.IsTrue( schema.Handle( "FCMaterials", ref journal, state, sender ) );
            Assert.HasCount( 2, sender.sentMessages );
            Assert.AreEqual( "https://eddn.edcd.io/schemas/fcmaterials_capi/1", sender.sentMessages[0].schema );
            Assert.AreEqual( "https://eddn.edcd.io/schemas/fcmaterials_journal/1", sender.sentMessages[1].schema );
        }
    }
}
