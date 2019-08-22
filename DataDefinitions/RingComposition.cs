namespace EddiDataDefinitions
{
    /// <summary>
    /// Ring compositions
    /// </summary>
    public class RingComposition : ResourceBasedLocalizedEDName<RingComposition>
    {
        static RingComposition()
        {
            resourceManager = Properties.RingCompositions.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new RingComposition(edname);

            var Icy = new RingComposition("Icy");
            var Rocky = new RingComposition("Rocky");
            var Metallic = new RingComposition("Metallic"); // sic (the journal uses "Metalic", EDSM uses "Metallic")
            var MetalRich = new RingComposition("MetalRich");
        }

        // dummy used to ensure that the static constructor has run
        public RingComposition() : this("")
        { }

        private RingComposition(string edname) : base(edname, edname
            .Replace("eRingClass_", "")
            .Replace("-", "")
            .Replace("Metalic", "Metallic"))
        { }

        new public static RingComposition FromEDName(string edname)
        {
            string normalizedEDName = edname
                .Replace("eRingClass_", "")
                .Replace("-", "")
                .Replace("Metalic", "Metallic");
            return ResourceBasedLocalizedEDName<RingComposition>.FromEDName(normalizedEDName);
        }
    }
}
