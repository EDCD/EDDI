using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Record defines the claims, fines and bounties associated with a faction, along with some additional data
    /// </summary>
    public class FactionRecord : INotifyPropertyChanged
    {
        /// <summary> The faction associated with the claim, fine, or bounty </summary>
        [PublicAPI( "name of the minor faction" )]
        public string faction
        {
            get => _faction;
            set
            {
                if (_faction != value)
                {
                    _faction = value;
                    OnPropertyChanged();
                }
            }
        }

        [PublicAPI( "the superpower to which the minor faction is aligned, if any" ), JsonProperty("allegiance")]
        public string allegiance
        {

            get => Allegiance?.invariantName ?? Superpower.None.invariantName;
            set
            {
                var aDef = Superpower.FromName(value);
                this.Allegiance = aDef;
            }
        }

        [JsonIgnore]
        public Superpower Allegiance { get; set; } = Superpower.None;

        /// <summary> The home system of the faction </summary>
        [PublicAPI( "faction presence determined by minor faction name or highest influence" )]
        public string system
        {
            get => _system;
            set
            {
                if (_system != value)
                {
                    _system = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary> The home station of the faction </summary>
        [PublicAPI( "station nearest to main star, filtered by landing pad & ship size" )]
        public string station
        {
            get => _station;
            set
            {
                if (_station != value)
                {
                    _station = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary> The final estimated credit value of claims (bounty vouchers and bonds), including any final-value discrepancy report </summary>
        [PublicAPI( "estimated final credits for minor faction's uncollected bounty voucher and combat bond rewards, after known current voucher modifiers. Pending claim values do not subtract NPC crew wages." ), JsonIgnore]
        public long claims
        {
            get => _finalClaims ?? ( _baseclaims + finalClaimDiscrepancy );
            set
            {
                var amount = value - claims;
                if ( amount == 0 )
                {
                    return;
                }

                ApplyFinalClaimDiscrepancy( amount );
                _finalClaims = value;
                OnPropertyChanged();
            }
        }

        /// <summary> The journal credit value of claims before non-journal modifiers and final-value discrepancy reports are applied </summary>
        [PublicAPI( "journal credits for minor faction's uncollected bounty voucher and combat bond rewards before known current voucher modifiers" ), JsonIgnore]
        public long baseclaims
        {
            get => _baseclaims;
            set
            {
                if (_baseclaims == value)
                {
                    return;
                }

                _baseclaims = value;
                _finalClaims = null;
                OnPropertyChanged();
                OnPropertyChanged(nameof(claims));
            }
        }

        /// <summary> The final estimated credit value of bounty voucher claims only </summary>
        [PublicAPI( "estimated final credits for the minor faction's uncollected bounty voucher rewards only, after known current voucher modifiers" ), JsonIgnore]
        public long bountyclaims
        {
            get => _finalBountyClaims ?? basebountyclaims;
            private set
            {
                if (_finalBountyClaims == value)
                {
                    return;
                }

                _finalBountyClaims = value;
                OnPropertyChanged();
            }
        }

        /// <summary> The journal credit value of bounty voucher claims before non-journal modifiers are applied </summary>
        [PublicAPI( "journal credits for the minor faction's uncollected bounty voucher rewards only before known current voucher modifiers" ), JsonIgnore]
        public long basebountyclaims => basebountiesAmount;

        /// <summary> The total credit value of fines incurred (including any discrepancy report) </summary>
        [PublicAPI( "total journal credits for minor faction's unpaid fines incurred, including any reductions already applied by the game" )]
        public long fines
        {
            get => _fines;
            set
            {
                _fines = value;
                OnPropertyChanged();
            }
        }

        /// <summary> The journal credit value of fines incurred </summary>
        [PublicAPI( "journal credits for minor faction's unpaid fines incurred" ), JsonIgnore]
        public long basefines => finesIncurred.Sum(ReportBaseAmount);

        /// <summary> The total credit value of bounties incurred (including any discrepancy report) </summary>
        [PublicAPI( "total journal credits for minor faction's unpaid bounties incurred, including any reductions already applied by the game" )]
        public long bounties
        {
            get => _bounties;
            set
            {
                _bounties = value;
                OnPropertyChanged();
            }
        }

        /// <summary> The journal credit value of bounties incurred </summary>
        [PublicAPI( "journal credits for minor faction's unpaid bounties incurred" ), JsonIgnore]
        public long basebounties => bountiesIncurred.Sum(ReportBaseAmount);

        public List<string> factionSystems { get; set; } = [ ];
        public List<string> interstellarBountyFactions { get; set; } = [ ];
        public List<FactionReport> factionReports { get; set; } = [ ];

        [JsonIgnore]
        private string _faction;

        [JsonIgnore]
        private string _system;

        [JsonIgnore]
        private string _station;

        [JsonProperty( "claims" )]
        private long _baseclaims;

        [JsonIgnore]
        private long _fines;

        [JsonIgnore]
        private long _bounties;

        [JsonIgnore]
        private long? _finalClaims;

        [JsonIgnore]
        private long? _finalBountyClaims;

        [JsonIgnore]
        private long finalClaimDiscrepancy => factionReports
            .Where( r => r.crimeDef == Crime.Claim )
            .Sum( r => r.amount );

        private void ApplyFinalClaimDiscrepancy ( long amount )
        {
            var report = factionReports.FirstOrDefault( r => r.crimeDef == Crime.Claim );
            if ( report == null )
            {
                report = new FactionReport( System.DateTime.UtcNow, false, Crime.Claim, null, 0 );
                factionReports.Add( report );
            }

            report.amount += amount;
            if ( report.amount == 0 )
            {
                factionReports.Remove( report );
            }
        }

        /// <summary> All bond vouchers awarded, excluding the discrepancy report </summary>
        [PublicAPI( "list of individual faction reports for uncollected bonds awarded" ), JsonIgnore]
        public List<FactionReport> bondsAwarded => factionReports
            .Where(r => !r.bounty && r.crimeDef == Crime.None && r.claimtype == FactionReport.BondClaimType)
            .ToList();

        [JsonIgnore] 
        public long bondsAmount => bondsAwarded.Sum(r => r.amount);

        [JsonIgnore]
        public long basebondsAmount => bondsAwarded.Sum(ReportBaseAmount);

        /// <summary> All bounty vouchers awarded, excluding the discrepancy report </summary>
        [PublicAPI( "list of individual faction reports for uncollected bounties awarded" ), JsonIgnore]
        public List<FactionReport> bountiesAwarded => factionReports
            .Where(r => r.bounty && r.crimeDef == Crime.None && r.claimtype == FactionReport.BountyClaimType)
            .ToList();

        [JsonIgnore] 
        public long bountiesAmount => bountiesAwarded.Sum(r => r.amount);

        [JsonIgnore]
        public long basebountiesAmount => bountiesAwarded.Sum(ReportBaseAmount);

        /// <summary> All fines incurred, including the discrepancy report </summary>
        [PublicAPI( "list of individual faction reports for unpaid fines incurred" ), JsonIgnore]
        public List<FactionReport> finesIncurred => factionReports
            .Where(r => !r.bounty && r.crimeDef != Crime.None)
            .ToList();

        /// <summary> All bounties incurred, including the discrepancy report </summary>
        [PublicAPI( "list of individual faction reports for unpaid bounties incurred" ), JsonIgnore]
        public List<FactionReport> bountiesIncurred => factionReports
            .Where(r => r.bounty && r.crimeDef != Crime.None)
            .ToList();

        // Default Constructor
        public FactionRecord() { }

        [JsonConstructor]
        public FactionRecord(string faction)
        {
            this.faction = faction;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateFinalClaimValues(long finalClaims, long? finalBountyClaims)
        {
            if (_finalClaims != finalClaims)
            {
                _finalClaims = finalClaims;
                OnPropertyChanged(nameof(claims));
            }

            if (_finalBountyClaims != finalBountyClaims)
            {
                _finalBountyClaims = finalBountyClaims;
                OnPropertyChanged(nameof(bountyclaims));
            }
        }

        private static long ReportBaseAmount(FactionReport report)
        {
            return report?.amount ?? 0;
        }

        [JetBrains.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
