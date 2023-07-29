using JetBrains.Annotations;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Net.PeerToPeer;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{

    public class GeologySpeciesBase
    {
        public string name;
        public int value;
        public string description;
        public string conditions;

        public GeologySpeciesBase ()
        {
            this.name = "";
            this.value = 0;
            this.description = "";
            this.conditions = "";
        }
    };

    public class GeologySpecies : GeologySpeciesBase
    {
        public GeologySpecies ()
        {
            this.name = "Invalid";
            this.value = 0;
            this.description = "Invalid species";
            this.conditions = "";
        }

        public GeologySpecies ( string species, int value, string desc, string conditions )
        {
            this.name = species;
            this.value = value;
            this.description = desc;
            this.conditions = conditions;
        }
    };

    public class GeologyGenusBase
    {
        public string name;
        public int distance;
        public string description;

        public GeologyGenusBase()
        {
            this.name = "";
            this.distance = 0;
            this.description = "";
        }
    };

    public class GeologyGenus : GeologyGenusBase
    {
        public Dictionary<string, GeologySpecies> species = new Dictionary<string, GeologySpecies>();

        public GeologyGenus ( string genus, int distance, string desc )
        {
            this.name = genus;
            this.distance = distance;
            this.description = desc;
        }

        public void Add ( string species, int distance, string desc, string conditions )
        {
            GeologySpecies myData = new GeologySpecies(species, distance, desc, conditions);
            this.species.Add( species, myData );
        }
    };

    public class GeologyData
    {
        public GeologyGenusBase genus;
        public GeologySpeciesBase species;
    }

    public class GeologyInfo
    {
        private static GeologyGenus Fumarole = new GeologyGenus( "Fumarole", 0, "Fumaroles are gaps in a planet’s crust through which gases and steam are emitted. The ejected material often accumulates around the opening." );
        private static GeologyGenus WaterGeyser = new GeologyGenus( "Water Geyser", 0, "Geysers are eruptions of liquid created by pressure from local geological activity." );
        private static GeologyGenus IceFumarole = new GeologyGenus( "Ice Fumarole", 0, "Ice fumaroles are gaps in an icy planet’s crust that allow liquid or gaseous material to escape under high pressure." );
        private static GeologyGenus IceGeyser = new GeologyGenus( "Ice Geyser", 0, "Ice geysers, also known as cryogeysers, are eruptions of ice, dust and volatiles." );
        private static GeologyGenus LavaSpout = new GeologyGenus( "Lava Spout", 0, "Lava spouts are weakened areas of a planet’s surface where molten material percolates, generating heat and emitting vapour and gases. The nature of the material varies according to the planet’s composition and circumstances." );
        private static GeologyGenus GasVent = new GeologyGenus( "Gas Vent", 0, "Gas vents are the result of internal pressure high enough to burst through a planet’s crust." );
        private static GeologyGenus LagrangeCloud = new GeologyGenus( "Lagrange Cloud", 0, "Lagrange clouds are a dense accumulation of gas fixed in place at a Lagrange point, where the combined gravitational forces of multiple bodies create a stable region." );
        private static GeologyGenus P_TypeAnomoly = new GeologyGenus( "P-Type Anomoly", 0, "P-Type anomolies are a range of phenomena characterised by intensily bright energy patterns." );
        private static GeologyGenus Q_TypeAnomoly = new GeologyGenus( "Q-Type Anomoly", 0, "Q-Type anomolies are a range of phenomena characterised by energetic center orbs." );
        private static GeologyGenus T_TypeAnomoly = new GeologyGenus( "T-Type Anomoly", 0, "T-Type anomolies are a range of phenomena characterised by the presence of a bright pulsing sphere." );
        private static GeologyGenus K_TypeAnomoly = new GeologyGenus( "K-Type Anomoly", 0, "K-Type anomolies are a range of high-energy phenomena characterised by luminous cloud patterns." );
        private static GeologyGenus L_TypeAnomoly = new GeologyGenus( "L-Type Anomoly", 0, "L-Type anomolies are a range of phenomena characterised by luminous clusters of energy." );
        private static GeologyGenus E_TypeAnomoly = new GeologyGenus( "E-Type Anomoly", 0, "E-Type anomolies are a range of phenomena characterised by slow-moving elements." );

        // For easier reverse lookups
        public static Dictionary<string, string> Species = new Dictionary<string, string>();

        static GeologyInfo ()
        {
            Fumarole.Add( "Sulphur Dioxide Fumarole", 50000, "This variation is found on terrestrial planets with sulphur dioxide magma.", "" );
            Fumarole.Add( "Water Fumarole", 50000, "This variation is found on terrestrial planets with water geysers.", "" );
            Fumarole.Add( "Silicate Vapour Fumarole", 50000, "This variation is found on terrestrial planets with silicate vapour geysers.", "" );
            WaterGeyser.Add( "Water Geyser", 50000, "This variation is found on terrestrial planets with water geysers.", "" );
            IceFumarole.Add( "Sulphur Dioxide Ice Fumarole", 50000, "This variation is found on terrestrial planets with sulphur dioxide magma.", "" );
            IceFumarole.Add( "Water Ice Fumarole", 50000, "This variation is found on terrestrial planets with water geysers.", "" );
            IceFumarole.Add( "Carbon Dioxide Ice Fumarole", 50000, "This variation is found on terrestrial planets with carbon dioxide geysers.", "" );
            IceFumarole.Add( "Ammonia Ice Fumarole", 50000, "This variation is found on terrestrial planets with ammonia geysers.", "" );
            IceFumarole.Add( "Methane Ice Fumarole", 50000, "This variation is found on terrestrial planets with methane geysers.", "" );
            IceFumarole.Add( "Nitrogen Ice Fumarole", 50000, "This variation is found on terrestrial planets with nitrogen geysers.", "" );
            IceFumarole.Add( "Silicate Vapour Ice Fumarole", 50000, "This variation is found on terrestrial planets with silicate vapour geysers.", "" );
            IceGeyser.Add( "Water Ice Geyser", 50000, "This variation is found on terrestrial planets with water geysers.", "" );
            IceGeyser.Add( "Carbon Dioxide Ice Geyser", 50000, "This variation is found on terrestrial planets with carbon dioxide geysers.", "" );
            IceGeyser.Add( "Ammonia Ice Geyser", 50000, "This variation is found on terrestrial planets with ammonia geysers.", "" );
            IceGeyser.Add( "Methane Ice Geyser", 50000, "This variation is found on terrestrial planets with methane geysers.", "" );
            IceGeyser.Add( "Nitrogen Ice Geyser", 50000, "This variation is found on terrestrial planets with helium geysers.", "" );
            LavaSpout.Add( "Silicate Magma Lava Spout", 50000, "This variation is found on terrestrial planets with silicate magma.", "" );
            LavaSpout.Add( "Iron Magma Lava Spout", 50000, "This variation is found on terrestrial planets with iron magma.", "" );
            GasVent.Add( "Sulphur Dioxide Gas Vent", 50000, "This variation is found on terrestrial planets with sulphur dioxide magma.", "" );
            GasVent.Add( "Water Gas Vent", 50000, "This variation is found on terrestrial planets with water geysers.", "" );
            GasVent.Add( "Carbon Dioxide Gas Vent", 50000, "This variation is found on terrestrial planets with carbon dioxide geysers.", "" );
            GasVent.Add( "Silicate Vapour Gas Vent", 50000, "This variation is found on terrestrial planets with silicate vapour geysers.", "" );
            LagrangeCloud.Add( "Caeruleum Lagrange Cloud", 50000, "This variation has a blue cloud.", "" );
            LagrangeCloud.Add( "Viride Lagrange Cloud", 50000, "This variation has a green cloud.", "" );
            LagrangeCloud.Add( "Viride Lagrange Storm Cloud", 50000, "This variation has a green cloud that produces irregular electrical discharges.", "" );
            LagrangeCloud.Add( "Luteolum Lagrange Cloud", 50000, "This variation has a orange cloud.", "" );
            LagrangeCloud.Add( "Luteolum Lagrange Storm Cloud", 50000, "This variation has a orange cloud that produces irregular electrical discharges.", "" );
            LagrangeCloud.Add( "Roseum Lagrange Cloud", 50000, "This variation has a pink cloud.", "" );
            LagrangeCloud.Add( "Roseum Lagrange Storm Cloud", 50000, "This variation has a pink cloud that produces irregular electrical discharges.", "" );
            LagrangeCloud.Add( "Rubicundum Lagrange Cloud", 50000, "This variation has a red cloud.", "" );
            LagrangeCloud.Add( "Rubicundum Lagrange Storm Cloud", 50000, "This variation has a red cloud that produces irregular electrical discharges.", "" );
            LagrangeCloud.Add( "Croceum Lagrange Cloud", 50000, "This variation has a yellow cloud.", "" );
            LagrangeCloud.Add( "Proto-Lagrange Cloud", 50000, "This variation is a light accumulation of gas.", "" );
            P_TypeAnomoly.Add( "P01-Type Anomaly", 50000, "This variation is comprised of a dark core with a bright halo and ring of clouds.", "" );
            P_TypeAnomoly.Add( "P02-Type Anomaly", 50000, "This variation is comprised of a rotating bright core emitting spiraling cloud trails.", "" );
            P_TypeAnomoly.Add( "P03-Type Anomaly", 50000, "This variation is comprised of a rotating bright core surrounded by a spiralling ring of clouds.", "" );
            P_TypeAnomoly.Add( "P04-Type Anomaly", 50000, "This variation is comprised of a bright hazy core with a ring of thick glowing bands.", "" );
            P_TypeAnomoly.Add( "P05-Type Anomaly", 50000, "This variation is comprised of a bright core surrounded by slow-moving rotating clouds.", "" );
            P_TypeAnomoly.Add( "P06-Type Anomaly", 50000, "This variation is comprised of a bright core with a cloudy halo emitting fast-moving pulses.", "" );
            P_TypeAnomoly.Add( "P07-Type Anomaly", 50000, "This variation is comprised of a bright cluster emitting green strings of wispy cloud.", "" );
            P_TypeAnomoly.Add( "P08-Type Anomaly", 50000, "This variation is comprised of a bright cluster emitting white strings of wispy cloud.", "" );
            P_TypeAnomoly.Add( "P09-Type Anomaly", 50000, "This variation is comprised of a bright hazy core emitting regular bursts of lightning.", "" );
            P_TypeAnomoly.Add( "P10-Type Anomaly", 50000, "This variation is comprised of bright fragments surrounded by a ring of clouds.", "" );
            P_TypeAnomoly.Add( "P11-Type Anomaly", 50000, "This variation is comprised of an intensely bright core emitting fast-moving clouds and particles.", "" );
            P_TypeAnomoly.Add( "P12-Type Anomaly", 50000, "This variation is comprised of an intensely bright core emitting fast-moving clusters of particles.", "" );
            P_TypeAnomoly.Add( "P13-Type Anomaly", 50000, "This variation is comprised of a bright circular core emitting slow-moving particles.", "" );
            P_TypeAnomoly.Add( "P14-Type Anomaly", 50000, "This variation is comprised of a rotating group of bright clusters emitting fast-moving clouds.", "" );
            P_TypeAnomoly.Add( "P15-Type Anomaly", 50000, "This variation is comprised of a bright core emitting cloud rings and regular bursts of lightning.", "" );
            Q_TypeAnomoly.Add( "Q01-Type Anomaly", 50000, "This variation is comprised of a bright core emitting slow-moving hazy clouds.", "" );
            Q_TypeAnomoly.Add( "Q02-Type Anomaly", 50000, "This variation is comprised of a bright hazy core pulsing fast-moving rings of clouds.", "" );
            Q_TypeAnomoly.Add( "Q03-Type Anomaly", 50000, "This variation is comprised of a bright core emitting slow-moving cloud rings and regular bursts of lightning.", "" );
            Q_TypeAnomoly.Add( "Q04-Type Anomaly", 50000, "This variation is comprised of a pulsing irregular core with a halo of clouds.", "" );
            Q_TypeAnomoly.Add( "Q05-Type Anomaly", 50000, "This variation is comprised of a pulsing irregular core with a ring of clouds.", "" );
            Q_TypeAnomoly.Add( "Q06-Type Anomaly", 50000, "This variation is comprised of a bright core radiating intense light.", "" );
            Q_TypeAnomoly.Add( "Q07-Type Anomaly", 50000, "This variation is comprised of a bright core with a circular halo radiating intense light.", "" );
            Q_TypeAnomoly.Add( "Q08-Type Anomaly", 50000, "This variation is comprised of a bright core radiating intense light with a ring of clouds.", "" );
            Q_TypeAnomoly.Add( "Q09-Type Anomaly", 50000, "This variation is comprised of a bright core radiating hazy light with a ring of clouds.", "" );
            T_TypeAnomoly.Add( "T01-Type Anomaly", 50000, "This variation is comprised of a bright pulsing sphere surrounded by scattered particles.", "" );
            T_TypeAnomoly.Add( "T02-Type Anomaly", 50000, "This variation is comprised of a bright pulsing sphere surrounded by clusters of particles.", "" );
            T_TypeAnomoly.Add( "T03-Type Anomaly", 50000, "This variation is comprised of a bright pulsing sphere surrounded by fast-moving clusters of particles.", "" );
            T_TypeAnomoly.Add( "T04-Type Anomaly", 50000, "This variation is comprised of a bright pulsing sphere surrounded by smaller slow-moving spheres.", "" );
            K_TypeAnomoly.Add( "K01-Type Anomaly", 50000, "This variation is comprised of wispy blue cloud patterns.", "" );
            K_TypeAnomoly.Add( "K02-Type Anomaly", 50000, "This variation is comprised of slow-moving clumps of drifting particles.", "" );
            K_TypeAnomoly.Add( "K03-Type Anomaly", 50000, "This variation is comprised of billowing multicoloured cloud patterns.", "" );
            K_TypeAnomoly.Add( "K04-Type Anomaly", 50000, "This variation is comprised of wispy blue cloud patterns around a denser core.", "" );
            K_TypeAnomoly.Add( "K05-Type Anomaly", 50000, "This variation is comprised of a slow-moving bright core trailing multicoloured rings.", "" );
            K_TypeAnomoly.Add( "K06-Type Anomaly", 50000, "This variation is comprised of a billowing multicoloured cloud surrounded by scattered streaks.", "" );
            K_TypeAnomoly.Add( "K07-Type Anomaly", 50000, "This variation is comprised of a bright red core emitting rapid pulsing clouds.", "" );
            K_TypeAnomoly.Add( "K08-Type Anomaly", 50000, "This variation is comprised of a dull red core emitting slow pulsing clouds.", "" );
            K_TypeAnomoly.Add( "K09-Type Anomaly", 50000, "This variation is comprised of a dull red core emitting rapid pulsing clouds.", "" );
            K_TypeAnomoly.Add( "K10-Type Anomaly", 50000, "This variation is comprised of multiple bright blue-green cores emitting slow-moving clouds. ", "" );
            K_TypeAnomoly.Add( "K11-Type Anomaly", 50000, "This variation is comprised of a hazy red core emitting rapid pulsing clouds.", "" );
            K_TypeAnomoly.Add( "K12-Type Anomaly", 50000, "This variation is comprised of a wispy blue core emitting slow-moving pulsing clouds.", "" );
            K_TypeAnomoly.Add( "K13-Type Anomaly", 50000, "This variation is Unknown to me", "" );
            L_TypeAnomoly.Add( "L01-Type Anomaly", 50000, "This variation is comprised of a bright pulsing sphere surrounded by particles with a billowing cloud trail.", "" );
            L_TypeAnomoly.Add( "L02-Type Anomaly", 50000, "This variation is Unknown to me", "" );
            L_TypeAnomoly.Add( "L03-Type Anomaly", 50000, "This variation is comprised of a bright cloud surrounded by slow-moving clusters of particles.", "" );
            L_TypeAnomoly.Add( "L04-Type Anomaly", 50000, "This variation is comprised of a bright slow-moving core with a cloud trail.", "" );
            L_TypeAnomoly.Add( "L05-Type Anomaly", 50000, "This variation is comprised of a group of bright clusters bursting into smaller particles.", "" );
            L_TypeAnomoly.Add( "L06-Type Anomaly", 50000, "This variation is comprised of a bright hazy core surrounded by swirling cloud rings.", "" );
            L_TypeAnomoly.Add( "L07-Type Anomaly", 50000, "This variation is comprised of a bright hazy core with wispy swirling edges.", "" );
            L_TypeAnomoly.Add( "L08-Type Anomaly", 50000, "This variation is comprised of a bright circular core surrounded by swirling particles.", "" );
            L_TypeAnomoly.Add( "L09-Type Anomaly", 50000, "This variation is comprised of a bright cloudy core surrounded by scattered streaks.", "" );
            E_TypeAnomoly.Add( "E01-Type Anomaly", 50000, "This variation is comprised of a slow-moving ring of red clouds.", "" );
            E_TypeAnomoly.Add( "E02-Type Anomaly", 50000, "This variation is comprised of a slow-moving collection of bright blue clouds.", "" );
            E_TypeAnomoly.Add( "E03-Type Anomaly", 50000, "This variation is comprised of scattered pale blue cloud patterns.", "" );
            E_TypeAnomoly.Add( "E04-Type Anomaly", 50000, "This variation is comprised of a bright blue-white core emitting scattered particles.", "" );


            // Reverse Lookup
            Species.Add( "Sulphur Dioxide Fumarole", "Fumarole" );
            Species.Add( "Water Fumarole", "Fumarole" );
            Species.Add( "Silicate Vapour Fumarole", "Fumarole" );
            Species.Add( "Water Geyser", "Water Geyser" );
            Species.Add( "Sulphur Dioxide Ice Fumarole", "Ice Fumarole" );
            Species.Add( "Water Ice Fumarole", "Ice Fumarole" );
            Species.Add( "Carbon Dioxide Ice Fumarole", "Ice Fumarole" );
            Species.Add( "Ammonia Ice Fumarole", "Ice Fumarole" );
            Species.Add( "Methane Ice Fumarole", "Ice Fumarole" );
            Species.Add( "Nitrogen Ice Fumarole", "Ice Fumarole" );
            Species.Add( "Silicate Vapour Ice Fumarole", "Ice Fumarole" );
            Species.Add( "Water Ice Geyser", "Ice Geyser" );
            Species.Add( "Carbon Dioxide Ice Geyser", "Ice Geyser" );
            Species.Add( "Ammonia Ice Geyser", "Ice Geyser" );
            Species.Add( "Methane Ice Geyser", "Ice Geyser" );
            Species.Add( "Nitrogen Ice Geyser", "Ice Geyser" );
            Species.Add( "Silicate Magma Lava Spout", "Lava Spout" );
            Species.Add( "Iron Magma Lava Spout", "Lava Spout" );
            Species.Add( "Sulphur Dioxide Gas Vent", "Gas Vent" );
            Species.Add( "Water Gas Vent", "Gas Vent" );
            Species.Add( "Carbon Dioxide Gas Vent", "Gas Vent" );
            Species.Add( "Silicate Vapour Gas Vent", "Gas Vent" );
            Species.Add( "Caeruleum Lagrange Cloud", "Lagrange Cloud" );
            Species.Add( "Viride Lagrange Cloud", "Lagrange Cloud" );
            Species.Add( "Viride Lagrange Storm Cloud", "Lagrange Cloud" );
            Species.Add( "Luteolum Lagrange Cloud", "Lagrange Cloud" );
            Species.Add( "Luteolum Lagrange Storm Cloud", "Lagrange Cloud" );
            Species.Add( "Roseum Lagrange Cloud", "Lagrange Cloud" );
            Species.Add( "Roseum Lagrange Storm Cloud", "Lagrange Cloud" );
            Species.Add( "Rubicundum Lagrange Cloud", "Lagrange Cloud" );
            Species.Add( "Rubicundum Lagrange Storm Cloud", "Lagrange Cloud" );
            Species.Add( "Croceum Lagrange Cloud", "Lagrange Cloud" );
            Species.Add( "Proto-Lagrange Cloud", "Lagrange Cloud" );
            Species.Add( "P01-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P02-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P03-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P04-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P05-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P06-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P07-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P08-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P09-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P10-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P11-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P12-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P13-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P14-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "P15-Type Anomaly", "P-Type Anomoly" );
            Species.Add( "Q01-Type Anomaly", "Q-Type Anomoly" );
            Species.Add( "Q02-Type Anomaly", "Q-Type Anomoly" );
            Species.Add( "Q03-Type Anomaly", "Q-Type Anomoly" );
            Species.Add( "Q04-Type Anomaly", "Q-Type Anomoly" );
            Species.Add( "Q05-Type Anomaly", "Q-Type Anomoly" );
            Species.Add( "Q06-Type Anomaly", "Q-Type Anomoly" );
            Species.Add( "Q07-Type Anomaly", "Q-Type Anomoly" );
            Species.Add( "Q08-Type Anomaly", "Q-Type Anomoly" );
            Species.Add( "Q09-Type Anomaly", "Q-Type Anomoly" );
            Species.Add( "T01-Type Anomaly", "T-Type Anomoly" );
            Species.Add( "T02-Type Anomaly", "T-Type Anomoly" );
            Species.Add( "T03-Type Anomaly", "T-Type Anomoly" );
            Species.Add( "T04-Type Anomaly", "T-Type Anomoly" );
            Species.Add( "K01-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K02-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K03-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K04-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K05-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K06-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K07-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K08-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K09-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K10-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K11-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K12-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "K13-Type Anomaly", "K-Type Anomoly" );
            Species.Add( "L01-Type Anomaly", "L-Type Anomoly" );
            Species.Add( "L02-Type Anomaly", "L-Type Anomoly" );
            Species.Add( "L03-Type Anomaly", "L-Type Anomoly" );
            Species.Add( "L04-Type Anomaly", "L-Type Anomoly" );
            Species.Add( "L05-Type Anomaly", "L-Type Anomoly" );
            Species.Add( "L06-Type Anomaly", "L-Type Anomoly" );
            Species.Add( "L07-Type Anomaly", "L-Type Anomoly" );
            Species.Add( "L08-Type Anomaly", "L-Type Anomoly" );
            Species.Add( "L09-Type Anomaly", "L-Type Anomoly" );
            Species.Add( "E01-Type Anomaly", "E-Type Anomoly" );
            Species.Add( "E02-Type Anomaly", "E-Type Anomoly" );
            Species.Add( "E03-Type Anomaly", "E-Type Anomoly" );
            Species.Add( "E04-Type Anomaly", "E-Type Anomoly" );
        }

        public static GeologyData LookupByVariant ( string localisedVariant )
        {
            GeologyData myData = new GeologyData();

            bool found = Species.TryGetValue( localisedVariant, out string genus );

            if (found)
            {
                myData = GetData( genus, localisedVariant );
            }
            else
            {
                myData.genus.name = "could not find genus.";
            }

            return myData;
        }

        public static GeologyData GetData ( string localisedGenus, string localisedSpecies )
        {
            GeologyData myData = new GeologyData();
            GeologySpecies val = new GeologySpecies();

            if ( localisedGenus == "Fumarole" )
            {
                myData.genus = Fumarole;
                Fumarole.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Water Geyser" )
            {
                myData.genus = WaterGeyser;
                WaterGeyser.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Ice Fumarole" )
            {
                myData.genus = IceFumarole;
                IceFumarole.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Ice Geyser" )
            {
                myData.genus = IceGeyser;
                IceGeyser.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Lava Spout" )
            {
                myData.genus = LavaSpout;
                LavaSpout.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Gas Vent" )
            {
                myData.genus = GasVent;
                GasVent.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Lagrange Cloud" )
            {
                myData.genus = LagrangeCloud;
                LagrangeCloud.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "P-Type Anomoly" )
            {
                myData.genus = P_TypeAnomoly;
                P_TypeAnomoly.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Q-Type Anomoly" )
            {
                myData.genus = Q_TypeAnomoly;
                Q_TypeAnomoly.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "T-Type Anomoly" )
            {
                myData.genus = T_TypeAnomoly;
                T_TypeAnomoly.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "K-Type Anomoly" )
            {
                myData.genus = K_TypeAnomoly;
                K_TypeAnomoly.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "L-Type Anomoly" )
            {
                myData.genus = L_TypeAnomoly;
                L_TypeAnomoly.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "E-Type Anomoly" )
            {
                myData.genus = E_TypeAnomoly;
                E_TypeAnomoly.species.TryGetValue( localisedSpecies, out val );
            }

            if ( val != null ) { myData.species = val; }
            else
            {
                myData.species.name = "Not found.";
                myData.species.value = 0;
                myData.species.description = "";
            }

            return myData;
        }
    }
}
