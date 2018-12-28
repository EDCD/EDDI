using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace EddiCargoMonitor
{
    public class CargoInfoReader
    {
        public int Count { get; set; }
        public List<CargoInfo> Inventory { get; set; }

        public CargoInfoReader()
        {
            Inventory = new List<CargoInfo>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static CargoInfoReader FromFile(string filename = null)
        {
            CargoInfoReader info = new CargoInfoReader();

            string data = Files.FromSavedGames("Cargo.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<CargoInfoReader>(data);
            }
            return info;
        }
    }
}
