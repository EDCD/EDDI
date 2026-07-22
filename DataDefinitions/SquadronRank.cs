using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Squadron Rank. Each squadron can define its own ranks so these are specific to a given squadron.Standardized localizations are not possible.
    /// </summary>
    public class SquadronRank : INotifyPropertyChanged
    {
        private string _localizedName;
        private string _invariantName;
        private int? _rankId;

        [ PublicAPI( "The numeric squadron rank ID." ) ]
        public int? rankID
        {
            get => _rankId;
            private set
            {
                if ( value == _rankId )
                {
                    return;
                }

                _rankId = value;
                OnPropertyChanged();
            }
        }

        [ PublicAPI( "The invariant name of the squadron rank." ) ]
        public string invariantName
        {
            get => _invariantName;
            private set
            {
                if ( value == _invariantName )
                {
                    return;
                }

                _invariantName = value;
                OnPropertyChanged();
            }
        }

        [ PublicAPI( "The localized name of the squadron rank." ) ]
        public string localizedName
        {
            get =>  _localizedName ?? _invariantName;
            set
            {
                if ( value == _localizedName )
                {
                    return;
                }

                _localizedName = value;
                OnPropertyChanged();
            }
        }

        public SquadronRank(int? rankId, string invariantName, string localizedName)
        {
            this.rankID = rankId;
            this.invariantName = invariantName;
            this.localizedName = localizedName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged ( [ CallerMemberName ] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected bool SetField<T> ( ref T field, T value, [ CallerMemberName ] string propertyName = null )
        {
            if ( EqualityComparer<T>.Default.Equals( field, value ) ) return false;
            field = value;
            OnPropertyChanged( propertyName );
            return true;
        }
    }
}
