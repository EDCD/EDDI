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
            OnFootInStation = 0x00000008,           // OnFootInStation ('station' means planetary star port, not true for orbital stations)
            OnFootOnPlanet = 0x00000010,            // OnFootOnPlanet
            AimDownSight = 0x00000020,              // AimDownSight
            LowOxygen = 0x00000040,                 // LowOxygen
            LowHealth = 0x00000080,                 // LowHealth
            Cold = 0x00000100,                      // Cold
            Hot = 0x00000200,                       // Hot
            VeryCold = 0x00000400,                  // Very Cold,
            VeryHot = 0x00000800,                   // Very Hot
            GlideMode = 0x00001000,                 // GlideMode
            OnFootInHangar = 0x00002000,            // On Foot In Hangar
            OnFootInSocialSpace = 0x00004000,       // On Foot In Social Space
            OnFootExterior = 0x00008000,            // On Foot Exterior (Not sure when this would actually be set. Space walks maybe?)
            BreathableAtmosphere = 0x00010000,      // Breathable Atmosphere
            TelepresenceMulticrew = 0x00020000,     // Telepresence Multicrew
            PhysicalMulticrew = 0x00040000,         // Physical Multicrew
            FsdHyperDriveCharging = 0x00080000,     // Fsd hyperdrive charging
            FsdSupercruiseBoost = 0x00100000,   // Supercruise Overdrive (SCO) active
            FsdSupercruiseAssist = 0x00200000,      // Supercruise Assist (SCA) active
            NpcCrewActive = 0x00400000,             // NPC crew active (assigned)
        }

        // Variables set from status flags (when not signed in, this is set to '0')
        [PublicAPI( @"the vehicle that is under the commander's control.  Can be one of ""Ship"", ""SRV"", ""Fighter"", ""On Foot"", ""Taxi"", and ""Multicrew""" )]
        public string vehicle =>
                  (flags & Flags.InSRV) != 0 ? Constants.VEHICLE_SRV
                : (flags & Flags.InFighter) != 0 ? Constants.VEHICLE_FIGHTER
                : (flags2 & Flags2.OnFoot) != 0 ? Constants.VEHICLE_LEGS 
                : (flags2 & Flags2.InTaxi) != 0 ? Constants.VEHICLE_TAXI
                : (flags2 & Flags2.InMultiCrew) != 0 ? Constants.VEHICLE_MULTICREW
                : (flags & Flags.InMainShip) != 0 ? Constants.VEHICLE_SHIP : ""; // "InMainShip" can be set when other flags are also set so we check "InMainShip" last.
                // Per private comments from Howard Chalkey, `InMainShip` means a FSD-capable ship, as against a SRV or Fighter: it will be set when in a Taxi

        [PublicAPI( @"a boolean value indicating whether the commander is currently being interdicted" )]
        public bool being_interdicted => (flags & Flags.BeingInterdicted) != 0;

        [PublicAPI( @"a boolean value indicating whether the commander is currently in danger" )]
        public bool in_danger => (flags & Flags.IsInDanger) != 0;

        [PublicAPI( @"a boolean value indicating whether the commander is near a landable surface (within it's gravity well)" )]
        public bool near_surface => (flags & Flags.HasLatLong) != 0;

        [PublicAPI( @"a boolean value indicating whether the commander's vehicle is overheating" )]
        public bool overheating => (flags & Flags.OverHeating) != 0;

        [PublicAPI( @"a boolean value indicating whether the commander has less than 25% fuel remaining" )]
        public bool low_fuel => (flags & Flags.LowFuel) != 0;
        
        [PublicAPI( @"a boolean value indicating whether the FSD is currently cooling down after a jump to hyperspace or supercruise." )]
        public bool fsd_cooldown => ( flags & Flags.FsdCooldown ) != 0;

        [PublicAPI( @"a boolean value indicating whether the FSD is currently charging for a jump to hyperspace." )] 
        public bool fsd_hyperdrive_charging => (flags2 & Flags2.FsdHyperDriveCharging) != 0;

        [PublicAPI( @"a boolean value indicating whether the FSD is currently mass locked." )]
        public bool fsd_mass_locked => ( flags & Flags.FsdMassLocked ) != 0;

        [PublicAPI( @"a boolean value indicating whether FSD supercruise assist (SCA) mode is activated." )]
        public bool fsd_supercruise_assist => ( flags2 & Flags2.FsdSupercruiseAssist ) != 0;

        [PublicAPI( @"a boolean value indicating whether FSD supercruise overdrive (SCO) mode is activated." )]
        public bool fsd_supercruise_boosting => ( flags2 & Flags2.FsdSupercruiseBoost ) != 0;

        [PublicAPI( @"a boolean value indicating whether the FSD is currently charging for a jump to supercruise." )]
        public bool fsd_supercruise_charging => !fsd_hyperdrive_charging && ( flags & Flags.FsdCharging ) != 0;

        [PublicAPI( @"a boolean value indicating whether SRV drive assist is active" )]
        public bool srv_drive_assist => (flags & Flags.SrvDriveAssist) != 0 && (flags & Flags.InSRV) != 0;

        [PublicAPI( @"a boolean value indicating whether the SRV in within the proximity zone around the ship" )]
        public bool srv_under_ship => (flags & Flags.SrvUnderShip) != 0 && (flags & Flags.InSRV) != 0;

        [PublicAPI( @"a boolean value indicating whether the SRV's turret has been deployed" )]
        public bool srv_turret_deployed => (flags & Flags.SrvTurret) != 0 && (flags & Flags.InSRV) != 0;

        [PublicAPI( @"a boolean value indicating whether the SRV's handbrake has been activated" )]
        public bool srv_handbrake_activated => (flags & Flags.SrvHandbrake) != 0 && (flags & Flags.InSRV) != 0;

        [PublicAPI( @"true if the lights in your SRV are set to the high beam mode." )]
        public bool srv_high_beams => (flags & Flags.SrvHighBeam) != 0 && (flags & Flags.InSRV) != 0;

        [PublicAPI( @"a boolean value indicating whether the ship is currently scooping fuel" )] 
        public bool scooping_fuel => (flags & Flags.ScoopingFuel) != 0; // This flag is set when scooping fuel conventionally. It is not set when supercharging the FSD (at a white dwarf or neutron star).

        [PublicAPI( @"a boolean value indicating whether silent running is active" )]
        public bool silent_running => (flags & Flags.SilentRunning) != 0;

        [PublicAPI( @"a boolean value indicating whether the cargo scoop has been deployed" )]
        public bool cargo_scoop_deployed => (flags & Flags.CargoScoopDeployed) != 0;

        [PublicAPI( @"a boolean value indicating whether the vehicle's external lights are active" )]
        public bool lights_on => (flags & Flags.LightsOn) != 0;

        [PublicAPI( @"a boolean value indicating whether the commander is currently in a wing" )]
        public bool in_wing => (flags & Flags.InWing) != 0;

        // FDev changes hardpoints status when the discovery scanner is used in supercruise. 
        // We want to keep hardpoints_deployed false if we are in supercruise or hyperspace.
        [PublicAPI( @"a boolean value indicating whether hardpoints are currently deployed" )]
        public bool hardpoints_deployed => ( ( flags & Flags.HardpointsDeployed ) != 0 ) && !supercruise && !hyperspace;

        [PublicAPI( @"a boolean value indicating whether flight assistance has been deactivated" )]
        public bool flight_assist_off => (flags & Flags.FlightAssistOff) != 0;

        [PublicAPI( @"a boolean value indicating whether the ship is currently in supercruise" )]
        public bool supercruise => (flags & Flags.Supercruise) != 0;

        [PublicAPI( @"a boolean value indicating whether the ship is currently jumping between star systems" )]
        public bool hyperspace => (flags & Flags.Hyperspace) != 0;

        [PublicAPI( @"a boolean value indicating whether the ship's shields are maintaining their integrity" )]
        public bool shields_up => (flags & Flags.ShieldsUp) != 0;

        [PublicAPI( @"a boolean value indicating whether the ship's landing gears have been deployed" )]
        public bool landing_gear_down => (flags & Flags.LandingGearDown) != 0;

        [PublicAPI( @"a boolean value indicating whether the ship is currently landed (on a surface)" )]
        public bool landed => (flags & Flags.Landed) != 0;

        [PublicAPI( @"a boolean value indicating whether the ship is currently docked (at a station)" )]
        public bool docked => (flags & Flags.Docked) != 0;

        [PublicAPI( @"a boolean value indicating whether the ship's HUD is currently in Analysis Mode" )]
        public bool analysis_mode => (flags & Flags.HudAnalysisMode) != 0;

        [PublicAPI( @"a boolean value indicating whether night vision is currently active" )]
        public bool night_vision => (flags & Flags.NightVision) != 0;

        [PublicAPI( @" true if the altitude is computed relative to the average radius (which is used at higher altitudes) rather than surface directly below the srv" )]
        public bool altitude_from_average_radius => (flags & Flags.AltitudeFromAverageRadius) != 0;

        [PublicAPI( @"true if you've disembarked at a station." )]
        public bool on_foot_in_station => (flags2 & Flags2.OnFootInStation) != 0;

        [PublicAPI( @"true if you've disembarked on a planet surface." )]
        public bool on_foot_on_planet => (flags2 & Flags2.OnFootOnPlanet) != 0;

        [PublicAPI( @"true if you've disembarked in a vehicle hangar." )]
        public bool on_foot_in_hangar => (flags2 & Flags2.OnFootInHangar) != 0;

        [PublicAPI( @"true if you've disembarked to a social space (e.g. station concourse)." )]
        public bool on_foot_social_space => (flags2 & Flags2.OnFootInSocialSpace) != 0;

        [PublicAPI(@"true if you've disembarked to an exterior space.")]
        public bool on_foot_exterior => (flags2 & Flags2.OnFootExterior) != 0;

        [PublicAPI( @"true if you are on foot and aiming through a scope." )]
        public bool aim_down_sight => (flags2 & Flags2.AimDownSight) != 0;

        [PublicAPI( @"true if you are on foot and oxygen is running low." )]
        public bool low_oxygen => (flags2 & Flags2.LowOxygen) != 0;

        [PublicAPI( @"true if you are on foot and health is running low." )]
        public bool low_health => (flags2 & Flags2.LowHealth) != 0;

        [ PublicAPI( @"the environment temperature when on foot. May be one of ""very cold"", ""cold"", ""temperate"", ""hot"", or ""very hot""." ) ]
        public string on_foot_temperature =>
            ( flags2 & Flags2.VeryCold ) != 0 ? "very cold" :
            ( flags2 & Flags2.Cold ) != 0 ? "cold" :
            ( flags2 & Flags2.Hot ) != 0 ? "hot" :
            ( flags2 & Flags2.VeryHot ) != 0 ? "very hot" :
            "temperate";

        [PublicAPI(@"true if your ship is in glide mode near a planet surface.")]
        public bool gliding => (flags2 & Flags2.GlideMode) != 0;

        [PublicAPI( @"true if you are on foot in an area with a breathable atmosphere." )]
        public bool breathable_atmosphere => (flags2 & Flags2.BreathableAtmosphere) != 0;

        [PublicAPI( @"true if you are participating in telepresence multicrew." )]
        public bool telepresence_multicrew => (flags2 & Flags2.TelepresenceMulticrew) != 0;

        [PublicAPI( @"true if you are participating in physical multicrew." )]
        public bool physical_multicrew => (flags2 & Flags2.PhysicalMulticrew) != 0;

        [PublicAPI("true if at least one NPC crew member is assigned to active duty on your ship.")]
        public bool npc_crew_active => ( flags2 & Flags2.NpcCrewActive ) != 0;

        // Variables set from pips (these are not always present in the event)

        [ PublicAPI( @"a decimal value indicating the power distributor allocation to systems" ) ]
        public decimal? system_pips = 0;

        [Obsolete(@"please use system_pips instead.")]
        public decimal? pips_sys => system_pips;

        [ PublicAPI( @"a decimal value indicating the power distributor allocation to engines" ) ]
        public decimal? engine_pips = 0;

        [Obsolete( @"please use engine_pips instead." )]
        public decimal? pips_eng => engine_pips;

        [ PublicAPI( @"a decimal value indicating the power distributor allocation to weapons" ) ]
        public decimal? weapon_pips = 0;

        [Obsolete( @"please use weapon_pips instead." )]
        public decimal? pips_wea => weapon_pips;

        // Variables set directly from the event (these are not always present in the event)

        [ PublicAPI( @"an integer value indicating the ship's currently selected firegroup" )]
        public int? firegroup = 0;

        [ PublicAPI( @"the commander's current focus. Can be one of: ""none"", ""internal panel"" (right panel), ""external panel"" (left panel), ""communications panel"" (top panel), ""role panel"" (bottom panel), ""station services"", ""galaxy map"", ""system map"", ""orrery"", ""fss mode"", ""saa mode"", or ""codex""" ) ]
        public string gui_focus = string.Empty;

        [ PublicAPI( @"a decimal value indicating the ship's current latitude (if near a surface)" ) ]
        public decimal? latitude;

        [PublicAPI(@"a decimal value indicating the ship's current longitude (if near a surface)")]
        public decimal? longitude;

        [PublicAPI(@"a decimal value indicating the ship's current altitude (if in flight near a surface)")]
        public decimal? altitude;

        [PublicAPI(@"a decimal value indicating the ship's current heading (if near a surface)")]
        public decimal? heading;
        
        [PublicAPI(@"the tons of cargo you are currently carrying")]
        public int? cargo_carried;

        [PublicAPI( @"the ship's current legal status. Can be one of ""Clean"", ""Illegal cargo"", ""Speeding"", ""Wanted"", ""Hostile"", ""Passenger wanted"", or ""Warrant""" )]
        public string legalstatus => (legalStatus ?? LegalStatus.Clean).localizedName;

        [PublicAPI(@"the name of the current body (if landed or in an srv)")]
        public string bodyname;

        [PublicAPI(@"the radius of the current body in meters (if landed or in an srv)")]
        public decimal? planetradius;

        [PublicAPI(@"a decimal value (0-100) indicating your current oxygen level (when on foot)")]
        public decimal? oxygen;

        [PublicAPI(@"a decimal value (0-100) indicating your current health level (when on foot)")]
        public decimal? health;

        [ PublicAPI( @"a decimal value indicating the current surface temperature in Kelvin (when on foot)" ) ]
        public decimal? temperature;

        [PublicAPI("the model of your current selected weapon (when on foot)")]
        public string selected_weapon;

        [PublicAPI(@"a decimal value indicating the surface gravity relative to 1G (when on foot)")]
        public decimal? gravity; 
        
        // Variables calculated from event data
        [PublicAPI( @"a decimal value indicating the ship's current slope relative to the horizon (if near a surface)" )]
        public decimal? slope { get; set; }

        [PublicAPI( @"a decimal value indicating the ship's current fuel (including fuel in the active fuel reservoir)" )]
        public decimal? fuel => fuelInTanks + fuelInReservoir;

        [ PublicAPI( @" a decimal percent value calculated from your current total fuel capacity" ) ] 
        public decimal? fuel_percent { get; set; } = new Random().Next( 1, 100 ); // Randomize at-rest fuel percentages for speech tests.

        [PublicAPI( @" an integer value projecting the time remaining before you run out of fuel, in seconds" )]
        public int? fuel_seconds { get; set; }

        [PublicAPI( @"the current credit balance of the commander, if available." )]
        public ulong? credit_balance { get; set; }

        [PublicAPI("the name of the currently selected destination (including in-system destinations)")]
        public string destination_name;

        [PublicAPI("the localized name of the currently selected destination, if available")]
        public string destination_localized_name; // Typically only written for signal sources and fleet carriers

        // Not intended to be user facing

        public decimal? fuelInTanks;
        public decimal? fuelInReservoir;
        public int? fuel_percentile => fuel_percent is null
            ? null
            : (int?)decimal.Ceiling( (decimal)fuel_percent / 5 );
        public Flags flags;
        public Flags2 flags2;
        public DateTime timestamp = DateTime.UtcNow;
        public LegalStatus legalStatus;
        public string raw;

        public ulong? destinationSystemAddress;
        public int? destinationBodyId; // Multiple destinations either on or orbiting the same body may share a single bodyId

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
