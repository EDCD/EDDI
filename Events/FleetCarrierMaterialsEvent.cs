using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FleetCarrierMaterialsEvent : Event
    {
        public const string NAME = "Fleet carrier materials";
        public const string DESCRIPTION = "Triggered when the FCMaterials.json file has been updated";
        public const string SAMPLE = @"{ ""timestamp"":""2022-03-24T11:37:28Z"", ""event"":""FCMaterials"", ""MarketID"":3700020480, ""CarrierName"":""ralph's carrier"", ""CarrierID"":""VHT-51W"", ""Items"":[ { ""id"":128961556, ""Name"":""$californium_name;"", ""Name_Localised"":""Californium"", ""Price"":74000, ""Stock"":0, ""Demand"":1 }, { ""id"":128961524, ""Name"":""$aerogel_name;"", ""Name_Localised"":""Aerogel"", ""Price"":500, ""Stock"":26, ""Demand"":0 }, { ""id"":128972334, ""Name"":""$meetingminutes_name;"", ""Name_Localised"":""Meeting Minutes"", ""Price"":1000, ""Stock"":0, ""Demand"":1 }, { ""id"":128962572, ""Name"":""$rdx_name;"", ""Name_Localised"":""RDX"", ""Price"":387, ""Stock"":0, ""Demand"":9 }, { ""id"":128972304, ""Name"":""$culinaryrecipes_name;"", ""Name_Localised"":""Culinary Recipes"", ""Price"":1000, ""Stock"":20, ""Demand"":0 }, { ""id"":128961527, ""Name"":""$chemicalcatalyst_name;"", ""Name_Localised"":""Chemical Catalyst"", ""Price"":400, ""Stock"":18, ""Demand"":0 } ] }";

        // Not intended to be user facing

        public long carrierId { get; private set; }

        public string carrierName { get; private set; }

        public string callsign { get; private set; }

        public FCMaterialsInfo info { get; private set; }

        public FleetCarrierMaterialsEvent(DateTime timestamp, long carrierId, string carrierName, string callsign, FCMaterialsInfo info) : base(timestamp, NAME)
        {
            this.carrierId = carrierId;
            this.carrierName = carrierName;
            this.callsign = callsign;
            this.info = info;
        }
    }
}