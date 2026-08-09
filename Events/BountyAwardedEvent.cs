using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class BountyAwardedEvent : Event
    {
        public const string NAME = "Bounty awarded";
        public const string DESCRIPTION = "Triggered when you are awarded a bounty";
        public const string SAMPLE = @"{ ""timestamp"":""2023-10-20T22:46:00Z"", ""event"":""Bounty"", ""Rewards"":[ { ""Faction"":""Amarishvaru Advanced Holdings"", ""Reward"":527518 } ], ""PilotName"":""$npc_name_decorate:#name=Christopher;"", ""PilotName_Localised"":""Christopher"", ""Target"":""anaconda"", ""TotalReward"":527518, ""VictimFaction"":""Orom Blue Council"" }";

        [PublicAPI("The name of the asset you destroyed (if applicable)")]
        public string target { get; private set; }

        [PublicAPI( "The pilot of the asset you destroyed (if applicable)" )]
        public string pilot { get; private set; }

        [PublicAPI("The name of the faction whose asset you destroyed")]
        public string faction { get; private set; }

        [PublicAPI("The total number of credits obtained for destroying the asset")]
        public long reward { get; private set; }

        [PublicAPI("The rewards obtained for destroying the asset")]
        public List<Reward> rewards { get; private set; }

        [PublicAPI("True if the rewards have been shared with wing-mates")]
        public bool shared { get; private set; }

        public BountyAwardedEvent(DateTime timestamp, string target, string target_localised, string victimName, string victimFaction, long reward, List<Reward> rewards, bool shared) : base(timestamp, NAME)
        {
            if ( target != null )
            {
                // Might be a ship
                var targetShip = ShipDefinitions.FromEDModel(target, false);

                // Might be a SRV or Fighter
                var targetVehicle = VesselDefinition.isVessel(target) ? VesselDefinition.FromEDName(target) : null;

                // Might be an on foot commander
                var targetCmdrSuit = Suit.EDNameExists(target) ? Suit.FromEDName(target) : null;

                // Might be an on foot NPC
                var targetNpcSuitLoadout = NpcSuitLoadout.EDNameExists(target) ? NpcSuitLoadout.FromEDName(target) : null;

                target = targetShip?.model
                         ?? targetCmdrSuit?.localizedName
                         ?? targetVehicle?.localizedName
                         ?? targetNpcSuitLoadout?.localizedName
                         ?? target_localised;
            }

            this.target = target;
            this.pilot = victimName;
            this.faction = victimFaction;
            this.reward = reward;
            this.rewards = rewards;
            this.shared = shared;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var target = JsonParsing.getString(data, "Target");
            var target_localised = JsonParsing.getString( data, "Target_Localised" );
            var victimName = JsonParsing.getString(data, "PilotName_Localised");
            var victimFaction = EventParsing.FactionName(data, "VictimFaction");

            data.TryGetValue( "SharedWithOthers", out var val );
            var shared = val != null && (long)val == 1;

            long reward;
            var rewards = new List<Reward>();

            if ( data.ContainsKey( "Reward" ) )
            {
                // Old-style
                reward = JsonParsing.getLong( data, "Reward" );
                if ( reward == 0 )
                {
                    // 0-credit reward; ignore
                    return true;
                }
                var factionName = EventParsing.FactionName(data, "Faction");
                rewards.Add( new Reward( factionName, reward ) );
            }
            else
            {
                reward = JsonParsing.getLong( data, "TotalReward" );
                if ( reward == 0 )
                {
                    // 0-credit reward; ignore
                    return true;
                }
                // Obtain list of rewards
                data.TryGetValue( "Rewards", out val );
                var rewardsData = (List<object>)val;
                if ( rewardsData != null )
                {
                    foreach ( var rewardData in rewardsData.Cast<IDictionary<string, object>>() )
                    {
                        var factionName = EventParsing.FactionName(rewardData, "Faction");
                        var factionReward = JsonParsing.getLong( rewardData, "Reward" );
                        rewards.Add( new Reward( factionName, factionReward ) );
                    }
                }
            }

            events.Add( new BountyAwardedEvent( timestamp, target, target_localised, victimName, victimFaction, reward, rewards, shared ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
