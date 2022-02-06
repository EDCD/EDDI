﻿namespace EddiDataDefinitions
{
    /// <summary>
    /// Target types
    /// </summary>
    public class TargetType : ResourceBasedLocalizedEDName<TargetType>
    {
        static TargetType()
        {
            resourceManager = Properties.TargetType.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new TargetType(edname);

            var BountyHunter = new TargetType("BountyHunter");
            var Civilian = new TargetType("Civilian");
            var GuardHumanoid = new TargetType("GuardHumanoid");
            var Hostage = new TargetType("Hostage");
            var Miner = new TargetType("Miner");
            var Pirate = new TargetType("Pirate");
            var PirateLord = new TargetType("PirateLord");
            var Security = new TargetType("Security");
            var Smuggler = new TargetType("Smuggler");
            var Terrorist = new TargetType("Terrorist");
            var TerroristLeader = new TargetType("TerroristLeader");
            var Trader = new TargetType("Trader");
            var VenerableGeneral = new TargetType("VenerableGeneral");
            var DeserterASS = new TargetType("DeserterASS");
            var AIHumanoid = new TargetType("AIHumanoid");
        }

        // dummy used to ensure that the static constructor has run
        public TargetType() : this("")
        { }

        private TargetType(string edname) : base(edname, edname)
        { }
    }
}
