using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class CargoInfo
    {
        [JsonProperty]
        public string name { get; set; }

        [JsonProperty]
        public long? missionid { get; set; }

        [JsonProperty]
        public int count { get; set; }

        [JsonProperty]
        public int stolen { get; set; }

        public CargoInfo()
        { }

        public CargoInfo(CargoInfo CargoInfo)
        {
            this.name = CargoInfo.name;
            this.missionid = CargoInfo.missionid;
            this.count = CargoInfo.count;
            this.stolen = CargoInfo.stolen;
        }

        public CargoInfo(string Name, long? MissionID, int Count, int Stolen)
        {
            this.name = Name;
            this.missionid = MissionID;
            this.count = Count;
            this.stolen = Stolen;

        }
    }
}
