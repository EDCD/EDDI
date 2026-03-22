using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HyperdictedEvent (
        DateTime timestamp,
        decimal fuelused,
        decimal fuelremaining,
        int? boostUsed,
        bool? taxi,
        bool? multicrew,
        ThargoidWar thargoidWar )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Hyperdicted";
        public const string DESCRIPTION = "Triggered when your ship is hyperdicted by a Thargoid";

        public static readonly HyperdictedEvent SAMPLE = new( 
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
        public decimal fuelused { get; private set; } = fuelused;

        [PublicAPI("The amount of fuel remaining after this jump attempt")]
        public decimal fuelremaining { get; private set; } = fuelremaining;

        [PublicAPI("True if the ship is a transport (e.g. taxi or dropship)")]
        public bool? taxi { get; private set; } = taxi;

        [PublicAPI("True if the ship is belongs to another player")]
        public bool? multicrew { get; private set; } = multicrew;

        // Thargoid War
        [PublicAPI("Thargoid war data, when applicable")]
        public ThargoidWar ThargoidWar { get; private set; } = thargoidWar;

        // These properties are not intended to be user facing

        public int? boostused { get; private set; } = boostUsed;
    }
}