namespace EddiDataDefinitions
{
    public class PowerplayState : ResourceBasedLocalizedEDName<PowerplayState>
    {
        static PowerplayState()
        {
            resourceManager = Properties.PowerplayState.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new PowerplayState(edname);

            None = new PowerplayState("None");
            HomeSystem = new PowerplayState("HomeSystem");
            var Contested = new PowerplayState("Contested");
            var Controlled = new PowerplayState("Controlled");
            var Exploited = new PowerplayState("Exploited");
            var InPrepareRadius = new PowerplayState("InPrepareRadius");
            var Prepared = new PowerplayState("Prepared");
            var Turmoil = new PowerplayState("Turmoil");
        }

        public static readonly PowerplayState None;
        public static readonly PowerplayState HomeSystem;

        // dummy used to ensure that the static constructor has run
        public PowerplayState() : this("")
        { }

        private PowerplayState(string edname) : base(edname, edname)
        { }
    }
}
