using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ColonisationContributionEvent : Event
    {
        public const string NAME = "Colonisation contribution";
        public const string DESCRIPTION = "Triggered when contributing commodities towards construction in colonised star system";
        public static readonly string[] SAMPLES =
        [
            @"{ ""timestamp"":""2025-04-11T08:30:20Z"", ""event"":""ColonisationContribution"", ""MarketID"":3950995714, ""Contributions"":[ { ""Name"":""$CeramicComposites_name;"", ""Name_Localised"":""Ceramic Composites"", ""Amount"":513 }, { ""Name"":""$InsulatingMembrane_name;"", ""Name_Localised"":""Insulating Membrane"", ""Amount"":263 }, { ""Name"":""$LiquidOxygen_name;"", ""Name_Localised"":""Liquid oxygen"", ""Amount"":8 } ] }",
            @"{ ""timestamp"":""2025-04-11T08:16:05Z"", ""event"":""ColonisationContribution"", ""MarketID"":3950995714, ""Contributions"":[ { ""Name"":""$Titanium_name;"", ""Name_Localised"":""Titanium"", ""Amount"":56 }, { ""Name"":""$Water_name;"", ""Name_Localised"":""Water"", ""Amount"":717 } ] }",
            @"{ ""timestamp"":""2025-04-11T08:01:06Z"", ""event"":""ColonisationContribution"", ""MarketID"":3955317762, ""Contributions"":[ { ""Name"":""$LiquidOxygen_name;"", ""Name_Localised"":""Liquid oxygen"", ""Amount"":677 } ] }"
        ];

        [PublicAPI( "The numeric market ID of the location receiving your contribution" )]
        public long marketID { get; private set; }

        [PublicAPI( "The commodities and amounts you have contributed" )]
        public List<CommodityAmount> contributions { get; private set; }

        public ColonisationContributionEvent ( DateTime timestamp, long marketID, List<CommodityAmount> commodityAmounts ) : base( timestamp, NAME )
        {
            this.marketID = marketID;
            this.contributions = commodityAmounts;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var marketID = JsonParsing.getLong( data, "MarketID" );
            var commodityAmounts = new List<CommodityAmount>();
            if ( data.TryGetValue( "Contributions", out var val ) )
            {
                if ( val is List<object> listVal )
                {
                    foreach ( var contributionVal in listVal )
                    {
                        if ( contributionVal is IDictionary<string, object> contribution )
                        {
                            var commodity = CommodityDefinition.FromEDName( JsonParsing.getString(contribution, "Name") );
                            if ( commodity != null )
                            {
                                commodity.fallbackLocalizedName =
                                    JsonParsing.getString( contribution, "Name_Localised" );
                                var amount = JsonParsing.getInt( contribution, "Amount" );
                                commodityAmounts.Add( new CommodityAmount( commodity, amount ) );
                            }
                        }
                    }
                    events.Add( new ColonisationContributionEvent( timestamp, marketID, commodityAmounts ) { raw = line, fromLoad = false } );
                }
            }
            return true;
        }
    }
}
