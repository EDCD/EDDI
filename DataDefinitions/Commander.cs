﻿using System.Collections.Generic;
using System.ComponentModel;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Details about a commander
    /// </summary>
    public class Commander : INotifyPropertyChanged
    {
        /// <summary>The commander's name</summary>
        public string name { get; set; }
        /// <summary>The commander's name as spoken</summary>
        public string phoneticname { get; set; }

        /// <summary> The commander's title.  This is dependent on the current system</summary>
        public string title { get; set; }

        /// <summary> The commander's gender.  This is set in EDDI's configuration</summary>
        public string gender { get; set; }

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

        /// <summary>The commander's powerplay power (if pledged)</summary>
        public Power Power { get; set; }

        /// <summary>The commander's powerplay power (localized) (if pledged)</summary>
        public string power => (Power ?? Power.None)?.localizedName;

        /// <summary>The commander's powerplay rating (if pledged)</summary>
        public int powerrating { get; set; }

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

        /// <summary>The number of credits the commander holds</summary>
        public long credits { get; set; }

        /// <summary>The amount of debt the commander owes</summary>
        public long debt { get; set; }

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
    }
}
