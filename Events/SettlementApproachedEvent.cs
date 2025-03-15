using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [ PublicAPI ]
    public class SettlementApproachedEvent : Event
    {
        public const string NAME = "Settlement approached";
        public const string DESCRIPTION = "Triggered when you approach a settlement";
        public static readonly string[] SAMPLES =
            {
                @"{ ""timestamp"":""2020-10-12T08:55:27Z"", ""event"":""ApproachSettlement"", ""Name"":""Tusi Beacon ++"", ""MarketID"":3511645696, ""SystemAddress"":1458309141194, ""BodyID"":6, ""BodyName"":""Makalu"", ""Latitude"":5.172286, ""Longitude"":-166.500275 }",
                @"{ ""timestamp"":""2024-04-11T06:28:35Z"", ""event"":""ApproachSettlement"", ""Name"":""Cunningham Depot"", ""MarketID"":3789728256, ""StationFaction"":{ ""Name"":""Brazilian League of Pilots"" }, ""StationGovernment"":""$government_Confederacy;"", ""StationGovernment_Localised"":""Confederacy"", ""StationServices"":[ ""dock"", ""autodock"", ""blackmarket"", ""commodities"", ""contacts"", ""exploration"", ""missions"", ""outfitting"", ""rearm"", ""refuel"", ""repair"", ""tuning"", ""engineer"", ""missionsgenerated"", ""flightcontroller"", ""stationoperations"", ""powerplay"", ""searchrescue"", ""stationMenu"", ""livery"", ""socialspace"", ""bartender"", ""vistagenomics"", ""pioneersupplies"", ""apexinterstellar"", ""frontlinesolutions"" ], ""StationEconomy"":""$economy_Military;"", ""StationEconomy_Localised"":""Military"", ""StationEconomies"":[ { ""Name"":""$economy_Military;"", ""Name_Localised"":""Military"", ""Proportion"":1.000000 } ], ""SystemAddress"":147949865307, ""BodyID"":52, ""BodyName"":""HIP 25679 11 g"", ""Latitude"":-38.485172, ""Longitude"":106.608040 }",
                @"{ ""timestamp"":""2019-09-26T05:43:00Z"", ""event"":""ApproachSettlement"", ""Name"":""$Ancient:#index=1;"", ""Name_Localised"":""Ancient Ruins (1)"", ""SystemAddress"":3755873388891, ""BodyID"":75, ""BodyName"":""Synuefe ZL-J d10-109 E 3"", ""Latitude"":39.921276, ""Longitude"":-109.192154 }"
            };

        [ PublicAPI( "The name of the settlement" ) ]
        public string name { get; private set; }

        [ PublicAPI( "The name of the body containing the settlement" ) ]
        public string bodyname { get; private set; }

        [PublicAPI( "The numeric ID of the body containing the settlement" )]
        public long? bodyId { get; private set; }

        [PublicAPI( "The numeric ID of the station, if the settlement is a station" )]
        public long? marketId { get; private set; } // Tourist beacons and guardian structures are reported as settlements without MarketID 

        [PublicAPI( "The numeric system address of the star system" )]
        public ulong systemAddress { get; private set; }

        [ PublicAPI( "The latitude coordinate of the settlement (if given)" ) ]
        public decimal? latitude { get; private set; }

        [ PublicAPI( "The longitude coordinate of the settlement (if given)" ) ]
        public decimal? longitude { get; private set; }

        [PublicAPI( "The economy of the settlement the commander is approaching, when applicable" )]
        public string economy => economyShares.Count > 0 ? ( economyShares[ 0 ]?.economy ?? Economy.None ).localizedName : Economy.None.localizedName;

        [PublicAPI( "The secondary economy of the settlement the commander is approaching, when applicable" )]
        public string secondeconomy => economyShares.Count > 1 ? ( economyShares[ 1 ]?.economy ?? Economy.None ).localizedName : Economy.None.localizedName;

        [ PublicAPI( "A list of possible services available at the settlement (when the settlement is a station)" ) ]
        public List<string> stationservices => stationServices.Select( s => s.localizedName ).ToList();

        // Faction properties

        [PublicAPI( "The faction controlling the settlement the commander is approaching, when applicable" )]
        public string faction => controllingFaction?.name;

        [PublicAPI( "The superpower allegiance of the settlement the commander is approaching, when applicable" )]
        public string allegiance => ( controllingFaction?.Allegiance ?? Superpower.None ).localizedName;

        [PublicAPI( "The government of the settlement the commander is approaching, when applicable" )]
        public string government => ( controllingFaction?.Government ?? Government.None ).localizedName;

        // Not intended to be user facing

        public Faction controllingFaction { get; private set; }

        public List<StationService> stationServices { get; private set; }

        public List<EconomyShare> economyShares { get; private set; }

        public SettlementApproachedEvent(DateTime timestamp, string settlementName, string localizedName, long? marketId, Faction controllingFaction, List<StationService> stationServices, List<EconomyShare> economyShares, ulong systemAddress, string bodyName, long? bodyId, decimal? latitude, decimal? longitude) : base(timestamp, NAME)
        {
            // Prefer our own localization of generic settlement names when available
            if ( settlementName.Contains("$") && !string.IsNullOrEmpty( localizedName ) )
            {
                var signalName = SignalSource.FromEDName( settlementName );
                signalName.fallbackLocalizedName = localizedName;
                this.name = signalName.localizedName;
            }
            else
            {
                this.name = settlementName;
            }

            this.marketId = marketId;
            this.controllingFaction = controllingFaction;
            this.stationServices = stationServices;
            this.economyShares = economyShares;
            this.systemAddress = systemAddress;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.latitude = latitude;
            this.longitude = longitude;
        }
    }
}
