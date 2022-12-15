namespace EddiDataDefinitions
{
    /// <summary>
    /// Legal Status
    /// </summary>
    public class LegalStatus : ResourceBasedLocalizedEDName<LegalStatus>
    {
        static LegalStatus()
        {
            resourceManager = Properties.LegalStatus.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new LegalStatus(edname);

            Clean = new LegalStatus("Clean");
            None = new LegalStatus("None");
            Unknown = new LegalStatus("Unknown");
            Lawless = new LegalStatus("Lawless");
            Wanted = new LegalStatus("Wanted");
            Enemy = new LegalStatus("Enemy");
            WantedEnemy = new LegalStatus("WantedEnemy");
            Warrant = new LegalStatus("Warrant");
            Hunter = new LegalStatus("Hunter");
            IllegalCargo = new LegalStatus("IllegalCargo");
            Speeding = new LegalStatus("Speeding");
            Hostile = new LegalStatus("Hostile");
            PassengerWanted = new LegalStatus("PassengerWanted");
            Thargoid = new LegalStatus("Thargoid");
        }

        public static readonly LegalStatus Clean;
        public static readonly LegalStatus None;
        public static readonly LegalStatus Unknown;
        public static readonly LegalStatus Lawless;
        public static readonly LegalStatus Wanted;
        public static readonly LegalStatus Enemy;
        public static readonly LegalStatus WantedEnemy;
        public static readonly LegalStatus Warrant;
        public static readonly LegalStatus Hunter;
        public static readonly LegalStatus IllegalCargo;
        public static readonly LegalStatus Speeding;
        public static readonly LegalStatus Hostile;
        public static readonly LegalStatus PassengerWanted;
        public static readonly LegalStatus Thargoid;

        // dummy used to ensure that the static constructor has run
        public LegalStatus() : this("")
        { }

        private LegalStatus(string edname) : base(edname, edname)
        { }
    }
}
