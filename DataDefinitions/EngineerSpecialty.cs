using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiDataDefinitions
{
    public class EngineerSpecialty : ResourceBasedLocalizedEDName<EngineerSpecialty>
    {
        static EngineerSpecialty()
        {
            resourceManager = Properties.EngineerSpecialty.ResourceManager;
            resourceManager.IgnoreCase = false;
        }

        public static readonly EngineerSpecialty AFMUs = new EngineerSpecialty("AFMUs");
        public static readonly EngineerSpecialty Armour = new EngineerSpecialty("Armour");
        public static readonly EngineerSpecialty Cannons = new EngineerSpecialty("Cannons");
        public static readonly EngineerSpecialty ChaffAndHeatSinkLaunchers = new EngineerSpecialty("ChaffAndHeatSinkLaunchers");
        public static readonly EngineerSpecialty ECMs = new EngineerSpecialty("ECMs");
        public static readonly EngineerSpecialty FragCannons = new EngineerSpecialty("FragCannons");
        public static readonly EngineerSpecialty FrameShiftDrives = new EngineerSpecialty("FrameShiftDrives");
        public static readonly EngineerSpecialty FrameShiftDriveInterdictors = new EngineerSpecialty("FrameShiftDriveInterdictors");
        public static readonly EngineerSpecialty FuelScoops = new EngineerSpecialty("FuelScoops");
        public static readonly EngineerSpecialty HullReinforcement = new EngineerSpecialty("HullReinforcement");
        public static readonly EngineerSpecialty Lasers = new EngineerSpecialty("Lasers");
        public static readonly EngineerSpecialty LifeSupportSystems = new EngineerSpecialty("LifeSupportSystems");
        public static readonly EngineerSpecialty LimpetControllers = new EngineerSpecialty("LimpetControllers");
        public static readonly EngineerSpecialty Mines = new EngineerSpecialty("Mines");
        public static readonly EngineerSpecialty Missiles = new EngineerSpecialty("Missiles");
        public static readonly EngineerSpecialty MultiCannons = new EngineerSpecialty("MultiCannons");
        public static readonly EngineerSpecialty PlasmaAccelerators = new EngineerSpecialty("PlasmaAccelerators");
        public static readonly EngineerSpecialty PointDefence = new EngineerSpecialty("PointDefence");
        public static readonly EngineerSpecialty PowerDistributors = new EngineerSpecialty("PowerDistributors");
        public static readonly EngineerSpecialty PowerPlants = new EngineerSpecialty("PowerPlants");
        public static readonly EngineerSpecialty RailGuns = new EngineerSpecialty("RailGuns");
        public static readonly EngineerSpecialty Refineries = new EngineerSpecialty("Refineries");
        public static readonly EngineerSpecialty Scanners = new EngineerSpecialty("Scanners");
        public static readonly EngineerSpecialty Sensors = new EngineerSpecialty("Sensors");
        public static readonly EngineerSpecialty ShieldBoosters = new EngineerSpecialty("ShieldBoosters");
        public static readonly EngineerSpecialty ShieldCellBanks = new EngineerSpecialty("ShieldCellBanks");
        public static readonly EngineerSpecialty ShieldGenerators = new EngineerSpecialty("ShieldGenerators");
        public static readonly EngineerSpecialty SurfaceScanners = new EngineerSpecialty("SurfaceScanners");
        public static readonly EngineerSpecialty Thrusters = new EngineerSpecialty("Thrusters");
        public static readonly EngineerSpecialty Torpedos = new EngineerSpecialty("Torpedos");

        // dummy used to ensure that the static constructor has run
        public EngineerSpecialty() : this("")
        { }

        private EngineerSpecialty(string edname) : base(edname, edname)
        { }
    }
}
