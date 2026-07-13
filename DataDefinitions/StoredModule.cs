using Utilities;

namespace EddiDataDefinitions
{
    public class StoredModule
    {
        [PublicAPI("the module being stored, as an object")]
        public Module module { get; set; }

        [PublicAPI( "localized name of the module" )]
        public string name => module?.localizedName;

        [PublicAPI("the storage slot where the module is assigned")]
        public int slot { get; set; }

        [PublicAPI("true if the module is currently in transit")]
        public bool intransit { get; set; }

        [PublicAPI("the system where the module is stored")]
        public string system { get; set; }

        [PublicAPI("the station where the module is stored")]
        public string station { get; set; }

        [PublicAPI("the market ID where the module is stored")]
        public long? marketid { get; set; }

        [PublicAPI("the cost of transferring the module, in credits")]
        public long? transfercost { get; set; }

        [PublicAPI("the time required to transfer the module, in seconds")]
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
