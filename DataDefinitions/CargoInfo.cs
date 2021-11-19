using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class CargoInfo
    {
        public int Count => Inventory.Count;
        public List<CargoInfoItem> Inventory { get; set; } = new List<CargoInfoItem>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static CargoInfo FromFile(string filename = null)
        {
            CargoInfo info = new CargoInfo();

            string data = Files.FromSavedGames("Cargo.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<CargoInfo>(data);
            }
            return info;
        }
    }
}
