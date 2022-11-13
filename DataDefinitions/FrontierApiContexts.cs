namespace EddiDataDefinitions
{
    public class FrontierApiContexts
    {
        /// <summary>Whether this profile describes a commander with access to the Cobra Mk IV</summary>
        public bool allowCobraMkIV { get; set; }

        /// <summary>Whether this profile describes a commander with the Horizons expansion available</summary>
        public bool hasHorizons { get; set; }

        /// <summary>Whether this profile describes a commander with the Horizons expansion available</summary>
        public bool hasOdyssey { get; set; }
    }
}