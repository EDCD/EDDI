namespace EddiDataDefinitions
{
    public class MessageSource : ResourceBasedLocalizedEDName<MessageSource>
    {
        static MessageSource()
        {
            resourceManager = Properties.MessageSources.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new MessageSource(edname);
        }

        public static readonly MessageSource AmbushedPilot = new MessageSource("AmbushedPilot");
        public static readonly MessageSource BountyHunter = new MessageSource("BountyHunter");
        public static readonly MessageSource CapShip = new MessageSource("CapShip");
        public static readonly MessageSource CargoHunter = new MessageSource("CargoHunter");
        public static readonly MessageSource CivilianPilot = new MessageSource("CivilianPilot");
        public static readonly MessageSource Commander = new MessageSource("Commander");
        public static readonly MessageSource ConflictZone = new MessageSource("ConflictZone");
        public static readonly MessageSource ConvoyExplorers = new MessageSource("ConvoyExplorers");
        public static readonly MessageSource ConvoyWedding = new MessageSource("ConvoyWedding");
        public static readonly MessageSource CrewMate = new MessageSource("CrewMate");
        public static readonly MessageSource CruiseLiner = new MessageSource("CruiseLiner");
        public static readonly MessageSource Escort = new MessageSource("Escort");
        public static readonly MessageSource Hitman = new MessageSource("Hitman");
        public static readonly MessageSource Messenger = new MessageSource("Messenger");
        public static readonly MessageSource Military = new MessageSource("Military");
        public static readonly MessageSource Miner = new MessageSource("Miner");
        public static readonly MessageSource NPC = new MessageSource("NPC");
        public static readonly MessageSource PassengerHunter = new MessageSource("PassengerHunter");
        public static readonly MessageSource PassengerLiner = new MessageSource("PassengerLiner");
        public static readonly MessageSource Pirate = new MessageSource("Pirate");
        public static readonly MessageSource Police = new MessageSource("Police");
        public static readonly MessageSource PowersAssassin = new MessageSource("PowersAssassin");
        public static readonly MessageSource PowersPirate = new MessageSource("PowersPirate");
        public static readonly MessageSource PowersSecurity = new MessageSource("PowersSecurity");
        public static readonly MessageSource Propagandist = new MessageSource("Propagandist");
        public static readonly MessageSource Protester = new MessageSource("Protester");
        public static readonly MessageSource Refugee = new MessageSource("Refugee");
        public static readonly MessageSource SearchandRescue = new MessageSource("SearchandRescue");
        public static readonly MessageSource SquadronMate = new MessageSource("SquadronMate");
        public static readonly MessageSource StarshipOne = new MessageSource("StarshipOne");
        public static readonly MessageSource Station = new MessageSource("Station");
        public static readonly MessageSource WingMate = new MessageSource("WingMate");

        // dummy used to ensure that the static constructor has run
        public MessageSource() : this("")
        { }

        private MessageSource(string edname) : base(edname, edname)
        { }

        public static MessageSource FromMessage(string from, string message)
        {
            MessageSource by;
            if (message.StartsWith("$AmbushedPilot_"))
            {
                by = AmbushedPilot;
            }
            else if (message.StartsWith("$BountyHunter"))
            {
                by = BountyHunter;
            }
            else if (message.StartsWith("$CapShip") || message.StartsWith("$FEDCapShip"))
            {
                by = CapShip;
            }
            else if (message.StartsWith("$CargoHunter"))
            {
                by = CargoHunter; // Mission specific
            }
            else if (message.StartsWith("$Commuter"))
            {
                by = CivilianPilot;
            }
            else if (message.StartsWith("$ConvoyExplorers"))
            {
                by = ConvoyExplorers;
            }
            else if (message.StartsWith("$ConvoyWedding"))
            {
                by = ConvoyWedding;
            }
            else if (message.StartsWith("$CruiseLiner"))
            {
                by = CruiseLiner;
            }
            else if (message.StartsWith("$Escort"))
            {
                by = Escort;
            }
            else if (message.StartsWith("$Hitman"))
            {
                by = Hitman;
            }
            else if (message.StartsWith("$Messenger"))
            {
                by = Messenger;
            }
            else if (message.StartsWith("$Military"))
            {
                by = Military;
            }
            else if (message.StartsWith("$Miner"))
            {
                by = Miner;
            }
            else if (message.StartsWith("$PassengerHunter"))
            {
                by = PassengerHunter; // Mission specific
            }
            else if (message.StartsWith("$PassengerLiner"))
            {
                by = PassengerLiner;
            }
            else if (message.StartsWith("$Pirate"))
            {
                by = Pirate;
            }
            else if (message.StartsWith("$Police"))
            {
                // Police messages appear to be re-used by bounty hunters.  Check from to see if it really is police
                if (from.Contains("Police"))
                {
                    by = Police;
                }
                else
                {
                    by = BountyHunter;
                }
            }
            else if (message.StartsWith("$PowersAssassin"))
            {
                by = PowersAssassin;  // Power play specific
            }
            else if (message.StartsWith("$PowersPirate"))
            {
                by = PowersPirate; // Power play specific
            }
            else if (message.StartsWith("$PowersSecurity"))
            {
                by = PowersSecurity; // Power play specific
            }
            else if (message.StartsWith("$Propagandist"))
            {
                by = Propagandist;
            }
            else if (message.StartsWith("$Protester"))
            {
                by = Protester;
            }
            else if (message.StartsWith("$Refugee"))
            {
                by = Refugee;
            }
            else if (message.StartsWith("$Smuggler"))
            {
                by = CivilianPilot; // We shouldn't recognize a smuggler without a cargo scan
            }
            else if (message.StartsWith("$StarshipOne"))
            {
                by = StarshipOne;
            }
            else if (message.Contains("_SearchandRescue_"))
            {
                by = SearchandRescue;
            }
            else if (message.Contains("_Warzone_NPC"))
            {
                by = ConflictZone;
            }
            else
            {
                by = NPC;
            }
            return by;
        }
    }
}
