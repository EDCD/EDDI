using EddiDataDefinitions;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Utilities;

namespace EddiCore.GameState
{
    internal sealed class EddiGameState : IEddiGameState
    {
        public bool inTelepresence
        {
            get => _inTelepresence;
            internal set => SetValue( ref _inTelepresence, value );
        }
        private bool _inTelepresence;

        public bool inHorizons
        {
            get => _inHorizons;
            internal set => SetValue( ref _inHorizons, value );
        }
        private bool _inHorizons = true;

        public bool inOdyssey
        {
            get => _inOdyssey;
            internal set => SetValue( ref _inOdyssey, value );
        }
        private bool _inOdyssey = true;

        public bool gameIsBeta
        {
            get => _gameIsBeta;
            internal set => SetValue( ref _gameIsBeta, value );
        }
        private bool _gameIsBeta;

        public System.Version GameVersion
        {
            get => _gameVersion;
            internal set => SetValue( ref _gameVersion, value );
        }
        private System.Version _gameVersion;

        internal string GameVersionRaw { get; set; }

        [CanBeNull]
        public StarSystem DestinationStarSystem
        {
            get => _destinationStarSystem;
            internal set => SetChild( ref _destinationStarSystem, value, ref _destinationStarSystemChangedHandler, nameof(DestinationStarSystem) );
        }
        private StarSystem _destinationStarSystem;
        private PropertyChangedEventHandler _destinationStarSystemChangedHandler;

        public decimal DestinationDistanceLy
        {
            get => _destinationDistanceLy;
            internal set => SetValue( ref _destinationDistanceLy, value );
        }
        private decimal _destinationDistanceLy;

        public string Environment
        {
            get => _environment;
            internal set => SetValue( ref _environment, value );
        }
        private string _environment;

        [CanBeNull]
        public StarSystem CurrentStarSystem
        {
            get => _currentStarSystem;
            internal set => SetChild( ref _currentStarSystem, value, ref _currentStarSystemChangedHandler, nameof(CurrentStarSystem) );
        }
        private StarSystem _currentStarSystem;
        private PropertyChangedEventHandler _currentStarSystemChangedHandler;

        [CanBeNull]
        public StarSystem LastStarSystem
        {
            get => _lastStarSystem;
            internal set => SetChild( ref _lastStarSystem, value, ref _lastStarSystemChangedHandler, nameof(LastStarSystem) );
        }
        private StarSystem _lastStarSystem;
        private PropertyChangedEventHandler _lastStarSystemChangedHandler;

        [CanBeNull]
        public StarSystem NextStarSystem
        {
            get => _nextStarSystem;
            internal set => SetChild( ref _nextStarSystem, value, ref _nextStarSystemChangedHandler, nameof(NextStarSystem) );
        }
        private StarSystem _nextStarSystem;
        private PropertyChangedEventHandler _nextStarSystemChangedHandler;

        [CanBeNull]
        public Station CurrentStation
        {
            get => _currentStation;
            internal set => SetChild( ref _currentStation, value, ref _currentStationChangedHandler, nameof(CurrentStation) );
        }
        private Station _currentStation;
        private PropertyChangedEventHandler _currentStationChangedHandler;

        [CanBeNull]
        public Body CurrentStellarBody
        {
            get => _currentStellarBody;
            internal set => SetChild( ref _currentStellarBody, value, ref _currentStellarBodyChangedHandler, nameof(CurrentStellarBody) );
        }
        private Body _currentStellarBody;
        private PropertyChangedEventHandler _currentStellarBodyChangedHandler;

        [CanBeNull]
        public FleetCarrier FleetCarrier
        {
            get => _fleetCarrier;
            internal set => SetChild( ref _fleetCarrier, value, ref _fleetCarrierChangedHandler, nameof(FleetCarrier) );
        }
        private FleetCarrier _fleetCarrier;
        private PropertyChangedEventHandler _fleetCarrierChangedHandler;

        [CanBeNull]
        public FleetCarrier SquadronCarrier
        {
            get => _squadronCarrier;
            internal set => SetChild( ref _squadronCarrier, value, ref _squadronCarrierChangedHandler, nameof(SquadronCarrier) );
        }
        private FleetCarrier _squadronCarrier;
        private PropertyChangedEventHandler _squadronCarrierChangedHandler;

        [CanBeNull]
        public Ship CurrentShip
        {
            get => _currentShip;
            internal set => SetChild( ref _currentShip, value, ref _currentShipChangedHandler, nameof(CurrentShip) );
        }
        private Ship _currentShip;
        private PropertyChangedEventHandler _currentShipChangedHandler;

        public string Vehicle
        {
            get => _vehicle;
            internal set
            {
                _vehicle = value;
                OnPropertyChanged();
            }
        }
        private string _vehicle = Constants.VEHICLE_SHIP;

        private void SetValue<T> ( ref T field, T value, [CallerMemberName] string propertyName = null )
        {
            if ( EqualityComparer<T>.Default.Equals( field, value ) ) { return; }
            field = value;
            OnPropertyChanged( propertyName );
        }

        private void SetChild<T> (
            ref T field,
            T value,
            ref PropertyChangedEventHandler propertyChangedHandler,
            string propertyName )
            where T : class
        {
            if ( field != null && propertyChangedHandler != null )
            {
                UnsubscribeChild( field, propertyChangedHandler );
            }

            field = value;
            propertyChangedHandler = value == null
                ? null
                : ( _, _ ) => OnPropertyChanged( propertyName );

            if ( value != null )
            {
                SubscribeChild( value, propertyChangedHandler );
            }

            OnPropertyChanged( propertyName );
        }

        private static void SubscribeChild<T> ( T child, PropertyChangedEventHandler handler )
            where T : class
        {
            switch ( child )
            {
                case StarSystem starSystem:
                    starSystem.PropertyChanged += handler;
                    break;
                case INotifyPropertyChanged observable:
                    observable.PropertyChanged += handler;
                    break;
            }
        }

        private static void UnsubscribeChild<T> ( T child, PropertyChangedEventHandler handler )
            where T : class
        {
            switch ( child )
            {
                case StarSystem starSystem:
                    starSystem.PropertyChanged -= handler;
                    break;
                case INotifyPropertyChanged observable:
                    observable.PropertyChanged -= handler;
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
