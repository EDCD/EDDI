using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace EddiShipMonitor
{
    public class ModuleInfoReader
    {
        public List<ModuleInfo> Modules { get; set; }

        public ModuleInfoReader()
        {
            Modules = new List<ModuleInfo>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static ModuleInfoReader FromFile(string filename = null)
        {
            ModuleInfoReader info = new ModuleInfoReader();

            string data = Files.FromSavedGames("ModulesInfo.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<ModuleInfoReader>(data);
            }
            return info;
        }
    }
}
