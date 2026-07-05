using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class BountyPaidEvent (
        DateTime timestamp,
        long amount,
        decimal? brokerpercentage,
        bool allbounties,
        string faction,
        int shipId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Bounty paid";
        public const string DESCRIPTION = "Triggered when you pay a bounty";
        public const string SAMPLE = "{ \"timestamp\":\"2018-03-19T10:25:10Z\", \"event\":\"PayBounties\", \"Amount\":400, \"AllFines\":false, \"Faction\":\"$faction_Federation;\", \"Faction_Localised\":\"Federation\", \"ShipID\":9, \"BrokerPercentage\":25.000000 }";

        [PublicAPI("The amount of the bounty paid")]
        public long amount { get; private set; } = amount;

        [PublicAPI("Broker percentage (if paid via a Broker)")]
        public decimal? brokerpercentage { get; private set; } = brokerpercentage;

        [PublicAPI("Whether this payment covers all current bounties (true or false)")]
        public bool allbounties { get; private set; } = allbounties;

        [PublicAPI("The faction to which the bounty was paid")]
        public string faction { get; private set; } = faction;

        [PublicAPI("The ship id of the ship associated with the fine (if any)")]
        public int shipid { get; private set; } = shipId;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            data.TryGetValue( "Amount", out var val );
            var amount = (long)val;
            var brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");
            var allBounties = JsonParsing.getOptionalBool(data, "AllFines") ?? false;
            var faction = EventParsing.FactionName(data, "Faction");
            int shipId;
            var shipIdLong = JsonParsing.getLong(data, "ShipID");
            if ( shipIdLong > 4293000000 )
            {
                // This is a suit loadout ID. Use a null value since bounties associated with the commander, rather than the ship, are being paid.
                shipId = -1;
            }
            else
            {
                shipId = (int)shipIdLong;
            }

            events.Add( new BountyPaidEvent( timestamp, amount, brokerpercentage, allBounties, faction, shipId ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
