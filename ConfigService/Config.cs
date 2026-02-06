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
    public class RelativePathAttribute : Attribute
    {
        public string relativePath { get; }

        public RelativePathAttribute(string relativePath) { this.relativePath = relativePath; }
    }
}