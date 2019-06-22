﻿using System.Linq;
using Utilities;

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
            var ALavignyDuval = new Power("ALavignyDuval", Superpower.Empire, "Kamadhenu");
            var AislingDuval = new Power("AislingDuval", Superpower.Empire, "Cubeo");
            var ArchonDelaine = new Power("ArchonDelaine", Superpower.Independent, "Harma");
            var DentonPatreus = new Power("DentonPatreus", Superpower.Empire, "Eotienses");
            var EdmundMahon = new Power("EdmundMahon", Superpower.Alliance, "Gateway");
            var FeliciaWinters = new Power("FeliciaWinters", Superpower.Federation, "Rhea");
            var LiYongRui = new Power("LiYongRui", Superpower.Independent, "Lembava");
            var PranavAntal = new Power("PranavAntal", Superpower.Independent, "Polevnic");
            var YuriGrom = new Power("YuriGrom", Superpower.Alliance, "Clayakarma");
            var ZacharyHudson = new Power("ZacharyHudson", Superpower.Federation, "Nanomam");
            var ZeminaTorval = new Power("ZeminaTorval", Superpower.Empire, "Synteini");
        }

        public static readonly Power None;

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
            return ResourceBasedLocalizedEDName<Power>.FromEDName(tidiedName);
        }

        new public static Power FromName(string name)
        {
            if (name == null)
            {
                return null;
            }

            // The player journal records an abbreviated version of Arissa's name. Fix that here to match our unabbreviated version.
            string tidiedName = name.Replace("A. Lavigny-Duval", "Arissa Lavigny-Duval");

            return ResourceBasedLocalizedEDName<Power>.FromName(tidiedName);
        }
    }
}
