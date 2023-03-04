﻿using Newtonsoft.Json;
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
        [PublicAPI]
        public string name { get; set; }

        /// <summary>The commander's combat rating</summary>
        [PublicAPI]
        public CombatRating combatrating { get; set; }

        /// <summary>The commander's trade rating</summary>
        [PublicAPI]
        public TradeRating traderating { get; set; }

        /// <summary>The commander's exploration rating</summary>
        [PublicAPI]
        public ExplorationRating explorationrating { get; set; }

        /// <summary>The commander's CQC rating</summary>
        [PublicAPI]
        public CQCRating cqcrating { get; set; }

        /// <summary>The commander's empire rating</summary>
        [PublicAPI]
        public EmpireRating empirerating { get; set; }

        /// <summary>The commander's federation rating</summary>
        [PublicAPI]
        public FederationRating federationrating { get; set; }

        /// <summary>The commander's mercenary rating</summary>
        [PublicAPI]
        public MercenaryRating mercenaryrating { get; set; }

        /// <summary>The commander's exobiologist rating</summary>
        [PublicAPI]
        public ExobiologistRating exobiologistrating { get; set; }

        /// <summary>The commander's crime rating</summary>
        public int crimerating { get; set; }

        /// <summary>The commander's service rating</summary>
        public int servicerating { get; set; }

        /// <summary>The commander's powerplay rating (if pledged)</summary>
        public int powerrating { get; set; }

        /// <summary>The number of credits the commander holds</summary>
        [PublicAPI]
        public ulong? credits { get; set; }

        /// <summary>The amount of debt the commander owes</summary>
        [PublicAPI]
        public ulong? debt { get; set; }
    }

    public class Commander : FrontierApiCommander, INotifyPropertyChanged
    {
        // Parameters not obtained from the Frontier API
        // Note: Any information not updated from the Frontier API will need to be reset when the Frontier API refreshes the commander definition.

        /// <summary>The commander's Frontier ID</summary>
        public string EDID { get; set; }

        [JsonIgnore]
        private string _phoneticName;
        /// <summary>The commander's phonetic name</summary>
        public string phoneticName
        {
            get => _phoneticName;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _phoneticName = null;
                }
                else
                {
                    _phoneticName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>The commander's spoken name (rendered using ssml and IPA)</summary>
        [PublicAPI]
        public string phoneticname => SpokenName();

        /// <summary> The commander's title.  This is dependent on the current system</summary>
        public string title
        {
            get => _title;
            set { _title = value; OnPropertyChanged();}
        }
        private string _title;

        /// <summary> The commander's gender.  This is set in EDDI's configuration</summary>
        [PublicAPI]
        public string gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged();}
        }
        private string _gender;

        /// <summary>The commander's powerplay power (if pledged)</summary>
        public Power Power
        {
            get => _power;
            set { _power = value; OnPropertyChanged();}
        }
        private Power _power;

        /// <summary>The commander's powerplay power (localized) (if pledged)</summary>
        [PublicAPI]
        public string power => (Power ?? Power.None)?.localizedName;

        /// <summary>The commander's powerplay merits (if pledged)</summary>
        public int? powermerits
        {
            get => _powermerits;
            set { _powermerits = value; OnPropertyChanged();}
        }
        private int? _powermerits;

        /// <summary>The commander's squadron name</summary>
        [PublicAPI]
        public string squadronname
        {
            get => _squadronname;
            set { _squadronname = value; OnPropertyChanged();}
        }
        private string _squadronname;

        /// <summary>The commander's squadron ID</summary>
        [PublicAPI]
        public string squadronid
        {
            get => _squadronid;
            set { _squadronid = value; OnPropertyChanged();}
        }
        private string _squadronid;

        /// <summary>The commander's squadron rank</summary>
        [PublicAPI]
        public SquadronRank squadronrank
        {
            get => _squadronrank;
            set { _squadronrank = value; OnPropertyChanged();}
        }
        private SquadronRank _squadronrank;

        /// <summary>The commander's squadron superpower</summary>
        [PublicAPI]
        public Superpower squadronallegiance
        {
            get => _squadronallegiance;
            set { _squadronallegiance = value; OnPropertyChanged();}
        }
        private Superpower _squadronallegiance;

        /// <summary>The commander's squadron power</summary>
        [PublicAPI]
        public Power squadronpower
        {
            get => _squadronpower;
            set { _squadronpower = value; OnPropertyChanged();}
        }
        private Power _squadronpower;

        /// <summary>The commander's squadron faction</summary>
        [PublicAPI]
        public string squadronfaction
        {
            get => _squadronfaction;
            set { _squadronfaction = value; OnPropertyChanged();}
        }
        private string _squadronfaction;

        /// <summary>The insurance excess percentage the commander has to pay</summary>
        public decimal? insurance { get; set; } = 0.05M;

        /// <summary>The Commander's friends</summary>
        [PublicAPI]
        public List<Friend> friends = new List<Friend>();

        /// <summary>The Commander's status and progress with the various engineers</summary>
        [PublicAPI]
        public List<Engineer> engineers => Engineer.ENGINEERS;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
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
            cqcrating = commander.cqcrating;
            mercenaryrating = commander.mercenaryrating;
            exobiologistrating = commander.exobiologistrating;
        }

        public static Commander FromFrontierApiCmdr(Commander currentCmdr, FrontierApiCommander frontierApiCommander, DateTime apiTimeStamp, DateTime journalTimeStamp, out bool cmdrMatches)
        {
            if (frontierApiCommander is null) { cmdrMatches = true; return currentCmdr; }

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
            Cmdr.debt = frontierApiCommander.debt ?? Cmdr.debt ?? 0;

            // Update our commander object with information obtainable from the journal
            // Since the parameters below only increase, we will take any that are higher in rank than we had before
            Cmdr.combatrating = (frontierApiCommander.combatrating?.rank ?? 0) >= (Cmdr.combatrating?.rank ?? 0)
                ? frontierApiCommander.combatrating : Cmdr.combatrating;
            Cmdr.traderating = (frontierApiCommander.traderating?.rank ?? 0) >= (Cmdr.traderating?.rank ?? 0)
                ? frontierApiCommander.traderating : Cmdr.traderating;
            Cmdr.explorationrating = (frontierApiCommander.explorationrating?.rank ?? 0) >= (Cmdr.explorationrating?.rank ?? 0)
                ? frontierApiCommander.explorationrating : Cmdr.explorationrating;
            Cmdr.cqcrating = (frontierApiCommander.cqcrating?.rank ?? 0) >= (Cmdr.cqcrating?.rank ?? 0)
                ? frontierApiCommander.cqcrating : Cmdr.cqcrating;
            Cmdr.empirerating = (frontierApiCommander.empirerating?.rank ?? 0) >= (Cmdr.empirerating?.rank ?? 0)
                ? frontierApiCommander.empirerating : Cmdr.empirerating;
            Cmdr.federationrating = (frontierApiCommander.federationrating?.rank ?? 0) >= (Cmdr.federationrating?.rank ?? 0)
                ? frontierApiCommander.federationrating : Cmdr.federationrating;
            Cmdr.mercenaryrating = (frontierApiCommander.mercenaryrating?.rank ?? 0) >= (Cmdr.mercenaryrating?.rank ?? 0)
                ? frontierApiCommander.mercenaryrating : Cmdr.mercenaryrating;
            Cmdr.exobiologistrating = (frontierApiCommander.exobiologistrating?.rank ?? 0) >= (Cmdr.exobiologistrating?.rank ?? 0)
                ? frontierApiCommander.exobiologistrating : Cmdr.exobiologistrating;
            // A few items are also updated from the journal but may decrease so we check the timestamp
            if (apiTimeStamp > journalTimeStamp)
            {
                Cmdr.credits = frontierApiCommander.credits ?? Cmdr.credits ?? 0;
                Cmdr.powerrating = frontierApiCommander.powerrating;
            }

            return Cmdr;
        }

        public string SpokenName()
        {
            string spokenName = string.Empty;
            if (!string.IsNullOrWhiteSpace(phoneticName))
            {
                spokenName = "<phoneme alphabet=\"ipa\" ph=\"" + phoneticName + "\">" + name + "</phoneme>";
            }
            else if (!string.IsNullOrWhiteSpace(name))
            {
                spokenName = name;
            }
            return spokenName;
        }
    }
}
