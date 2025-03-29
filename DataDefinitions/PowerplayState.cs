namespace EddiDataDefinitions
{
    public class PowerplayState : ResourceBasedLocalizedEDName<PowerplayState>
    {
        static PowerplayState()
        {
            resourceManager = Properties.PowerplayState.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new PowerplayState(edname);
            
            Unoccupied = new PowerplayState( "Unoccupied" ); // No power controls the star system.
            Contested = new PowerplayState( "Contested" ); // This is a special variant of the "Unoccupied" state which occurs when multiple powers have 30+% progress towards control.
            Exploited = new PowerplayState( "Exploited" ); // Tier 1 control. If there are powers listed in the "Powers" property other than the "ControllingPower", those powers are trying to undermine control of the system. The system has no expansion radius.
            Fortified = new PowerplayState( "Fortified" ); // Tier 2 control. If there are powers listed in the "Powers" property other than the "ControllingPower", those powers are trying to undermine control of the system. The system has a small expansion radius.
            Stronghold = new PowerplayState( "Stronghold" ); // Tier 3 control. A power can have multiple "Stronghold" systems. The system has a large expansion radius.
            Headquarters = new PowerplayState( "Headquarters" ); // The power's headquarters. This is a special Stronghold system which cannot be undermined.
        }
        
        public static readonly PowerplayState Unoccupied;
        public static readonly PowerplayState Headquarters;
        public static readonly PowerplayState Contested;
        public static readonly PowerplayState Exploited;
        public static readonly PowerplayState Fortified;
        public static readonly PowerplayState Stronghold;

        // dummy used to ensure that the static constructor has run
        public PowerplayState () : this("")
        { }

        private PowerplayState(string edname) : base(edname, edname)
        { }
    }
}
