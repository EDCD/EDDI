using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// List of PowerPlay Leaders
    /// </summary>
    public class PowerPlay
    {
        public static readonly List<string> PPlay = new List<string>();

		public static readonly string None = "None";
        public static readonly string PranavAntal = "Pranav Antal";
        public static readonly string ArchonDelaine = "Archon Delaine";
        public static readonly string AislingDuval = "Aisling Duval";
        public static readonly string YuriGrom = "Yuri Grom";
        public static readonly string ZacharyHudson = "Zachary Hudson";
        public static readonly string ArrissaLavignyDuval = "A. Lavigny-Duval";
        public static readonly string EdmondMahon = "Edmond Mahon";
        public static readonly string DentonPatreus = "Denton Patreus";
        public static readonly string ZeminaTorval = "Zemina Torval";
        public static readonly string FeliciaWinters = "Felicia Winters";
        public static readonly string LiYongRui = "Li Yong-Rui";
		
		
        static PowerPlay()
        {
			PPlay.Add(None);
            PPlay.Add(PranavAntal);
            PPlay.Add(ArchonDelaine);
            PPlay.Add(AislingDuval);
            PPlay.Add(YuriGrom);
            PPlay.Add(ZacharyHudson);
            PPlay.Add(ArrissaLavignyDuval);
            PPlay.Add(EdmondMahon);
            PPlay.Add(DentonPatreus);
            PPlay.Add(ZeminaTorval);
            PPlay.Add(FeliciaWinters);
            PPlay.Add(LiYongRui);
        }
    }
}
