using System.Collections.Generic;
using System.Linq;
using Utilities;
using Newtonsoft.Json;

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
        public static readonly OrganicSpecies AleoidaArcus = new OrganicSpecies( "Aleoids_01", OrganicGenus.Aleoids,0.3M, 175, 180, "","CarbonDioxide","None","B;A;F;K;M;L;T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaCoronamus = new OrganicSpecies( "Aleoids_02", OrganicGenus.Aleoids,0.3M, 180, 190, "","CarbonDioxide","None","B;A;F;K;M;L;T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaGravis = new OrganicSpecies( "Aleoids_05", OrganicGenus.Aleoids,0.3M, 190, 195, "","CarbonDioxide","None","B;A;F;K;M;L;T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaLaminiae = new OrganicSpecies( "Aleoids_04", OrganicGenus.Aleoids,0.3M, null, null, "","Ammonia","","B;A;F;K;M;L;T;TTS;Y;N" );
        public static readonly OrganicSpecies AleoidaSpica = new OrganicSpecies( "Aleoids_03", OrganicGenus.Aleoids,0.3M, null, null, "","Ammonia","","B;A;F;K;M;L;T;TTS;Y;N" );
        public static readonly OrganicSpecies AmphoraPlant = new OrganicSpecies( "Vents", OrganicGenus.Vents,null, 1000, null, "MetalRichBody","None","","A" );
        public static readonly OrganicSpecies BlatteumBioluminescentAnemone = new OrganicSpecies( "SphereEFGH", OrganicGenus.Sphere,null, 210, null, "MetalRichBody;HighMetalContentBody","Argon;CarbonDioxide;CarbonDioxideRich;HotSilicateVapour;None","","B" );
        public static readonly OrganicSpecies CroceumAnemone = new OrganicSpecies( "SphereABCD_01", OrganicGenus.Sphere,0.42M, 200, 440, "RockyBody","Water;SulphurDioxide;None","","B;A" );
        public static readonly OrganicSpecies LuteolumAnemone = new OrganicSpecies( "Sphere", OrganicGenus.Sphere,1.32M, 200, 440, "RockyBody","CarbonDioxide;Water;SulphurDioxide;None","","B" );
        public static readonly OrganicSpecies PrasinumBioluminescentAnemone = new OrganicSpecies( "SphereEFGH_02", OrganicGenus.Sphere,null, 20, null, "RockyBody;MetalRichBody;HighMetalContentBody","CarbonDioxide;Argon;Ammonia;Nitrogen;SulphurDioxide;NeonRich;HotSulphurDioxide;None","","O" );
        public static readonly OrganicSpecies PuniceumAnemone = new OrganicSpecies( "SphereABCD_02", OrganicGenus.Sphere,2.61M, 65, 860, "IceBody","Oxygen;Nitrogen;None","","O;W" );
        public static readonly OrganicSpecies RoseumAnemone = new OrganicSpecies( "SphereABCD_03", OrganicGenus.Sphere,0.45M, 200, 440, "RockyBody","SulphurDioxide;None","","B" );
        public static readonly OrganicSpecies RoseumBioluminescentAnemone = new OrganicSpecies( "SphereEFGH_03", OrganicGenus.Sphere,null, 190, null, "MetalRichBody;HighMetalContentBody","CarbonDioxide;SulphurDioxide;None","","B" );
        public static readonly OrganicSpecies RubeumBioluminescentAnemone = new OrganicSpecies( "SphereEFGH_01", OrganicGenus.Sphere,null, 160, null, "MetalRichBody;HighMetalContentBody","Argon;CarbonDioxide;SulphurDioxide;None","","B" );
        public static readonly OrganicSpecies BacteriumAcies = new OrganicSpecies( "Bacterial_04", OrganicGenus.Bacterial,0.75M, null, null, "IcyBody;RockyIceBody","Neon;NeonRich","","" );
        public static readonly OrganicSpecies BacteriumAlcyoneum = new OrganicSpecies( "Bacterial_06", OrganicGenus.Bacterial,0.38M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody","Ammonia","","" );
        public static readonly OrganicSpecies BacteriumAurasus = new OrganicSpecies( "Bacterial_01", OrganicGenus.Bacterial,1, null, null, "","CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies BacteriumBullaris = new OrganicSpecies( "Bacterial_10", OrganicGenus.Bacterial,0.61M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody","Methane;MethaneRich","","" );
        public static readonly OrganicSpecies BacteriumCerbrus = new OrganicSpecies( "Bacterial_12", OrganicGenus.Bacterial,1, null, null, "","Water;WaterRich;SulphurDioxide","","" );
        public static readonly OrganicSpecies BacteriumInformem = new OrganicSpecies( "Bacterial_08", OrganicGenus.Bacterial,0.6M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody","Nitrogen","","" );
        public static readonly OrganicSpecies BacteriumNebulus = new OrganicSpecies( "Bacterial_02", OrganicGenus.Bacterial,0.55M, null, null, "IcyBody","Helium","","" );
        public static readonly OrganicSpecies BacteriumOmentum = new OrganicSpecies( "Bacterial_11", OrganicGenus.Bacterial,0.61M, null, null, "IcyBody","Neon;NeonRich","Nitrogen;Ammonia","" );
        public static readonly OrganicSpecies BacteriumScopulum = new OrganicSpecies( "Bacterial_03", OrganicGenus.Bacterial,0.62M, null, null, "IcyBody;RockyIceBody","Neon;NeonRich","Carbon;Methane","" );
        public static readonly OrganicSpecies BacteriumTela = new OrganicSpecies( "Bacterial_07", OrganicGenus.Bacterial,0.62M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody","Any","Helium;Iron;Silicate","" );
        public static readonly OrganicSpecies BacteriumVerrata = new OrganicSpecies( "Bacterial_13", OrganicGenus.Bacterial,0.61M, null, null, "IcyBody;RockyBody;RockyIceBody","Neon;NeonRich","Water","" );
        public static readonly OrganicSpecies BacteriumVesicula = new OrganicSpecies( "Bacterial_05", OrganicGenus.Bacterial,1, null, null, "IcyBody;RockyBody;HighMetalContentBody;RockyIceBody","Argon;ArgonRich","","" );
        public static readonly OrganicSpecies BacteriumVolu = new OrganicSpecies( "Bacterial_09", OrganicGenus.Bacterial,0.61M, null, null, "IcyBody;RockyBody;HighMetalContentBody;RockyIceBody","Oxygen","","" );
        public static readonly OrganicSpecies BarkMounds = new OrganicSpecies( "Cone", OrganicGenus.Cone,null, 88, 440, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody","None;CarbonDioxide;CarbonDioxideRich;ArgonRich;SulphurDioxide;ThickArgonRich","","" );
        public static readonly OrganicSpecies AureumBrainTree = new OrganicSpecies( "SeedEFGH_01", OrganicGenus.Brancae,null, 300, 500, "MetalRichBody;HighMetalContentBody","None;SulphurDioxide","Any","" );
        public static readonly OrganicSpecies GypseeumBrainTree = new OrganicSpecies( "SeedABCD_01", OrganicGenus.Brancae,0.42M, 170, 330, "RockyBody","Ammonia;None;Oxygen;SulphurDioxide","Any","" );
        public static readonly OrganicSpecies LindigoticumBrainTree = new OrganicSpecies( "SeedEFGH_03", OrganicGenus.Brancae,null, 300, 500, "RockyBody;HighMetalContentBody","None","Any","" );
        public static readonly OrganicSpecies LividumBrainTree = new OrganicSpecies( "SeedEFGH", OrganicGenus.Brancae,0.48M, 300, 500, "RockyBody","None;Water;SulphurDioxide","Any","" );
        public static readonly OrganicSpecies OstrinumBrainTree = new OrganicSpecies( "SeedABCD_02", OrganicGenus.Brancae,null, 20, null, "MetalRichBody;HighMetalContentBody","None;CarbonDioxide;Ammonia;CarbonDioxideRich;ArgonRich;SulphurDioxide;Helium;NeonRich","Any","" );
        public static readonly OrganicSpecies PuniceumBrainTree = new OrganicSpecies( "SeedEFGH_02", OrganicGenus.Brancae,null, 20, null, "MetalRichBody;HighMetalContentBody","None;CarbonDioxide;Oxygen;SulphurDioxide;Helium;NeonRich","Any","" );
        public static readonly OrganicSpecies RoseumBrainTree = new OrganicSpecies( "Seed", OrganicGenus.Brancae,null, 115, 500, "RockyBody;MetalRichBody;HighMetalContentBody;RockyIceBody","None;CarbonDioxide;Argon;Ammonia;CarbonDioxideRich;Oxygen;Water;SulphurDioxide;ArgonRich;WaterRich","Any","" );
        public static readonly OrganicSpecies VirideBrainTree = new OrganicSpecies( "SeedABCD_03", OrganicGenus.Brancae,0.4M, 100, 255, "RockyIceBody","Ammonia;None;SulphurDioxide","Any","" );
        public static readonly OrganicSpecies CactoidaCortexum = new OrganicSpecies( "Cactoid_01", OrganicGenus.Cactoid,0.27M, 158, 196, "RockyBody;HighMetalContentBody","CarbonDioxide","None","F;G;A;L;K;N;B;M;H" );
        public static readonly OrganicSpecies CactoidaLapis = new OrganicSpecies( "Cactoid_02", OrganicGenus.Cactoid,0.28M, 160, 225, "RockyBody;HighMetalContentBody","Ammonia","","F;G;H;A;K;N;B;A" );
        public static readonly OrganicSpecies CactoidaPeperatis = new OrganicSpecies( "Cactoid_05", OrganicGenus.Cactoid,0.28M, 160, 186, "RockyBody;HighMetalContentBody","Ammonia","","F;G;A;K;N;B;H" );
        public static readonly OrganicSpecies CactoidaPullulanta = new OrganicSpecies( "Cactoid_04", OrganicGenus.Cactoid,0.27M, 127, 195, "RockyBody;HighMetalContentBody","CarbonDioxide","None","F;G;H;A;K;N;B" );
        public static readonly OrganicSpecies CactoidaVermis = new OrganicSpecies( "Cactoid_03", OrganicGenus.Cactoid,0.28M, 160, 450, "RockyBody;HighMetalContentBody","Water;SulphurDioxide","","F;G;H;A;M;N;B;K" );
        public static readonly OrganicSpecies ClypeusLacrimam = new OrganicSpecies( "Clypeus_01", OrganicGenus.Clypeus,0.28M, 190, null, "RockyBody;HighMetalContentBody","Water;CarbonDioxide","","A;F;G;K;M;L;N" );
        public static readonly OrganicSpecies ClypeusMargaritus = new OrganicSpecies( "Clypeus_02", OrganicGenus.Clypeus,0.28M, 190, null, "RockyBody;HighMetalContentBody","Water;CarbonDioxide","None","A;F;G;K;M;L;N" );
        public static readonly OrganicSpecies ClypeusSpeculumi = new OrganicSpecies( "Clypeus_03", OrganicGenus.Clypeus,0.28M, 190, null, "RockyBody;HighMetalContentBody","Water;CarbonDioxide","","A;F;G;K;M;L;N" );
        public static readonly OrganicSpecies ConchaAureolas = new OrganicSpecies( "Conchas_02", OrganicGenus.Conchas,0.28M, null, null, "","Ammonia","","" );
        public static readonly OrganicSpecies ConchaBiconcavis = new OrganicSpecies( "Conchas_04", OrganicGenus.Conchas,0.28M, null, null, "","Nitrogen","None","" );
        public static readonly OrganicSpecies ConchaLabiata = new OrganicSpecies( "Conchas_03", OrganicGenus.Conchas,0.28M, null, 190, "","CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies ConchaRenibus = new OrganicSpecies( "Conchas_01", OrganicGenus.Conchas,0.28M, 180, 195, "","Water;WaterRich","","" );
        public static readonly OrganicSpecies CrystallineShards = new OrganicSpecies( "Ground_Struct_Ice", OrganicGenus.Ground_Struct_Ice,2, null, 266, "IcyBody;HighMetalContentBody;RockyIceBody;RockyBody","None;CarbonDioxide;Argon;CarbonDioxideRich;Methane;ArgonRich;Neon;Helium;NeonRich","","A;F;G;K;M;S" );
        public static readonly OrganicSpecies ElectricaePluma = new OrganicSpecies( "Electricae_01", OrganicGenus.Electricae,0.28M, null, 150, "IcyBody","Neon;NeonRich;Argon;ArgonRich","","A;N" );
        public static readonly OrganicSpecies ElectricaeRadialem = new OrganicSpecies( "Electricae_02", OrganicGenus.Electricae,0.28M, null, 150, "IcyBody","Neon;NeonRich;Argon;ArgonRich;Methane","","" );
        public static readonly OrganicSpecies FonticuluaCampestris = new OrganicSpecies( "Fonticulus_02", OrganicGenus.Fonticulus,0.28M, null, 150, "IcyBody;RockyBody","Argon","","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaDigitos = new OrganicSpecies( "Fonticulus_06", OrganicGenus.Fonticulus,0.28M, null, null, "IcyBody;RockyBody","Methane;MethaneRich","","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaFluctus = new OrganicSpecies( "Fonticulus_05", OrganicGenus.Fonticulus,0.28M, null, null, "IcyBody;RockyBody","Oxygen","","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaLapida = new OrganicSpecies( "Fonticulus_04", OrganicGenus.Fonticulus,0.28M, null, null, "IcyBody;RockyBody","Nitrogen","","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaSegmentatus = new OrganicSpecies( "Fonticulus_01", OrganicGenus.Fonticulus,0.28M, null, null, "IcyBody;RockyBody","Neon;NeonRich","None","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FonticuluaUpupam = new OrganicSpecies( "Fonticulus_03", OrganicGenus.Fonticulus,0.28M, null, null, "IcyBody;RockyBody","ArgonRich","","B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" );
        public static readonly OrganicSpecies FrutexaAcus = new OrganicSpecies( "Shrubs_02", OrganicGenus.Shrubs,0.28M, null, 195, "RockyBody","CarbonDioxide;CarbonDioxideRich","","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaCollum = new OrganicSpecies( "Shrubs_07", OrganicGenus.Shrubs,0.28M, null, null, "RockyBody","SulphurDioxide","","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaFera = new OrganicSpecies( "Shrubs_05", OrganicGenus.Shrubs,0.28M, null, 195, "RockyBody","CarbonDioxide;CarbonDioxideRich","None","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaFlabellum = new OrganicSpecies( "Shrubs_01", OrganicGenus.Shrubs,0.28M, null, null, "RockyBody","Ammonia","","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaFlammasis = new OrganicSpecies( "Shrubs_04", OrganicGenus.Shrubs,0.28M, null, null, "RockyBody","Ammonia","","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaMetallicum = new OrganicSpecies( "Shrubs_03", OrganicGenus.Shrubs,0.28M, null, 195, "HighMetalContentBody","CarbonDioxide;CarbonDioxideRich;Ammonia","None","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FrutexaSponsae = new OrganicSpecies( "Shrubs_06", OrganicGenus.Shrubs,0.28M, null, null, "RockyBody","Water;WaterRich","","B;F;G;M;L;TTS;D;N" );
        public static readonly OrganicSpecies FumerolaAquatis = new OrganicSpecies( "Fumerolas_04", OrganicGenus.Fumerolas,0.28M, null, 450, "IcyBody;RockyIceBody","Any","Water","" );
        public static readonly OrganicSpecies FumerolaCarbosis = new OrganicSpecies( "Fumerolas_01", OrganicGenus.Fumerolas,0.28M, null, 275, "IcyBody;RockyIceBody","Any","Carbon;Methane","" );
        public static readonly OrganicSpecies FumerolaExtremus = new OrganicSpecies( "Fumerolas_02", OrganicGenus.Fumerolas,0.28M, null, 205, "RockyBody;HighMetalContentBody","Any","Silicate;Iron;Rocky","" );
        public static readonly OrganicSpecies FumerolaNitris = new OrganicSpecies( "Fumerolas_03", OrganicGenus.Fumerolas,0.28M, null, 250, "IcyBody;RockyIceBody","Any","Nitrogen;Ammonia","" );
        public static readonly OrganicSpecies FungoidaBullarum = new OrganicSpecies( "Fungoids_03", OrganicGenus.Fungoids,0.28M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody","Argon;ArgonRich","None","" );
        public static readonly OrganicSpecies FungoidaGelata = new OrganicSpecies( "Fungoids_04", OrganicGenus.Fungoids,0.28M, 180, 195, "RockyBody;HighMetalContentBody;RockyIceBody","Water;WaterRich;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies FungoidaSetisis = new OrganicSpecies( "Fungoids_01", OrganicGenus.Fungoids,0.28M, null, null, "RockyBody;HighMetalContentBody;RockyIceBody","Ammonia;Methane;MethaneRich","","" );
        public static readonly OrganicSpecies FungoidaStabitis = new OrganicSpecies( "Fungoids_02", OrganicGenus.Fungoids,0.28M, 180, 195, "RockyBody;HighMetalContentBody;RockyIceBody","Water;WaterRich;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies OsseusCornibus = new OrganicSpecies( "Osseus_05", OrganicGenus.Osseus,0.28M, 180, 195, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","" );
        public static readonly OrganicSpecies OsseusDiscus = new OrganicSpecies( "Osseus_02", OrganicGenus.Osseus,0.28M, null, 455, "RockyBody;HighMetalContentBody","Water;WaterRich","","" );
        public static readonly OrganicSpecies OsseusFractus = new OrganicSpecies( "Osseus_01", OrganicGenus.Osseus,0.28M, 180, 190, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","" );
        public static readonly OrganicSpecies OsseusPellebantus = new OrganicSpecies( "Osseus_06", OrganicGenus.Osseus,0.28M, 190, 195, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","" );
        public static readonly OrganicSpecies OsseusPumice = new OrganicSpecies( "Osseus_04", OrganicGenus.Osseus,0.28M, null, 135, "RockyBody;HighMetalContentBody;RockyIceBody","Argon;ArgonRich;Methane;MethaneRich;Nitrogen","","" );
        public static readonly OrganicSpecies OsseusSpiralis = new OrganicSpecies( "Osseus_03", OrganicGenus.Osseus,0.28M, 160, null, "RockyBody;HighMetalContentBody","Ammonia","","" );
        public static readonly OrganicSpecies ReceptaConditivus = new OrganicSpecies( "Recepta_03", OrganicGenus.Recepta,0.28M, 130, 300, "IcyBody;RockyIceBody","SulphurDioxide","","" );
        public static readonly OrganicSpecies ReceptaDeltahedronix = new OrganicSpecies( "Recepta_02", OrganicGenus.Recepta,0.28M, 130, 300, "RockyBody;HighMetalContentBody","SulphurDioxide","","" );
        public static readonly OrganicSpecies ReceptaUmbrux = new OrganicSpecies( "Recepta_01", OrganicGenus.Recepta,0.28M, 130, 300, "IcyBody;RockyIceBody;RockyBody;HighMetalContentBody","SulphurDioxide","","" );
        public static readonly OrganicSpecies AlbidumSinuousTubers = new OrganicSpecies( "TubeABCD_02", OrganicGenus.Tubers,null, 200, 500, "RockyBody;HighMetalContentBody","None","Any","" );
        public static readonly OrganicSpecies BlatteumSinuousTubers = new OrganicSpecies( "TubeEFGH", OrganicGenus.Tubers,null, 200, 500, "RockyBody;HighMetalContentBody","SulphurDioxide;None","Any","" );
        public static readonly OrganicSpecies CaeruleumSinuousTubers = new OrganicSpecies( "TubeABCD_03", OrganicGenus.Tubers,null, 200, 500, "RockyBody;HighMetalContentBody","SulphurDioxide;None","Any","" );
        public static readonly OrganicSpecies LindigoticumSinuousTubers = new OrganicSpecies( "TubeEFGH_01", OrganicGenus.Tubers,null, 200, 500, "RockyBody;HighMetalContentBody","None","Any","" );
        public static readonly OrganicSpecies PrasinumSinuousTubers = new OrganicSpecies( "TubeABCD_01", OrganicGenus.Tubers,null, 200, 500, "RockyBody;HighMetalContentBody;RockyIceBody","CarbonDioxideRich;None;CarbonDioxide;SulphurDioxide","Any","" );
        public static readonly OrganicSpecies RoseumSinuousTubers = new OrganicSpecies( "Tube", OrganicGenus.Tubers,null, 200, 500, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich;ArgonRich;SulphurDioxide;None","Any","" );
        public static readonly OrganicSpecies ViolaceumSinuousTubers = new OrganicSpecies( "TubeEFGH_02", OrganicGenus.Tubers,null, 200, 500, "RockyBody;HighMetalContentBody","None","Any","" );
        public static readonly OrganicSpecies VirideSinuousTubers = new OrganicSpecies( "TubeEFGH_03", OrganicGenus.Tubers,null, 200, 500, "RockyBody;HighMetalContentBody","SulphurDioxide;None","Any","" );
        public static readonly OrganicSpecies StratumAraneamus = new OrganicSpecies( "Stratum_04", OrganicGenus.Stratum,0.55M, 165, null, "RockyBody","SulphurDioxide","","" );
        public static readonly OrganicSpecies StratumCucumisis = new OrganicSpecies( "Stratum_06", OrganicGenus.Stratum,0.6M, 190, null, "RockyBody","SulphurDioxide;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies StratumExcutitus = new OrganicSpecies( "Stratum_01", OrganicGenus.Stratum,0.48M, 165, 190, "RockyBody","SulphurDioxide;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies StratumFrigus = new OrganicSpecies( "Stratum_08", OrganicGenus.Stratum,0.55M, 190, null, "RockyBody","SulphurDioxide;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies StratumLaminamus = new OrganicSpecies( "Stratum_03", OrganicGenus.Stratum,0.34M, 165, null, "RockyBody","Ammonia","","" );
        public static readonly OrganicSpecies StratumLimaxus = new OrganicSpecies( "Stratum_05", OrganicGenus.Stratum,0.48M, 165, 190, "RockyBody","SulphurDioxide;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies StratumPaleas = new OrganicSpecies( "Stratum_02", OrganicGenus.Stratum,0.58M, 165, null, "RockyBody","Ammonia;Water;WaterRich;CarbonDioxide;CarbonDioxideRich","","" );
        public static readonly OrganicSpecies StratumTectonicas = new OrganicSpecies( "Stratum_07", OrganicGenus.Stratum,0.9M, 165, null, "HighMetalContentBody","Oxygen;Ammonia;Water;WaterRich;CarbonDioxide;CarbonDioxideRich;SulphurDioxide","","" );
        public static readonly OrganicSpecies TubusCavas = new OrganicSpecies( "Tubus_03", OrganicGenus.Tubus,0.16M, 160, 200, "RockyBody","CarbonDioxide","None","F;G;H;A;K;N;M;B" );
        public static readonly OrganicSpecies TubusCompagibus = new OrganicSpecies( "Tubus_05", OrganicGenus.Tubus,0.19M, 150, 190, "RockyBody","CarbonDioxide","None","S;A;K;M;N;M;DC;H;K" );
        public static readonly OrganicSpecies TubusConifer = new OrganicSpecies( "Tubus_01", OrganicGenus.Tubus,0.17M, 160, 200, "RockyBody","CarbonDioxide","None","F;G;A;K;N;M;H" );
        public static readonly OrganicSpecies TubusRosarium = new OrganicSpecies( "Tubus_04", OrganicGenus.Tubus,0.16M, 160, 180, "RockyBody","Ammonia","","F;G;A;K;N;B;K" );
        public static readonly OrganicSpecies TubusSororibus = new OrganicSpecies( "Tubus_02", OrganicGenus.Tubus,0.16M, 160, 200, "HighMetalContentBody","Ammonia;CarbonDioxide","None","F;G;A;L;K;N;M;M;DC" );
        public static readonly OrganicSpecies TussockAlbata = new OrganicSpecies( "Tussocks_08", OrganicGenus.Tussocks,0.28M, 175, 180, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCapillum = new OrganicSpecies( "Tussocks_15", OrganicGenus.Tussocks,0.28M, 80, 165, "RockyBody;RockyIceBody","Argon;ArgonRich;Methane;MethaneRich","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCaputus = new OrganicSpecies( "Tussocks_11", OrganicGenus.Tussocks,0.28M, 180, 190, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCatena = new OrganicSpecies( "Tussocks_05", OrganicGenus.Tussocks,0.28M, 150, 190, "RockyBody;HighMetalContentBody","Ammonia","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockCultro = new OrganicSpecies( "Tussocks_04", OrganicGenus.Tussocks,0.28M, null, null, "RockyBody;HighMetalContentBody","Ammonia","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockDivisa = new OrganicSpecies( "Tussocks_10", OrganicGenus.Tussocks,0.28M, 150, 180, "RockyBody;HighMetalContentBody","Ammonia","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockIgnis = new OrganicSpecies( "Tussocks_03", OrganicGenus.Tussocks,0.28M, 160, 170, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockPennata = new OrganicSpecies( "Tussocks_01", OrganicGenus.Tussocks,0.28M, 145, 155, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockPennatis = new OrganicSpecies( "Tussocks_06", OrganicGenus.Tussocks,0.28M, null, 195, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockPropagito = new OrganicSpecies( "Tussocks_09", OrganicGenus.Tussocks,0.28M, null, 195, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockSerrati = new OrganicSpecies( "Tussocks_07", OrganicGenus.Tussocks,0.28M, 170, 175, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockStigmasis = new OrganicSpecies( "Tussocks_13", OrganicGenus.Tussocks,0.28M, 130, 210, "RockyBody;HighMetalContentBody","SulphurDioxide","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockTriticum = new OrganicSpecies( "Tussocks_12", OrganicGenus.Tussocks,0.28M, 190, 195, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","None","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockVentusa = new OrganicSpecies( "Tussocks_02", OrganicGenus.Tussocks,0.28M, 155, 160, "RockyBody;HighMetalContentBody","CarbonDioxide;CarbonDioxideRich","","F;G;K;M;L;T;D;H" );
        public static readonly OrganicSpecies TussockVirgam = new OrganicSpecies( "Tussocks_14", OrganicGenus.Tussocks,0.28M, 390, 450, "RockyBody;HighMetalContentBody","Water;WaterRich","","F;G;K;M;L;T;D;H" );

        // Species without any known criteria (including non-terrestrial species)
        public static readonly OrganicSpecies SolidMineralSpheres = new OrganicSpecies( "SPOI", OrganicGenus.MineralSpheres,null, null, null, "","","","" );
        public static readonly OrganicSpecies LatticeMineralSpheres = new OrganicSpecies( "SPOI_Ball", OrganicGenus.MineralSpheres,null, null, null, "","","","" );
        public static readonly OrganicSpecies MetallicCrystals = new OrganicSpecies( "L_Cry_MetCry", OrganicGenus.MetallicCrystals,null, null, null, "","","","" );
        public static readonly OrganicSpecies SilicateCrystals = new OrganicSpecies( "L_Cry_QtzCry", OrganicGenus.SilicateCrystals,null, null, null, "","","","" );
        public static readonly OrganicSpecies IceCrystals = new OrganicSpecies( "L_Cry_IcCry", OrganicGenus.IceCrystals,null, null, null, "","","","" );
        public static readonly OrganicSpecies ReelMollusc = new OrganicSpecies( "L_Org_Moll03_V6", OrganicGenus.MolluscReel,null, null, null, "","","","" );
        public static readonly OrganicSpecies GlobeMollusc = new OrganicSpecies( "Small_Org_Moll01_V5", OrganicGenus.MolluscGlobe,null, null, null, "","","","" );
        public static readonly OrganicSpecies BellMollusc = new OrganicSpecies( "Small_Org_Moll01_V6", OrganicGenus.MolluscBell,null, null, null, "","","","" );
        public static readonly OrganicSpecies UmbrellaMollusc = new OrganicSpecies( "L_Org_Moll03_V3", OrganicGenus.MolluscUmbrella,null, null, null, "","","","" );
        public static readonly OrganicSpecies GourdMollusc = new OrganicSpecies( "Small_Org_Moll01_V1", OrganicGenus.MolluscGourd,null, null, null, "","","","" );
        public static readonly OrganicSpecies TorusMollusc = new OrganicSpecies( "Small_Org_Moll01_V2", OrganicGenus.MolluscTorus,null, null, null, "","","","" );
        public static readonly OrganicSpecies BulbMollusc = new OrganicSpecies( "L_Org_Moll03_V2", OrganicGenus.MolluscBulb,null, null, null, "","","","" );
        public static readonly OrganicSpecies ParasolMollusc = new OrganicSpecies( "L_Org_Moll03_V1", OrganicGenus.MolluscParasol,null, null, null, "","","","" );
        public static readonly OrganicSpecies SquidMollusc = new OrganicSpecies( "Small_Org_Moll01_V3", OrganicGenus.MolluscSquid,null, null, null, "","","","" );
        public static readonly OrganicSpecies BulletMollusc = new OrganicSpecies( "Small_Org_Moll01_V4", OrganicGenus.MolluscBullet,null, null, null, "","","","" );
        public static readonly OrganicSpecies CapsuleMollusc = new OrganicSpecies( "L_Org_Moll03_V4", OrganicGenus.MolluscCapsule,null, null, null, "","","","" );
        public static readonly OrganicSpecies CollaredPod = new OrganicSpecies( "S_Seed_SdTp04", OrganicGenus.CollaredPod,null, null, null, "","","","" );
        public static readonly OrganicSpecies StolonPod = new OrganicSpecies( "SPOI_Root", OrganicGenus.StolonPod,null, null, null, "","","","" );
        public static readonly OrganicSpecies StolonTree = new OrganicSpecies( "L_Seed_SdRt02", OrganicGenus.StolonTree,null, null, null, "","","","" );
        public static readonly OrganicSpecies AsterPod = new OrganicSpecies( "S_Seed_SdTp02", OrganicGenus.AsterPod,null, null, null, "","","","" );
        public static readonly OrganicSpecies ChalicePod = new OrganicSpecies( "S_Seed_SdTp05", OrganicGenus.ChalicePod,null, null, null, "","","","" );
        public static readonly OrganicSpecies PedunclePod = new OrganicSpecies( "S_Seed_SdTp01", OrganicGenus.PedunclePod,null, null, null, "","","","" ); 
        public static readonly OrganicSpecies RhizomePod = new OrganicSpecies( "S_Seed_SdTp07", OrganicGenus.RhizomePod,null, null, null, "","","","" );
        public static readonly OrganicSpecies QuadripartitePod = new OrganicSpecies( "S_Seed_SdTp08", OrganicGenus.QuadripartitePod,null, null, null, "","","","" );
        public static readonly OrganicSpecies OctahedralPod = new OrganicSpecies( "S_Seed_SdTp03", OrganicGenus.VoidPod,null, null, null, "","","","" ); 
        public static readonly OrganicSpecies AsterTree = new OrganicSpecies( "L_Seed_Pln02_V3", OrganicGenus.AsterTree,null, null, null, "","","","" );
        public static readonly OrganicSpecies PeduncleTree = new OrganicSpecies( "L_Seed_Pln01_V1", OrganicGenus.PeduncleTree,null, null, null, "","","","" );
        public static readonly OrganicSpecies AurariumGyreTree = new OrganicSpecies( "SPOI_SeedPolyp01_V1", OrganicGenus.GyreTree,null, null, null, "","","","" );
        public static readonly OrganicSpecies VirideGyreTree = new OrganicSpecies( "SPOI_SeedPolyp01", OrganicGenus.GyreTree,null, null, null, "","","","" );
        public static readonly OrganicSpecies GyrePod = new OrganicSpecies( "S_Seed_SdTp06", OrganicGenus.GyrePod,null, null, null, "","","","" );
        public static readonly OrganicSpecies ChryseumVoidHeart = new OrganicSpecies( "SPOI_SeedWeed01", OrganicGenus.VoidHeart,null, null, null, "","","","" );
        public static readonly OrganicSpecies CalcitePlates = new OrganicSpecies( "L_Org_PltFun_V1", OrganicGenus.CalcitePlates,null, null, null, "","","","" );
        public static readonly OrganicSpecies ThargoidBarnacle = new OrganicSpecies( "Thargoid_Barnacle", OrganicGenus.ThargoidBarnacle,null, null, null, "","","","" );
        public static readonly OrganicSpecies IngensradicesUnicus = new OrganicSpecies( "Ingensradices_Unicus", OrganicGenus.Ingensradices,null, null, null, "","","","" ); // Appears to be unique to HIP 87621.

        public OrganicGenus genus;

        public decimal? maxG;
        public decimal? minK;
        public decimal? maxK;
        public IList<string> planetClass;
        public IList<string> atmosphereClass;
        public IList<string> starClass;
        public IList<string> volcanism;

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
        {
            this.planetClass = new List<string>();
            this.atmosphereClass = new List<string>();
            this.starClass = new List<string>();
            this.volcanism = new List<string>();
        }

        private OrganicSpecies ( string edname,
                                 OrganicGenus genus,
                                 decimal? maxG,
                                 decimal? minK,
                                 decimal? maxK,
                                 string planetClass,
                                 string atmosphereClass,
                                 string volcanism,
                                 string starClass ) : base( edname, NormalizeSpecies( edname ) )
        {
            this.genus = genus;
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