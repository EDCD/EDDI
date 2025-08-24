using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierLocationEvent : Event
    {
        public const string NAME = "Carrier location";
        public const string DESCRIPTION = "Triggered at startup and shortly before a monitored fleet carrier arrives at a new destination";
        public const string SAMPLE = @"{ ""timestamp"":""2025-03-19T19:02:10Z"", ""event"":""CarrierLocation"", ""CarrierID"":3705689344, ""StarSystem"":""HR 3635"", ""SystemAddress"":1694121347427, ""BodyID"":1 }";

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; }

        [ PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" ) ]
        public StationModel carrierType { get; private set; }

        [PublicAPI("The name of the star system where your fleet carrier is located")]
        public string systemname { get; private set; }

        [PublicAPI( "The numeric system address of the star system where your fleet carrier is located" )]
        public ulong systemAddress { get; private set; }

        [PublicAPI("The numeric body ID of the body your fleet carrier is orbiting")]
        public long bodyID { get; private set; }

        public CarrierLocationEvent ( DateTime timestamp, long carrierId, StationModel carrierType,
            ulong systemAddress, string systemName,
            long bodyID ) : base(timestamp, NAME)
        {
            this.carrierID = carrierId;
            this.carrierType = carrierType;
            this.systemAddress = systemAddress;
            this.systemname = systemName;
            this.bodyID = bodyID;
        }
    }
}