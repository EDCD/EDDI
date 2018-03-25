using System;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Details about the current game status
    /// </summary>
    public class Status
    {
        // Variables set from status flags (when not signed in, this is set to '0')
        public string vehicle = string.Empty;
        public bool being_interdicted = false;
        public bool in_danger = false;
        public bool near_surface = false;
        public bool overheating = false;
        public bool low_fuel = false;
        public string fsd_status = "ready";
        public bool srv_drive_assist = false;
        public bool srv_under_ship = false;
        public bool srv_turret_deployed = false;
        public bool srv_handbrake_activated = false;
        public bool scooping_fuel = false;
        public bool silent_running = false;
        public bool cargo_scoop_deployed = false;
        public bool lights_on = false;
        public bool in_wing = false;
        public bool hardpoints_deployed = false;
        public bool flight_assist_off = false;
        public bool supercruise = false;
        public bool shields_up = false;
        public bool landing_gear_down = false;
        public bool landed = false;
        public bool docked = false;

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
        public long flags;
        public DateTime timestamp = DateTime.Now;
    }
}
