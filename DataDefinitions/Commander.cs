using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Details about a commander
    /// </summary>

    public class FrontierApiCommander : INotifyPropertyChanged
    {
        // Parameters obtained from the Frontier API

        /// <summary>The commander's name</summary>
        [ PublicAPI ]
        public string name
        {
            get => _name;
            set
            {
                if ( value == _name )
                {
                    return;
                }

                _name = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>The number of credits the commander holds</summary>
        [PublicAPI]
        public long credits
        {
            get => _credits;
            set
            {
                if ( value == _credits )
                {
                    return;
                }

                _credits = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The amount of debt the commander owes</summary>
        [PublicAPI]
        public long debt
        {
            get => _debt;
            set
            {
                if ( value == _debt )
                {
                    return;
                }

                _debt = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's combat rating</summary>
        [ PublicAPI ]
        public CombatRating combatrating
        {
            get => _combatrating;
            set
            {
                if ( value == _combatrating )
                {
                    return;
                }

                _combatrating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's trade rating</summary>
        [ PublicAPI ]
        public TradeRating traderating
        {
            get => _traderating;
            set
            {
                if ( value == _traderating )
                {
                    return;
                }

                _traderating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's exploration rating</summary>
        [ PublicAPI ]
        public ExplorationRating explorationrating
        {
            get => _explorationrating;
            set
            {
                if ( value == _explorationrating )
                {
                    return;
                }

                _explorationrating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's CQC rating</summary>
        [ PublicAPI ]
        public CQCRating cqcrating
        {
            get => _cqcrating;
            set
            {
                if ( value == _cqcrating )
                {
                    return;
                }

                _cqcrating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's empire rating</summary>
        [ PublicAPI ]
        public EmpireRating empirerating
        {
            get => _empirerating;
            set
            {
                if ( value == _empirerating )
                {
                    return;
                }

                _empirerating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's federation rating</summary>
        [ PublicAPI ]
        public FederationRating federationrating
        {
            get => _federationrating;
            set
            {
                if ( value == _federationrating )
                {
                    return;
                }

                _federationrating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's mercenary rating</summary>
        [ PublicAPI ]
        public MercenaryRating mercenaryrating
        {
            get => _mercenaryrating;
            set
            {
                if ( value == _mercenaryrating )
                {
                    return;
                }

                _mercenaryrating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's exobiologist rating</summary>
        [ PublicAPI ]
        public ExobiologistRating exobiologistrating
        {
            get => _exobiologistrating;
            set
            {
                if ( value == _exobiologistrating )
                {
                    return;
                }

                _exobiologistrating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's crime rating</summary>
        public int crimerating
        {
            get => _crimerating;
            set
            {
                if ( value == _crimerating )
                {
                    return;
                }

                _crimerating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's service rating</summary>
        public int servicerating
        {
            get => _servicerating;
            set
            {
                if ( value == _servicerating )
                {
                    return;
                }

                _servicerating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's powerplay rating (if pledged)</summary>
        public int powerrating
        {
            get => _powerrating;
            set
            {
                if ( value == _powerrating )
                {
                    return;
                }

                _powerrating = value;
                OnPropertyChanged();
            }
        }

        /// <summary>The commander's squadron name</summary>
        [PublicAPI]
        public string squadronname
        {
            get => _squadronname;
            set { _squadronname = value; OnPropertyChanged(); }
        }

        /// <summary>The commander's squadron tag</summary>
        [PublicAPI]
        public string squadrontag
        {
            get => _squadrontag;
            set { _squadrontag = value; OnPropertyChanged(); }
        }

        private string _name;
        private string _squadronname;
        private string _squadrontag;
        private int _powerrating;
        private int _servicerating;
        private int _crimerating;
        private ExobiologistRating _exobiologistrating;
        private MercenaryRating _mercenaryrating;
        private FederationRating _federationrating;
        private EmpireRating _empirerating;
        private CQCRating _cqcrating;
        private ExplorationRating _explorationrating;
        private TradeRating _traderating;
        private CombatRating _combatrating;
        private long _credits;
        private long _debt;

        #region INotifyPopertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private protected void OnPropertyChanged ( [System.Runtime.CompilerServices.CallerMemberName] string propName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
        }

        #endregion
    }

    public class Commander : FrontierApiCommander
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
            get => _gender.ToString();
            set
            {
                _gender = Enum.TryParse( value, out Gender parsedGender ) 
                    ? parsedGender 
                    : Gender.Neither; // Default value
                OnPropertyChanged();
            }
        }

        public Gender Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(); }
        }
        private Gender _gender;

        /// <summary>The commander's powerplay power (if pledged)</summary>
        public Power Power
        {
            get => _power ?? Power.None;
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

        /// <summary>The name of the commander's home system (if selected)</summary>
        public string homeSystemName
        {
            get => _homeSystemName;
            set { _homeSystemName = value; OnPropertyChanged(); }
        }
        private string _homeSystemName;

        /// <summary>The system address ID of the commander's home system (if selected)</summary>
        public ulong? homeSystemAddress
        {
            get => _homeSystemAddress;
            set { _homeSystemAddress = value; OnPropertyChanged(); }
        }
        private ulong? _homeSystemAddress;

        /// <summary>The name of the commander's home station (if selected)</summary>
        public string homeStationName
        {
            get => _homeStationName;
            set { _homeStationName = value; OnPropertyChanged(); }
        }
        private string _homeStationName;

        /// <summary>The market ID of the commander's home station (if selected)</summary>
        public long? homeStationMarketID
        {
            get => _homeStationMarketID;
            set { _homeStationMarketID = value; OnPropertyChanged(); }
        }
        private long? _homeStationMarketID;

        public decimal? homeSystemX
        {
            get => _homeSystemX;
            set { _homeSystemX = value; OnPropertyChanged(); }
        }
        private decimal? _homeSystemX;
        
        public decimal? homeSystemY
        {
            get => _homeSystemY;
            set { _homeSystemY = value; OnPropertyChanged(); }
        }
        private decimal? _homeSystemY;
        
        public decimal? homeSystemZ
        {
            get => _homeSystemZ;
            set { _homeSystemZ = value; OnPropertyChanged(); }
        }
        private decimal? _homeSystemZ;

        /// <summary>The commander's squadron rank</summary>
        [PublicAPI]
        public SquadronRank squadronrank
        {
            get => _squadronrank;
            set 
            {
                void childPropertyChangedHandler ( object sender, PropertyChangedEventArgs e )
                {
                    OnPropertyChanged();
                }
                if ( _squadronrank != null ) { _squadronrank.PropertyChanged -= childPropertyChangedHandler; }
                if ( value != null ) { value.PropertyChanged += childPropertyChangedHandler; }
                _squadronrank = value ?? new SquadronRank(null, string.Empty, string.Empty); 
                OnPropertyChanged();
            }
        }
        private SquadronRank _squadronrank;

        /// <summary>The commander's squadron system name</summary>
        [PublicAPI]
        public string squadronSystemName
        {
            get => _squadronSystemName;
            set { _squadronSystemName = value; OnPropertyChanged(); }
        }
        private string _squadronSystemName;

        /// <summary>The commander's squadron system numeric address</summary>
        [PublicAPI]
        public ulong? squadronSystemAddress
        {
            get => _squadronSystemAddress;
            set { _squadronSystemAddress = value; OnPropertyChanged(); }
        }
        private ulong? _squadronSystemAddress;

        /// <summary>The commander's squadron superpower</summary>
        [PublicAPI]
        public Superpower squadronallegiance
        {
            get => _squadronallegiance ?? Superpower.None;
            set { _squadronallegiance = value; OnPropertyChanged();}
        }
        private Superpower _squadronallegiance;

        /// <summary>The commander's squadron power</summary>
        [PublicAPI]
        public Power squadronpower
        {
            get => _squadronpower ?? Power.None;
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

        public static Commander FromFrontierApiCmdr(Commander currentCmdr, FrontierApiCommander frontierApiCommander, DateTime apiTimeStamp, DateTime journalTimeStamp, out bool cmdrMatches)
        {
            if (frontierApiCommander is null) { cmdrMatches = true; return currentCmdr; }

            // Copy our current commander to a new commander object
            var Cmdr = currentCmdr.Copy();

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
            Cmdr.debt = frontierApiCommander.debt;

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
            // A few items are also updated from the journal but may change so we check the timestamp
            if (apiTimeStamp > journalTimeStamp)
            {
                Cmdr.credits = frontierApiCommander.credits;
                Cmdr.squadronname = frontierApiCommander.squadronname;
                Cmdr.squadrontag = frontierApiCommander.squadrontag;
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

    public enum Gender
    {
        Male,
        Female,
        Neither
    }
}
