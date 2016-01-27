namespace EliteDangerousDataDefinitions
{
    public class Module
    {
        // Definition of the module
        public string Name { get; set; }
        public int Class { get; set; }
        public string Grade { get; set; }
        public long Value { get; set; } // The undiscounted value

        // State of the module
        public long Cost { get; set; } // How much we actually paid for it
        public bool Enabled { get; set; }
        public int Priority { get; set; }
        public decimal Health { get; set; }

        // Admin
        public long EDDBID { get; set; }

        public Module() { }

        public Module(Module Module)
        {
            this.Name = Module.Name;
            this.Class = Module.Class;
            this.Grade = Module.Grade;
            this.Value = Module.Value;
            this.EDDBID = Module.EDDBID;
        }

        public Module(long EDDBID, string Name, int Class, string Grade, long Value)
        {
            this.EDDBID = EDDBID;
            this.Name = Name;
            this.Class = Class;
            this.Grade = Grade;
            this.Value = Value;
        }
    }
}
