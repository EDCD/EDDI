using System;
using System.ComponentModel;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Details about the current game status
    /// </summary>
    public class Status
    {
        [Flags]
        public enum Flags : uint
        {
            None = 0,
            Docked = 0x00000001,                    // Docked, (on a landing pad)
            Landed = 0x00000002,                    // Landed, (on planet surface)
            LandingGearDown = 0x00000004,           // Landing Gear Down
            ShieldsUp = 0x00000008,                 // Shields Up
            Supercruise = 0x00000010,               // Supercruise
            FlightAssistOff = 0x00000020,           // FlightAssist Off
            HardpointsDeployed = 0x00000040,        // Hardpoints Deployed
            InWing = 0x00000080,                    // In Wing
            LightsOn = 0x00000100,                  // LightsOn
            CargoScoopDeployed = 0x00000200,        // Cargo Scoop Deployed
            SilentRunning = 0x00000400,             // Silent Running,
            ScoopingFuel = 0x00000800,              // Scooping Fuel
            SrvHandbrake = 0x00001000,              // Srv Handbrake
            SrvTurret = 0x00002000,                 // Srv using Turret view
            SrvUnderShip = 0x00004000,              // Srv Turret retracted (close to ship)
            SrvDriveAssist = 0x00008000,            // Srv DriveAssist
            FsdMassLocked = 0x00010000,             // Fsd MassLocked
            FsdCharging = 0x00020000,               // Fsd Charging
            FsdCooldown = 0x00040000,               // Fsd Cooldown
            LowFuel = 0x00080000,                   // Low Fuel ( < 25% )
            OverHeating = 0x00100000,               // Over Heating ( > 100% )
            HasLatLong = 0x00200000,                // Has Lat Long
            IsInDanger = 0x00400000,                // IsInDanger
            BeingInterdicted = 0x00800000,          // Being Interdicted
            InMainShip = 0x01000000,                // In MainShip
            InFighter = 0x02000000,                 // In Fighter
            InSRV = 0x04000000,                     // In SRV
            HudAnalysisMode = 0x08000000,           // Hud in Analysis mode
            NightVision = 0x10000000,               // Night Vision
            AltitudeFromAverageRadius = 0x20000000, // Altitude from Average radius
            Hyperspace = 0x40000000,                // fsdJump
            SrvHighBeam = 0x80000000,               // srvHighBeam
        }

        [Flags]
        public enum Flags2 : uint
        {
            None = 0,
            OnFoot = 0x00000001,                    // OnFoot
            InTaxi = 0x00000002,                    // InTaxi (or dropship/shuttle)
            InMultiCrew = 0x00000004,               // InMulticrew (ie in someone else’s ship)
            OnFootInStation = 0x00000008,           // OnFootInStation
            OnFootOnPlanet = 0x00000010,            // OnFootOnPlanet
            AimDownSight = 0x00000020,              // AimDownSight
            LowOxygen = 0x00000040,                 // LowOxygen
            LowHealth = 0x00000080,                 // LowHealth
            Cold = 0x00000100,                      // Cold
            Hot = 0x00000200,                       // Hot
            VeryCold = 0x00000400,                  // Very Cold,
            VeryHot = 0x00000800,                   // Very Hot
        }

        // Variables set from status flags (when not signed in, this is set to '0')
        public string vehicle =>
                  ((flags & Flags.InSRV) != 0) ? Constants.VEHICLE_SRV
                : ((flags & Flags.InFighter) != 0) ? Constants.VEHICLE_FIGHTER
                : ((flags2 & Flags2.OnFoot) != 0) ? Constants.VEHICLE_LEGS 
                : ((flags2 & Flags2.InTaxi) != 0) ? Constants.VEHICLE_TAXI
                : ((flags2 & Flags2.InMultiCrew) != 0) ? Constants.VEHICLE_MULTICREW
                : ((flags & Flags.InMainShip) != 0) ? Constants.VEHICLE_SHIP : ""; // "InMainShip" can be set when other flags are also set so we check "InMainShip" last.
                // Per private comments from Howard Chalkey, `InMainShip` means a FSD-capable ship, as against a SRV or Fighter: it will be set when in a Taxi
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
        public bool srv_drive_assist => (flags & Flags.SrvDriveAssist) != 0 && (flags & Flags.InSRV) != 0;
        public bool srv_under_ship => (flags & Flags.SrvUnderShip) != 0 && (flags & Flags.InSRV) != 0;
        public bool srv_turret_deployed => (flags & Flags.SrvTurret) != 0 && (flags & Flags.InSRV) != 0;
        public bool srv_handbrake_activated => (flags & Flags.SrvHandbrake) != 0 && (flags & Flags.InSRV) != 0;
        public bool srv_high_beams => (flags & Flags.SrvHighBeam) != 0 && (flags & Flags.InSRV) != 0;
        public bool scooping_fuel => (flags & Flags.ScoopingFuel) != 0;
        public bool silent_running => (flags & Flags.SilentRunning) != 0;
        public bool cargo_scoop_deployed => (flags & Flags.CargoScoopDeployed) != 0;
        public bool lights_on => (flags & Flags.LightsOn) != 0;
        public bool in_wing => (flags & Flags.InWing) != 0;
        public bool flight_assist_off => (flags & Flags.FlightAssistOff) != 0;
        public bool supercruise => (flags & Flags.Supercruise) != 0;
        public bool hyperspace => (flags & Flags.Hyperspace) != 0;
        public bool shields_up => (flags & Flags.ShieldsUp) != 0;
        public bool landing_gear_down => (flags & Flags.LandingGearDown) != 0;
        public bool landed => (flags & Flags.Landed) != 0;
        public bool docked => (flags & Flags.Docked) != 0;
        public bool analysis_mode => (flags & Flags.HudAnalysisMode) != 0;
        public bool night_vision => (flags & Flags.NightVision) != 0;
        public bool altitude_from_average_radius => (flags & Flags.AltitudeFromAverageRadius) != 0;
        public bool on_foot_in_station => (flags2 & Flags2.OnFootInStation) != 0;
        public bool on_foot_on_planet => (flags2 & Flags2.OnFootOnPlanet) != 0;
        public bool aim_down_sight => (flags2 & Flags2.AimDownSight) != 0;
        public bool low_oxygen => (flags2 & Flags2.LowOxygen) != 0;
        public bool low_health => (flags2 & Flags2.LowHealth) != 0;
        public string on_foot_temperature =>
            (flags2 & Flags2.VeryCold) != 0 ? "very cold" :
            (flags2 & Flags2.Cold) != 0 ? "cold" :
            (flags2 & Flags2.Hot) != 0 ? "hot" :
            (flags2 & Flags2.VeryHot) != 0 ? "very hot" :
            "temperate";

        // FDev changes hardpoints status when the discovery scanner is used in supercruise. 
        // We want to keep hardpoints_deployed false if we are in supercruise or hyperspace.
        public bool hardpoints_deployed => ((flags & Flags.HardpointsDeployed) != 0) && !supercruise && !hyperspace;
        
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
        public decimal? fuelInTanks;
        public decimal? fuelInReservoir;
        public int? cargo_carried;
        public string legalstatus => (legalStatus ?? LegalStatus.Clean).localizedName;
        public string bodyname;
        public decimal? planetradius;
        public decimal? oxygen; // 0..100, when on foot
        public decimal? health; // 0..100, when on foot
        public decimal? temperature; // kelvin, when on foot
        public string selected_weapon; // name, when on foot
        public decimal? gravity; // relative to 1G, when on foot
        
        // Variables calculated from event data
        public decimal? slope { get; set; }
        public decimal? fuel => fuelInTanks + fuelInReservoir;
        public decimal? fuel_percent { get; set; }
        public int? fuel_seconds { get; set; }

        // Admin values
        public Flags flags;
        public Flags2 flags2;
        public DateTime timestamp = DateTime.UtcNow;
        public LegalStatus legalStatus;
        public string raw;

        public Status(Flags flags = Flags.None, Flags2 flags2 = Flags2.None)
        {
            this.flags = flags;
            this.flags2 = flags2;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
