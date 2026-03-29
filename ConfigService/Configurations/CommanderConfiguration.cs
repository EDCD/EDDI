using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the Galnet monitor</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\commandermonitor.json")]
    public class CommanderConfiguration : Config
    {
        private string _commanderName;
        private long _credits;
        private string _frontierId;
        private string _gender = "Male";
        private string _phoneticName;
        private string _homeSystemName;
        private ulong? _homeSystemAddress;
        private decimal? _homeSystemX;
        private decimal? _homeSystemY;
        private decimal? _homeSystemZ;
        private string _homeStationName;
        private long? _homeStationMarketId;
        private int? _powerMerits;
        private int _powerRank;
        private string _squadronName;
        private string _squadronTag;
        private SquadronRank _squadronRank;
        private string _squadronSystemName;
        private ulong? _squadronSystemAddress;
        private string _squadronFaction;
        private DateTime _updatedat;

        [ JsonProperty( "commanderName" ) ]
        public string commanderName
        {
            get => _commanderName;
            set
            {
                if ( value == _commanderName )
                {
                    return;
                }

                _commanderName = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "credits" ) ]
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

        [JsonIgnore] // This is a temporary dictionary to track status of online friends - it should not be saved to disk
        public List<Friend> friends { get; set; } = [ ];

        [ JsonProperty( "frontierID" ) ]
        public string frontierID
        {
            get => _frontierId;
            set
            {
                if ( value == _frontierId )
                {
                    return;
                }

                _frontierId = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "gender" ) ]
        public string gender
        {
            get => _gender;
            set
            {
                if ( value == _gender )
                {
                    return;
                }

                _gender = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "phoneticName" ) ]
        public string phoneticName
        {
            get => _phoneticName;
            set
            {
                if ( value == _phoneticName )
                {
                    return;
                }

                _phoneticName = value;
                OnPropertyChanged();
            }
        }

        #region Home System Properties

        [ JsonProperty( "homeSystemName" ) ]
        public string homeSystemName
        {
            get => _homeSystemName;
            set
            {
                if ( value == _homeSystemName )
                {
                    return;
                }

                _homeSystemName = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "homeSystemAddress" ) ]
        public ulong? homeSystemAddress
        {
            get => _homeSystemAddress;
            set
            {
                if ( value == _homeSystemAddress )
                {
                    return;
                }

                _homeSystemAddress = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "homeSystemX" ) ]
        public decimal? homeSystemX
        {
            get => _homeSystemX;
            set
            {
                if ( value == _homeSystemX )
                {
                    return;
                }

                _homeSystemX = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "homeSystemY" ) ]
        public decimal? homeSystemY
        {
            get => _homeSystemY;
            set
            {
                if ( value == _homeSystemY )
                {
                    return;
                }

                _homeSystemY = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "homeSystemZ" ) ]
        public decimal? homeSystemZ
        {
            get => _homeSystemZ;
            set
            {
                if ( value == _homeSystemZ )
                {
                    return;
                }

                _homeSystemZ = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "homeStationName" ) ]
        public string homeStationName
        {
            get => _homeStationName;
            set
            {
                if ( value == _homeStationName )
                {
                    return;
                }

                _homeStationName = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "homeStationMarketID" ) ]
        public long? homeStationMarketID
        {
            get => _homeStationMarketId;
            set
            {
                if ( value == _homeStationMarketId )
                {
                    return;
                }

                _homeStationMarketId = value;
                OnPropertyChanged();
            }
        }

        #endregion 

        #region Powerplay Properties

        [JsonProperty( "power" )]
        public string power
        {

            get => Power?.edname ?? Power.None.edname;
            set
            {
                if ( value == Power?.edname )
                {
                    return;
                }
                
                var pwDef = Power.FromEDName(value);
                this.Power = pwDef;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public Power Power { get; set; } = Power.None;

        [ JsonProperty( "powerMerits" ) ]
        public int? powerMerits
        {
            get => _powerMerits;
            set
            {
                if ( value == _powerMerits )
                {
                    return;
                }

                _powerMerits = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "powerRank" ) ]
        public int powerRank
        {
            get => _powerRank;
            set
            {
                if ( value == _powerRank )
                {
                    return;
                }

                _powerRank = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Squadron Properties

        [ JsonProperty( "squadronName" ) ]
        public string squadronName
        {
            get => _squadronName;
            set
            {
                if ( value == _squadronName )
                {
                    return;
                }

                _squadronName = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "squadronID" ) ]
        public string squadronTag
        {
            get => _squadronTag;
            set
            {
                if ( value == _squadronTag )
                {
                    return;
                }

                _squadronTag = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "SquadronRank" ) ]
        public SquadronRank SquadronRank
        {
            get => _squadronRank;
            set
            {
                if ( Equals( value, _squadronRank ) )
                {
                    return;
                }

                _squadronRank = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty( "squadronAllegiance" )]
        public string squadronAllegiance
        {

            get => SquadronAllegiance?.edname ?? Superpower.None.edname;
            set
            {
                if ( value == SquadronAllegiance?.edname )
                {
                    return;
                }
                 
                var saDef = Superpower.FromEDName(value);
                this.SquadronAllegiance = saDef;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public Superpower SquadronAllegiance { get; set; } = Superpower.None;

        [JsonProperty( "squadronPower" )]
        public string squadronPower
        {

            get => SquadronPower?.edname ?? Power.None.edname;
            set
            {
                if ( value == SquadronPower?.edname )
                {
                    return;
                }
                
                var spDef = Power.FromEDName(value);
                this.SquadronPower = spDef;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public Power SquadronPower { get; set; } = Power.None;

        [ JsonProperty( "squadronSystemName" ) ]
        public string squadronSystemName
        {
            get => _squadronSystemName;
            set
            {
                if ( value == _squadronSystemName )
                {
                    return;
                }

                _squadronSystemName = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "squadronSystemAddress" ) ]
        public ulong? squadronSystemAddress
        {
            get => _squadronSystemAddress;
            set
            {
                if ( value == _squadronSystemAddress )
                {
                    return;
                }

                _squadronSystemAddress = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "squadronFaction" ) ]
        public string squadronFaction
        {
            get => _squadronFaction;
            set
            {
                if ( value == _squadronFaction )
                {
                    return;
                }

                _squadronFaction = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public DateTime updatedat
        {
            get => _updatedat;
            set
            {
                if ( value.Equals( _updatedat ) )
                {
                    return;
                }

                _updatedat = value;
                OnPropertyChanged();
            }
        }
    }
}
