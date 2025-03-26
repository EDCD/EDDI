using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipFsdEvent : Event
    {
        public const string NAME = "Ship fsd";
        public const string DESCRIPTION = "Triggered when there is a change to the status of your ship's fsd";
        public static ShipFsdEvent SAMPLE = new ShipFsdEvent( DateTime.UtcNow, false, false, true, false, false, false, false, false, false, false,
            false, false, false, false, false, false );

        // FSD cooldown state

        [PublicAPI( "True if the FSD is currently cooling down after a jump to hyperspace or supercruise." )]
        public bool is_cooldown { get; private set; }

        [PublicAPI( "True if the FSD was cooling down after a jump to hyperspace or supercruise." )]
        public bool was_cooldown { get; private set; }

        // Hyperdrive charging state

        [PublicAPI( "True if the FSD is currently charging for a jump to hyperspace." )]
        public bool is_hyperdrive_charging { get; private set; }

        [PublicAPI( "True if the FSD was charging for a jump to hyperspace." )]
        public bool was_hyperdrive_charging { get; private set; }

        // Hyperspace state

        [PublicAPI( "True if the FSD is currently in hyperspace." )]
        public bool is_hyperspace { get; private set; }

        [PublicAPI( "True if the FSD was in hyperspace." )]
        public bool was_hyperspace { get; private set; }

        // Mass lock state

        [PublicAPI( "True if the FSD is currently mass locked." )]
        public bool is_mass_locked { get; private set; }

        [PublicAPI( "True if the FSD was mass locked." )]
        public bool was_mass_locked { get; private set; }

        // Supercruise state

        [PublicAPI( "True if the FSD is currently in supercruise." )]
        public bool is_supercruise { get; private set; }

        [PublicAPI( "True if the FSD was in supercruise." )]
        public bool was_supercruise { get; private set; }

        // Supercruise Assist (SCA) state

        [PublicAPI( "True if FSD supercruise assist (SCA) mode is currently activated." )]
        public bool is_supercruise_assisted { get; private set; }

        [PublicAPI( "True if FSD supercruise assist (SCA) mode was activated." )]
        public bool was_supercruise_assisted { get; private set; }

        // Supercruise Overdrive (SCO) state

        [PublicAPI( "True if FSD supercruise overdrive (SCO) mode is currently activated." )]
        public bool is_supercruise_boosting { get; private set; }

        [PublicAPI( "True if FSD supercruise overdrive (SCO) mode was activated." )]
        public bool was_supercruise_boosting { get; private set; }

        // Supercruise charging state

        [PublicAPI( "True if the FSD is currently charging for a jump to supercruise." )]
        public bool is_supercruise_charging { get; private set; }

        [PublicAPI( "True if the FSD was charging for a jump to supercruise." )]
        public bool was_supercruise_charging { get; private set; }

        public ShipFsdEvent ( DateTime timestamp, 
            bool isCooldown, bool wasCooldown,
            bool isHyperdriveCharging, bool wasHyperdriveCharging,
            bool isHyperspace, bool wasHyperspace,
            bool isMassLocked, bool wasMassLocked, 
            bool isSupercruise, bool wasSupercruise,
            bool isSupercruiseAssist, bool wasSupercruiseAssist, 
            bool isSupercruiseBoosting, bool wasSupercruiseBoosting, 
            bool isSupercruiseCharging, bool wasSupercruiseCharging ) : base( timestamp, NAME )
        {
            this.is_cooldown = isCooldown;
            this.was_cooldown = wasCooldown;
            this.is_hyperspace = isHyperspace;
            this.was_hyperspace = wasHyperspace;
            this.is_hyperdrive_charging = isHyperdriveCharging;
            this.was_hyperdrive_charging = wasHyperdriveCharging;
            this.is_mass_locked = isMassLocked;
            this.was_mass_locked = wasMassLocked;
            this.is_supercruise = isSupercruise;
            this.was_supercruise = wasSupercruise;
            this.is_supercruise_assisted = isSupercruiseAssist;
            this.was_supercruise_assisted = wasSupercruiseAssist;
            this.is_supercruise_boosting = isSupercruiseBoosting;
            this.was_supercruise_boosting = wasSupercruiseBoosting;
            this.is_supercruise_charging = isSupercruiseCharging;
            this.was_supercruise_charging = wasSupercruiseCharging;
        }

        [ Obsolete, PublicAPI( @"(OBSOLETE) ""The status of your ship's fsd as a string ('cooldown', 'cooldown complete', 'charging', 'charging cancelled', 'charging complete', 'masslock', or 'masslock cleared')" ) ]
        public string fsd_status =>
            is_cooldown ? "cooldown" :
            is_hyperdrive_charging || is_supercruise_charging ? "charging" :
            is_mass_locked ? "masslock" :
            is_hyperspace ? "hyperspace" :
            is_supercruise ? "supercruise" :
            was_supercruise_charging || was_hyperdrive_charging ? "charging cancelled" :
            was_cooldown ? "cooldown complete" :
            was_mass_locked ? "masslock cleared" :
            "ready";
        
        [ Obsolete, PublicAPI( "(OBSOLETE) True if the FSD is currently charging for a jump to hyperspace." ) ]
        public bool hyperdrive_charging => is_hyperdrive_charging;
    }
}
