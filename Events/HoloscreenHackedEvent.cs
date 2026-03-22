using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HoloscreenHackedEvent ( DateTime timestamp, Power powerBefore, Power powerAfter )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Holoscreen hacked";
        public const string DESCRIPTION = "Triggered when you hack a power's holoscreen.";
        public static readonly string[] SAMPLES =
        [
            @"{""timestamp"":""2024-10-22T20:40:06Z"",""event"":""HoloscreenHacked"",""PowerBefore"":""Aisling Duval"",""PowerAfter"":""Yuri Grom""}"
        ];

        [ PublicAPI( "The powerplay power displayed before the hack, as a localized string" ) ]
        public string before => powerBefore?.localizedName;

        [PublicAPI( "The powerplay power displayed before the hack, as an object" )]
        public Power powerBefore { get; private set; } = powerBefore;

        [ PublicAPI( "The powerplay power displayed after the hack, as a localized string" ) ]
        public string after => powerAfter?.localizedName;

        [PublicAPI( "The powerplay power displayed after the hack, as an object" )]
        public Power powerAfter { get; private set; } = powerAfter;
    }
}
