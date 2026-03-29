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
        internal static List<Engineer> ENGINEERS = [ ];

        public long id { get; private set; }

        [PublicAPI]
        public string name { get; private set; }

        [PublicAPI]
        public string systemname { get; private set; }

        [PublicAPI]
        public ulong systemAddress { get; private set; }

        [PublicAPI]
        public string stationname { get; private set; }

        [PublicAPI]
        public long marketId { get; private set; }

        [PublicAPI]
        public string bodyname { get; private set; }

        [PublicAPI]
        public int bodyId { get; private set; }

        // Top tier specialties
        public HashSet<EngineerSpecialty> majorSpecialties { get; private set; }

        [PublicAPI]
        public List<string> majorspecialties => majorSpecialties?.Select(s => s.localizedName).ToList();

        // Other specialties
        public HashSet<EngineerSpecialty> minorSpecialties { get; private set; }

        [PublicAPI]
        public List<string> minorspecialties => minorSpecialties?.Select(s => s.localizedName).ToList();

        // Progress
        [PublicAPI]
        public string stage { get; set; }

        [PublicAPI]
        public int? rankprogress { get; set; }

        [PublicAPI]
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

        private Engineer ( long engineerId, string engineerName, string systemName, ulong systemAddress,
            string stationName, long marketId, string bodyName, int bodyId, HashSet<EngineerSpecialty> majorSpecialties,
            HashSet<EngineerSpecialty> minorSpecialties )
        {
            this.id = engineerId;
            this.name = engineerName;
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.stationname = stationName;
            this.marketId = marketId;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.majorSpecialties = majorSpecialties;
            this.minorSpecialties = minorSpecialties;

            ENGINEERS.Add(this);
        }

        public static readonly Engineer DidiVatermann = new(300000, "Didi Vatermann", "Leesti", 3932277478114, "Vatermann LLC", 128673927, "Leesti 1 a", 9,
            [ EngineerSpecialty.ShieldBoosters ], [ EngineerSpecialty.ShieldGenerators ] );
        public static readonly Engineer BillTurner = new(300010, "Bill Turner", "Alioth", 1109989017963, "Turner Metallics Inc", 128674183, "Alioth 4 a", 57,
            [ EngineerSpecialty.PlasmaAccelerators, EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners ],
            [
                EngineerSpecialty.LifeSupportSystems, EngineerSpecialty.Refineries, EngineerSpecialty.AFMUs,
                EngineerSpecialty.FuelScoops, EngineerSpecialty.Scanners
            ] );
        public static readonly Engineer BrooTarquin = new(300030, "Broo Tarquin", "Muang", 4481966019282, "Broo's Legacy", 128674695, "Muang 5 a", 13,
            [ EngineerSpecialty.Lasers ], [ ] );
        public static readonly Engineer TheSarge = new(300040, "The Sarge", "Beta-3 Tucani", 2827992680811, "The Beach", 128674951, "Beta-3 Tucani 2 b a", 16,
            [ EngineerSpecialty.LimpetControllers, EngineerSpecialty.Cannons ], [ EngineerSpecialty.RailGuns ] );
        public static readonly Engineer ZacariahNemo = new(300050, "Zacariah Nemo", "Yoru", 6131367744226, "Nemo Cyber Party Base", 128675207, "Yoru 4", 9,
            [ EngineerSpecialty.FragCannons ],
            [ EngineerSpecialty.MultiCannons, EngineerSpecialty.PlasmaAccelerators ] );
        public static readonly Engineer LizRyder = new(300080, "Liz Ryder", "Eurybia", 1458309141194, "Demolition Unlimited", 128675975, "Makalu", 6,
            [ EngineerSpecialty.Missiles, EngineerSpecialty.Torpedos ],
            [ EngineerSpecialty.Mines, EngineerSpecialty.HullReinforcement ] );
        public static readonly Engineer HeraTani = new(300090, "Hera Tani", "Kuwemaki", 1733321102034, "The Jet's Hole", 128676231, "Kuwemaki A 3 a", 15,
            [ EngineerSpecialty.PowerPlants, EngineerSpecialty.SurfaceScanners ],
            [ EngineerSpecialty.Sensors, EngineerSpecialty.PowerDistributors ] );
        public static readonly Engineer FelicityFarseer = new(300100, "Felicity Farseer", "Deciat", 6681123623626, "Farseer Inc", 128676487, "Deciat 6 a", 25,
            [ EngineerSpecialty.FrameShiftDrives ],
            [
                EngineerSpecialty.Thrusters, EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners,
                EngineerSpecialty.ShieldBoosters, EngineerSpecialty.FrameShiftDriveInterdictors,
                EngineerSpecialty.PowerPlants
            ] );
        public static readonly Engineer RamTah = new(300110, "Ram Tah", "Meene", 3790082132323, "Phoenix Base", 128676743, "Meene AB 5 d", 16,
            [ EngineerSpecialty.ECMs, EngineerSpecialty.PointDefence, EngineerSpecialty.ChaffAndHeatSinkLaunchers ],
            [ EngineerSpecialty.LimpetControllers ] );
        public static readonly Engineer LeiCheung = new(300120, "Lei Cheung", "Laksak", 4305444669811, "Trader's Rest", 128676999, "Laksak A 1", 5,
            [ EngineerSpecialty.ShieldGenerators, EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners ],
            [ EngineerSpecialty.ShieldBoosters ] );
        public static readonly Engineer PetraOlmanova = new(300130, "Petra Olmanova", "Asura", 12274907287851, "Sanctuary", 128677255, "Asura 1 d", 24,
        [
            EngineerSpecialty.HullReinforcement, EngineerSpecialty.Missiles,
            EngineerSpecialty.ChaffAndHeatSinkLaunchers, EngineerSpecialty.PointDefence
        ], [ EngineerSpecialty.Mines, EngineerSpecialty.Torpedos, EngineerSpecialty.ECMs, EngineerSpecialty.AFMUs ] );
        public static readonly Engineer ColBrisDekker = new(300140, "Colonel Bris Dekker", "Sol", 10477373803, "Dekker's Yard", 128677511, "Iapetus", 20,
            [ ], [ EngineerSpecialty.FrameShiftDriveInterdictors, EngineerSpecialty.FrameShiftDrives ] );
        public static readonly Engineer MarshaHicks = new(300150, "Marsha Hicks", "Tir", 48996147307082, "The Watchtower", 128677767, "Tir A 2", 4,
        [
            EngineerSpecialty.LimpetControllers, EngineerSpecialty.Refineries, EngineerSpecialty.FuelScoops,
            EngineerSpecialty.MultiCannons, EngineerSpecialty.FragCannons
        ], [ EngineerSpecialty.Cannons ] );
        public static readonly Engineer ElviraMartuuk = new(300160, "Elvira Martuuk", "Khun", 3107241104074, "Long Sight Base", 128678023, "Khun 5", 13,
            [ EngineerSpecialty.FrameShiftDrives ],
            [ EngineerSpecialty.ShieldGenerators, EngineerSpecialty.Thrusters, EngineerSpecialty.ShieldCellBanks ] );
        public static readonly Engineer TheDweller = new(300180, "The Dweller", "Wyrd", 5031654888146, "Black Hide", 128678535, "Wyrd A 2", 10,
            [ EngineerSpecialty.PowerDistributors ], [ EngineerSpecialty.Lasers ] );
        public static readonly Engineer MarcoQwent = new(300200, "Marco Qwent", "Sirius", 121569805492, "Qwent Research Base", 128679047, "Lucifer", 4,
            [ ], [ EngineerSpecialty.PowerPlants, EngineerSpecialty.PowerDistributors ] );
        public static readonly Engineer SeleneJean = new(300210, "Selene Jean", "Kuk", 24859942069665, "Prospector's Rest", 128679303, "Kuk B 3", 12,
            [ EngineerSpecialty.Armour, EngineerSpecialty.HullReinforcement ], [ ] );
        public static readonly Engineer ProfessorPalin = new(300220, "Professor Palin", "Arque", 113573366131, "Abel Laboratory", 128679559, "Arque 4 e", 28,
            [ EngineerSpecialty.Thrusters ], [ EngineerSpecialty.FrameShiftDrives ] );
        public static readonly Engineer LoriJameson = new(300230, "Lori Jameson", "Shinrarta Dezhra", 3932277478106, "Jameson Base", 128679815, "Shinrarta Dezhra A 1", 11,
            [ EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners ],
            [
                EngineerSpecialty.Refineries, EngineerSpecialty.FuelScoops, EngineerSpecialty.AFMUs,
                EngineerSpecialty.LifeSupportSystems, EngineerSpecialty.Scanners, EngineerSpecialty.ShieldCellBanks
            ] );
        public static readonly Engineer JuriIshmaak = new(300250, "Juri Ishmaak", "Giryak", 4481899074282, "Pater's Memorial", 128680327, "Giryak 2 a", 3,
            [ EngineerSpecialty.Mines, EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners ],
            [ EngineerSpecialty.Torpedos, EngineerSpecialty.Missiles, EngineerSpecialty.Scanners ] );
        public static readonly Engineer TodMcQuinn = new(300260, "Tod 'The Blaster' McQuinn", "Wolf 397", 3107576681170, "Trophy camp", 128680583, "Trus Madi", 7,
            [ EngineerSpecialty.MultiCannons, EngineerSpecialty.RailGuns ],
            [ EngineerSpecialty.FragCannons, EngineerSpecialty.Cannons ] );
        public static readonly Engineer TianaFortune = new(300270, "Tiana Fortune", "Achenar", 164098653, "Fortune's Loss", 128680839, "Achenar 4a", 5,
            [ EngineerSpecialty.Scanners, EngineerSpecialty.LimpetControllers, EngineerSpecialty.Sensors ],
            [ EngineerSpecialty.FrameShiftDriveInterdictors, EngineerSpecialty.SurfaceScanners ] );
        public static readonly Engineer MelBrandon = new(300280, "Mel Brandon", "Luchtaine", 66038577537618, "The Brig", 128681095, "Luchtaine A 1 c", 14,
        [
            EngineerSpecialty.FrameShiftDrives, EngineerSpecialty.Thrusters, EngineerSpecialty.ShieldGenerators,
            EngineerSpecialty.Lasers, EngineerSpecialty.FrameShiftDriveInterdictors, EngineerSpecialty.ShieldBoosters
        ], [ EngineerSpecialty.ShieldCellBanks ] );
        public static readonly Engineer EtienneDorn = new(300290, "Etienne Dorn", "Los", 11887629902418, "Kraken's Retreat", 128681351, "Los A 2 b", 19,
        [
            EngineerSpecialty.PlasmaAccelerators, EngineerSpecialty.Sensors, EngineerSpecialty.SurfaceScanners,
            EngineerSpecialty.LifeSupportSystems, EngineerSpecialty.PowerPlants, EngineerSpecialty.PowerDistributors,
            EngineerSpecialty.RailGuns
        ], [ EngineerSpecialty.Scanners ] );
        public static readonly Engineer ChloeSedesi = new(300300, "Chloe Sedesi", "Shenve", 594676730147, "Cinder Dock", 128954244, "Shenve A 6", 15,
            [ EngineerSpecialty.Thrusters ], [ EngineerSpecialty.FrameShiftDrives ] );

        public static readonly Engineer JudeNavarro = new(400001, "Jude Navarro", "Aurai", 7268024067513, "Marshall's Drift", 128972903, "Aurai 1 a", 12,
            [ ], [ ] );
        public static readonly Engineer DominoGreen = new(400002, "Domino Green", "Orishis", 5068464399785, "The Jackrabbit", 128973159, "Orishis 4", 8,
            [ ], [ ] );
        public static readonly Engineer HeroFerrari = new(400003, "Hero Ferrari", "Siris", 7269634614689, "Nevermore Terrace", 128973415, "Siris 5 c", 40,
            [ ], [ ] );
        public static readonly Engineer KitFowler = new(400004, "Kit Fowler", "Capoya", 2827975936355, "The Last Call", 128973671, "Capoya 2", 6,
            [ ], [ ] );
        public static readonly Engineer WellingtonBeck = new(400005, "Wellington Beck", "Jolapa", 2832832893634, "Beck Facility", 128973927, "Jolapa 6 a", 20,
            [ ], [ ] );
        public static readonly Engineer TerraVelasquez = new(400006, "Terra Velasquez", "Shou Xing", 3721329101171, "Rascal's Choice", 128974183, "Shou Xing 1", 2,
            [ ], [ ] );
        public static readonly Engineer UmaLaszlo = new(400007, "Uma Laszlo", "Xuane", 16065190962585, "Laszlo's Resolve", 128974439, "Xuane A 3", 5,
            [ ], [ ] );
        public static readonly Engineer OdenGeiger = new(400008, "Oden Geiger", "Candiaei", 8879744226018, "Ankh's Promise", 128974695, "Candiaei 9 c", 23,
            [ ], [ ] );
        public static readonly Engineer YardenBond = new(400009, "Yarden Bond", "Bayan", 670686455169, "Salamander Bank", 128974951, "Bayan 7 b", 23,
            [ ], [ ] );
        public static readonly Engineer Baltanos = new(400010, "Baltanos", "Deriso", 71536135676490, "The Divine Apparatus", 128986843, "Deriso 3 a", 36,
            [ ], [ ] );
        public static readonly Engineer EleanorBresa = new(400011, "Eleanor Bresa", "Desy", 38001031029322, "Bresa Modifications", 128987099, "Desy 7 a", 9,
            [ ], [ ] );
        public static readonly Engineer RosaDayette = new(400012, "Rosa Dayette", "Kojeara", 59166629864010, "Rosa's Shop", 128986587, "Kojeara 4 b", 31,
            [ ], [ ] );
        public static readonly Engineer YiShen = new(400013, "Yi Shen", "Einheriar", 13736779007129, "Eidolon Hold", 128987355, "Einheriar 1", 12,
            [ ], [ ] );

        public static Engineer FromName(string from)
        {
            if (string.IsNullOrEmpty(from)) { return null; }

            var result =
                ENGINEERS.FirstOrDefault( v => v.name.Equals( from.Trim(), StringComparison.OrdinalIgnoreCase ) );
            if (result == null)
            {
                Logging.Debug("Unknown Engineer name " + from);
            }
            
            return result;
        }

        public static Engineer FromSystemAddress ( ulong from )
        {
            if ( from <= 0 ) { return null; }

            var result = ENGINEERS.FirstOrDefault( v => v.systemAddress == from );
            if ( result == null )
            {
                Logging.Debug( "Unknown Engineer system address " + from );
            }

            return result;
        }

        public static Engineer FromSystemName ( string from )
        {
            if ( string.IsNullOrEmpty( from ) ) { return null; }

            var result =
                ENGINEERS.FirstOrDefault( v => v.systemname.Equals( from.Trim(), StringComparison.OrdinalIgnoreCase ) );
            if ( result == null )
            {
                Logging.Debug( "Unknown Engineer system address " + from );
            }
            
            return result;
        }

        public static Engineer FromNameOrId ( string from, long id )
        {
            var result = ENGINEERS.FirstOrDefault( v => v.id == id );
            if ( result == null )
            {
                result = ENGINEERS.FirstOrDefault( v => v.name == from );
                if ( result == null )
                {
                    Logging.Error( "Unknown Engineer name " + from + " or EngineerID: " + id );
                }
            }

            return result;
        }

        public static void AddOrUpdate(Engineer engineer)
        {
            var index = ENGINEERS.FindIndex(candidate => candidate.id == engineer.id);
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

        #region Overrides of Object

        public override string ToString () => name;

        #endregion
    }
}
