using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class PowerAcquisitionProgress
    {
        [ JsonProperty( "Power" ) ]
        public string power
        {
            get => Power.edname;
            set => Power = Power.FromEDName( value );
        }

        [JsonIgnore]
        public Power Power { get; private set; }

        [JsonProperty( "Progress" )]
        public decimal progress { get; set; }

        public PowerAcquisitionProgress ( string power, decimal progress )
        {
            this.power = power;
            this.progress = progress;
        }
    }
}
