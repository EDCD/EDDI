using Utilities;

namespace EddiDataDefinitions
{
    public class ThargoidWar
    {
        // new states related to Thargoid systems are "Thargoid_Probing", "Thargoid_Harvest", "Thargoid_Controlled", "Thargoid_Stronghold", "Thargoid_Recovery"

        [PublicAPI("The current phase of the Thargoid war, as a localizable object.")]
        public FactionState CurrentState { get; set; }

        [PublicAPI( "The number of remaining days in the current phase of the Thargoid war." )]
        public int remainingDays { get; set; }

        [PublicAPI( "The next phase of the Thargoid war, if success is not achieved, as a localizable object." )]
        public FactionState FailureState { get; set; }

        [PublicAPI( "The next phase of the Thargoid war, if success is achieved, as a localizable object." )]
        public FactionState SuccessState { get; set; } // May be an empty string if success would drive Thargoids completely from the system.

        [PublicAPI("The number of ports which have not yet fallen into Thargoid control.")]
        public int? remainingPorts { get; set; } // Seems to be zero except possibly during the Thargoid_Harvest state

        [PublicAPI( "True if commanders have achieved a success state in the current phase of the Thargoid war." )]
        public bool succeeded { get; set; }

        [PublicAPI( "Percent progress in achieving a success state in the current phase of the Thargoid war. Progress can decrease if success is not reached before the weekly 'tick'." )]
        public decimal progress { get; set; } // As a percentage on a 0 - 1 scale.
    }
}
