using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierLocationEvent (
        DateTime timestamp,
        long carrierId,
        StationModel carrierType,
        ulong systemAddress,
        string systemName,
        long bodyId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Carrier location";
        public const string DESCRIPTION = "Triggered at startup and shortly before a monitored fleet carrier arrives at a new destination";
        public const string SAMPLE = @"{ ""timestamp"":""2025-03-19T19:02:10Z"", ""event"":""CarrierLocation"", ""CarrierID"":3705689344, ""StarSystem"":""HR 3635"", ""SystemAddress"":1694121347427, ""BodyID"":1 }";

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; } = carrierId;

        [ PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" ) ]
        public StationModel carrierType { get; private set; } = carrierType;

        [PublicAPI("The name of the star system where your fleet carrier is located")]
        public string systemname { get; private set; } = systemName;

        [PublicAPI( "The numeric system address of the star system where your fleet carrier is located" )]
        public ulong systemAddress { get; private set; } = systemAddress;

        [PublicAPI("The numeric body ID of the body your fleet carrier is orbiting")]
        public long bodyID { get; private set; } = bodyId;
    }
}