using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{
    public class OrganicSpecies : ResourceBasedLocalizedEDName<OrganicSpecies>
    {
        public static readonly IDictionary<string, OrganicSpecies> SPECIES = new Dictionary<string, OrganicSpecies>();

        static OrganicSpecies ()
        {
            resourceManager = Properties.OrganicSpecies.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( species ) => new OrganicSpecies( species );

            SPECIES.Add( "AleoidaArcus", new OrganicSpecies( "AleoidaArcus", "Aleoids", (int)7252500, (decimal?)0.28, (decimal?)175, "<k<", (decimal?)180, "", "CarbonDioxide", "None", "B;A;F;K;M;L:T;TTS;Y;N" ) );
            SPECIES.Add( "AleoidaCoronamus", new OrganicSpecies( "AleoidaCoronamus", "Aleoids", (int)6284600, (decimal?)0.28, (decimal?)180, "<k<", (decimal?)190, "", "CarbonDioxide", "None", "B;A;F;K;M;L:T;TTS;Y;N" ) );
            SPECIES.Add( "AleoidaGravis", new OrganicSpecies( "AleoidaGravis", "Aleoids", (int)12934900, (decimal?)0.28, (decimal?)190, "<k<", (decimal?)195, "", "CarbonDioxide", "None", "B;A;F;K;M;L:T;TTS;Y;N" ) );
            SPECIES.Add( "AleoidaLaminiae", new OrganicSpecies( "AleoidaLaminiae", "Aleoids", (int)3385200, (decimal?)0.28, null, "", null, "", "Ammonia", "", "B;A;F;K;M;L:T;TTS;Y;N" ) );
            SPECIES.Add( "AleoidaSpica", new OrganicSpecies( "AleoidaSpica", "Aleoids", (int)3385200, (decimal?)0.28, null, "", null, "", "Ammonia", "", "B;A;F;K;M;L:T;TTS;Y;N" ) );
            SPECIES.Add( "AmphoraPlant", new OrganicSpecies( "AmphoraPlant", "Vents", (int)1628800, null, (decimal?)1000, "<k", null, "MetalRichBody", "None", "", "A" ) );
            SPECIES.Add( "BlatteumBioluminescentAnemone", new OrganicSpecies( "BlatteumBioluminescentAnemone", "Sphere", (int)1499900, null, null, "", null, "MetalRichBody;HighMetalContentBody", "SulphurDioxide;None", "", "B" ) );
            SPECIES.Add( "CroceumAnemone", new OrganicSpecies( "CroceumAnemone", "Sphere", (int)1499900, null, null, "", null, "RockyBody", "SulphurDioxide;None", "", "B;A" ) );
            SPECIES.Add( "LuteolumAnemone", new OrganicSpecies( "LuteolumAnemone", "Sphere", (int)1499900, null, null, "", null, "RockyBody", "SulphurDioxide;None", "", "B" ) );
            SPECIES.Add( "PrasinumBioluminescentAnemone", new OrganicSpecies( "PrasinumBioluminescentAnemone", "Sphere", (int)1499900, null, null, "", null, "RockyBody;MetalRichBody;HighMetalContentBody", "SulphurDioxide;None", "", "O" ) );
            SPECIES.Add( "PuniceumAnemone", new OrganicSpecies( "PuniceumAnemone", "Sphere", (int)1499900, null, null, "", null, "RockyIceBody;IceBody", "SulphurDioxide;None", "", "O" ) );
            SPECIES.Add( "RoseumAnemone", new OrganicSpecies( "RoseumAnemone", "Sphere", (int)1499900, null, null, "", null, "RockyBody", "SulphurDioxide;None", "", "B" ) );
            SPECIES.Add( "RoseumBioluminescentAnemone", new OrganicSpecies( "RoseumBioluminescentAnemone", "Sphere", (int)1499900, null, null, "", null, "MetalRichBody;HighMetalContentBody", "SulphurDioxide;None", "", "B" ) );
            SPECIES.Add( "RubeumBioluminescentAnemone", new OrganicSpecies( "RubeumBioluminescentAnemone", "Sphere", (int)1499900, null, null, "", null, "MetalRichBody;HighMetalContentBody", "SulphurDioxide;None", "", "B" ) );
            SPECIES.Add( "BacteriumAcies", new OrganicSpecies( "BacteriumAcies", "Bacterial", (int)1000000, (decimal?)0.75, null, "", null, "IcyBody;RockyIceBody", "Neon;NeonRich", "", "" ) );
            SPECIES.Add( "BacteriumAlcyoneum", new OrganicSpecies( "BacteriumAlcyoneum", "Bacterial", (int)1658500, (decimal?)0.38, null, "", null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody", "Ammonia", "", "" ) );
            SPECIES.Add( "BacteriumAurasus", new OrganicSpecies( "BacteriumAurasus", "Bacterial", (int)1000000, (decimal?)1, null, "", null, "", "CarbonDioxide;CarbonDioxideRich", "", "" ) );
            SPECIES.Add( "BacteriumBullaris", new OrganicSpecies( "BacteriumBullaris", "Bacterial", (int)1152500, (decimal?)0.61, null, "", null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody", "Methane;MethaneRich", "", "" ) );
            SPECIES.Add( "BacteriumCerbrus", new OrganicSpecies( "BacteriumCerbrus", "Bacterial", (int)1689800, (decimal?)1, null, "", null, "", "Water;WaterRich;SulphurDioxide", "", "" ) );
            SPECIES.Add( "BacteriumInformem", new OrganicSpecies( "BacteriumInformem", "Bacterial", (int)8418000, (decimal?)0.6, null, "", null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody", "Nitrogen", "", "" ) );
            SPECIES.Add( "BacteriumNebulus", new OrganicSpecies( "BacteriumNebulus", "Bacterial", (int)5289900, (decimal?)0.55, null, "", null, "IcyBody", "Helium", "", "" ) );
            SPECIES.Add( "BacteriumOmentum", new OrganicSpecies( "BacteriumOmentum", "Bacterial", (int)4638900, (decimal?)0.61, null, "", null, "IcyBody", "Neon;NeonRich", "Nitrogen;Ammonia", "" ) );
            SPECIES.Add( "BacteriumScopulum", new OrganicSpecies( "BacteriumScopulum", "Bacterial", (int)4934500, (decimal?)0.62, null, "", null, "IcyBody;RockyIceBody", "Neon;NeonRich", "Carbon;Methane", "" ) );
            SPECIES.Add( "BacteriumTela", new OrganicSpecies( "BacteriumTela", "Bacterial", (int)1949000, (decimal?)0.62, null, "", null, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody", "Any", "Helium;Iron;Silicate", "" ) );
            SPECIES.Add( "BacteriumVerrata", new OrganicSpecies( "BacteriumVerrata", "Bacterial", (int)3897000, (decimal?)0.61, null, "", null, "IcyBody;RockyBody;RockyIceBody", "Neon;NeonRich", "Water", "" ) );
            SPECIES.Add( "BacteriumVesicula", new OrganicSpecies( "BacteriumVesicula", "Bacterial", (int)1000000, (decimal?)1, null, "", null, "IcyBody;RockyBody;HighMetalContentBody;RockyIceBody", "Argon;ArgonRich", "", "" ) );
            SPECIES.Add( "BacteriumVolu", new OrganicSpecies( "BacteriumVolu", "Bacterial", (int)7774700, (decimal?)0.61, null, "", null, "IcyBody;RockyBody;HighMetalContentBody;RockyIceBody", "Oxygen", "", "" ) );
            SPECIES.Add( "BarkMounds", new OrganicSpecies( "BarkMounds", "Cone", (int)1471900, null, (decimal?)80, "<k<", (decimal?)450, "RockyBody;HighMetalContentBody;RockyIceBody;IcyBody", "None;CarbonDioxide;CarbonDioxideRich;ArgonRich;SulphurDioxide;ThickArgonRich", "", "" ) );
            SPECIES.Add( "AureumBrainTree", new OrganicSpecies( "AureumBrainTree", "Brancae", (int)1593700, null, (decimal?)300, "<k<", (decimal?)500, "MetalRichBody;HighMetalContentBody", "None;SulphurDioxide", "Any", "" ) );
            SPECIES.Add( "GypseeumBrainTree", new OrganicSpecies( "GypseeumBrainTree", "Brancae", (int)1593700, null, (decimal?)200, "<k<", (decimal?)300, "RockyBody", "Ammonia;None;Oxygen;SulphurDioxide", "Any", "" ) );
            SPECIES.Add( "LindigoticumBrainTree", new OrganicSpecies( "LindigoticumBrainTree", "Brancae", (int)1593700, null, (decimal?)300, "<k<", (decimal?)500, "RockyBody;HighMetalContentBody", "None", "Any", "" ) );
            SPECIES.Add( "LividumBrainTree", new OrganicSpecies( "LividumBrainTree", "Brancae", (int)1593700, null, (decimal?)300, "<k<", (decimal?)500, "RockyBody", "None;Water;SulphurDioxide", "Any", "" ) );
            SPECIES.Add( "OstrinumBrainTree", new OrganicSpecies( "OstrinumBrainTree", "Brancae", (int)1593700, null, null, "", null, "MetalRichBody;HighMetalContentBody", "None;CarbonDioxide;Ammonia;CarbonDioxideRich;ArgonRich;SulphurDioxide;Helium;NeonRich", "Any", "" ) );
            SPECIES.Add( "PuniceumBrainTree", new OrganicSpecies( "PuniceumBrainTree", "Brancae", (int)1593700, null, null, "", null, "MetalRichBody;HighMetalContentBody", "None;CarbonDioxide;Oxygen;SulphurDioxide;Helium;NeonRich", "Any", "" ) );
            SPECIES.Add( "RoseumBrainTree", new OrganicSpecies( "RoseumBrainTree", "Brancae", (int)1593700, null, (decimal?)200, "<k<", (decimal?)500, "RockyBody;MetalRichBody;HighMetalContentBody;RockyIceBody", "None;CarbonDioxide;Argon;Ammonia;CarbonDioxideRich;Oxygen;Water;SulphurDioxide;ArgonRich;WaterRich", "Any", "" ) );
            SPECIES.Add( "VirideBrainTree", new OrganicSpecies( "VirideBrainTree", "Brancae", (int)1593700, null, (decimal?)100, "<k<", (decimal?)270, "RockyIceBody", "Ammonia;None;SulphurDioxide", "Any", "" ) );
            SPECIES.Add( "CactoidaCortexum", new OrganicSpecies( "CactoidaCortexum", "Cactoid", (int)3667600, (decimal?)0.28, (decimal?)180, "<k<", (decimal?)195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "A;F;G;M;L;T;TTS;N" ) );
            SPECIES.Add( "CactoidaLapis", new OrganicSpecies( "CactoidaLapis", "Cactoid", (int)2483600, (decimal?)0.28, (decimal?)160, "<k<", (decimal?)225, "RockyBody;HighMetalContentBody", "Ammonia", "", "A;F;G;M;L;T;TTS;N" ) );
            SPECIES.Add( "CactoidaPeperatis", new OrganicSpecies( "CactoidaPeperatis", "Cactoid", (int)2483600, (decimal?)0.28, (decimal?)160, "<k<", (decimal?)190, "RockyBody;HighMetalContentBody", "Ammonia", "", "A;F;G;M;L;T;TTS;N" ) );
            SPECIES.Add( "CactoidaPullulanta", new OrganicSpecies( "CactoidaPullulanta", "Cactoid", (int)3667600, (decimal?)0.28, (decimal?)180, "<k<", (decimal?)195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "A;F;G;M;L;T;TTS;N" ) );
            SPECIES.Add( "CactoidaVermis", new OrganicSpecies( "CactoidaVermis", "Cactoid", (int)16202800, (decimal?)0.28, (decimal?)160, "<k<", (decimal?)450, "RockyBody;HighMetalContentBody", "Water", "", "A;F;G;M;L;T;TTS;N" ) );
            SPECIES.Add( "ClypeusLacrimam", new OrganicSpecies( "ClypeusLacrimam", "Clypeus", (int)8418000, (decimal?)0.28, (decimal?)190, "<k", null, "RockyBody;HighMetalContentBody", "Water;CarbonDioxide", "", "A;F;G;K;M;L;N" ) );
            SPECIES.Add( "ClypeusMargaritus", new OrganicSpecies( "ClypeusMargaritus", "Clypeus", (int)11873200, (decimal?)0.28, (decimal?)190, "<k", null, "RockyBody;HighMetalContentBody", "Water;CarbonDioxide", "None", "A;F;G;K;M;L;N" ) );
            SPECIES.Add( "ClypeusSpeculumi", new OrganicSpecies( "ClypeusSpeculumi", "Clypeus", (int)16202800, (decimal?)0.28, (decimal?)190, "<k", null, "RockyBody;HighMetalContentBody", "Water;CarbonDioxide", "", "A;F;G;K;M;L;N" ) );
            SPECIES.Add( "ConchaAureolas", new OrganicSpecies( "ConchaAureolas", "Conchas", (int)7774700, (decimal?)0.28, null, "", null, "", "Ammonia", "", "" ) );
            SPECIES.Add( "ConchaBiconcavis", new OrganicSpecies( "ConchaBiconcavis", "Conchas", (int)19010800, (decimal?)0.28, null, "", null, "", "Nitrogen", "None", "" ) );
            SPECIES.Add( "ConchaLabiata", new OrganicSpecies( "ConchaLabiata", "Conchas", (int)2352400, (decimal?)0.28, null, "k<", (decimal?)190, "", "CarbonDioxide;CarbonDioxideRich", "", "" ) );
            SPECIES.Add( "ConchaRenibus", new OrganicSpecies( "ConchaRenibus", "Conchas", (int)4572400, (decimal?)0.28, (decimal?)180, "<k<", (decimal?)195, "", "Water;WaterRich", "", "" ) );
            SPECIES.Add( "CrystallineShards", new OrganicSpecies( "CrystallineShards", "GroundStructIce", (int)1628800, null, null, "k<", (decimal?)273, "", "None;CarbonDioxide;Argon;CarbonDioxideRich;Methane;ArgonRich;Neon;Helium;NeonRich", "", "A;F;G;K;M;S" ) );
            SPECIES.Add( "ElectricaePluma", new OrganicSpecies( "ElectricaePluma", "Electricae", (int)6284600, (decimal?)0.28, null, "", null, "IcyBody", "Helium;Neon;Argon", "", "A;N" ) );
            SPECIES.Add( "ElectricaeRadialem", new OrganicSpecies( "ElectricaeRadialem", "Electricae", (int)6284600, (decimal?)0.28, null, "k<", (decimal?)150, "IcyBody", "Helium;Neon;Argon", "", "" ) );
            SPECIES.Add( "FonticuluaCampestris", new OrganicSpecies( "FonticuluaCampestris", "Fonticulus", (int)1000000, (decimal?)0.28, null, "k<", (decimal?)150, "IcyBody;RockyBody", "Argon", "", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" ) );
            SPECIES.Add( "FonticuluaDigitos", new OrganicSpecies( "FonticuluaDigitos", "Fonticulus", (int)1804100, (decimal?)0.28, null, "", null, "IcyBody;RockyBody", "Methane;MethaneRich", "", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" ) );
            SPECIES.Add( "FonticuluaFluctus", new OrganicSpecies( "FonticuluaFluctus", "Fonticulus", (int)20000000, (decimal?)0.28, null, "", null, "IcyBody;RockyBody", "Oxygen", "", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" ) );
            SPECIES.Add( "FonticuluaLapida", new OrganicSpecies( "FonticuluaLapida", "Fonticulus", (int)3111000, (decimal?)0.28, null, "", null, "IcyBody;RockyBody", "Nitrogen", "", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" ) );
            SPECIES.Add( "FonticuluaSegmentatus", new OrganicSpecies( "FonticuluaSegmentatus", "Fonticulus", (int)19010800, (decimal?)0.28, null, "", null, "IcyBody;RockyBody", "Neon;NeonRich", "None", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" ) );
            SPECIES.Add( "FonticuluaUpupam", new OrganicSpecies( "FonticuluaUpupam", "Fonticulus", (int)5727600, (decimal?)0.28, null, "", null, "IcyBody;RockyBody", "ArgonRich", "", "B;A;F;G;K;M;L;T;TTS;Y;D;N;AEBE" ) );
            SPECIES.Add( "FrutexaAcus", new OrganicSpecies( "FrutexaAcus", "Shrubs", (int)7774700, (decimal?)0.28, null, "k<", (decimal?)195, "RockyBody", "CarbonDioxide;CarbonDioxideRich", "", "B;F;G;M;L;TTS;D;N" ) );
            SPECIES.Add( "FrutexaCollum", new OrganicSpecies( "FrutexaCollum", "Shrubs", (int)1639800, (decimal?)0.28, null, "", null, "RockyBody", "SulphurDioxide", "", "B;F;G;M;L;TTS;D;N" ) );
            SPECIES.Add( "FrutexaFera", new OrganicSpecies( "FrutexaFera", "Shrubs", (int)1632500, (decimal?)0.28, null, "k<", (decimal?)195, "RockyBody", "CarbonDioxide;CarbonDioxideRich", "None", "B;F;G;M;L;TTS;D;N" ) );
            SPECIES.Add( "FrutexaFlabellum", new OrganicSpecies( "FrutexaFlabellum", "Shrubs", (int)1808900, (decimal?)0.28, null, "", null, "RockyBody", "Ammonia", "", "B;F;G;M;L;TTS;D;N" ) );
            SPECIES.Add( "FrutexaFlammasis", new OrganicSpecies( "FrutexaFlammasis", "Shrubs", (int)10326000, (decimal?)0.28, null, "", null, "RockyBody", "Ammonia", "", "B;F;G;M;L;TTS;D;N" ) );
            SPECIES.Add( "FrutexaMetallicum", new OrganicSpecies( "FrutexaMetallicum", "Shrubs", (int)1632500, (decimal?)0.28, null, "k<", (decimal?)195, "HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich;Ammonia", "None", "B;F;G;M;L;TTS;D;N" ) );
            SPECIES.Add( "FrutexaSponsae", new OrganicSpecies( "FrutexaSponsae", "Shrubs", (int)5988000, (decimal?)0.28, null, "", null, "RockyBody", "Water;WaterRich", "", "B;F;G;M;L;TTS;D;N" ) );
            SPECIES.Add( "FumerolaAquatis", new OrganicSpecies( "FumerolaAquatis", "Fumerolas", (int)6284600, (decimal?)0.28, null, "k<", (decimal?)450, "IcyBody;RockyIceBody", "Any", "Water", "" ) );
            SPECIES.Add( "FumerolaCarbosis", new OrganicSpecies( "FumerolaCarbosis", "Fumerolas", (int)6284600, (decimal?)0.28, null, "k<", (decimal?)275, "IcyBody;RockyIceBody", "Any", "Carbon;Methane", "" ) );
            SPECIES.Add( "FumerolaExtremus", new OrganicSpecies( "FumerolaExtremus", "Fumerolas", (int)16202800, (decimal?)0.28, null, "k<", (decimal?)205, "RockyBody;HighMetalContentBody", "Any", "Silicate;Iron;Rocky", "" ) );
            SPECIES.Add( "FumerolaNitris", new OrganicSpecies( "FumerolaNitris", "Fumerolas", (int)7500900, (decimal?)0.28, null, "k<", (decimal?)250, "IcyBody;RockyIceBody", "Any", "Nitrogen;Ammonia", "" ) );
            SPECIES.Add( "FungoidaBullarum", new OrganicSpecies( "FungoidaBullarum", "Fungoids", (int)3703200, (decimal?)0.28, null, "", null, "RockyBody;HighMetalContentBody;RockyIceBody", "Argon;ArgonRich", "None", "" ) );
            SPECIES.Add( "FungoidaGelata", new OrganicSpecies( "FungoidaGelata", "Fungoids", (int)3330300, (decimal?)0.28, (decimal?)180, "<k<", (decimal?)195, "RockyBody;HighMetalContentBody;RockyIceBody", "Water;WaterRich;CarbonDioxide;CarbonDioxideRich", "", "" ) );
            SPECIES.Add( "FungoidaSetisis", new OrganicSpecies( "FungoidaSetisis", "Fungoids", (int)1670100, (decimal?)0.28, null, "", null, "RockyBody;HighMetalContentBody;RockyIceBody", "Ammonia;Methane;MethaneRich", "", "" ) );
            SPECIES.Add( "FungoidaStabitis", new OrganicSpecies( "FungoidaStabitis", "Fungoids", (int)2680300, (decimal?)0.28, (decimal?)180, "<k<", (decimal?)195, "RockyBody;HighMetalContentBody;RockyIceBody", "Water;WaterRich;CarbonDioxide;CarbonDioxideRich", "", "" ) );
            SPECIES.Add( "OsseusCornibus", new OrganicSpecies( "OsseusCornibus", "Osseus", (int)1483000, (decimal?)0.28, (decimal?)180, "<k<", (decimal?)195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "" ) );
            SPECIES.Add( "OsseusDiscus", new OrganicSpecies( "OsseusDiscus", "Osseus", (int)12934900, (decimal?)0.28, null, "k<", (decimal?)455, "RockyBody;HighMetalContentBody", "Water;WaterRich", "", "" ) );
            SPECIES.Add( "OsseusFractus", new OrganicSpecies( "OsseusFractus", "Osseus", (int)4027800, (decimal?)0.28, (decimal?)180, "<k<", (decimal?)190, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "" ) );
            SPECIES.Add( "OsseusPellebantus", new OrganicSpecies( "OsseusPellebantus", "Osseus", (int)9739000, (decimal?)0.28, (decimal?)190, "<k<", (decimal?)195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "" ) );
            SPECIES.Add( "OsseusPumice", new OrganicSpecies( "OsseusPumice", "Osseus", (int)3156300, (decimal?)0.28, null, "k<", (decimal?)135, "RockyBody;HighMetalContentBody;RockyIceBody", "Argon;ArgonRich;Methane;MethaneRich;Nitrogen", "", "" ) );
            SPECIES.Add( "OsseusSpiralis", new OrganicSpecies( "OsseusSpiralis", "Osseus", (int)2404700, (decimal?)0.28, (decimal?)160, "<k", null, "RockyBody;HighMetalContentBody", "Ammonia", "", "" ) );
            SPECIES.Add( "ReceptaConditivus", new OrganicSpecies( "ReceptaConditivus", "Recepta", (int)14313700, (decimal?)0.28, (decimal?)130, "<k<", (decimal?)300, "IcyBody;RockyIceBody", "SulphurDioxide", "", "" ) );
            SPECIES.Add( "ReceptaDeltahedronix", new OrganicSpecies( "ReceptaDeltahedronix", "Recepta", (int)16202800, (decimal?)0.28, (decimal?)130, "<k<", (decimal?)300, "RockyBody;HighMetalContentBody", "SulphurDioxide", "", "" ) );
            SPECIES.Add( "ReceptaUmbrux", new OrganicSpecies( "ReceptaUmbrux", "Recepta", (int)12934900, (decimal?)0.28, (decimal?)130, "<k<", (decimal?)300, "IcyBody;RockyIceBody;RockyBody;HighMetalContentBody", "SulphurDioxide", "", "" ) );
            SPECIES.Add( "AlbidumSinuousTubers", new OrganicSpecies( "AlbidumSinuousTubers", "Tubers", (int)1514500, null, (decimal?)200, "<k<", (decimal?)500, "RockyBody;HighMetalContentBody", "None", "Any", "" ) );
            SPECIES.Add( "BlatteumSinuousTubers", new OrganicSpecies( "BlatteumSinuousTubers", "Tubers", (int)1514500, null, (decimal?)200, "<k<", (decimal?)500, "RockyBody;HighMetalContentBody", "None", "Any", "" ) );
            SPECIES.Add( "CaeruleumSinuousTubers", new OrganicSpecies( "CaeruleumSinuousTubers", "Tubers", (int)1514500, null, (decimal?)200, "<k<", (decimal?)500, "RockyBody;HighMetalContentBody", "None", "Any", "" ) );
            SPECIES.Add( "LindigoticumSinuousTubers", new OrganicSpecies( "LindigoticumSinuousTubers", "Tubers", (int)1514500, null, (decimal?)200, "<k<", (decimal?)500, "RockyBody;HighMetalContentBody", "None", "Any", "" ) );
            SPECIES.Add( "PrasinumSinuousTubers", new OrganicSpecies( "PrasinumSinuousTubers", "Tubers", (int)1514500, null, (decimal?)200, "<k<", (decimal?)500, "RockyBody;HighMetalContentBody;RockyIceBody", "None", "Any", "" ) );
            SPECIES.Add( "RoseumSinuousTubers", new OrganicSpecies( "RoseumSinuousTubers", "Tubers", (int)1514500, null, (decimal?)200, "<k<", (decimal?)500, "RockyBody;HighMetalContentBody", "None", "Any", "" ) );
            SPECIES.Add( "ViolaceumSinuousTubers", new OrganicSpecies( "ViolaceumSinuousTubers", "Tubers", (int)1514500, null, (decimal?)200, "<k<", (decimal?)500, "RockyBody;HighMetalContentBody", "None", "Any", "" ) );
            SPECIES.Add( "VirideSinuousTubers", new OrganicSpecies( "VirideSinuousTubers", "Tubers", (int)1514500, null, (decimal?)200, "<k<", (decimal?)500, "RockyBody;HighMetalContentBody", "None", "Any", "" ) );
            SPECIES.Add( "StratumAraneamus", new OrganicSpecies( "StratumAraneamus", "Stratum", (int)2448900, (decimal?)0.55, (decimal?)165, "<k", null, "RockyBody", "SulphurDioxide", "", "B;A;F;G;K;M;L;T;TTS;N" ) );
            SPECIES.Add( "StratumCucumisis", new OrganicSpecies( "StratumCucumisis", "Stratum", (int)16202800, (decimal?)0.6, (decimal?)190, "<k", null, "RockyBody", "SulphurDioxide;CarbonDioxide;CarbonDioxideRich", "", "" ) );
            SPECIES.Add( "StratumExcutitus", new OrganicSpecies( "StratumExcutitus", "Stratum", (int)2448900, (decimal?)0.48, (decimal?)165, "<k<", (decimal?)190, "RockyBody", "SulphurDioxide;CarbonDioxide;CarbonDioxideRich", "", "" ) );
            SPECIES.Add( "StratumFrigus", new OrganicSpecies( "StratumFrigus", "Stratum", (int)2637500, (decimal?)0.55, (decimal?)190, "<k", null, "RockyBody", "SulphurDioxide;CarbonDioxide;CarbonDioxideRich", "", "" ) );
            SPECIES.Add( "StratumLaminamus", new OrganicSpecies( "StratumLaminamus", "Stratum", (int)2788300, (decimal?)0.34, (decimal?)165, "<k", null, "RockyBody", "Ammonia", "", "" ) );
            SPECIES.Add( "StratumLimaxus", new OrganicSpecies( "StratumLimaxus", "Stratum", (int)1362000, (decimal?)0.48, (decimal?)165, "<k<", (decimal?)190, "RockyBody", "SulphurDioxide;CarbonDioxide;CarbonDioxideRich", "", "" ) );
            SPECIES.Add( "StratumPaleas", new OrganicSpecies( "StratumPaleas", "Stratum", (int)1362000, (decimal?)0.58, (decimal?)165, "<k", null, "RockyBody", "Ammonia;Water;WaterRich;CarbonDioxide;CarbonDioxideRich", "", "" ) );
            SPECIES.Add( "StratumTectonicas", new OrganicSpecies( "StratumTectonicas", "Stratum", (int)19010800, (decimal?)0.9, (decimal?)165, "<k", null, "HighMetalContentBody", "Oxygen;Ammonia;Water;WaterRich;CarbonDioxide;CarbonDioxideRich;SulphurDioxide", "", "" ) );
            SPECIES.Add( "TubusCavas", new OrganicSpecies( "TubusCavas", "Tubus", (int)11873200, (decimal?)0.19, (decimal?)160, "<k<", (decimal?)190, "RockyBody", "CarbonDioxide;CarbonDioxideRich", "None", "B;A;F;G;K;M;L;T;TTS;N" ) );
            SPECIES.Add( "TubusCompagibus", new OrganicSpecies( "TubusCompagibus", "Tubus", (int)7774700, (decimal?)0.19, (decimal?)160, "<k<", (decimal?)190, "RockyBody", "CarbonDioxide;CarbonDioxideRich", "None", "B;A;F;G;K;M;L;T;TTS;N" ) );
            SPECIES.Add( "TubusConifer", new OrganicSpecies( "TubusConifer", "Tubus", (int)2415500, (decimal?)0.19, (decimal?)160, "<k<", (decimal?)190, "RockyBody", "CarbonDioxide;CarbonDioxideRich", "None", "B;A;F;G;K;M;L;T;TTS;N" ) );
            SPECIES.Add( "TubusRosarium", new OrganicSpecies( "TubusRosarium", "Tubus", (int)2637500, (decimal?)0.19, (decimal?)160, "<k", null, "RockyBody", "Ammonia", "", "B;A;F;G;K;M;L;T;TTS;N" ) );
            SPECIES.Add( "TubusSororibus", new OrganicSpecies( "TubusSororibus", "Tubus", (int)5727600, (decimal?)0.19, (decimal?)160, "<k<", (decimal?)190, "HighMetalContentBody", "Ammonia;CarbonDioxide;CarbonDioxideRich", "None", "B;A;F;G;K;M;L;T;TTS;N" ) );
            SPECIES.Add( "TussockAlbata", new OrganicSpecies( "TussockAlbata", "Tussocks", (int)3252500, (decimal?)0.28, (decimal?)175, "<k<", (decimal?)180, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockCapillum", new OrganicSpecies( "TussockCapillum", "Tussocks", (int)7025800, (decimal?)0.28, (decimal?)80, "<k<", (decimal?)165, "RockyBody;RockyIceBody", "Argon;ArgonRich;Methane;MethaneRich", "", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockCaputus", new OrganicSpecies( "TussockCaputus", "Tussocks", (int)3472400, (decimal?)0.28, (decimal?)180, "<k<", (decimal?)190, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockCatena", new OrganicSpecies( "TussockCatena", "Tussocks", (int)1766600, (decimal?)0.28, (decimal?)150, "<k<", (decimal?)190, "RockyBody;HighMetalContentBody", "Ammonia", "", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockCultro", new OrganicSpecies( "TussockCultro", "Tussocks", (int)1766600, (decimal?)0.28, null, "", null, "RockyBody;HighMetalContentBody", "Ammonia", "", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockDivisa", new OrganicSpecies( "TussockDivisa", "Tussocks", (int)1766600, (decimal?)0.28, (decimal?)150, "<k<", (decimal?)180, "RockyBody;HighMetalContentBody", "Ammonia", "", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockIgnis", new OrganicSpecies( "TussockIgnis", "Tussocks", (int)1849000, (decimal?)0.28, (decimal?)160, "<k<", (decimal?)170, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockPennata", new OrganicSpecies( "TussockPennata", "Tussocks", (int)5853800, (decimal?)0.28, (decimal?)145, "<k<", (decimal?)155, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockPennatis", new OrganicSpecies( "TussockPennatis", "Tussocks", (int)1000000, (decimal?)0.28, null, "k<", (decimal?)195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockPropagito", new OrganicSpecies( "TussockPropagito", "Tussocks", (int)1000000, (decimal?)0.28, null, "k<", (decimal?)195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockSerrati", new OrganicSpecies( "TussockSerrati", "Tussocks", (int)4447100, (decimal?)0.28, (decimal?)170, "<k<", (decimal?)175, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockStigmasis", new OrganicSpecies( "TussockStigmasis", "Tussocks", (int)19010800, (decimal?)0.28, (decimal?)130, "<k<", (decimal?)210, "RockyBody;HighMetalContentBody", "SulphurDioxide", "", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockTriticum", new OrganicSpecies( "TussockTriticum", "Tussocks", (int)7774700, (decimal?)0.28, (decimal?)190, "<k<", (decimal?)195, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "None", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockVentusa", new OrganicSpecies( "TussockVentusa", "Tussocks", (int)3227700, (decimal?)0.28, (decimal?)155, "<k<", (decimal?)160, "RockyBody;HighMetalContentBody", "CarbonDioxide;CarbonDioxideRich", "", "F;G;K;M;L;T;D;H" ) );
            SPECIES.Add( "TussockVirgam", new OrganicSpecies( "TussockVirgam", "Tussocks", (int)14313700, (decimal?)0.28, (decimal?)390, "<k<", (decimal?)450, "RockyBody;HighMetalContentBody", "Water;WaterRich", "", "F;G;K;M;L;T;D;H" ) );
        }

        // Localised description data
        public static ResourceManager rmOrganicSpeciesDesc = new ResourceManager("EddiDataDefinitions.Properties.OrganicSpeciesDesc", Assembly.GetExecutingAssembly());
        public static ResourceManager rmOrganicSpeciesCond = new ResourceManager("EddiDataDefinitions.Properties.OrganicSpeciesCond", Assembly.GetExecutingAssembly());

        public string genus;
        public long value;
        public List<string> planetClass;
        public decimal? maxG;
        public decimal? minK;
        public string kRange;
        public decimal? maxK;
        public List<string> atmosphereClass;
        public List<string> starClass;
        public List<string> volcanism;
        public string description;
        public string conditions;

        // dummy used to ensure that the static constructor has run
        public OrganicSpecies () : this( "" )
        { }

        private OrganicSpecies ( string species ) : base( species, species )
        {
            this.genus = "";
            this.value = 0;
            this.planetClass = new List<string>();
            this.maxG = 0;
            this.minK = 0;
            this.kRange = "";
            this.maxK = 0;
            this.atmosphereClass = new List<string>();
            this.starClass = new List<string>();
            this.volcanism = new List<string>();
            this.description = rmOrganicSpeciesDesc.GetString( species );
            this.conditions = rmOrganicSpeciesCond.GetString( species );
        }

        /// <summary>
        /// Convert comma separated string lists to class objects (planet, atmos, star, volc)
        /// </summary>
        private OrganicSpecies ( string species,
                                 string genus,
                                 long value,
                                 decimal? maxG,
                                 decimal? minK,
                                 string kRange,
                                 decimal? maxK,
                                 string planetClass,
                                 string atmosphereClass,
                                 string volcanism,
                                 string starClass ) : base( species, species )
        {
            this.genus = genus;
            this.value = value;

            if ( planetClass != "" )
            {
                this.planetClass = planetClass.Split( ';' ).ToList();
            }
            else
            {
                this.planetClass = new List<string>();
            }

            this.maxG = maxG;
            this.minK = minK;
            this.kRange = kRange;
            this.maxK = maxK;

            if ( atmosphereClass != "" )
            {
                this.atmosphereClass = atmosphereClass.Split( ';' ).ToList();
            }
            else
            {
                this.atmosphereClass = new List<string>();
            }

            if ( starClass != "" )
            {
                this.starClass = starClass.Split( ';' ).ToList();
            }
            else
            {
                this.starClass = new List<string>();
            }

            if ( volcanism != "" )
            {
                this.volcanism = volcanism.Split( ';' ).ToList();
            }
            else
            {
                this.volcanism = new List<string>();
            }

            this.description = rmOrganicSpeciesDesc.GetString( species );
            this.conditions = rmOrganicSpeciesCond.GetString( species );
        }

        private OrganicSpecies ( string species,
                                 string genus,
                                 long value,
                                 decimal? maxG,
                                 decimal? minK,
                                 string kRange,
                                 decimal? maxK,
                                 List<string> planetClass,
                                 List<string> atmosphereClass,
                                 List<string> volcanism,
                                 List<string> starClass ) : base( species, species )
        {
            this.genus = genus;
            this.value = value;

            this.planetClass = planetClass;

            this.maxG = maxG;
            this.minK = minK;
            this.kRange = kRange;
            this.maxK = maxK;

            this.atmosphereClass = atmosphereClass;
            this.starClass = starClass;
            this.volcanism = volcanism;

            this.description = rmOrganicSpeciesDesc.GetString( species );
            this.conditions = rmOrganicSpeciesCond.GetString( species );
        }

        /// <summary>
        /// Try getting data from the entryid first, then use variant name as a fallback
        /// </summary>
        public static OrganicSpecies Lookup ( string species )
        {
            if ( species != "" )
            {
                if ( SPECIES.ContainsKey( species ) )
                {
                    return SPECIES[ species ];
                }
            }
            return null;
        }
    }
}
