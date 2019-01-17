using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class StoredModule
    {
        public Module module { get; set; }

        public string name => module?.localizedName;

        public int slot { get; set; }

        public bool intransit { get; set; }

        public string system { get; set; }

        public string station { get; set; }

        public long? marketid { get; set; }

        public long? transfercost { get; set; }

        public long? transfertime { get; set; }

        public StoredModule()
        { }

        public StoredModule(StoredModule StoredModule)
        {
            this.module = StoredModule.module;
            this.slot = StoredModule.slot;
            this.intransit = StoredModule.intransit;
            this.marketid = StoredModule.marketid;
            this.system = StoredModule.system;
            this.station = StoredModule.station;
            this.transfercost = StoredModule.transfercost;
            this.transfertime = StoredModule.transfertime;
        }
    }
}