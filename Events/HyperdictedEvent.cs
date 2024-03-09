using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HyperdictedEvent : Event
    {
        public const string NAME = "Hyperdicted";
        public const string DESCRIPTION = "Triggered when your ship is hyperdicted by a Thargoid";

        public static HyperdictedEvent SAMPLE = new HyperdictedEvent( 
            DateTime.UtcNow, 
            0.086031M, 
            31.554825M, 
            0, 
            false,
            false,
            new ThargoidWar()
            {
                CurrentState = FactionState.ThargoidControlled,
                SuccessState = FactionState.ThargoidRecovery,
                FailureState = FactionState.ThargoidControlled,
                succeeded = false,
                progress = 0.6071M,
                remainingDays = 0,
                remainingPorts = 0
            } );

        [PublicAPI("The amount of fuel used in this jump attempt")]
        public decimal fuelused { get; private set; }

        [PublicAPI("The amount of fuel remaining after this jump attempt")]
        public decimal fuelremaining { get; private set; }

        [PublicAPI("True if the ship is a transport (e.g. taxi or dropship)")]
        public bool? taxi { get; private set; }

        [PublicAPI("True if the ship is belongs to another player")]
        public bool? multicrew { get; private set; }

        // Thargoid War
        [PublicAPI("Thargoid war data, when applicable")]
        public ThargoidWar ThargoidWar { get; private set; }

        // These properties are not intended to be user facing

        public int? boostused { get; private set; }

        public HyperdictedEvent ( DateTime timestamp, decimal fuelused, decimal fuelremaining, int? boostUsed, bool? taxi, bool? multicrew, ThargoidWar thargoidWar) : base(timestamp, NAME)
        {
            this.fuelused = fuelused;
            this.fuelremaining = fuelremaining;
            this.boostused = boostUsed;
            this.taxi = taxi;
            this.multicrew = multicrew;
            this.ThargoidWar = thargoidWar;
        }
    }
}