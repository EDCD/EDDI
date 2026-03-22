using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipFsdEvent (
        DateTime timestamp,
        bool isCooldown,
        bool wasCooldown,
        bool isHyperdriveCharging,
        bool wasHyperdriveCharging,
        bool isHyperspace,
        bool wasHyperspace,
        bool isMassLocked,
        bool wasMassLocked,
        bool isSupercruise,
        bool wasSupercruise,
        bool isSupercruiseAssist,
        bool wasSupercruiseAssist,
        bool isSupercruiseBoosting,
        bool wasSupercruiseBoosting,
        bool isSupercruiseCharging,
        bool wasSupercruiseCharging )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Ship fsd";
        public const string DESCRIPTION = "Triggered when there is a change to the status of your ship's fsd";
        public static readonly ShipFsdEvent SAMPLE = new( DateTime.UtcNow, false, false, true, false, false, false, false, false, false, false,
            false, false, false, false, false, false );

        // FSD cooldown state

        [PublicAPI( "True if the FSD is currently cooling down after a jump to hyperspace or supercruise." )]
        public bool is_cooldown { get; private set; } = isCooldown;

        [PublicAPI( "True if the FSD was cooling down after a jump to hyperspace or supercruise." )]
        public bool was_cooldown { get; private set; } = wasCooldown;

        // Hyperdrive charging state

        [PublicAPI( "True if the FSD is currently charging for a jump to hyperspace." )]
        public bool is_hyperdrive_charging { get; private set; } = isHyperdriveCharging;

        [PublicAPI( "True if the FSD was charging for a jump to hyperspace." )]
        public bool was_hyperdrive_charging { get; private set; } = wasHyperdriveCharging;

        // Hyperspace state

        [PublicAPI( "True if the FSD is currently in hyperspace." )]
        public bool is_hyperspace { get; private set; } = isHyperspace;

        [PublicAPI( "True if the FSD was in hyperspace." )]
        public bool was_hyperspace { get; private set; } = wasHyperspace;

        // Mass lock state

        [PublicAPI( "True if the FSD is currently mass locked." )]
        public bool is_mass_locked { get; private set; } = isMassLocked;

        [PublicAPI( "True if the FSD was mass locked." )]
        public bool was_mass_locked { get; private set; } = wasMassLocked;

        // Supercruise state

        [PublicAPI( "True if the FSD is currently in supercruise." )]
        public bool is_supercruise { get; private set; } = isSupercruise;

        [PublicAPI( "True if the FSD was in supercruise." )]
        public bool was_supercruise { get; private set; } = wasSupercruise;

        // Supercruise Assist (SCA) state

        [PublicAPI( "True if FSD supercruise assist (SCA) mode is currently activated." )]
        public bool is_supercruise_assisted { get; private set; } = isSupercruiseAssist;

        [PublicAPI( "True if FSD supercruise assist (SCA) mode was activated." )]
        public bool was_supercruise_assisted { get; private set; } = wasSupercruiseAssist;

        // Supercruise Overdrive (SCO) state

        [PublicAPI( "True if FSD supercruise overdrive (SCO) mode is currently activated." )]
        public bool is_supercruise_boosting { get; private set; } = isSupercruiseBoosting;

        [PublicAPI( "True if FSD supercruise overdrive (SCO) mode was activated." )]
        public bool was_supercruise_boosting { get; private set; } = wasSupercruiseBoosting;

        // Supercruise charging state

        [PublicAPI( "True if the FSD is currently charging for a jump to supercruise." )]
        public bool is_supercruise_charging { get; private set; } = isSupercruiseCharging;

        [PublicAPI( "True if the FSD was charging for a jump to supercruise." )]
        public bool was_supercruise_charging { get; private set; } = wasSupercruiseCharging;

        [ PublicAPI( @"The status of your ship's fsd as a string ('cooldown', 'cooldown complete', 'charging', 'charging cancelled', 'charging complete', 'masslock', or 'masslock cleared')" ) ]
        public string fsd_status
        {
            get
            {
                if ( was_supercruise_charging || was_hyperdrive_charging )
                {
                    if ( ( is_supercruise_charging || is_hyperdrive_charging ) && is_hyperspace )
                    {
                        return "charging complete";
                    }

                    if ( is_hyperspace && !was_hyperspace )
                    {
                        return "hyperspace";
                    }

                    if ( is_supercruise && !was_supercruise )
                    {
                        return "supercruise";
                    }

                    return "charging cancelled";
                }
                if ( ( is_supercruise_charging || is_hyperdrive_charging ) &&
                !( was_supercruise_charging || was_hyperdrive_charging ) )
                {
                    return "charging";
                }

                if ( was_cooldown && !is_cooldown )
                {
                    return "cooldown complete";
                }
                if ( is_cooldown && !was_cooldown )
                {
                    return "cooldown";
                }

                if ( was_mass_locked && !is_mass_locked )
                {
                    return "masslock cleared";
                }
                if ( is_mass_locked && !was_mass_locked )
                {
                    return "masslock";
                }

                // None of the above apply
                return "ready";
            }
        }

        [ Obsolete("May still be used in player Cottle scripts"), PublicAPI( "(OBSOLETE) True if the FSD is currently charging for a jump to hyperspace." ) ]
        public bool hyperdrive_charging => is_hyperdrive_charging;
    }
}
