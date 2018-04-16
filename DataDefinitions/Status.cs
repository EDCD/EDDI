using System;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Details about the current game status
    /// </summary>
    public class Status
    {
        [Flags]
        public enum Flags
        {
            None = 0,
            Docked = 0x00000001,
            Landed = 0x00000002,
            LandingGearDown = 0x00000004,
            ShieldsUp = 0x00000008,
            Supercruise = 0x00000010,
            FlightAssistOff = 0x00000020,
            HardpointsDeployed = 0x00000040,
            InWing = 0x00000080,
            LightsOn = 0x00000100,
            CargoScoopDeployed = 0x00000200,
            SilentRunning = 0x00000400,
            ScoopingFuel = 0x00000800,
            SrvHandbrake = 0x00001000,
            SrvTurret = 0x00002000,
            SrvUnderShip = 0x00004000,
            SrvDriveAssist = 0x00008000,
            FsdMassLocked = 0x00010000,
            FsdCharging = 0x00020000,
            FsdCooldown = 0x00040000,
            LowFuel = 0x00080000,
            OverHeating = 0x00100000,
            HasLatLong = 0x00200000,
            IsInDanger = 0x00400000,
            BeingInterdicted = 0x00800000,
            InMainShip = 0x01000000,
            InFighter = 0x02000000,
            InSRV = 0x04000000,
        }

        // Variables set from status flags (when not signed in, this is set to '0')
        public string vehicle => 
                ((flags & Flags.InSRV) != 0) ? Constants.VEHICLE_SRV
                : ((flags & Flags.InFighter) != 0) ? Constants.VEHICLE_FIGHTER
                : ((flags & Flags.InMainShip) != 0) ? Constants.VEHICLE_SHIP
                : "";
        public bool being_interdicted => (flags & Flags.BeingInterdicted) != 0;
        public bool in_danger => (flags & Flags.IsInDanger) != 0;
        public bool near_surface => (flags & Flags.HasLatLong) != 0;
        public bool overheating => (flags & Flags.OverHeating) != 0;
        public bool low_fuel => (flags & Flags.LowFuel) != 0;
        public string fsd_status =>
                ((flags & Flags.FsdCooldown) != 0) ? "cooldown"
                : ((flags & Flags.FsdCharging) != 0) ? "charging"
                : ((flags & Flags.FsdMassLocked) != 0) ? "masslock"
                : "ready";
        public bool srv_drive_assist => (flags & Flags.SrvDriveAssist) != 0;
        public bool srv_under_ship => (flags & Flags.SrvUnderShip) != 0;
        public bool srv_turret_deployed => (flags & Flags.SrvTurret) != 0;
        public bool srv_handbrake_activated => (flags & Flags.SrvHandbrake) != 0;
        public bool scooping_fuel => (flags & Flags.ScoopingFuel) != 0;
        public bool silent_running => (flags & Flags.SilentRunning) != 0;
        public bool cargo_scoop_deployed => (flags & Flags.CargoScoopDeployed) != 0;
        public bool lights_on => (flags & Flags.LightsOn) != 0;
        public bool in_wing => (flags & Flags.InWing) != 0;
        public bool hardpoints_deployed => (flags & Flags.HardpointsDeployed) != 0;
        public bool flight_assist_off => (flags & Flags.FlightAssistOff) != 0;
        public bool supercruise => (flags & Flags.Supercruise) != 0;
        public bool shields_up => (flags & Flags.ShieldsUp) != 0;
        public bool landing_gear_down => (flags & Flags.LandingGearDown) != 0;
        public bool landed => (flags & Flags.Landed) != 0;
        public bool docked => (flags & Flags.Docked) != 0;

        // Variables set from pips (these are not always present in the event)
        public decimal? pips_sys = 0;
        public decimal? pips_eng = 0;
        public decimal? pips_wea = 0;

        // Variables set directly from the event (these are not always present in the event)
        public int? firegroup = 0;
        public string gui_focus = string.Empty;
        public decimal? latitude;
        public decimal? longitude;
        public decimal? altitude;
        public decimal? heading;

        // Admin values
        public Flags flags;
        public DateTime timestamp = DateTime.Now;

        public Status(Flags flags = Flags.None)
        {
            this.flags = flags;
        }
    }
}
