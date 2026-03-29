namespace EddiDataDefinitions
{
    /// <summary>
    /// Target types
    /// </summary>
    public class TargetType : ResourceBasedLocalizedEDName<TargetType>
    {
        static TargetType()
        {
            resourceManager = Properties.TargetType.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new TargetType(edname);
        }

        public static readonly TargetType AIHumanoid = new("AIHumanoid");
        public static readonly TargetType BountyHunter = new("BountyHunter");
        public static readonly TargetType Civilian = new("Civilian");
        public static readonly TargetType CitizenHumanoid = new("CitizenHumanoid");
        public static readonly TargetType Deserter = new("Deserter");
        public static readonly TargetType DeserterASS = new("DeserterASS");
        public static readonly TargetType GuardHumanoid = new("GuardHumanoid");
        public static readonly TargetType Hostage = new("Hostage");
        public static readonly TargetType Miner = new("Miner");
        public static readonly TargetType Pirate = new("Pirate");
        public static readonly TargetType PirateLord = new("PirateLord");
        public static readonly TargetType Politician = new("Politician");
        public static readonly TargetType Security = new("Security");
        public static readonly TargetType Scout = new("Scout");
        public static readonly TargetType Smuggler = new("Smuggler");
        public static readonly TargetType Terrorist = new("Terrorist");
        public static readonly TargetType TerroristLeader = new("TerroristLeader");
        public static readonly TargetType Trader = new("Trader");
        public static readonly TargetType VenerableGeneral = new("VenerableGeneral");

        // dummy used to ensure that the static constructor has run
        public TargetType () : this("")
        { }

        private TargetType(string edname) : base(edname, edname)
        { }
    }
}
