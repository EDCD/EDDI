using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class OrganicSpecies : ResourceBasedLocalizedEDName<OrganicSpecies>
    {
        static OrganicSpecies ()
        {
            resourceManager = Properties.OrganicSpecies.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new OrganicSpecies( NormalizeSpecies( edname ) );
        }

        // Terrestrial Species
        public static readonly OrganicSpecies AleoidaArcus = new OrganicSpecies( "AleoidaArcus", OrganicGenus.Aleoids, 7252500, (decimal?)0.28, 175, 180, "", "CarbonDioxide", "None", "B;A;F;K;M;L:T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaCoronamus = new OrganicSpecies( "AleoidaCoronamus", OrganicGenus.Aleoids, 6284600, (decimal?)0.28, 180, 190, "", "CarbonDioxide", "None", "B;A;F;K;M;L:T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaGravis = new OrganicSpecies( "AleoidaGravis", OrganicGenus.Aleoids, 12934900, (decimal?)0.28, 190, 195, "", "CarbonDioxide", "None", "B;A;F;K;M;L:T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaLaminiae = new OrganicSpecies( "AleoidaLaminiae", OrganicGenus.Aleoids, 3385200, (decimal?)0.28, null, null, "", "Ammonia", "", "B;A;F;K;M;L:T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaSpica = new OrganicSpecies( "AleoidaSpica", OrganicGenus.Aleoids, 3385200, (decimal?)0.28, null, null, "", "Ammonia", "", "B;A;F;K;M;L:T;TTS;Y;N" );
        public static readonly OrganicSpecies AmphoraPlant = new OrganicSpecies( "AmphoraPlant", OrganicGenus.Vents, 1628800, null, 1000, null, "MetalRichBody", "None", "", "A" );
        public static readonly OrganicSpecies BlatteumBioluminescentAnemone = new OrganicSpecies( "BlatteumBioluminescentAnemone", OrganicGenus.Sphere, 1499900, null, null, null, "MetalRichBody;HighMetalContentBody", "SulphurDioxide;None", "", "B" );
        public static readonly OrganicSpecies CroceumAnemone = new OrganicSpecies( "CroceumAnemone", OrganicGenus.Sphere, 1499900, null, null, null, "RockyBody", "SulphurDioxide;None", "", "B;A" );
        public static readonly OrganicSpecies LuteolumAnemone = new OrganicSpecies( "LuteolumAnemone", OrganicGenus.Sphere, 1499900, null, null, null, "RockyBody", "SulphurDioxide;None", "", "B" );
        public static readonly OrganicSpecies PrasinumBioluminescentAnemone = new OrganicSpecies( "PrasinumBioluminescentAnemone", OrganicGenus.Sphere, 1499900, null, null, null, "RockyBody;MetalRichBody;HighMetalContentBody", "SulphurDioxide;None", "", "O" );
        public static readonly OrganicSpecies PuniceumAnemone = new OrganicSpecies( "PuniceumAnemone", OrganicGenus.Sphere, 1499900, null, null, null, "RockyIceBody;IceBody", "SulphurDioxide;None", "", "O" );
        public static readonly OrganicSpecies RoseumAnemone = new OrganicSpecies( "RoseumAnemone", OrganicGenus.Sphere, 1499900, null, null, null, "RockyBody", "SulphurDioxide;None", "", "B" );
        public static readonly OrganicSpecies RoseumBioluminescentAnemone = new OrganicSpecies( "RoseumBioluminescentAnemone", OrganicGenus.Sphere, 1499900, null, null, null, "MetalRichBody;HighMetalContentBody", "SulphurDioxide;None", "", "B" );
        public static readonly OrganicSpecies RubeumBioluminescentAnemone = new OrganicSpecies( "RubeumBioluminescentAnemone", OrganicGenus.Sphere, 1499900, null, null, null, "MetalRichBody;HighMetalContentBody", "SulphurDioxide;None", "", "B" );
        public static readonly OrganicSpecies BacteriumAcies = new OrganicSpecies( "BacteriumAcies", OrganicGenus.Bacterial, 1000000, (decimal?)0.75, null, null, "IcyBody;RockyIceBody", "Neon;NeonRich", "", "" );
        public static readonly OrganicSpecies BacteriumAlcyoneum = new OrganicSpecies( "BacteriumAlcyoneum", OrganicGenus.Bacterial, 1658500, (decimal?)0.38, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody", "Ammonia", "", "" );
        public static readonly OrganicSpecies BacteriumAurasus = new OrganicSpecies( "BacteriumAurasus", OrganicGenus.Bacterial, 1000000, 1, null, null, "", "CarbonDioxide;CarbonDioxideRich", "", "" );
        public static readonly OrganicSpecies BacteriumBullaris = new OrganicSpecies( "BacteriumBullaris", OrganicGenus.Bacterial, 1152500, (decimal?)0.61, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody", "Methane;MethaneRich", "", "" );
        public static readonly OrganicSpecies BacteriumCerbrus = new OrganicSpecies( "BacteriumCerbrus", OrganicGenus.Bacterial, 1689800, 1, null, null, "", "Water;WaterRich;SulphurDioxide", "", "" );
        public static readonly OrganicSpecies BacteriumInformem = new OrganicSpecies( "BacteriumInformem", OrganicGenus.Bacterial, 8418000, (decimal?)0.6, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody", "Nitrogen", "", "" );
        public static readonly OrganicSpecies BacteriumNebulus = new OrganicSpecies( "BacteriumNebulus", OrganicGenus.Bacterial, 5289900, (decimal?)0.55, null, null, "IcyBody", "Helium", "", "" );
        public static readonly OrganicSpecies BacteriumOmentum = new OrganicSpecies( "BacteriumOmentum", OrganicGenus.Bacterial, 4638900, (decimal?)0.61, null, null, "IcyBody", "Neon;NeonRich", "Nitrogen;Ammonia", "" );
        public static readonly OrganicSpecies BacteriumScopulum = new OrganicSpecies( "BacteriumScopulum", OrganicGenus.Bacterial, 4934500, (decimal?)0.62, null, null, "IcyBody;RockyIceBody", "Neon;NeonRich", "Carbon;Methane", "" );
        public static readonly OrganicSpecies BacteriumTela = new OrganicSpecies( "BacteriumTela", OrganicGenus.Bacterial, 1949000, (decimal?)0.62, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody", "Any", "Helium;Iron;Silicate", "" );
        public static readonly OrganicSpecies BacteriumVerrata = new OrganicSpecies( "BacteriumVerrata", OrganicGenus.Bacterial, 3897000, (decimal?)0.61, null, null, "IcyBody;RockyBody;RockyIceBody", "Neon;NeonRich", "Water", "" );
        public static readonly OrganicSpecies BacteriumVesicula = new OrganicSpecies( "BacteriumVesicula", OrganicGenus.Bacterial, 1000000, 1, null, null, "IcyBody;RockyBody;HighMetalContentBody;RockyIceBody", "Argon;ArgonRich", "", "" );
        public static readonly OrganicSpecies BacteriumVolu = new OrganicSpecies( "BacteriumVolu", OrganicGenus.Bacterial, 7774700, (decimal?)0.61, null, null, "IcyBody;RockyBody;HighMetalContentBody;RockyIceBody", "Oxygen", "", "" );
        public static readonly OrganicSpecies BarkMounds = new OrganicSpecies( "BarkMounds", OrganicGenus.Cone, 1471900, null, 80, 450, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody", "None;CarbonDioxide;CarbonDioxideRich;ArgonRich;SulphurDioxide;ThickArgonRich", "", "" );
        public static readonly OrganicSpecies AureumBrainTree = new OrganicSpecies( "AureumBrainTree", OrganicGenus.Brancae, 1593700, null, 300, 500, "MetalRichBody;HighMetalContentBody", "None;SulphurDioxide", "Any", "" );
        public static readonly OrganicSpecies GypseeumBrainTree = new OrganicSpecies( "GypseeumBrainTree", OrganicGenus.Brancae, 1593700, null, 200, 300, "RockyBody", "Ammonia;None;Oxygen;SulphurDioxide", "Any", "" );
        public static readonly OrganicSpecies LindigoticumBrainTree = new OrganicSpecies( "LindigoticumBrainTree", OrganicGenus.Brancae, 1593700, null, 300, 500, "RockyBody;HighMetalContentBody", "None", "Any", "" );
        public static readonly OrganicSpecies LividumBrainTree = new OrganicSpecies( "LividumBrainTree", OrganicGenus.Brancae, 1593700, null, 300, 500, "RockyBody", "None;Water;SulphurDioxide", "Any", "" );
        public static readonly OrganicSpecies OstrinumBrainTree = new OrganicSpecies( "OstrinumBrainTree", OrganicGenus.Brancae, 1593700, null, null, null, "MetalRichBody;HighMetalContentBody", "None;CarbonDioxide;Ammonia;CarbonDioxideRich;ArgonRich;SulphurDioxide;Helium;NeonRich", "Any", "" );
        public static readonly OrganicSpecies PuniceumBrainTree = new OrganicSpecies( "PuniceumBrainTree", OrganicGenus.Brancae, 1593700, null, null, null, "MetalRichBody;HighMetalContentBody", "None;CarbonDioxide;Oxygen;SulphurDioxide;Helium;NeonRich", "Any", "" );
        public static readonly OrganicSpecies RoseumBrainTree = new OrganicSpecies( "RoseumBrainTree", OrganicGenus.Brancae, 1593700, null, 200, 500, "RockyBody;MetalRichBody;HighMetalContentBody;RockyIceBody", "None;CarbonDioxide;Argon;Ammonia;CarbonDioxideRich;Oxygen;Water;SulphurDioxide;ArgonRich;WaterRich", "Any", "" );
        public static readonly OrganicSpecies VirideBrainTree = new OrganicSpecies( "VirideBrainTree", OrganicGenus.Brancae, 1593700, null, 100, 270, "RockyIceBody", "Ammonia;None;SulphurDioxide", "Any", "" );
        public static readonly OrganicSpecies CactoidaCortexum = new OrganicSpecies( "CactoidaCortexum", OrganicGenus.Cactoid, 3667600, (decimal?)0.28, 180, 195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "A;F;G;M;L;T;TTS;N" );
        public static readonly OrganicSpecies CactoidaLapis = new OrganicSpecies( "CactoidaLapis", OrganicGenus.Cactoid, 2483600, (decimal?)0.28, 160, 225, "RockyBody;HighMetalContentBody", "Ammonia", "", "A;F;G;M;L;T;TTS;N" );
        public static readonly OrganicSpecies CactoidaPeperatis = new OrganicSpecies( "CactoidaPeperatis", OrganicGenus.Cactoid, 2483600, (decimal?)0.28, 160, 190, "RockyBody;HighMetalContentBody", "Ammonia", "", "A;F;G;M;L;T;TTS;N" );
        public static readonly OrganicSpecies CactoidaPullulanta = new OrganicSpecies( "CactoidaPullulanta", OrganicGenus.Cactoid, 3667600, (decimal?)0.28, 180, 195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "A;F;G;M;L;T;TTS;N" );
        public static readonly OrganicSpecies CactoidaVermis = new OrganicSpecies( "CactoidaVermis", OrganicGenus.Cactoid, 16202800, (decimal?)0.28, 160, 450, "RockyBody;HighMetalContentBody", "Water", "", "A;F;G;M;L;T;TTS;N" );
        public static readonly OrganicSpecies ClypeusLacrimam = new OrganicSpecies( "ClypeusLacrimam", OrganicGenus.Clypeus, 8418000, (decimal?)0.28, 190, null, "RockyBody;HighMetalContentBody", "Water;CarbonDioxide", "", "A;F;G;K;M;L;N" );
        public static readonly OrganicSpecies ClypeusMargaritus = new OrganicSpecies( "ClypeusMargaritus", OrganicGenus.Clypeus, 11873200, (decimal?)0.28, 190, null, "RockyBody;HighMetalContentBody", "Water;CarbonDioxide", "None", "A;F;G;K;M;L;N" );
        public static readonly OrganicSpecies ClypeusSpeculumi = new OrganicSpecies( "ClypeusSpeculumi", OrganicGenus.Clypeus, 16202800, (decimal?)0.28, 190, null, "RockyBody;HighMetalContentBody", "Water;CarbonDioxide", "", "A;F;G;K;M;L;N" );
        public static readonly OrganicSpecies ConchaAureolas = new OrganicSpecies( "ConchaAureolas", OrganicGenus.Conchas, 7774700, (decimal?)0.28, null, null, "", "Ammonia", "", "" );
        public static readonly OrganicSpecies ConchaBiconcavis = new OrganicSpecies( "ConchaBiconcavis", OrganicGenus.Conchas, 19010800, (decimal?)0.28, null, null, "", "Nitrogen", "None", "" );
        public static readonly OrganicSpecies ConchaLabiata = new OrganicSpecies( "ConchaLabiata", OrganicGenus.Conchas, 2352400, (decimal?)0.28, null, 190, "", "CarbonDioxide;CarbonDioxideRich", "", "" );
        public static readonly OrganicSpecies ConchaRenibus = new OrganicSpecies( "ConchaRenibus", OrganicGenus.Conchas, 4572400, (decimal?)0.28, 180, 195, "", "Water;WaterRich", "", "" );
        public static readonly OrganicSpecies CrystallineShards = new OrganicSpecies( "CrystallineShards", OrganicGenus.GroundStructIce, 1628800, null, null, 273, "", "None;CarbonDioxide;Argon;CarbonDioxideRich;Methane;ArgonRich;Neon;Helium;NeonRich", "", "A;F;G;K;M;S" );
        public static readonly OrganicSpecies ElectricaePluma = new OrganicSpecies( "ElectricaePluma", OrganicGenus.Electricae, 6284600, (decimal?)0.28, null, null, "IcyBody", "Helium;Neon;Argon", "", "A;N" );
        public static readonly OrganicSpecies ElectricaeRadialem = new OrganicSpecies( "ElectricaeRadialem", OrganicGenus.Electricae, 6284600, (decimal?)0.28, null, 150, "IcyBody", "Helium;Neon;Argon", "", "" );
        public static readonly OrganicSpecies FonticuluaCampestris = new OrganicSpecies( "FonticuluaCampestris", OrganicGenus.Fonticulus, 1000000, (decimal?)0.28, null, 150, "IcyBody;RockyBody", "Argon", "", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaDigitos = new OrganicSpecies( "FonticuluaDigitos", OrganicGenus.Fonticulus, 1804100, (decimal?)0.28, null, null, "IcyBody;RockyBody", "Methane;MethaneRich", "", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaFluctus = new OrganicSpecies( "FonticuluaFluctus", OrganicGenus.Fonticulus, 20000000, (decimal?)0.28, null, null, "IcyBody;RockyBody", "Oxygen", "", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaLapida = new OrganicSpecies( "FonticuluaLapida", OrganicGenus.Fonticulus, 3111000, (decimal?)0.28, null, null, "IcyBody;RockyBody", "Nitrogen", "", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaSegmentatus = new OrganicSpecies( "FonticuluaSegmentatus", OrganicGenus.Fonticulus, 19010800, (decimal?)0.28, null, null, "IcyBody;RockyBody", "Neon;NeonRich", "None", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaUpupam = new OrganicSpecies( "FonticuluaUpupam", OrganicGenus.Fonticulus, 5727600, (decimal?)0.28, null, null, "IcyBody;RockyBody", "ArgonRich", "", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FrutexaAcus = new OrganicSpecies( "FrutexaAcus", OrganicGenus.Shrubs, 7774700, (decimal?)0.28, null, 195, "RockyBody", "CarbonDioxide;CarbonDioxideRich", "", "B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaCollum = new OrganicSpecies( "FrutexaCollum", OrganicGenus.Shrubs, 1639800, (decimal?)0.28, null, null, "RockyBody", "SulphurDioxide", "", "B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaFera = new OrganicSpecies( "FrutexaFera", OrganicGenus.Shrubs, 1632500, (decimal?)0.28, null, 195, "RockyBody", "CarbonDioxide;CarbonDioxideRich", "None", "B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaFlabellum = new OrganicSpecies( "FrutexaFlabellum", OrganicGenus.Shrubs, 1808900, (decimal?)0.28, null, null, "RockyBody", "Ammonia", "", "B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaFlammasis = new OrganicSpecies( "FrutexaFlammasis", OrganicGenus.Shrubs, 10326000, (decimal?)0.28, null, null, "RockyBody", "Ammonia", "", "B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaMetallicum = new OrganicSpecies( "FrutexaMetallicum", OrganicGenus.Shrubs, 1632500, (decimal?)0.28, null, 195, "HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich;Ammonia", "None", "B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaSponsae = new OrganicSpecies( "FrutexaSponsae", OrganicGenus.Shrubs, 5988000, (decimal?)0.28, null, null, "RockyBody", "Water;WaterRich", "", "B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FumerolaAquatis = new OrganicSpecies( "FumerolaAquatis", OrganicGenus.Fumerolas, 6284600, (decimal?)0.28, null, 450, "IcyBody;RockyIceBody", "Any", "Water", "" );
        public static readonly OrganicSpecies FumerolaCarbosis = new OrganicSpecies( "FumerolaCarbosis", OrganicGenus.Fumerolas, 6284600, (decimal?)0.28, null, 275, "IcyBody;RockyIceBody", "Any", "Carbon;Methane", "" );
        public static readonly OrganicSpecies FumerolaExtremus = new OrganicSpecies( "FumerolaExtremus", OrganicGenus.Fumerolas, 16202800, (decimal?)0.28, null, 205, "RockyBody;HighMetalContentBody", "Any", "Silicate;Iron;Rocky", "" );
        public static readonly OrganicSpecies FumerolaNitris = new OrganicSpecies( "FumerolaNitris", OrganicGenus.Fumerolas, 7500900, (decimal?)0.28, null, 250, "IcyBody;RockyIceBody", "Any", "Nitrogen;Ammonia", "" );
        public static readonly OrganicSpecies FungoidaBullarum = new OrganicSpecies( "FungoidaBullarum", OrganicGenus.Fungoids, 3703200, (decimal?)0.28, null, null, "RockyBody;HighMetalContentBody;RockyIceBody", "Argon;ArgonRich", "None", "" );
        public static readonly OrganicSpecies FungoidaGelata = new OrganicSpecies( "FungoidaGelata", OrganicGenus.Fungoids, 3330300, (decimal?)0.28, 180, 195, "RockyBody;HighMetalContentBody;RockyIceBody", "Water;WaterRich;CarbonDioxide;CarbonDioxideRich", "", "" );
        public static readonly OrganicSpecies FungoidaSetisis = new OrganicSpecies( "FungoidaSetisis", OrganicGenus.Fungoids, 1670100, (decimal?)0.28, null, null, "RockyBody;HighMetalContentBody;RockyIceBody", "Ammonia;Methane;MethaneRich", "", "" );
        public static readonly OrganicSpecies FungoidaStabitis = new OrganicSpecies( "FungoidaStabitis", OrganicGenus.Fungoids, 2680300, (decimal?)0.28, 180, 195, "RockyBody;HighMetalContentBody;RockyIceBody", "Water;WaterRich;CarbonDioxide;CarbonDioxideRich", "", "" );
        public static readonly OrganicSpecies OsseusCornibus = new OrganicSpecies( "OsseusCornibus", OrganicGenus.Osseus, 1483000, (decimal?)0.28, 180, 195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "" );
        public static readonly OrganicSpecies OsseusDiscus = new OrganicSpecies( "OsseusDiscus", OrganicGenus.Osseus, 12934900, (decimal?)0.28, null, 455, "RockyBody;HighMetalContentBody", "Water;WaterRich", "", "" );
        public static readonly OrganicSpecies OsseusFractus = new OrganicSpecies( "OsseusFractus", OrganicGenus.Osseus, 4027800, (decimal?)0.28, 180, 190, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "" );
        public static readonly OrganicSpecies OsseusPellebantus = new OrganicSpecies( "OsseusPellebantus", OrganicGenus.Osseus, 9739000, (decimal?)0.28, 190, 195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "" );
        public static readonly OrganicSpecies OsseusPumice = new OrganicSpecies( "OsseusPumice", OrganicGenus.Osseus, 3156300, (decimal?)0.28, null, 135, "RockyBody;HighMetalContentBody;RockyIceBody", "Argon;ArgonRich;Methane;MethaneRich;Nitrogen", "", "" );
        public static readonly OrganicSpecies OsseusSpiralis = new OrganicSpecies( "OsseusSpiralis", OrganicGenus.Osseus, 2404700, (decimal?)0.28, 160, null, "RockyBody;HighMetalContentBody", "Ammonia", "", "" );
        public static readonly OrganicSpecies ReceptaConditivus = new OrganicSpecies( "ReceptaConditivus", OrganicGenus.Recepta, 14313700, (decimal?)0.28, 130, 300, "IcyBody;RockyIceBody", "SulphurDioxide", "", "" );
        public static readonly OrganicSpecies ReceptaDeltahedronix = new OrganicSpecies( "ReceptaDeltahedronix", OrganicGenus.Recepta, 16202800, (decimal?)0.28, 130, 300, "RockyBody;HighMetalContentBody", "SulphurDioxide", "", "" );
        public static readonly OrganicSpecies ReceptaUmbrux = new OrganicSpecies( "ReceptaUmbrux", OrganicGenus.Recepta, 12934900, (decimal?)0.28, 130, 300, "IcyBody;RockyIceBody;RockyBody;HighMetalContentBody", "SulphurDioxide", "", "" );
        public static readonly OrganicSpecies AlbidumSinuousTubers = new OrganicSpecies( "AlbidumSinuousTubers", OrganicGenus.Tubers, 1514500, null, 200, 500, "RockyBody;HighMetalContentBody", "None", "Any", "" );
        public static readonly OrganicSpecies BlatteumSinuousTubers = new OrganicSpecies( "BlatteumSinuousTubers", OrganicGenus.Tubers, 1514500, null, 200, 500, "RockyBody;HighMetalContentBody", "None", "Any", "" );
        public static readonly OrganicSpecies CaeruleumSinuousTubers = new OrganicSpecies( "CaeruleumSinuousTubers", OrganicGenus.Tubers, 1514500, null, 200, 500, "RockyBody;HighMetalContentBody", "None", "Any", "" );
        public static readonly OrganicSpecies LindigoticumSinuousTubers = new OrganicSpecies( "LindigoticumSinuousTubers", OrganicGenus.Tubers, 1514500, null, 200, 500, "RockyBody;HighMetalContentBody", "None", "Any", "" );
        public static readonly OrganicSpecies PrasinumSinuousTubers = new OrganicSpecies( "PrasinumSinuousTubers", OrganicGenus.Tubers, 1514500, null, 200, 500, "RockyBody;HighMetalContentBody;RockyIceBody", "None", "Any", "" );
        public static readonly OrganicSpecies RoseumSinuousTubers = new OrganicSpecies( "RoseumSinuousTubers", OrganicGenus.Tubers, 1514500, null, 200, 500, "RockyBody;HighMetalContentBody", "None", "Any", "" );
        public static readonly OrganicSpecies ViolaceumSinuousTubers = new OrganicSpecies( "ViolaceumSinuousTubers", OrganicGenus.Tubers, 1514500, null, 200, 500, "RockyBody;HighMetalContentBody", "None", "Any", "" );
        public static readonly OrganicSpecies VirideSinuousTubers = new OrganicSpecies( "VirideSinuousTubers", OrganicGenus.Tubers, 1514500, null, 200, 500, "RockyBody;HighMetalContentBody", "None", "Any", "" );
        public static readonly OrganicSpecies StratumAraneamus = new OrganicSpecies( "StratumAraneamus", OrganicGenus.Stratum, 2448900, (decimal?)0.55, 165, null, "RockyBody", "SulphurDioxide", "", "B;A;F;G;K;M;L;T;TTS;N" );
        public static readonly OrganicSpecies StratumCucumisis = new OrganicSpecies( "StratumCucumisis", OrganicGenus.Stratum, 16202800, (decimal?)0.6, 190, null, "RockyBody", "SulphurDioxide;CarbonDioxide;CarbonDioxideRich", "", "" );
        public static readonly OrganicSpecies StratumExcutitus = new OrganicSpecies( "StratumExcutitus", OrganicGenus.Stratum, 2448900, (decimal?)0.48, 165, 190, "RockyBody", "SulphurDioxide;CarbonDioxide;CarbonDioxideRich", "", "" );
        public static readonly OrganicSpecies StratumFrigus = new OrganicSpecies( "StratumFrigus", OrganicGenus.Stratum, 2637500, (decimal?)0.55, 190, null, "RockyBody", "SulphurDioxide;CarbonDioxide;CarbonDioxideRich", "", "" );
        public static readonly OrganicSpecies StratumLaminamus = new OrganicSpecies( "StratumLaminamus", OrganicGenus.Stratum, 2788300, (decimal?)0.34, 165, null, "RockyBody", "Ammonia", "", "" );
        public static readonly OrganicSpecies StratumLimaxus = new OrganicSpecies( "StratumLimaxus", OrganicGenus.Stratum, 1362000, (decimal?)0.48, 165, 190, "RockyBody", "SulphurDioxide;CarbonDioxide;CarbonDioxideRich", "", "" );
        public static readonly OrganicSpecies StratumPaleas = new OrganicSpecies( "StratumPaleas", OrganicGenus.Stratum, 1362000, (decimal?)0.58, 165, null, "RockyBody", "Ammonia;Water;WaterRich;CarbonDioxide;CarbonDioxideRich", "", "" );
        public static readonly OrganicSpecies StratumTectonicas = new OrganicSpecies( "StratumTectonicas", OrganicGenus.Stratum, 19010800, (decimal?)0.9, 165, null, "HighMetalContentBody", "Oxygen;Ammonia;Water;WaterRich;CarbonDioxide;CarbonDioxideRich;SulphurDioxide", "", "" );
        public static readonly OrganicSpecies TubusCavas = new OrganicSpecies( "TubusCavas", OrganicGenus.Tubus, 11873200, (decimal?)0.19, 160, 190, "RockyBody", "CarbonDioxide;CarbonDioxideRich", "None", "B;A;F;G;K;M;L;T;TTS;N" );
        public static readonly OrganicSpecies TubusCompagibus = new OrganicSpecies( "TubusCompagibus", OrganicGenus.Tubus, 7774700, (decimal?)0.19, 160, 190, "RockyBody", "CarbonDioxide;CarbonDioxideRich", "None", "B;A;F;G;K;M;L;T;TTS;N" );
        public static readonly OrganicSpecies TubusConifer = new OrganicSpecies( "TubusConifer", OrganicGenus.Tubus, 2415500, (decimal?)0.19, 160, 190, "RockyBody", "CarbonDioxide;CarbonDioxideRich", "None", "B;A;F;G;K;M;L;T;TTS;N" );
        public static readonly OrganicSpecies TubusRosarium = new OrganicSpecies( "TubusRosarium", OrganicGenus.Tubus, 2637500, (decimal?)0.19, 160, null, "RockyBody", "Ammonia", "", "B;A;F;G;K;M;L;T;TTS;N" );
        public static readonly OrganicSpecies TubusSororibus = new OrganicSpecies( "TubusSororibus", OrganicGenus.Tubus, 5727600, (decimal?)0.19, 160, 190, "HighMetalContentBody", "Ammonia;CarbonDioxide;CarbonDioxideRich", "None", "B;A;F;G;K;M;L;T;TTS;N" );
        public static readonly OrganicSpecies TussockAlbata = new OrganicSpecies( "TussockAlbata", OrganicGenus.Tussocks, 3252500, (decimal?)0.28, 175, 180, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCapillum = new OrganicSpecies( "TussockCapillum", OrganicGenus.Tussocks, 7025800, (decimal?)0.28, 80, 165, "RockyBody;RockyIceBody", "Argon;ArgonRich;Methane;MethaneRich", "", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCaputus = new OrganicSpecies( "TussockCaputus", OrganicGenus.Tussocks, 3472400, (decimal?)0.28, 180, 190, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCatena = new OrganicSpecies( "TussockCatena", OrganicGenus.Tussocks, 1766600, (decimal?)0.28, 150, 190, "RockyBody;HighMetalContentBody", "Ammonia", "", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCultro = new OrganicSpecies( "TussockCultro", OrganicGenus.Tussocks, 1766600, (decimal?)0.28, null, null, "RockyBody;HighMetalContentBody", "Ammonia", "", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockDivisa = new OrganicSpecies( "TussockDivisa", OrganicGenus.Tussocks, 1766600, (decimal?)0.28, 150, 180, "RockyBody;HighMetalContentBody", "Ammonia", "", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockIgnis = new OrganicSpecies( "TussockIgnis", OrganicGenus.Tussocks, 1849000, (decimal?)0.28, 160, 170, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockPennata = new OrganicSpecies( "TussockPennata", OrganicGenus.Tussocks, 5853800, (decimal?)0.28, 145, 155, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockPennatis = new OrganicSpecies( "TussockPennatis", OrganicGenus.Tussocks, 1000000, (decimal?)0.28, null, 195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockPropagito = new OrganicSpecies( "TussockPropagito", OrganicGenus.Tussocks, 1000000, (decimal?)0.28, null, 195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockSerrati = new OrganicSpecies( "TussockSerrati", OrganicGenus.Tussocks, 4447100, (decimal?)0.28, 170, 175, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockStigmasis = new OrganicSpecies( "TussockStigmasis", OrganicGenus.Tussocks, 19010800, (decimal?)0.28, 130, 210, "RockyBody;HighMetalContentBody", "SulphurDioxide", "", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockTriticum = new OrganicSpecies( "TussockTriticum", OrganicGenus.Tussocks, 7774700, (decimal?)0.28, 190, 195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockVentusa = new OrganicSpecies( "TussockVentusa", OrganicGenus.Tussocks, 3227700, (decimal?)0.28, 155, 160, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "", "F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockVirgam = new OrganicSpecies( "TussockVirgam", OrganicGenus.Tussocks, 14313700, (decimal?)0.28, 390, 450, "RockyBody;HighMetalContentBody", "Water;WaterRich", "", "F;G;K;M;L;T;D;H" );

        // Species without any known criteria (including non-terrestrial species)
        public static readonly OrganicSpecies LindigoticumIceCrystals = new OrganicSpecies("LindigoticumIceCrystals", OrganicGenus.IceCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PrasinumIceCrystals = new OrganicSpecies("PrasinumIceCrystals", OrganicGenus.IceCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RoseumIceCrystals = new OrganicSpecies("RoseumIceCrystals", OrganicGenus.IceCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PurpureumIceCrystals = new OrganicSpecies("PurpureumIceCrystals", OrganicGenus.IceCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubeumIceCrystals = new OrganicSpecies("RubeumIceCrystals", OrganicGenus.IceCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AlbidumIceCrystals = new OrganicSpecies("AlbidumIceCrystals", OrganicGenus.IceCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies FlavumIceCrystals = new OrganicSpecies("FlavumIceCrystals", OrganicGenus.IceCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PrasinumMetallicCrystals = new OrganicSpecies("PrasinumMetallicCrystals", OrganicGenus.MetallicCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PurpureumMetallicCrystals = new OrganicSpecies("PurpureumMetallicCrystals", OrganicGenus.MetallicCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubeumMetallicCrystals = new OrganicSpecies("RubeumMetallicCrystals", OrganicGenus.MetallicCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies FlavumMetallicCrystals = new OrganicSpecies("FlavumMetallicCrystals", OrganicGenus.MetallicCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LindigoticumSilicateCrystals = new OrganicSpecies("LindigoticumSilicateCrystals", OrganicGenus.SilicateCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PrasinumSilicateCrystals = new OrganicSpecies("PrasinumSilicateCrystals", OrganicGenus.SilicateCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RoseumSilicateCrystals = new OrganicSpecies("RoseumSilicateCrystals", OrganicGenus.SilicateCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PurpureumSilicateCrystals = new OrganicSpecies("PurpureumSilicateCrystals", OrganicGenus.SilicateCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubeumSilicateCrystals = new OrganicSpecies("RubeumSilicateCrystals", OrganicGenus.SilicateCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AlbidumSilicateCrystals = new OrganicSpecies("AlbidumSilicateCrystals", OrganicGenus.SilicateCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies FlavumSilicateCrystals = new OrganicSpecies("FlavumSilicateCrystals", OrganicGenus.SilicateCrystals, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LindigoticumParasolMollusc = new OrganicSpecies("LindigoticumParasolMollusc", OrganicGenus.MolluscParasol, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LuteolumParasolMollusc = new OrganicSpecies("LuteolumParasolMollusc", OrganicGenus.MolluscParasol, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideParasolMollusc = new OrganicSpecies("VirideParasolMollusc", OrganicGenus.MolluscParasol, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LindigoticumBulbMollusc = new OrganicSpecies("LindigoticumBulbMollusc", OrganicGenus.MolluscBulb, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LuteolumBulbMollusc = new OrganicSpecies("LuteolumBulbMollusc", OrganicGenus.MolluscBulb, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideBulbMollusc = new OrganicSpecies("VirideBulbMollusc", OrganicGenus.MolluscBulb, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LindigoticumUmbrellaMollusc = new OrganicSpecies("LindigoticumUmbrellaMollusc", OrganicGenus.MolluscUmbrella, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LuteolumUmbrellaMollusc = new OrganicSpecies("LuteolumUmbrellaMollusc", OrganicGenus.MolluscUmbrella, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideUmbrellaMollusc = new OrganicSpecies("VirideUmbrellaMollusc", OrganicGenus.MolluscUmbrella, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LindigoticumCapsuleMollusc = new OrganicSpecies("LindigoticumCapsuleMollusc", OrganicGenus.MolluscCapsule, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LuteolumCapsuleMollusc = new OrganicSpecies("LuteolumCapsuleMollusc", OrganicGenus.MolluscCapsule, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideCapsuleMollusc = new OrganicSpecies("VirideCapsuleMollusc", OrganicGenus.MolluscCapsule, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LindigoticumReelMollusc = new OrganicSpecies("LindigoticumReelMollusc", OrganicGenus.MolluscReel, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LuteolumReelMollusc = new OrganicSpecies("LuteolumReelMollusc", OrganicGenus.MolluscReel, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideReelMollusc = new OrganicSpecies("VirideReelMollusc", OrganicGenus.MolluscReel, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LindigoticumCalcitePlates = new OrganicSpecies("LindigoticumCalcitePlates", OrganicGenus.CalcitePlates, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LuteolumCalcitePlates = new OrganicSpecies("LuteolumCalcitePlates", OrganicGenus.CalcitePlates, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideCalcitePlates = new OrganicSpecies("VirideCalcitePlates", OrganicGenus.CalcitePlates, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RutulumCalcitePlates = new OrganicSpecies("RutulumCalcitePlates", OrganicGenus.CalcitePlates, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CaeruleumPeduncleTree = new OrganicSpecies("CaeruleumPeduncleTree", OrganicGenus.PeduncleTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AlbidumPeduncleTree = new OrganicSpecies("AlbidumPeduncleTree", OrganicGenus.PeduncleTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies ViridePeduncleTree = new OrganicSpecies("ViridePeduncleTree", OrganicGenus.PeduncleTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies OstrinumPeduncleTree = new OrganicSpecies("OstrinumPeduncleTree", OrganicGenus.PeduncleTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubellumPeduncleTree = new OrganicSpecies("RubellumPeduncleTree", OrganicGenus.PeduncleTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CereumAsterTree = new OrganicSpecies("CereumAsterTree", OrganicGenus.AsterTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PrasinumAsterTree = new OrganicSpecies("PrasinumAsterTree", OrganicGenus.AsterTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubellumAsterTree = new OrganicSpecies("RubellumAsterTree", OrganicGenus.AsterTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies StolonTree = new OrganicSpecies("StolonTree", OrganicGenus.StolonTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CaeruleumPedunclePod = new OrganicSpecies("CaeruleumPedunclePod", OrganicGenus.PedunclePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CandidumPedunclePod = new OrganicSpecies("CandidumPedunclePod", OrganicGenus.PedunclePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies GypseeumPedunclePod = new OrganicSpecies("GypseeumPedunclePod", OrganicGenus.PedunclePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PurpureumPedunclePod = new OrganicSpecies("PurpureumPedunclePod", OrganicGenus.PedunclePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RufumPedunclePod = new OrganicSpecies("RufumPedunclePod", OrganicGenus.PedunclePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LindigoticumAsterPod = new OrganicSpecies("LindigoticumAsterPod", OrganicGenus.AsterPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CereumAsterPod = new OrganicSpecies("CereumAsterPod", OrganicGenus.AsterPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PrasinumAsterPod = new OrganicSpecies("PrasinumAsterPod", OrganicGenus.AsterPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PuniceumAsterPod = new OrganicSpecies("PuniceumAsterPod", OrganicGenus.AsterPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubellumAsterPod = new OrganicSpecies("RubellumAsterPod", OrganicGenus.AsterPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CaeruleumOctahedralPod = new OrganicSpecies("CaeruleumOctahedralPod", OrganicGenus.VoidPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies NiveumOctahedralPod = new OrganicSpecies("NiveumOctahedralPod", OrganicGenus.VoidPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideOctahedralPod = new OrganicSpecies("VirideOctahedralPod", OrganicGenus.VoidPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies BlatteumOctahedralPod = new OrganicSpecies("BlatteumOctahedralPod", OrganicGenus.VoidPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubeumOctahedralPod = new OrganicSpecies("RubeumOctahedralPod", OrganicGenus.VoidPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LividumCollaredPod = new OrganicSpecies("LividumCollaredPod", OrganicGenus.CollaredPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AlbidumCollaredPod = new OrganicSpecies("AlbidumCollaredPod", OrganicGenus.CollaredPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies BlatteumCollaredPod = new OrganicSpecies("BlatteumCollaredPod", OrganicGenus.CollaredPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubicundumCollaredPod = new OrganicSpecies("RubicundumCollaredPod", OrganicGenus.CollaredPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CaeruleumChalicePod = new OrganicSpecies("CaeruleumChalicePod", OrganicGenus.ChalicePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AlbidumChalicePod = new OrganicSpecies("AlbidumChalicePod", OrganicGenus.ChalicePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideChalicePod = new OrganicSpecies("VirideChalicePod", OrganicGenus.ChalicePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies OstrinumChalicePod = new OrganicSpecies("OstrinumChalicePod", OrganicGenus.ChalicePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubellumChalicePod = new OrganicSpecies("RubellumChalicePod", OrganicGenus.ChalicePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RoseumGyrePod = new OrganicSpecies("RoseumGyrePod", OrganicGenus.GyrePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AurariumGyrePod = new OrganicSpecies("AurariumGyrePod", OrganicGenus.GyrePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CobalteumRhizomePod = new OrganicSpecies("CobalteumRhizomePod", OrganicGenus.RhizomePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CandidumRhizomePod = new OrganicSpecies("CandidumRhizomePod", OrganicGenus.RhizomePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies GypseeumRhizomePod = new OrganicSpecies("GypseeumRhizomePod", OrganicGenus.RhizomePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PurpureumRhizomePod = new OrganicSpecies("PurpureumRhizomePod", OrganicGenus.RhizomePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubeumRhizomePod = new OrganicSpecies("RubeumRhizomePod", OrganicGenus.RhizomePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CaeruleumQuadripartitePod = new OrganicSpecies("CaeruleumQuadripartitePod", OrganicGenus.QuadripartitePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AlbidumQuadripartitePod = new OrganicSpecies("AlbidumQuadripartitePod", OrganicGenus.QuadripartitePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideQuadripartitePod = new OrganicSpecies("VirideQuadripartitePod", OrganicGenus.QuadripartitePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies BlatteumQuadripartitePod = new OrganicSpecies("BlatteumQuadripartitePod", OrganicGenus.QuadripartitePod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CaeruleumGourdMollusc = new OrganicSpecies("CaeruleumGourdMollusc", OrganicGenus.MolluscGourd, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AlbulumGourdMollusc = new OrganicSpecies("AlbulumGourdMollusc", OrganicGenus.MolluscGourd, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideGourdMollusc = new OrganicSpecies("VirideGourdMollusc", OrganicGenus.MolluscGourd, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PhoeniceumGourdMollusc = new OrganicSpecies("PhoeniceumGourdMollusc", OrganicGenus.MolluscGourd, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PurpureumGourdMollusc = new OrganicSpecies("PurpureumGourdMollusc", OrganicGenus.MolluscGourd, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RufumGourdMollusc = new OrganicSpecies("RufumGourdMollusc", OrganicGenus.MolluscGourd, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CroceumGourdMollusc = new OrganicSpecies("CroceumGourdMollusc", OrganicGenus.MolluscGourd, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CaeruleumTorusMollusc = new OrganicSpecies("CaeruleumTorusMollusc", OrganicGenus.MolluscTorus, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideTorusMollusc = new OrganicSpecies("VirideTorusMollusc", OrganicGenus.MolluscTorus, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies BlatteumTorusMollusc = new OrganicSpecies("BlatteumTorusMollusc", OrganicGenus.MolluscTorus, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubellumTorusMollusc = new OrganicSpecies("RubellumTorusMollusc", OrganicGenus.MolluscTorus, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies FlavumTorusMollusc = new OrganicSpecies("FlavumTorusMollusc", OrganicGenus.MolluscTorus, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CaeruleumSquidMollusc = new OrganicSpecies("CaeruleumSquidMollusc", OrganicGenus.MolluscSquid, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AlbulumSquidMollusc = new OrganicSpecies("AlbulumSquidMollusc", OrganicGenus.MolluscSquid, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RoseumSquidMollusc = new OrganicSpecies("RoseumSquidMollusc", OrganicGenus.MolluscSquid, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PuniceumSquidMollusc = new OrganicSpecies("PuniceumSquidMollusc", OrganicGenus.MolluscSquid, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubeumSquidMollusc = new OrganicSpecies("RubeumSquidMollusc", OrganicGenus.MolluscSquid, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LividumBulletMollusc = new OrganicSpecies("LividumBulletMollusc", OrganicGenus.MolluscBullet, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CereumBulletMollusc = new OrganicSpecies("CereumBulletMollusc", OrganicGenus.MolluscBullet, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideBulletMollusc = new OrganicSpecies("VirideBulletMollusc", OrganicGenus.MolluscBullet, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RubeumBulletMollusc = new OrganicSpecies("RubeumBulletMollusc", OrganicGenus.MolluscBullet, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies FlavumBulletMollusc = new OrganicSpecies("FlavumBulletMollusc", OrganicGenus.MolluscBullet, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CobalteumGlobeMollusc = new OrganicSpecies("CobalteumGlobeMollusc", OrganicGenus.MolluscGlobe, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies NiveumGlobeMollusc = new OrganicSpecies("NiveumGlobeMollusc", OrganicGenus.MolluscGlobe, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies PrasinumGlobeMollusc = new OrganicSpecies("PrasinumGlobeMollusc", OrganicGenus.MolluscGlobe, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RoseumGlobeMollusc = new OrganicSpecies("RoseumGlobeMollusc", OrganicGenus.MolluscGlobe, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies OstrinumGlobeMollusc = new OrganicSpecies("OstrinumGlobeMollusc", OrganicGenus.MolluscGlobe, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies RutulumGlobeMollusc = new OrganicSpecies("RutulumGlobeMollusc", OrganicGenus.MolluscGlobe, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CroceumGlobeMollusc = new OrganicSpecies("CroceumGlobeMollusc", OrganicGenus.MolluscGlobe, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LindigoticumBellMollusc = new OrganicSpecies("LindigoticumBellMollusc", OrganicGenus.MolluscBell, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AlbensBellMollusc = new OrganicSpecies("AlbensBellMollusc", OrganicGenus.MolluscBell, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies GypseeumBellMollusc = new OrganicSpecies("GypseeumBellMollusc", OrganicGenus.MolluscBell, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies BlatteumBellMollusc = new OrganicSpecies("BlatteumBellMollusc", OrganicGenus.MolluscBell, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LuteolumBellMollusc = new OrganicSpecies("LuteolumBellMollusc", OrganicGenus.MolluscBell, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LatticeMineralSpheres = new OrganicSpecies("LatticeMineralSpheres", OrganicGenus.MineralSpheres, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies SolidMineralSpheres = new OrganicSpecies("SolidMineralSpheres", OrganicGenus.MineralSpheres, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies StolonPod = new OrganicSpecies("StolonPod", OrganicGenus.StolonPod, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies AurariumGyreTree = new OrganicSpecies("AurariumGyreTree", OrganicGenus.GyreTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies VirideGyreTree = new OrganicSpecies("VirideGyreTree", OrganicGenus.GyreTree, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies ChryseumVoidHeart = new OrganicSpecies("ChryseumVoidHeart", OrganicGenus.VoidHeart, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies CommonThargoidBarnacle = new OrganicSpecies("CommonThargoidBarnacle", OrganicGenus.ThargoidBarnacle, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies LargeThargoidBarnacle = new OrganicSpecies("LargeThargoidBarnacle", OrganicGenus.ThargoidBarnacle, 0, null, null, null, "", "", "", "" );
        public static readonly OrganicSpecies ThargoidBarnacleBarbs = new OrganicSpecies("ThargoidBarnacleBarbs", OrganicGenus.ThargoidBarnacle, 0, null, null, null, "", "", "", "" );

        public OrganicGenus genus;

        [PublicAPI( "The credit value for this species" )]
        public long value;
        
        public decimal? maxG;
        public decimal? minK;
        public decimal? maxK;
        public IList<string> planetClass;
        public IList<string> atmosphereClass;
        public IList<string> starClass;
        public IList<string> volcanism;

        public string description => Properties.OrganicSpeciesDesc.ResourceManager.GetString( NormalizeSpecies( edname ) );
        public string conditions => Properties.OrganicSpeciesCond.ResourceManager.GetString( NormalizeSpecies( edname ) );
        public int minimumDistanceMeters => genus.minimumDistanceMeters;

        public bool isPredictable => maxG != null ||
                                     minK != null ||
                                     maxK != null ||
                                     planetClass.Any() ||
                                     atmosphereClass.Any() ||
                                     volcanism.Any() ||
                                     starClass.Any();

        // dummy used to ensure that the static constructor has run
        public OrganicSpecies () : this( "" )
        { }

        private OrganicSpecies ( string edname ) : base( edname, edname )
        {
            this.planetClass = new List<string>();
            this.atmosphereClass = new List<string>();
            this.starClass = new List<string>();
            this.volcanism = new List<string>();
        }

        private OrganicSpecies ( string edname,
                                 OrganicGenus genus,
                                 long value,
                                 decimal? maxG,
                                 decimal? minK,
                                 decimal? maxK,
                                 string planetClass,
                                 string atmosphereClass,
                                 string volcanism,
                                 string starClass ) : base( edname, NormalizeSpecies( edname ) )
        {
            this.genus = genus;
            this.value = value;
            this.maxG = maxG;
            this.minK = minK;
            this.maxK = maxK;
            this.planetClass = !string.IsNullOrEmpty( planetClass ) ? planetClass.Split( ';' ).ToList() : new List<string>();
            this.atmosphereClass = !string.IsNullOrEmpty( atmosphereClass ) ? atmosphereClass.Split( ';' ).ToList() : new List<string>();
            this.starClass = !string.IsNullOrEmpty( starClass ) ? starClass.Split( ';' ).ToList() : new List<string>();
            this.volcanism = !string.IsNullOrEmpty( volcanism ) ? volcanism.Split( ';' ).ToList() : new List<string>();
        }

        public static new OrganicSpecies FromEDName ( string edname )
        {
            return ResourceBasedLocalizedEDName<OrganicSpecies>.FromEDName( NormalizeSpecies( edname ) );
        }

        public static string NormalizeSpecies ( string edname )
        {
            return edname?
                .Replace( "Codex_Ent_", "" )
                .Replace( "$", "" )
                .Replace( "_Name;", "" )
                .Replace( "_name;", "" )
                .Replace( ";", "" );
        }
    }
}
