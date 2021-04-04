using Utilities;

namespace EddiDataDefinitions
{
    public class StoredModule
    {
        [PublicAPI]
        public Module module { get; set; }

        [PublicAPI]
        public string name => module?.localizedName;

        [PublicAPI]
        public int slot { get; set; }

        [PublicAPI]
        public bool intransit { get; set; }

        [PublicAPI]
        public string system { get; set; }

        [PublicAPI]
        public string station { get; set; }

        [PublicAPI]
        public long? marketid { get; set; }

        [PublicAPI]
        public long? transfercost { get; set; }

        [PublicAPI]
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