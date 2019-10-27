using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Details about a commander
    /// </summary>

    public class FrontierApiCommander
    {
        // Parameters obtained from the Frontier API

        /// <summary>The commander's name</summary>
        public string name { get; set; }

        /// <summary>The commander's Frontier ID</summary>
        public string EDID { get; set; }

        /// <summary>The commander's combat rating</summary>
        public CombatRating combatrating { get; set; }

        /// <summary>The commander's trade rating</summary>
        public TradeRating traderating { get; set; }

        /// <summary>The commander's exploration rating</summary>
        public ExplorationRating explorationrating { get; set; }

        /// <summary>The commander's CQC rating</summary>
        public CQCRating cqcrating { get; set; }

        /// <summary>The commander's empire rating</summary>
        public EmpireRating empirerating { get; set; }

        /// <summary>The commander's federation rating</summary>
        public FederationRating federationrating { get; set; }

        /// <summary>The commander's crime rating</summary>
        public int crimerating { get; set; }

        /// <summary>The commander's service rating</summary>
        public int servicerating { get; set; }

        /// <summary>The commander's powerplay rating (if pledged)</summary>
        public int powerrating { get; set; }

        /// <summary>The number of credits the commander holds</summary>
        public long credits { get; set; }

        /// <summary>The amount of debt the commander owes</summary>
        public long debt { get; set; }
    }

    public class Commander : FrontierApiCommander, INotifyPropertyChanged
    {
        // Parameters not obtained from the Frontier API
        // Note: Any information not updated from the Frontier API will need to be reset when the Frontier API refreshes the commander definition.

        /// <summary>The commander's name as spoken</summary>
        public string phoneticname { get; set; }

        /// <summary> The commander's title.  This is dependent on the current system</summary>
        public string title { get; set; }

        /// <summary> The commander's gender.  This is set in EDDI's configuration</summary>
        public string gender { get; set; }

        /// <summary>The commander's powerplay power (if pledged)</summary>
        public Power Power { get; set; }

        /// <summary>The commander's powerplay power (localized) (if pledged)</summary>
        public string power => (Power ?? Power.None)?.localizedName;

        /// <summary>The commander's powerplay merits (if pledged)</summary>
        public int? powermerits { get; set; }

        /// <summary>The commander's squadron name</summary>
        public string squadronname { get; set; }

        /// <summary>The commander's squadron ID</summary>
        public string squadronid { get; set; }

        /// <summary>The commander's squadron rank</summary>
        public SquadronRank squadronrank { get; set; }

        /// <summary>The commander's squadron superpower</summary>
        public Superpower squadronallegiance { get; set; }

        /// <summary>The commander's squadron power</summary>
        public Power squadronpower { get; set; }

        /// <summary>The commander's squadron faction</summary>
        public string squadronfaction { get; set; }

        /// <summary>The insurance excess percentage the commander has to pay</summary>
        public decimal? insurance { get; set; }

        /// <summary>The Commander's friends</summary>
        public List<Friend> friends = new List<Friend>();

        /// <summary>The Commander's status and progress with the various engineers</summary>
        public List<Engineer> engineers => Engineer.ENGINEERS;

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public Commander()
        { }

        // Copy constructor
        public Commander(Commander commander)
        {
            name = commander.name;
            combatrating = commander.combatrating;
            traderating = commander.traderating;
            explorationrating = commander.explorationrating;
            cqcrating = CQCRating.FromRank(commander.cqcrating.rank);
        }

        public static Commander FromFrontierApiCmdr(Commander currentCmdr, FrontierApiCommander frontierApiCommander, DateTime apiTimeStamp, DateTime journalTimeStamp, out bool cmdrMatches)
        {
            // Copy our current commander to a new commander object
            Commander Cmdr = currentCmdr.Copy();

            // Set our commander name if it hadn't already been set
            Cmdr.name = string.IsNullOrEmpty(Cmdr.name) ? frontierApiCommander.name : Cmdr.name;

            // Verify that the profile information matches the current Cmdr name
            if (frontierApiCommander.name != Cmdr.name)
            {
                Logging.Warn("Frontier API incorrectly configured: Returning information for Commander " +
                    frontierApiCommander.name + " rather than for " + Cmdr.name + ". Disregarding incorrect information.");
                cmdrMatches = false;
                return Cmdr;
            }
            cmdrMatches = true;

            // Update our commander object with information exclusively available from the Frontier API
            Cmdr.crimerating = frontierApiCommander.crimerating;
            Cmdr.servicerating = frontierApiCommander.servicerating;
            Cmdr.credits = frontierApiCommander.credits;
            Cmdr.debt = frontierApiCommander.debt;

            // Update our commander object with information obtainable from the journal
            // Since the parameters below only increase, we will take any that are higher in rank than we had before
            Cmdr.combatrating = frontierApiCommander.combatrating.rank > Cmdr.combatrating.rank
                ? frontierApiCommander.combatrating : Cmdr.combatrating;
            Cmdr.traderating = frontierApiCommander.traderating.rank > Cmdr.traderating.rank
                ? frontierApiCommander.traderating : Cmdr.traderating;
            Cmdr.explorationrating = frontierApiCommander.explorationrating.rank > Cmdr.explorationrating.rank
                ? frontierApiCommander.explorationrating : Cmdr.explorationrating;
            Cmdr.cqcrating = frontierApiCommander.cqcrating.rank > Cmdr.cqcrating.rank
                ? frontierApiCommander.cqcrating : Cmdr.cqcrating;
            Cmdr.empirerating = frontierApiCommander.empirerating.rank > Cmdr.empirerating.rank
                ? frontierApiCommander.empirerating : Cmdr.empirerating;
            Cmdr.federationrating = frontierApiCommander.federationrating.rank > Cmdr.federationrating.rank
                ? frontierApiCommander.federationrating : Cmdr.federationrating;
            // Power rating is also updated from the journal but may decrease so we check the timestamp
            if (apiTimeStamp > journalTimeStamp)
            {
                Cmdr.powerrating = frontierApiCommander.powerrating;
            }

            return Cmdr;
        }
    }
}
