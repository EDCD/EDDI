using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HullDamagedEvent ( DateTime timestamp, bool? piloted, decimal health )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Hull damaged";
        public const string DESCRIPTION = "Triggered when your hull is damaged to a certain extent";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-29T10:26:07Z"", ""event"":""HullDamage"", ""Health"":0.615263, ""PlayerPilot"":false, ""Fighter"":true }";

        [PublicAPI("The vehicle that has been damaged (Ship, SRV, Fighter)")]
        public string vehicle { get; private set; }

        [PublicAPI("True if the vehicle receiving damage is piloted by the player")]
        public bool? piloted { get; private set; } = piloted;

        [PublicAPI("The percentage health of the hull")]
        public decimal health { get; private set; } = health;

        // Not intended to be user facing

        public bool? JournalFighter { get; private set; }
        private bool resolveVehicle;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var health = EventParsing.sensibleHealth(JsonParsing.getDecimal(data, "Health") * 100);
            var piloted = JsonParsing.getOptionalBool(data, "PlayerPilot");
            var fighter = JsonParsing.getOptionalBool(data, "Fighter");

            events.Add( new HullDamagedEvent( timestamp, piloted, health )
                { JournalFighter = fighter, resolveVehicle = true, raw = line, fromLoad = fromLogLoad } );
            return true;
        }

        public void ResolveVehicle ( string currentVehicle )
        {
            if ( !resolveVehicle ) { return; }
            vehicle = currentVehicle;
            if ( piloted == false )
            {
                vehicle = JournalFighter == true ? Constants.VEHICLE_FIGHTER
                    : currentVehicle == Constants.VEHICLE_SRV ? Constants.VEHICLE_SHIP
                    : currentVehicle == Constants.VEHICLE_SHIP ? Constants.VEHICLE_SRV
                    : currentVehicle;
            }
        }
    }
}
