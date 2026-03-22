using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class RespawnedEvent ( DateTime timestamp, string option, long price ) : Event( timestamp, NAME )
    {
        public const string NAME = "Respawned";
        public const string DESCRIPTION = "Triggered when you respawn (either after injury or after handing yourself in to local authorities)";
        public const string SAMPLE = @"{ ""timestamp"":""2016-09-20T12:27:59Z"", ""event"":""Resurrect"", ""Option"":""rebuy"", ""Cost"":36479, ""Bankrupt"":false }";

        [PublicAPI("The action triggering the respawn. One of 'rebuy', 'recover', 'rejoin', or 'handin'")]
        public string trigger { get; private set; } = option;

        [PublicAPI("The price paid to complete the respawn, if any")]
        public long price { get; private set; } = price;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var option = JsonParsing.getString(data, "Option");
            var price = JsonParsing.getLong(data, "Cost");
            events.Add( new RespawnedEvent( timestamp, option, price ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}