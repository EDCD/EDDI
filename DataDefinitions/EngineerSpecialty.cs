namespace EddiDataDefinitions
{
    public class EngineerSpecialty : ResourceBasedLocalizedEDName<EngineerSpecialty>
    {
        static EngineerSpecialty()
        {
            resourceManager = Properties.EngineerSpecialty.ResourceManager;
            resourceManager.IgnoreCase = false;
        }

        public static readonly EngineerSpecialty AFMUs = new("AFMUs");
        public static readonly EngineerSpecialty Armour = new("Armour");
        public static readonly EngineerSpecialty Cannons = new("Cannons");
        public static readonly EngineerSpecialty ChaffAndHeatSinkLaunchers = new("ChaffAndHeatSinkLaunchers");
        public static readonly EngineerSpecialty ECMs = new("ECMs");
        public static readonly EngineerSpecialty FragCannons = new("FragCannons");
        public static readonly EngineerSpecialty FrameShiftDrives = new("FrameShiftDrives");
        public static readonly EngineerSpecialty FrameShiftDriveInterdictors = new("FrameShiftDriveInterdictors");
        public static readonly EngineerSpecialty FuelScoops = new("FuelScoops");
        public static readonly EngineerSpecialty HullReinforcement = new("HullReinforcements");
        public static readonly EngineerSpecialty Lasers = new("Lasers");
        public static readonly EngineerSpecialty LifeSupportSystems = new("LifeSupportSystems");
        public static readonly EngineerSpecialty LimpetControllers = new("LimpetControllers");
        public static readonly EngineerSpecialty Mines = new("Mines");
        public static readonly EngineerSpecialty Missiles = new("Missiles");
        public static readonly EngineerSpecialty MultiCannons = new("MultiCannons");
        public static readonly EngineerSpecialty PlasmaAccelerators = new("PlasmaAccelerators");
        public static readonly EngineerSpecialty PointDefence = new("PointDefence");
        public static readonly EngineerSpecialty PowerDistributors = new("PowerDistributors");
        public static readonly EngineerSpecialty PowerPlants = new("PowerPlants");
        public static readonly EngineerSpecialty RailGuns = new("RailGuns");
        public static readonly EngineerSpecialty Refineries = new("Refineries");
        public static readonly EngineerSpecialty Scanners = new("Scanners");
        public static readonly EngineerSpecialty Sensors = new("Sensors");
        public static readonly EngineerSpecialty ShieldBoosters = new("ShieldBoosters");
        public static readonly EngineerSpecialty ShieldCellBanks = new("ShieldCellBanks");
        public static readonly EngineerSpecialty ShieldGenerators = new("ShieldGenerators");
        public static readonly EngineerSpecialty SurfaceScanners = new("SurfaceScanners");
        public static readonly EngineerSpecialty Thrusters = new("Thrusters");
        public static readonly EngineerSpecialty Torpedos = new("Torpedos");

        // dummy used to ensure that the static constructor has run
        public EngineerSpecialty() : this("")
        { }

        private EngineerSpecialty(string edname) : base(edname, edname)
        { }
    }
}
