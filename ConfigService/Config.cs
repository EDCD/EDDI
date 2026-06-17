using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EddiConfigService
{
    public abstract class Config : INotifyPropertyChanged
    {
        #region Gather data needed for legacy data conversions

        [JsonExtensionData]
        internal IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

        public bool HasAdditionalData ( string key )
        {
            return !string.IsNullOrWhiteSpace( key ) &&
                   _additionalData?.ContainsKey( key ) == true;
        }

        public bool TryGetAdditionalData<T> ( string key, out T value )
        {
            value = default;
            if ( string.IsNullOrWhiteSpace( key ) ||
                 _additionalData?.TryGetValue( key, out var token ) != true )
            {
                return false;
            }

            try
            {
                value = token.ToObject<T>();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveAdditionalData ( string key )
        {
            if ( string.IsNullOrWhiteSpace( key ) ||
                 _additionalData?.Remove( key ) != true )
            {
                return false;
            }

            OnPropertyChanged();
            return true;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class RelativePathAttribute ( string relativePath ) : Attribute
    {
        public string relativePath { get; } = relativePath;
    }
}
