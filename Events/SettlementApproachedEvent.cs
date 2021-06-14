using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SettlementApproachedEvent : Event
    {
        public const string NAME = "Settlement approached";
        public const string DESCRIPTION = "Triggered when you approach a settlement";
        public const string SAMPLE = @"{ ""timestamp"":""2018-11-04T03:11:56Z"", ""event"":""ApproachSettlement"", ""Name"":""Bulmer Enterprise"", ""MarketID"":3510380288, ""Latitude"":-23.121552, ""Longitude"":-98.177559 }";

        [PublicAPI("The name of the settlement")]
        public string name => settlementName.localizedName;

        [PublicAPI("The name of the body containing the settlement")]
        public string bodyname { get; private set; }

        [PublicAPI("The latitude coordinate of the settlement (if given)")]
        public decimal? latitude { get; private set; }

        [PublicAPI("The longitude coordinate of the settlement (if given)")]
        public decimal? longitude { get; private set; }

        // Not intended to be user facing

        public long? marketId { get; private set; }

        public long systemAddress { get; private set; }

        public long? bodyId { get; private set; }

        public SignalSource settlementName { get; private set; }

        public SettlementApproachedEvent(DateTime timestamp, SignalSource settlementName, long? marketId, long systemAddress, string bodyName, long? bodyId, decimal? latitude, decimal? longitude) : base(timestamp, NAME)
        {
            this.settlementName = settlementName;
            this.marketId = marketId;
            this.systemAddress = systemAddress;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.latitude = latitude;
            this.longitude = longitude;
        }
    }
}
