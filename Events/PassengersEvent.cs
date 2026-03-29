using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class PassengersEvent ( DateTime timestamp, List<Passenger> passengers ) : Event( timestamp, NAME )
    {
        public const string NAME = "Passengers";
        public const string DESCRIPTION = "Triggered at startup, with basic information of the Passenger Manifest";
        public const string SAMPLE = "{ \"timestamp\":\"2018-06-04T17:07:02Z\", \"event\":\"Passengers\", \"Manifest\":[ { \"MissionID\":387643501, \"Type\":\"Criminal\", \"VIP\":true, \"Wanted\":true, \"Count\":5 }, { \"MissionID\":387642036, \"Type\":\"Criminal\", \"VIP\":true, \"Wanted\":true, \"Count\":5 } ] }";

        [PublicAPI("The manifest of passengers on your ship (as objects)")]
        public List<Passenger> passengers { get; private set; } = passengers;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var passengers = new List<Passenger>();
            data.TryGetValue( "Manifest", out var val );
            var passengerManifest = (List<object>)val;

            if ( passengerManifest != null )
            {
                foreach ( var passenger in passengerManifest )
                {
                    var passengerProperties = (Dictionary<string, object>)passenger;
                    var missionid = JsonParsing.getULong( passengerProperties, "MissionID" );
                    var type = JsonParsing.getString( passengerProperties, "Type" );
                    var vip = JsonParsing.getBool( passengerProperties, "VIP" );
                    var wanted = JsonParsing.getBool( passengerProperties, "Wanted" );
                    var amount = JsonParsing.getInt( passengerProperties, "Count" );

                    var newPassenger = new Passenger( missionid, type, vip, wanted, amount );
                    passengers.Add( newPassenger );
                }
            }

            events.Add( new PassengersEvent( timestamp, passengers ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
