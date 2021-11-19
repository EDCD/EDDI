using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class CargoInfoItem
    {
        [JsonProperty]
        public string name { get; set; }

        [JsonProperty]
        public long? missionid { get; set; }

        [JsonProperty]
        public int count { get; set; }

        [JsonProperty]
        public int stolen { get; set; }

        public CargoInfoItem()
        { }

        public CargoInfoItem(CargoInfoItem cargoInfoItem)
        {
            this.name = cargoInfoItem.name;
            this.missionid = cargoInfoItem.missionid;
            this.count = cargoInfoItem.count;
            this.stolen = cargoInfoItem.stolen;
        }

        public CargoInfoItem(string Name, long? MissionID, int Count, int Stolen)
        {
            this.name = Name;
            this.missionid = MissionID;
            this.count = Count;
            this.stolen = Stolen;
        }
    }
}
