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

            var AIHumanoid = new TargetType("AIHumanoid");
            var BountyHunter = new TargetType("BountyHunter");
            var Civilian = new TargetType("Civilian");
            var CitizenHumanoid = new TargetType("CitizenHumanoid");
            var Deserter = new TargetType("Deserter");
            var DeserterASS = new TargetType("DeserterASS");
            var GuardHumanoid = new TargetType("GuardHumanoid");
            var Hostage = new TargetType("Hostage");
            var Miner = new TargetType("Miner");
            var Pirate = new TargetType("Pirate");
            var PirateLord = new TargetType("PirateLord");
            var Security = new TargetType("Security");
            var Scout = new TargetType("Scout");
            var Smuggler = new TargetType("Smuggler");
            var Terrorist = new TargetType("Terrorist");
            var TerroristLeader = new TargetType("TerroristLeader");
            var Trader = new TargetType("Trader");
            var VenerableGeneral = new TargetType("VenerableGeneral");
        }

        // dummy used to ensure that the static constructor has run
        public TargetType() : this("")
        { }

        private TargetType(string edname) : base(edname, edname)
        { }
    }
}
