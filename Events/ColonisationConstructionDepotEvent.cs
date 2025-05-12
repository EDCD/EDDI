using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ColonisationConstructionDepotEvent : Event
    {
        public const string NAME = "Colonisation construction depot";
        public const string DESCRIPTION = "Triggered when progress is updated at the colonisation construction depot where you are docked";
        public static readonly string[] SAMPLES =
        {
            @"{ ""timestamp"":""2025-04-12T22:45:43Z"", ""event"":""ColonisationConstructionDepot"", ""MarketID"":3958371586, ""ConstructionProgress"":0.902384, ""ConstructionComplete"":false, ""ConstructionFailed"":false, ""ResourcesRequired"":[ { ""Name"":""$aluminium_name;"", ""Name_Localised"":""Aluminium"", ""RequiredAmount"":483, ""ProvidedAmount"":483, ""Payment"":3239 }, { ""Name"":""$ceramiccomposites_name;"", ""Name_Localised"":""Ceramic Composites"", ""RequiredAmount"":546, ""ProvidedAmount"":330, ""Payment"":724 }, { ""Name"":""$cmmcomposite_name;"", ""Name_Localised"":""CMM Composite"", ""RequiredAmount"":4439, ""ProvidedAmount"":4439, ""Payment"":6788 }, { ""Name"":""$computercomponents_name;"", ""Name_Localised"":""Computer Components"", ""RequiredAmount"":62, ""ProvidedAmount"":62, ""Payment"":1112 }, { ""Name"":""$copper_name;"", ""Name_Localised"":""Copper"", ""RequiredAmount"":248, ""ProvidedAmount"":248, ""Payment"":1050 }, { ""Name"":""$foodcartridges_name;"", ""Name_Localised"":""Food Cartridges"", ""RequiredAmount"":93, ""ProvidedAmount"":93, ""Payment"":673 }, { ""Name"":""$fruitandvegetables_name;"", ""Name_Localised"":""Fruit and Vegetables"", ""RequiredAmount"":48, ""ProvidedAmount"":48, ""Payment"":865 }, { ""Name"":""$insulatingmembrane_name;"", ""Name_Localised"":""Insulating Membrane"", ""RequiredAmount"":346, ""ProvidedAmount"":152, ""Payment"":11788 }, { ""Name"":""$liquidoxygen_name;"", ""Name_Localised"":""Liquid oxygen"", ""RequiredAmount"":1848, ""ProvidedAmount"":1848, ""Payment"":2260 }, { ""Name"":""$medicaldiagnosticequipment_name;"", ""Name_Localised"":""Medical Diagnostic Equipment"", ""RequiredAmount"":13, ""ProvidedAmount"":13, ""Payment"":3609 }, { ""Name"":""$nonlethalweapons_name;"", ""Name_Localised"":""Non-Lethal Weapons"", ""RequiredAmount"":12, ""ProvidedAmount"":12, ""Payment"":2503 }, { ""Name"":""$polymers_name;"", ""Name_Localised"":""Polymers"", ""RequiredAmount"":500, ""ProvidedAmount"":500, ""Payment"":682 }, { ""Name"":""$powergenerators_name;"", ""Name_Localised"":""Power Generators"", ""RequiredAmount"":20, ""ProvidedAmount"":20, ""Payment"":3072 }, { ""Name"":""$semiconductors_name;"", ""Name_Localised"":""Semiconductors"", ""RequiredAmount"":66, ""ProvidedAmount"":66, ""Payment"":1526 }, { ""Name"":""$steel_name;"", ""Name_Localised"":""Steel"", ""RequiredAmount"":6605, ""ProvidedAmount"":6605, ""Payment"":5057 }, { ""Name"":""$superconductors_name;"", ""Name_Localised"":""Superconductors"", ""RequiredAmount"":115, ""ProvidedAmount"":115, ""Payment"":7657 }, { ""Name"":""$titanium_name;"", ""Name_Localised"":""Titanium"", ""RequiredAmount"":5492, ""ProvidedAmount"":3785, ""Payment"":5360 }, { ""Name"":""$water_name;"", ""Name_Localised"":""Water"", ""RequiredAmount"":715, ""ProvidedAmount"":715, ""Payment"":662 }, { ""Name"":""$waterpurifiers_name;"", ""Name_Localised"":""Water Purifiers"", ""RequiredAmount"":36, ""ProvidedAmount"":36, ""Payment"":849 } ] }",
            @"{ ""timestamp"":""2025-04-11T08:34:36Z"", ""event"":""ColonisationConstructionDepot"", ""MarketID"":3950995714, ""ConstructionProgress"":0.241349, ""ConstructionComplete"":false, ""ConstructionFailed"":false, ""ResourcesRequired"":[ { ""Name"":""$aluminium_name;"", ""Name_Localised"":""Aluminium"", ""RequiredAmount"":502, ""ProvidedAmount"":0, ""Payment"":3239 }, { ""Name"":""$ceramiccomposites_name;"", ""Name_Localised"":""Ceramic Composites"", ""RequiredAmount"":513, ""ProvidedAmount"":513, ""Payment"":724 }, { ""Name"":""$cmmcomposite_name;"", ""Name_Localised"":""CMM Composite"", ""RequiredAmount"":4368, ""ProvidedAmount"":0, ""Payment"":6788 }, { ""Name"":""$computercomponents_name;"", ""Name_Localised"":""Computer Components"", ""RequiredAmount"":63, ""ProvidedAmount"":63, ""Payment"":1112 }, { ""Name"":""$copper_name;"", ""Name_Localised"":""Copper"", ""RequiredAmount"":234, ""ProvidedAmount"":0, ""Payment"":1050 }, { ""Name"":""$foodcartridges_name;"", ""Name_Localised"":""Food Cartridges"", ""RequiredAmount"":91, ""ProvidedAmount"":0, ""Payment"":673 }, { ""Name"":""$fruitandvegetables_name;"", ""Name_Localised"":""Fruit and Vegetables"", ""RequiredAmount"":51, ""ProvidedAmount"":0, ""Payment"":865 }, { ""Name"":""$insulatingmembrane_name;"", ""Name_Localised"":""Insulating Membrane"", ""RequiredAmount"":350, ""ProvidedAmount"":263, ""Payment"":11788 }, { ""Name"":""$liquidoxygen_name;"", ""Name_Localised"":""Liquid oxygen"", ""RequiredAmount"":1711, ""ProvidedAmount"":8, ""Payment"":2260 }, { ""Name"":""$medicaldiagnosticequipment_name;"", ""Name_Localised"":""Medical Diagnostic Equipment"", ""RequiredAmount"":13, ""ProvidedAmount"":13, ""Payment"":3609 }, { ""Name"":""$nonlethalweapons_name;"", ""Name_Localised"":""Non-Lethal Weapons"", ""RequiredAmount"":12, ""ProvidedAmount"":12, ""Payment"":2503 }, { ""Name"":""$polymers_name;"", ""Name_Localised"":""Polymers"", ""RequiredAmount"":515, ""ProvidedAmount"":0, ""Payment"":682 }, { ""Name"":""$powergenerators_name;"", ""Name_Localised"":""Power Generators"", ""RequiredAmount"":20, ""ProvidedAmount"":0, ""Payment"":3072 }, { ""Name"":""$semiconductors_name;"", ""Name_Localised"":""Semiconductors"", ""RequiredAmount"":71, ""ProvidedAmount"":0, ""Payment"":1526 }, { ""Name"":""$steel_name;"", ""Name_Localised"":""Steel"", ""RequiredAmount"":6590, ""ProvidedAmount"":2288, ""Payment"":5057 }, { ""Name"":""$superconductors_name;"", ""Name_Localised"":""Superconductors"", ""RequiredAmount"":107, ""ProvidedAmount"":0, ""Payment"":7657 }, { ""Name"":""$titanium_name;"", ""Name_Localised"":""Titanium"", ""RequiredAmount"":5390, ""ProvidedAmount"":1240, ""Payment"":5360 }, { ""Name"":""$water_name;"", ""Name_Localised"":""Water"", ""RequiredAmount"":717, ""ProvidedAmount"":717, ""Payment"":662 }, { ""Name"":""$waterpurifiers_name;"", ""Name_Localised"":""Water Purifiers"", ""RequiredAmount"":37, ""ProvidedAmount"":37, ""Payment"":849 } ] }"
        };

        [PublicAPI( "The numeric market ID of the construction location" )]
        public long marketID { get; private set; }

        [PublicAPI("The percent progress towards completion of the construction location")]
        public decimal progress { get; private set; }

        [PublicAPI("True if construction is completed")]
        public bool isCompleted { get; private set; }

        [PublicAPI( "True if construction is failed" )]
        public bool isFailed { get; private set; }

        [PublicAPI( "The commodities and amounts needed to complete construction, ordered from most needed to least" )]
        public List<CommodityAmount> needs { get; private set; }

        public ColonisationConstructionDepotEvent ( DateTime timestamp, long marketID, decimal progress, bool constructionComplete, bool constructionFailed, List<CommodityAmount> requiredResources ) : base( timestamp, NAME )
        {
            this.marketID = marketID;
            this.progress = progress;
            this.isCompleted = constructionComplete;
            this.isFailed = constructionFailed;
            this.needs = requiredResources.OrderByDescending(r => r.amount).ToList();
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var marketID = JsonParsing.getLong( data, "MarketID" );
            var progress = JsonParsing.getDecimal( data, "ConstructionProgress" ) * 100; // Convert from 0-1 to 0-100
            var constructionComplete = JsonParsing.getBool( data, "ConstructionComplete" );
            var constructionFailed = JsonParsing.getBool( data, "ConstructionFailed" );

            var requiredResources = new List<CommodityAmount>();
            if ( data.TryGetValue( "ResourcesRequired", out var val ) )
            {
                if ( val is List<object> listVal )
                {
                    foreach ( var resourceVal in listVal )
                    {
                        if ( resourceVal is IDictionary<string, object> resource )
                        {
                            var commodity = CommodityDefinition.FromEDName( JsonParsing.getString( resource, "Name" ) );
                            if ( commodity != null )
                            {
                                commodity.fallbackLocalizedName = JsonParsing.getString( resource, "Name_Localised" );
                                var amount = JsonParsing.getInt( resource, "RequiredAmount" ) -
                                             JsonParsing.getInt( resource, "ProvidedAmount" );
                                if ( amount > 0 )
                                {
                                    requiredResources.Add( new CommodityAmount( commodity, amount ) );
                                }
                            }
                        }
                    }

                    if ( lastMarketID != marketID || requiredResources.Sum( r => r.amount ) != lastRequiredResourceAmount )
                    {
                        events.Add( new ColonisationConstructionDepotEvent( timestamp, marketID, progress, constructionComplete, constructionFailed, requiredResources )
                        {
                            raw = line,
                            fromLoad = fromLogLoad
                        } );
                    }

                    // Set up suppression in case of repetitious and unchanged data.
                    lastMarketID = marketID;
                    lastRequiredResourceAmount = requiredResources.Sum( r => r.amount );
                }
            }
            return true;
        }

        private static long lastMarketID { get; set; }
        private static int lastRequiredResourceAmount { get; set; }
    }
}
