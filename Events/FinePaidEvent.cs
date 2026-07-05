using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FinePaidEvent (
        DateTime timestamp,
        long amount,
        decimal? brokerpercentage,
        bool allFines,
        string faction,
        int shipId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Fine paid";
        public const string DESCRIPTION = "Triggered when you pay a fine";
        public const string SAMPLE = "{ \"timestamp\":\"2018-03-19T10:24:21Z\", \"event\":\"PayFines\", \"Amount\":250, \"AllFines\":false, \"Faction\":\"Batz Transport Commodities\", \"ShipID\":9 }";

        [PublicAPI("The amount of the fine paid")]
        public long amount { get; private set; } = amount;

        [PublicAPI("Broker percentage (if paid via a Broker)")]
        public decimal? brokerpercentage { get; private set; } = brokerpercentage;

        [PublicAPI("Whether this payment covers all current fines (true or false)")]
        public bool allfines { get; private set; } = allFines;

        [PublicAPI("The faction to which the fine was paid (if the payment does not cover all current fines)")]
        public string faction { get; private set; } = faction;

        [PublicAPI("The ship id of the ship associated with the fine (if any)")]
        public int shipid { get; private set; } = shipId;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            data.TryGetValue( "Amount", out var val );
            var amount = (long)val;
            var brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");
            var allFines = JsonParsing.getOptionalBool(data, "AllFines") ?? false;
            var faction = EventParsing.FactionName(data, "Faction");
            var shipId = JsonParsing.getLong(data, "ShipID");
            if ( shipId >= 4293000000 )
            {
                // This is a suit loadout ID. Use a -1 value to signal that fines associated with the commander, rather than the ship, are being paid.
                shipId = -1;
            }

            events.Add( new FinePaidEvent( timestamp, amount, brokerpercentage, allFines, faction, (int)shipId ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
