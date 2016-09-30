using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousSpeechResponder
{
    public class Script : INotifyPropertyChanged
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("description")]
        public string Description { get; private set; }
        [JsonProperty("enabled")]
        private bool enabled;
        [JsonProperty("interruptable")]
        private bool interruptable = true;
        [JsonProperty("responder")]
        private bool responder;
        [JsonProperty("script")]
        private string script;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value;  OnPropertyChanged("Enabled"); }
        }

        [JsonIgnore]
        public bool Interruptable
        {
            get { return interruptable; }
            set { interruptable = value; OnPropertyChanged("Interruptable"); }
        }

        [JsonIgnore]
        public bool Responder
        {
            get { return responder; }
            private set {  }
        }

        [JsonIgnore]
        public bool IsDeleteable
        {
            get { return !responder;  }
        }

        [JsonIgnore]
        public string Value
        {
            get { return script; }
            set { script = value; OnPropertyChanged("Value"); }
        }

        [JsonIgnore]
        public bool HasValue
        {
            get { return script != null; }
        }

        public Script(string name, string description, bool responder, string script, bool interruptable = true)
        {
            Name = name;
            Description = description;
            this.responder = responder;
            Value = script;
            Interruptable = interruptable;
            Enabled = true;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
