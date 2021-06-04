using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Engineers
    /// </summary>
    public class Engineer
    {
        internal static List<Engineer> ENGINEERS = new List<Engineer>();

        public long id { get; private set; }

        public string name { get; private set; }

        public string systemname { get; private set; }
        
        public long systemAddress { get; private set; }

        public string stationname { get; private set; }

        public long marketId { get; private set; }

        // Top tier specialties
        public HashSet<EngineerSpecialty> majorSpecialties { get; private set; }
        public List<string> majorspecialties => majorSpecialties?.Select(s => s.localizedName).ToList();

        // Other specialties
        public HashSet<EngineerSpecialty> minorSpecialties { get; private set; }
        public List<string> minorspecialties => minorSpecialties?.Select(s => s.localizedName).ToList();

        // Progress
        public string stage { get; set; }

        public int? rankprogress { get; set; }

        public int? rank { get; set; }

        public Engineer(string name, long engineerId, string progressStage, int? rankProgress, int? rank)
        {
            this.name = name;
            this.id = engineerId;
            this.stage = progressStage;
            this.rankprogress = rankProgress;
            this.rank = rank;
            this.majorSpecialties = ENGINEERS.SingleOrDefault(e => e.id == engineerId)?.majorSpecialties;
            this.minorSpecialties = ENGINEERS.SingleOrDefault(e => e.id == engineerId)?.minorSpecialties;
        }

        private Engineer(long engineerId, string engineerName, string systemName, long systemAddress, string stationName, long marketId, HashSet<EngineerSpecialty> majorSpecialties, HashSet<EngineerSpecialty> minorSpecialties)
        {
            this.id = engineerId;
            this.name = engineerName;
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.stationname = stationName;
            this.marketId = marketId;
            this.majorSpecialties = majorSpecialties;
            this.minorSpecialties = minorSpecialties;

            ENGINEERS.Add(this);
        }

        public static readonly Engineer DidiVatermann = new Engineer(300000, "Didi Vatermann", "Leesti", 3932277478114, "Vatermann LLC", 128673927, new HashSet<EngineerSpecialty> { EngineerSpecialty.ShieldBoosters }, new HashSet<EngineerSpecialty> { EngineerSpecialty.ShieldGenerators } );
        public static readonly Engineer BillTurner = new Engineer(300010, "Bill Turner", "Alioth", 1109989017963, "Turner Metallics Inc", 128674183, new HashSet<EngineerSpecialty> { EngineerSpecialty.PlasmaAccelerators, EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners }, new HashSet<EngineerSpecialty> { EngineerSpecialty.LifeSupportSystems, EngineerSpecialty.Refineries, EngineerSpecialty.AFMUs, EngineerSpecialty.FuelScoops, EngineerSpecialty.Scanners });
        public static readonly Engineer BrooTarquin = new Engineer(300030, "Broo Tarquin", "Muang", 4481966019282, "Broo's Legacy", 128674695, new HashSet<EngineerSpecialty> { EngineerSpecialty.Lasers }, new HashSet<EngineerSpecialty>() );
        public static readonly Engineer TheSarge = new Engineer(300040, "The Sarge", "Beta-3 Tucani", 2827992680811, "The Beach", 128674951, new HashSet<EngineerSpecialty> { EngineerSpecialty.LimpetControllers, EngineerSpecialty.Cannons }, new HashSet<EngineerSpecialty> { EngineerSpecialty.RailGuns } );
        public static readonly Engineer ZacariahNemo = new Engineer(300050, "Zacariah Nemo", "Yoru", 6131367744226, "Nemo Cyber Party Base", 128675207, new HashSet<EngineerSpecialty> { EngineerSpecialty.FragCannons }, new HashSet<EngineerSpecialty> { EngineerSpecialty.MultiCannons, EngineerSpecialty.PlasmaAccelerators });
        public static readonly Engineer LizRyder = new Engineer(300080, "Liz Ryder", "Eurybia", 1458309141194, "Demolition Unlimited", 128675975, new HashSet<EngineerSpecialty> { EngineerSpecialty.Missiles, EngineerSpecialty.Torpedos }, new HashSet<EngineerSpecialty> { EngineerSpecialty.Mines, EngineerSpecialty.HullReinforcement });
        public static readonly Engineer HeraTani = new Engineer(300090, "Hera Tani", "Kuwemaki", 1733321102034, "The Jet's Hole", 128676231, new HashSet<EngineerSpecialty> { EngineerSpecialty.PowerPlants, EngineerSpecialty.SurfaceScanners }, new HashSet<EngineerSpecialty> { EngineerSpecialty.Sensors, EngineerSpecialty.PowerDistributors } );
        public static readonly Engineer FelicityFarseer = new Engineer(300100, "Felicity Farseer", "Deciat", 6681123623626, "Farseer Inc", 128676487, new HashSet<EngineerSpecialty> { EngineerSpecialty.FrameShiftDrives }, new HashSet<EngineerSpecialty> { EngineerSpecialty.Thrusters, EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners, EngineerSpecialty.ShieldBoosters, EngineerSpecialty.FrameShiftDriveInterdictors, EngineerSpecialty.PowerPlants});
        public static readonly Engineer RamTah = new Engineer(300110, "Ram Tah", "Meene", 3790082132323, "Phoenix Base", 128676743, new HashSet<EngineerSpecialty> { EngineerSpecialty.ECMs, EngineerSpecialty.PointDefence, EngineerSpecialty.ChaffAndHeatSinkLaunchers }, new HashSet<EngineerSpecialty> { EngineerSpecialty.LimpetControllers });
        public static readonly Engineer LeiCheung = new Engineer(300120, "Lei Cheung", "Laksak", 4305444669811, "Trader's Rest", 128676999, new HashSet<EngineerSpecialty> { EngineerSpecialty.ShieldGenerators, EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners }, new HashSet<EngineerSpecialty> { EngineerSpecialty.ShieldBoosters } );
        public static readonly Engineer PetraOlmanova = new Engineer(300130, "Petra Olmanova", "Asura", 12274907287851, "Sanctuary", 128677255, new HashSet<EngineerSpecialty> { EngineerSpecialty.HullReinforcement, EngineerSpecialty.Missiles, EngineerSpecialty.ChaffAndHeatSinkLaunchers, EngineerSpecialty.PointDefence }, new HashSet<EngineerSpecialty> {EngineerSpecialty.Mines, EngineerSpecialty.Torpedos, EngineerSpecialty.ECMs, EngineerSpecialty.AFMUs});
        public static readonly Engineer ColBrisDekker = new Engineer(300140, "Colonel Bris Dekker", "Sol", 10477373803, "Dekker's Yard", 128677511, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty> { EngineerSpecialty.FrameShiftDriveInterdictors, EngineerSpecialty.FrameShiftDrives } );
        public static readonly Engineer MarshaHicks = new Engineer(300150, "Marsha Hicks", "Tir", 48996147307082, "The Watchtower", 128677767, new HashSet<EngineerSpecialty> { EngineerSpecialty.LimpetControllers, EngineerSpecialty.Refineries, EngineerSpecialty.FuelScoops, EngineerSpecialty.MultiCannons, EngineerSpecialty.FragCannons }, new HashSet<EngineerSpecialty>{EngineerSpecialty.Cannons});
        public static readonly Engineer ElviraMartuuk = new Engineer(300160, "Elvira Martuuk", "Khun", 3107241104074, "Long Sight Base", 128678023, new HashSet<EngineerSpecialty> { EngineerSpecialty.FrameShiftDrives }, new HashSet<EngineerSpecialty> { EngineerSpecialty.ShieldGenerators, EngineerSpecialty.Thrusters, EngineerSpecialty.ShieldCellBanks});
        public static readonly Engineer TheDweller = new Engineer(300180, "The Dweller", "Wyrd", 5031654888146, "Black Hide", 128678535, new HashSet<EngineerSpecialty> { EngineerSpecialty.PowerDistributors }, new HashSet<EngineerSpecialty> { EngineerSpecialty.Lasers } );
        public static readonly Engineer MarcoQwent = new Engineer(300200, "Marco Qwent", "Sirius", 121569805492, "Qwent Research Base", 128679047, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty> { EngineerSpecialty.PowerPlants, EngineerSpecialty.PowerDistributors } );
        public static readonly Engineer SeleneJean = new Engineer(300210, "Selene Jean", "Kuk", 24859942069665, "Prospector's Rest", 128679303, new HashSet<EngineerSpecialty> { EngineerSpecialty.Armour, EngineerSpecialty.HullReinforcement }, new HashSet<EngineerSpecialty>());
        public static readonly Engineer ProfessorPalin = new Engineer(300220, "Professor Palin", "Arque", 113573366131, "Abel Laboratory", 128679559, new HashSet<EngineerSpecialty> { EngineerSpecialty.Thrusters }, new HashSet<EngineerSpecialty> { EngineerSpecialty.FrameShiftDrives });
        public static readonly Engineer LoriJameson = new Engineer(300230, "Lori Jameson", "Shinrarta Dezhra", 3932277478106, "Jameson Base", 128679815, new HashSet<EngineerSpecialty> { EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners }, new HashSet<EngineerSpecialty> { EngineerSpecialty.Refineries, EngineerSpecialty.FuelScoops, EngineerSpecialty.AFMUs, EngineerSpecialty.LifeSupportSystems, EngineerSpecialty.Scanners, EngineerSpecialty.ShieldCellBanks } );
        public static readonly Engineer JuriIshmaak = new Engineer(300250, "Juri Ishmaak", "Giryak", 4481899074282, "Pater's Memorial", 128680327, new HashSet<EngineerSpecialty> { EngineerSpecialty.Mines, EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners }, new HashSet<EngineerSpecialty> {EngineerSpecialty.Torpedos, EngineerSpecialty.Missiles, EngineerSpecialty.Scanners});
        public static readonly Engineer TodMcQuinn = new Engineer(300260, "Tod 'The Blaster' McQuinn", "Wolf 397", 3107576681170, "Trophy camp", 128680583, new HashSet<EngineerSpecialty> { EngineerSpecialty.MultiCannons, EngineerSpecialty.RailGuns }, new HashSet<EngineerSpecialty> { EngineerSpecialty.FragCannons, EngineerSpecialty.Cannons });
        public static readonly Engineer TianaFortune = new Engineer(300270, "Tiana Fortune", "Achenar", 164098653, "Fortune's Loss", 128680839, new HashSet<EngineerSpecialty> { EngineerSpecialty.Scanners, EngineerSpecialty.LimpetControllers, EngineerSpecialty.Sensors }, new HashSet<EngineerSpecialty> { EngineerSpecialty.FrameShiftDriveInterdictors, EngineerSpecialty.SurfaceScanners });
        public static readonly Engineer MelBrandon = new Engineer(300280, "Mel Brandon", "Luchtaine", 66038577537618, "The Brig", 128681095, new HashSet<EngineerSpecialty> { EngineerSpecialty.FrameShiftDrives, EngineerSpecialty.Thrusters, EngineerSpecialty.ShieldGenerators, EngineerSpecialty.Lasers, EngineerSpecialty.FrameShiftDriveInterdictors, EngineerSpecialty.ShieldBoosters}, new HashSet<EngineerSpecialty> { EngineerSpecialty.ShieldCellBanks});
        public static readonly Engineer EtienneDorn = new Engineer(300290, "Etienne Dorn", "Los", 11887629902418, "Kraken's Retreat", 128681351, new HashSet<EngineerSpecialty> { EngineerSpecialty.PlasmaAccelerators, EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners, EngineerSpecialty.LifeSupportSystems, EngineerSpecialty.PowerPlants, EngineerSpecialty.PowerDistributors, EngineerSpecialty.RailGuns}, new HashSet<EngineerSpecialty> { EngineerSpecialty.Scanners});
        public static readonly Engineer ChloeSedesi = new Engineer(300300, "Chloe Sedesi", "Shenve", 594676730147, "Cinder Dock", 128954244, new HashSet<EngineerSpecialty> { EngineerSpecialty.Thrusters }, new HashSet<EngineerSpecialty> { EngineerSpecialty.FrameShiftDrives } );

        public static readonly Engineer JudeNavarro = new Engineer(400001, "Jude Navarro", "Aurai", 7268024067513, "Marshall's Drift", 128972903, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty>());
        public static readonly Engineer DominoGreen = new Engineer(400002, "Domino Green", "Orishis", 5068464399785, "The Jackrabbit", 128973159, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty>());
        public static readonly Engineer HeroFerrari = new Engineer(400003, "Hero Ferrari", "Siris", 7269634614689, "Nevermore Terrace", 128973415, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty>());
        public static readonly Engineer KitFowler = new Engineer(400004, "Kit Fowler", "Capoya", 2827975936355, "The Last Call", 128973671, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty>());
        
        public static readonly Engineer TerraVelasquez = new Engineer(400006, "Terra Velasquez", "Shou Xing", 3721329101171, "Rascal's Choice", 128974183, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty>());

        // Needs engineerId
        // public static readonly Engineer UmaLaszlo = new Engineer(null, "Uma Laszlo", "Xuane", 16065190962585, "Laszlo's Resolve", 128974439, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty>());
        // public static readonly Engineer WellingtonBeck = new Engineer(null, "Wellington Beck", "Jolapa", 2832832893634, "Beck Facility", 128973927, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty>());
        // public static readonly Engineer OdenGeiger = new Engineer(null, "Oden Geiger", "Candiaei", 8879744226018, "Ankh's Promise", 128974695, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty>());
        // public static readonly Engineer YardenBond = new Engineer(null, "Yarden Bond", null, null, null, null, new HashSet<EngineerSpecialty>(), new HashSet<EngineerSpecialty>());

        public static Engineer FromName(string from)
        {
            if (string.IsNullOrEmpty(from)) { return null; }
            Engineer result = ENGINEERS.FirstOrDefault(v => v.name.Equals(from, StringComparison.InvariantCultureIgnoreCase));
            if (result == null)
            {
                Logging.Debug("Unknown Engineer name " + from);
            }
            return result;
        }

        public static Engineer FromSystemName(string from)
        {
            if (string.IsNullOrEmpty(from)) { return null; }
            Engineer result = ENGINEERS.FirstOrDefault(v => v.systemname?.Equals(from, StringComparison.InvariantCultureIgnoreCase) ?? false);
            if (result == null)
            {
                Logging.Debug("Unknown Engineer system name " + from);
            }
            return result;
        }

        public static Engineer FromNameOrId(string from, long id)
        {
            Engineer result = ENGINEERS.FirstOrDefault(v => v.id == id);
            if (result == null)
            {
                result = ENGINEERS.FirstOrDefault(v => v.name == from);
                if (result == null)
                {
                    Logging.Error("Unknown Engineer name " + from + " EngineerID: " + id);
                }
            }
            return result;
        }

        public static void AddOrUpdate(Engineer engineer)
        {
            int index = ENGINEERS.FindIndex(candidate => candidate.id == engineer.id);
            if (index != -1)
            {
                ENGINEERS[index].name = engineer.name;
                ENGINEERS[index].id = engineer.id;
                ENGINEERS[index].stage = engineer.stage;
                ENGINEERS[index].rankprogress = engineer.rankprogress;
                ENGINEERS[index].rank = engineer.rank;
            }
            else
            {
                ENGINEERS.Add(engineer);
            }
        }
    }
}
