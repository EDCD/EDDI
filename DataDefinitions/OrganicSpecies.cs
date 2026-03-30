using Newtonsoft.Json;
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
        // - Many of these ednames have been generated and assumed from the variant name and may not be correct
        // - The prediction data here is old and not used anymore, will probably remove in future
        public static readonly OrganicSpecies AleoidaArcus = new( "Aleoids_01", OrganicGenus.Aleoids, 7252500,0.3M, 175, 180, "","CarbonDioxide","None","B;A;F;K;M;L;T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaCoronamus = new( "Aleoids_02", OrganicGenus.Aleoids, 6284600,0.3M, 180, 190, "","CarbonDioxide","None","B;A;F;K;M;L;T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaGravis = new( "Aleoids_05", OrganicGenus.Aleoids, 12934900,0.3M, 190, 195, "","CarbonDioxide","None","B;A;F;K;M;L;T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaLaminiae = new( "Aleoids_04", OrganicGenus.Aleoids, 3385200,0.3M, null, null, "","Ammonia","","B;A;F;K;M;L;T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaSpica = new( "Aleoids_03", OrganicGenus.Aleoids, 3385200,0.3M, null, null, "","Ammonia","","B;A;F;K;M;L;T;TTS;Y;N" );
        public static readonly OrganicSpecies AmphoraPlant = new( "Vents", OrganicGenus.Vents, 1628800,null, 1000, null, "MetalRichBody","None","","A" );
        public static readonly OrganicSpecies BlatteumBioluminescentAnemone = new( "SphereEFGH", OrganicGenus.Sphere, 1499900,null, 210, null, "MetalRichBody;HighMetalContentBody","Argon;CarbonDioxide;CarbonDioxideRich;HotSilicateVapour;None","","B" );
        public static readonly OrganicSpecies CroceumAnemone = new( "SphereABCD_01", OrganicGenus.Sphere, 1499900,0.42M, 200, 440, "RockyBody","Water;SulphurDioxide;None","","B;A" );
        public static readonly OrganicSpecies LuteolumAnemone = new( "Sphere", OrganicGenus.Sphere, 1499900,1.32M, 200, 440, "RockyBody","CarbonDioxide;Water;SulphurDioxide;None","","B" );
        public static readonly OrganicSpecies PrasinumBioluminescentAnemone = new( "SphereEFGH_02", OrganicGenus.Sphere, 1499900,null, 20, null, "RockyBody;MetalRichBody;HighMetalContentBody","CarbonDioxide;Argon;Ammonia;Nitrogen;SulphurDioxide;NeonRich;HotSulphurDioxide;None","","O" );
        public static readonly OrganicSpecies PuniceumAnemone = new( "SphereABCD_02", OrganicGenus.Sphere, 1499900,2.61M, 65, 860, "IceBody","Oxygen;Nitrogen;None","","O;W" );
        public static readonly OrganicSpecies RoseumAnemone = new( "SphereABCD_03", OrganicGenus.Sphere, 1499900,0.45M, 200, 440, "RockyBody","SulphurDioxide;None","","B" );
        public static readonly OrganicSpecies RoseumBioluminescentAnemone = new( "SphereEFGH_03", OrganicGenus.Sphere, 1499900,null, 190, null, "MetalRichBody;HighMetalContentBody","CarbonDioxide;SulphurDioxide;None","","B" );
        public static readonly OrganicSpecies RubeumBioluminescentAnemone = new( "SphereEFGH_01", OrganicGenus.Sphere, 1499900,null, 160, null, "MetalRichBody;HighMetalContentBody","Argon;CarbonDioxide;SulphurDioxide;None","","B" );
        public static readonly OrganicSpecies BacteriumAcies = new( "Bacterial_04", OrganicGenus.Bacterial, 1000000,0.75M, null, null, "IcyBody;RockyIceBody","Neon;NeonRich","","" );
        public static readonly OrganicSpecies BacteriumAlcyoneum = new( "Bacterial_06", OrganicGenus.Bacterial, 1658500,0.38M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody","Ammonia","","" );
        public static readonly OrganicSpecies BacteriumAurasus = new( "Bacterial_01", OrganicGenus.Bacterial, 1000000,1, null, null, "","CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies BacteriumBullaris = new( "Bacterial_10", OrganicGenus.Bacterial, 1152500,0.61M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody","Methane;MethaneRich","","" );
        public static readonly OrganicSpecies BacteriumCerbrus = new( "Bacterial_12", OrganicGenus.Bacterial, 1689800,1, null, null, "","Water;WaterRich;SulphurDioxide","","" );
        public static readonly OrganicSpecies BacteriumInformem = new( "Bacterial_08", OrganicGenus.Bacterial, 8418000,0.6M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody","Nitrogen","","" );
        public static readonly OrganicSpecies BacteriumNebulus = new( "Bacterial_02", OrganicGenus.Bacterial, 5289900,0.55M, null, null, "IcyBody","Helium","","" );
        public static readonly OrganicSpecies BacteriumOmentum = new( "Bacterial_11", OrganicGenus.Bacterial, 4638900,0.61M, null, null, "IcyBody","Neon;NeonRich","Nitrogen;Ammonia","" );
        public static readonly OrganicSpecies BacteriumScopulum = new( "Bacterial_03", OrganicGenus.Bacterial, 4934500,0.62M, null, null, "IcyBody;RockyIceBody","Neon;NeonRich","Carbon;Methane","" );
        public static readonly OrganicSpecies BacteriumTela = new( "Bacterial_07", OrganicGenus.Bacterial, 1949000,0.62M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody","Any","Helium;Iron;Silicate","" );
        public static readonly OrganicSpecies BacteriumVerrata = new( "Bacterial_13", OrganicGenus.Bacterial, 3897000,0.61M, null, null, "IcyBody;RockyBody;RockyIceBody","Neon;NeonRich","Water","" );
        public static readonly OrganicSpecies BacteriumVesicula = new( "Bacterial_05", OrganicGenus.Bacterial, 1000000,1, null, null, "IcyBody;RockyBody;HighMetalContentBody;RockyIceBody","Argon;ArgonRich","","" );
        public static readonly OrganicSpecies BacteriumVolu = new( "Bacterial_09", OrganicGenus.Bacterial, 7774700,0.61M, null, null, "IcyBody;RockyBody;HighMetalContentBody;RockyIceBody","Oxygen","","" );
        public static readonly OrganicSpecies BarkMounds = new( "Cone", OrganicGenus.Cone, 1471900,null, 88, 440, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody","None;CarbonDioxide;CarbonDioxideRich;ArgonRich;SulphurDioxide;ThickArgonRich","","" );
        public static readonly OrganicSpecies AureumBrainTree = new( "SeedEFGH_01", OrganicGenus.Brancae, 1593700,null, 300, 500, "MetalRichBody;HighMetalContentBody","None;SulphurDioxide","Any","" );
        public static readonly OrganicSpecies GypseeumBrainTree = new( "SeedABCD_01", OrganicGenus.Brancae, 1593700,0.42M, 170, 330, "RockyBody","Ammonia;None;Oxygen;SulphurDioxide","Any","" );
        public static readonly OrganicSpecies LindigoticumBrainTree = new( "SeedEFGH_03", OrganicGenus.Brancae, 1593700,null, 300, 500, "RockyBody;HighMetalContentBody","None","Any","" );
        public static readonly OrganicSpecies LividumBrainTree = new( "SeedEFGH", OrganicGenus.Brancae, 1593700,0.48M, 300, 500, "RockyBody","None;Water;SulphurDioxide","Any","" );
        public static readonly OrganicSpecies OstrinumBrainTree = new( "SeedABCD_02", OrganicGenus.Brancae, 1593700,null, 20, null, "MetalRichBody;HighMetalContentBody","None;CarbonDioxide;Ammonia;CarbonDioxideRich;ArgonRich;SulphurDioxide;Helium;NeonRich","Any","" );
        public static readonly OrganicSpecies PuniceumBrainTree = new( "SeedEFGH_02", OrganicGenus.Brancae, 1593700,null, 20, null, "MetalRichBody;HighMetalContentBody","None;CarbonDioxide;Oxygen;SulphurDioxide;Helium;NeonRich","Any","" );
        public static readonly OrganicSpecies RoseumBrainTree = new( "Seed", OrganicGenus.Brancae, 1593700,null, 115, 500, "RockyBody;MetalRichBody;HighMetalContentBody;RockyIceBody","None;CarbonDioxide;Argon;Ammonia;CarbonDioxideRich;Oxygen;Water;SulphurDioxide;ArgonRich;WaterRich","Any","" );
        public static readonly OrganicSpecies VirideBrainTree = new( "SeedABCD_03", OrganicGenus.Brancae, 1593700,0.4M, 100, 255, "RockyIceBody","Ammonia;None;SulphurDioxide","Any","" );
        public static readonly OrganicSpecies CactoidaCortexum = new( "Cactoid_01", OrganicGenus.Cactoid, 3667600,0.27M, 158, 196, "RockyBody;HighMetalContentBody","CarbonDioxide","None","F;G;A;L;K;N;B;M;H" );
        public static readonly OrganicSpecies CactoidaLapis = new( "Cactoid_02", OrganicGenus.Cactoid, 2483600,0.28M, 160, 225, "RockyBody;HighMetalContentBody","Ammonia","","F;G;H;A;K;N;B;A" );
        public static readonly OrganicSpecies CactoidaPeperatis = new( "Cactoid_05", OrganicGenus.Cactoid, 2483600,0.28M, 160, 186, "RockyBody;HighMetalContentBody","Ammonia","","F;G;A;K;N;B;H" );
        public static readonly OrganicSpecies CactoidaPullulanta = new( "Cactoid_04", OrganicGenus.Cactoid, 3667600,0.27M, 127, 195, "RockyBody;HighMetalContentBody","CarbonDioxide","None","F;G;H;A;K;N;B" );
        public static readonly OrganicSpecies CactoidaVermis = new( "Cactoid_03", OrganicGenus.Cactoid, 16202800,0.28M, 160, 450, "RockyBody;HighMetalContentBody","Water;SulphurDioxide","","F;G;H;A;M;N;B;K" );
        public static readonly OrganicSpecies ClypeusLacrimam = new( "Clypeus_01", OrganicGenus.Clypeus, 8418000,0.28M, 190, null, "RockyBody;HighMetalContentBody","Water;CarbonDioxide","","A;F;G;K;M;L;N" );
        public static readonly OrganicSpecies ClypeusMargaritus = new( "Clypeus_02", OrganicGenus.Clypeus, 11873200,0.28M, 190, null, "RockyBody;HighMetalContentBody","Water;CarbonDioxide","None","A;F;G;K;M;L;N" );
        public static readonly OrganicSpecies ClypeusSpeculumi = new( "Clypeus_03", OrganicGenus.Clypeus, 16202800,0.28M, 190, null, "RockyBody;HighMetalContentBody","Water;CarbonDioxide","","A;F;G;K;M;L;N" );
        public static readonly OrganicSpecies ConchaAureolas = new( "Conchas_02", OrganicGenus.Conchas, 7774700,0.28M, null, null, "","Ammonia","","" );
        public static readonly OrganicSpecies ConchaBiconcavis = new( "Conchas_04", OrganicGenus.Conchas, 19010800,0.28M, null, null, "","Nitrogen","None","" );
        public static readonly OrganicSpecies ConchaLabiata = new( "Conchas_03", OrganicGenus.Conchas, 2352400,0.28M, null, 190, "","CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies ConchaRenibus = new( "Conchas_01", OrganicGenus.Conchas, 4572400,0.28M, 180, 195, "","Water;WaterRich","","" );
        public static readonly OrganicSpecies CrystallineShards = new( "Ground_Struct_Ice", OrganicGenus.Ground_Struct_Ice, 1628800,2, null, 266, "IcyBody;HighMetalContentBody;RockyIceBody;RockyBody","None;CarbonDioxide;Argon;CarbonDioxideRich;Methane;ArgonRich;Neon;Helium;NeonRich","","A;F;G;K;M;S" );
        public static readonly OrganicSpecies ElectricaePluma = new( "Electricae_01", OrganicGenus.Electricae, 6284600,0.28M, null, 150, "IcyBody","Neon;NeonRich;Argon;ArgonRich","","A;N" );
        public static readonly OrganicSpecies ElectricaeRadialem = new( "Electricae_02", OrganicGenus.Electricae, 6284600,0.28M, null, 150, "IcyBody","Neon;NeonRich;Argon;ArgonRich;Methane","","" );
        public static readonly OrganicSpecies FonticuluaCampestris = new( "Fonticulus_02", OrganicGenus.Fonticulus, 1000000,0.28M, null, 150, "IcyBody;RockyBody","Argon","","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaDigitos = new( "Fonticulus_06", OrganicGenus.Fonticulus, 1804100,0.28M, null, null, "IcyBody;RockyBody","Methane;MethaneRich","","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaFluctus = new( "Fonticulus_05", OrganicGenus.Fonticulus, 20000000,0.28M, null, null, "IcyBody;RockyBody","Oxygen","","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaLapida = new( "Fonticulus_04", OrganicGenus.Fonticulus, 3111000,0.28M, null, null, "IcyBody;RockyBody","Nitrogen","","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaSegmentatus = new( "Fonticulus_01", OrganicGenus.Fonticulus, 19010800,0.28M, null, null, "IcyBody;RockyBody","Neon;NeonRich","None","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaUpupam = new( "Fonticulus_03", OrganicGenus.Fonticulus, 5727600,0.28M, null, null, "IcyBody;RockyBody","ArgonRich","","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FrutexaAcus = new( "Shrubs_02", OrganicGenus.Shrubs, 7774700,0.28M, null, 195, "RockyBody","CarbonDioxide;CarbonDioxideRich","","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaCollum = new( "Shrubs_07", OrganicGenus.Shrubs, 1639800,0.28M, null, null, "RockyBody","SulphurDioxide","","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaFera = new( "Shrubs_05", OrganicGenus.Shrubs, 1632500,0.28M, null, 195, "RockyBody","CarbonDioxide;CarbonDioxideRich","None","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaFlabellum = new( "Shrubs_01", OrganicGenus.Shrubs, 1808900,0.28M, null, null, "RockyBody","Ammonia","","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaFlammasis = new( "Shrubs_04", OrganicGenus.Shrubs, 10326000,0.28M, null, null, "RockyBody","Ammonia","","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaMetallicum = new( "Shrubs_03", OrganicGenus.Shrubs, 1632500,0.28M, null, 195, "HighMetalContentBody","CarbonDioxide;CarbonDioxideRich;Ammonia","None","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaSponsae = new( "Shrubs_06", OrganicGenus.Shrubs, 5988000,0.28M, null, null, "RockyBody","Water;WaterRich","","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FumerolaAquatis = new( "Fumerolas_04", OrganicGenus.Fumerolas, 6284600,0.28M, null, 450, "IcyBody;RockyIceBody","Any","Water","" );
        public static readonly OrganicSpecies FumerolaCarbosis = new( "Fumerolas_01", OrganicGenus.Fumerolas, 6284600,0.28M, null, 275, "IcyBody;RockyIceBody","Any","Carbon;Methane","" );
        public static readonly OrganicSpecies FumerolaExtremus = new( "Fumerolas_02", OrganicGenus.Fumerolas, 16202800,0.28M, null, 205, "RockyBody;HighMetalContentBody","Any","Silicate;Iron;Rocky","" );
        public static readonly OrganicSpecies FumerolaNitris = new( "Fumerolas_03", OrganicGenus.Fumerolas, 7500900,0.28M, null, 250, "IcyBody;RockyIceBody","Any","Nitrogen;Ammonia","" );
        public static readonly OrganicSpecies FungoidaBullarum = new( "Fungoids_03", OrganicGenus.Fungoids, 3703200,0.28M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody","Argon;ArgonRich","None","" );
        public static readonly OrganicSpecies FungoidaGelata = new( "Fungoids_04", OrganicGenus.Fungoids, 3330300,0.28M, 180, 195, "RockyBody;HighMetalContentBody;RockyIceBody","Water;WaterRich;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies FungoidaSetisis = new( "Fungoids_01", OrganicGenus.Fungoids, 1670100,0.28M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody","Ammonia;Methane;MethaneRich","","" );
        public static readonly OrganicSpecies FungoidaStabitis = new( "Fungoids_02", OrganicGenus.Fungoids, 2680300,0.28M, 180, 195, "RockyBody;HighMetalContentBody;RockyIceBody","Water;WaterRich;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies OsseusCornibus = new( "Osseus_05", OrganicGenus.Osseus, 1483000,0.28M, 180, 195, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","" );
        public static readonly OrganicSpecies OsseusDiscus = new( "Osseus_02", OrganicGenus.Osseus, 12934900,0.28M, null, 455, "RockyBody;HighMetalContentBody","Water;WaterRich","","" );
        public static readonly OrganicSpecies OsseusFractus = new( "Osseus_01", OrganicGenus.Osseus, 4027800,0.28M, 180, 190, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","" );
        public static readonly OrganicSpecies OsseusPellebantus = new( "Osseus_06", OrganicGenus.Osseus, 9739000,0.28M, 190, 195, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","" );
        public static readonly OrganicSpecies OsseusPumice = new( "Osseus_04", OrganicGenus.Osseus, 3156300,0.28M, null, 135, "RockyBody;HighMetalContentBody;RockyIceBody","Argon;ArgonRich;Methane;MethaneRich;Nitrogen","","" );
        public static readonly OrganicSpecies OsseusSpiralis = new( "Osseus_03", OrganicGenus.Osseus, 2404700,0.28M, 160, null, "RockyBody;HighMetalContentBody","Ammonia","","" );
        public static readonly OrganicSpecies ReceptaConditivus = new( "Recepta_03", OrganicGenus.Recepta, 14313700,0.28M, 130, 300, "IcyBody;RockyIceBody","SulphurDioxide","","" );
        public static readonly OrganicSpecies ReceptaDeltahedronix = new( "Recepta_02", OrganicGenus.Recepta, 16202800,0.28M, 130, 300, "RockyBody;HighMetalContentBody","SulphurDioxide","","" );
        public static readonly OrganicSpecies ReceptaUmbrux = new( "Recepta_01", OrganicGenus.Recepta, 12934900,0.28M, 130, 300, "IcyBody;RockyIceBody;RockyBody;HighMetalContentBody","SulphurDioxide","","" );
        public static readonly OrganicSpecies AlbidumSinuousTubers = new( "TubeABCD_02", OrganicGenus.Tubers, 1514500,null, 200, 500, "RockyBody;HighMetalContentBody","None","Any","" );
        public static readonly OrganicSpecies BlatteumSinuousTubers = new( "TubeEFGH", OrganicGenus.Tubers, 1514500,null, 200, 500, "RockyBody;HighMetalContentBody","SulphurDioxide;None","Any","" );
        public static readonly OrganicSpecies CaeruleumSinuousTubers = new( "TubeABCD_03", OrganicGenus.Tubers, 1514500,null, 200, 500, "RockyBody;HighMetalContentBody","SulphurDioxide;None","Any","" );
        public static readonly OrganicSpecies LindigoticumSinuousTubers = new( "TubeEFGH_01", OrganicGenus.Tubers, 1514500,null, 200, 500, "RockyBody;HighMetalContentBody","None","Any","" );
        public static readonly OrganicSpecies PrasinumSinuousTubers = new( "TubeABCD_01", OrganicGenus.Tubers, 1514500,null, 200, 500, "RockyBody;HighMetalContentBody;RockyIceBody","CarbonDioxideRich;None;CarbonDioxide;SulphurDioxide","Any","" );
        public static readonly OrganicSpecies RoseumSinuousTubers = new( "Tube", OrganicGenus.Tubers, 1514500,null, 200, 500, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich;ArgonRich;SulphurDioxide;None","Any","" );
        public static readonly OrganicSpecies ViolaceumSinuousTubers = new( "TubeEFGH_02", OrganicGenus.Tubers, 1514500,null, 200, 500, "RockyBody;HighMetalContentBody","None","Any","" );
        public static readonly OrganicSpecies VirideSinuousTubers = new( "TubeEFGH_03", OrganicGenus.Tubers, 1514500,null, 200, 500, "RockyBody;HighMetalContentBody","SulphurDioxide;None","Any","" );
        public static readonly OrganicSpecies StratumAraneamus = new( "Stratum_04", OrganicGenus.Stratum, 2448900,0.55M, 165, null, "RockyBody","SulphurDioxide","","" );
        public static readonly OrganicSpecies StratumCucumisis = new( "Stratum_06", OrganicGenus.Stratum, 16202800,0.6M, 190, null, "RockyBody","SulphurDioxide;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies StratumExcutitus = new( "Stratum_01", OrganicGenus.Stratum, 2448900,0.48M, 165, 190, "RockyBody","SulphurDioxide;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies StratumFrigus = new( "Stratum_08", OrganicGenus.Stratum, 2637500,0.55M, 190, null, "RockyBody","SulphurDioxide;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies StratumLaminamus = new( "Stratum_03", OrganicGenus.Stratum, 2788300,0.34M, 165, null, "RockyBody","Ammonia","","" );
        public static readonly OrganicSpecies StratumLimaxus = new( "Stratum_05", OrganicGenus.Stratum, 1362000,0.48M, 165, 190, "RockyBody","SulphurDioxide;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies StratumPaleas = new( "Stratum_02", OrganicGenus.Stratum, 1362000,0.58M, 165, null, "RockyBody","Ammonia;Water;WaterRich;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies StratumTectonicas = new( "Stratum_07", OrganicGenus.Stratum, 19010800,0.9M, 165, null, "HighMetalContentBody","Oxygen;Ammonia;Water;WaterRich;CarbonDioxide;CarbonDioxideRich;SulphurDioxide","","" );
        public static readonly OrganicSpecies TubusCavas = new( "Tubus_03", OrganicGenus.Tubus, 11873200,0.16M, 160, 200, "RockyBody","CarbonDioxide","None","F;G;H;A;K;N;M;B" );
        public static readonly OrganicSpecies TubusCompagibus = new( "Tubus_05", OrganicGenus.Tubus, 7774700,0.19M, 150, 190, "RockyBody","CarbonDioxide","None","S;A;K;M;N;M;DC;H;K" );
        public static readonly OrganicSpecies TubusConifer = new( "Tubus_01", OrganicGenus.Tubus, 2415500,0.17M, 160, 200, "RockyBody","CarbonDioxide","None","F;G;A;K;N;M;H" );
        public static readonly OrganicSpecies TubusRosarium = new( "Tubus_04", OrganicGenus.Tubus, 2637500,0.16M, 160, 180, "RockyBody","Ammonia","","F;G;A;K;N;B;K" );
        public static readonly OrganicSpecies TubusSororibus = new( "Tubus_02", OrganicGenus.Tubus, 5727600,0.16M, 160, 200, "HighMetalContentBody","Ammonia;CarbonDioxide","None","F;G;A;L;K;N;M;M;DC" );
        public static readonly OrganicSpecies TussockAlbata = new( "Tussocks_08", OrganicGenus.Tussocks, 3252500,0.28M, 175, 180, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCapillum = new( "Tussocks_15", OrganicGenus.Tussocks, 7025800,0.28M, 80, 165, "RockyBody;RockyIceBody","Argon;ArgonRich;Methane;MethaneRich","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCaputus = new( "Tussocks_11", OrganicGenus.Tussocks, 3472400,0.28M, 180, 190, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCatena = new( "Tussocks_05", OrganicGenus.Tussocks, 1766600,0.28M, 150, 190, "RockyBody;HighMetalContentBody","Ammonia","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCultro = new( "Tussocks_04", OrganicGenus.Tussocks, 1766600,0.28M, null, null, "RockyBody;HighMetalContentBody","Ammonia","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockDivisa = new( "Tussocks_10", OrganicGenus.Tussocks, 1766600,0.28M, 150, 180, "RockyBody;HighMetalContentBody","Ammonia","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockIgnis = new( "Tussocks_03", OrganicGenus.Tussocks, 1849000,0.28M, 160, 170, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockPennata = new( "Tussocks_01", OrganicGenus.Tussocks, 5853800,0.28M, 145, 155, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockPennatis = new( "Tussocks_06", OrganicGenus.Tussocks, 1000000,0.28M, null, 195, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockPropagito = new( "Tussocks_09", OrganicGenus.Tussocks, 1000000,0.28M, null, 195, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockSerrati = new( "Tussocks_07", OrganicGenus.Tussocks, 4447100,0.28M, 170, 175, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockStigmasis = new( "Tussocks_13", OrganicGenus.Tussocks, 19010800,0.28M, 130, 210, "RockyBody;HighMetalContentBody","SulphurDioxide","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockTriticum = new( "Tussocks_12", OrganicGenus.Tussocks, 7774700,0.28M, 190, 195, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockVentusa = new( "Tussocks_02", OrganicGenus.Tussocks, 3227700,0.28M, 155, 160, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockVirgam = new( "Tussocks_14", OrganicGenus.Tussocks, 14313700,0.28M, 390, 450, "RockyBody;HighMetalContentBody","Water;WaterRich","","F;G;K;M;L;T;D;H" );

        // Species without any known criteria (including non-terrestrial species)
        public static readonly OrganicSpecies SolidMineralSpheres = new( "SPOI", OrganicGenus.MineralSpheres, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies LatticeMineralSpheres = new( "SPOI_Ball", OrganicGenus.MineralSpheres, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies MetallicCrystals = new( "L_Cry_MetCry", OrganicGenus.MetallicCrystals, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies SilicateCrystals = new( "L_Cry_QtzCry", OrganicGenus.SilicateCrystals, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies IceCrystals = new( "L_Cry_IcCry", OrganicGenus.IceCrystals, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies ReelMollusc = new( "L_Org_Moll03_V6", OrganicGenus.MolluscReel, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies GlobeMollusc = new( "Small_Org_Moll01_V5", OrganicGenus.MolluscGlobe, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies BellMollusc = new( "Small_Org_Moll01_V6", OrganicGenus.MolluscBell, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies UmbrellaMollusc = new( "L_Org_Moll03_V3", OrganicGenus.MolluscUmbrella, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies GourdMollusc = new( "Small_Org_Moll01_V1", OrganicGenus.MolluscGourd, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies TorusMollusc = new( "Small_Org_Moll01_V2", OrganicGenus.MolluscTorus, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies BulbMollusc = new( "L_Org_Moll03_V2", OrganicGenus.MolluscBulb, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies ParasolMollusc = new( "L_Org_Moll03_V1", OrganicGenus.MolluscParasol, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies SquidMollusc = new( "Small_Org_Moll01_V3", OrganicGenus.MolluscSquid, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies BulletMollusc = new( "Small_Org_Moll01_V4", OrganicGenus.MolluscBullet, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies CapsuleMollusc = new( "L_Org_Moll03_V4", OrganicGenus.MolluscCapsule, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies CollaredPod = new( "S_Seed_SdTp04", OrganicGenus.CollaredPod, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies StolonPod = new( "SPOI_Root", OrganicGenus.StolonPod, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies StolonTree = new( "L_Seed_SdRt02", OrganicGenus.StolonTree, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies AsterPod = new( "S_Seed_SdTp02", OrganicGenus.AsterPod, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies ChalicePod = new( "S_Seed_SdTp05", OrganicGenus.ChalicePod, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies PedunclePod = new( "S_Seed_SdTp01", OrganicGenus.PedunclePod, 50000,null, null, null, "","","","" ); 
        public static readonly OrganicSpecies RhizomePod = new( "S_Seed_SdTp07", OrganicGenus.RhizomePod, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies QuadripartitePod = new( "S_Seed_SdTp08", OrganicGenus.QuadripartitePod, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies OctahedralPod = new( "S_Seed_SdTp03", OrganicGenus.VoidPod, 50000,null, null, null, "","","","" ); 
        public static readonly OrganicSpecies AsterTree = new( "L_Seed_Pln02_V3", OrganicGenus.AsterTree, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies PeduncleTree = new( "L_Seed_Pln01_V1", OrganicGenus.PeduncleTree, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies AurariumGyreTree = new( "SPOI_SeedPolyp01_V1", OrganicGenus.GyreTree, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies VirideGyreTree = new( "SPOI_SeedPolyp01", OrganicGenus.GyreTree, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies GyrePod = new( "S_Seed_SdTp06", OrganicGenus.GyrePod, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies ChryseumVoidHeart = new( "SPOI_SeedWeed01", OrganicGenus.VoidHeart, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies CalcitePlates = new( "L_Org_PltFun_V1", OrganicGenus.CalcitePlates, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies ThargoidBarnacle = new( "Thargoid_Barnacle", OrganicGenus.ThargoidBarnacle, 50000,null, null, null, "","","","" );
        public static readonly OrganicSpecies IngensradicesUnicus = new( "Ingensradices_Unicus", OrganicGenus.Ingensradices, 952296,null, null, null, "","","","" ); // Appears to be unique to HIP 87621.

        [PublicAPI( "The genus for this species" )]
        public OrganicGenus genus;

        [PublicAPI( "The credit value for this species" )]
        public long? value;
        
        public decimal? maxG;
        public decimal? minK;
        public decimal? maxK;
        public IList<string> planetClass = [ ];
        public IList<string> atmosphereClass = [ ];
        public IList<string> starClass = [ ];
        public IList<string> volcanism = [ ];

        [JsonIgnore, PublicAPI]
        public string localizedDescription => Properties.OrganicSpeciesDesc.ResourceManager.GetString( edname );

        [JsonIgnore, PublicAPI]
        public string localizedConditions => Properties.OrganicSpeciesCond.ResourceManager.GetString( edname );

        [JsonIgnore]
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
        { }

        private OrganicSpecies ( string edname,
            OrganicGenus genus,
            long? value,
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
            this.planetClass = !string.IsNullOrEmpty( planetClass ) ? planetClass.Split( ';' ).ToList() : [ ];
            this.atmosphereClass = !string.IsNullOrEmpty( atmosphereClass ) ? atmosphereClass.Split( ';' ).ToList() : [ ];
            this.starClass = !string.IsNullOrEmpty( starClass ) ? starClass.Split( ';' ).ToList() : [ ];
            this.volcanism = !string.IsNullOrEmpty( volcanism ) ? volcanism.Split( ';' ).ToList() : [ ];
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