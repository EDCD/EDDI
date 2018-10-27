using EddiDataDefinitions;
using Newtonsoft.Json;
using System.ComponentModel;

namespace EddiEddpMonitor
{
    /// <summary>
    /// The parameters to match EDDP messages
    /// </summary>
    public class Watch : INotifyPropertyChanged
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("system")]
        public string System { get; set; }

        [JsonProperty("station")]
        public string Station { get; set; }

        [JsonProperty("faction")]
        public string Faction { get; set; }

        [JsonProperty("state")]
        public FactionState State
        {
            get { return _state; }
            set { _state = value; OnPropertyChanged("State"); }
        }
        private FactionState _state;

        [JsonProperty("maxdistancefromship")]
        public long? MaxDistanceFromShip { get; set; }

        [JsonProperty("maxdistancefromhome")]
        public long? MaxDistanceFromHome { get; set; }

        public Watch() { }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
