using System.Linq;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Squadron Ranks
    /// </summary>
    public class Power : ResourceBasedLocalizedEDName<Power>
    {
        static Power()
        {
            resourceManager = Properties.Powers.ResourceManager;
            resourceManager.IgnoreCase = false;

            None = new Power("None", Superpower.None, "None");
            ALavignyDuval = new Power("ALavignyDuval", Superpower.Empire, "Kamadhenu");
            AislingDuval = new Power("AislingDuval", Superpower.Empire, "Cubeo");
            ArchonDelaine = new Power("ArchonDelaine", Superpower.Independent, "Harma");
            DentonPatreus = new Power("DentonPatreus", Superpower.Empire, "Eotienses");
            EdmundMahon = new Power("EdmundMahon", Superpower.Alliance, "Gateway");
            FeliciaWinters = new Power("FeliciaWinters", Superpower.Federation, "Rhea");
            LiYongRui = new Power("LiYongRui", Superpower.Independent, "Lembava");
            PranavAntal = new Power("PranavAntal", Superpower.Independent, "Polevnic");
            YuriGrom = new Power("YuriGrom", Superpower.Alliance, "Clayakarma");
            ZacharyHudson = new Power("ZacharyHudson", Superpower.Federation, "Nanomam");
            ZeminaTorval = new Power("ZeminaTorval", Superpower.Empire, "Synteini");
        }

        public static readonly Power None;
        public static readonly Power ALavignyDuval;
        public static readonly Power AislingDuval;
        public static readonly Power ArchonDelaine;
        public static readonly Power DentonPatreus;
        public static readonly Power EdmundMahon;
        public static readonly Power FeliciaWinters;
        public static readonly Power LiYongRui;
        public static readonly Power PranavAntal;
        public static readonly Power YuriGrom;
        public static readonly Power ZacharyHudson;
        public static readonly Power ZeminaTorval;

        public Superpower Allegiance { get; private set; }
        public string headquarters { get; private set; }

        // dummy used to ensure that the static constructor has run
        public Power() : this("", Superpower.None, "None")
        { }

        private Power(string edname, Superpower allegiance, string headquarters) : base(edname, edname)
        {
            this.Allegiance = allegiance;
            this.headquarters = headquarters;
        }

        public new static Power FromEDName(string edName)
        {
            if (edName == null)
            {
                return null;
            }

            string tidiedName = edName.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("-", "");
            return AllOfThem.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedName);
        }
    }
}
