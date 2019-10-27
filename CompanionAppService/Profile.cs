using EddiDataDefinitions;
using Newtonsoft.Json.Linq;

namespace EddiCompanionAppService
{
    /// <summary>
    /// Profile information returned by the companion app service
    /// </summary>
    public class Profile
    {
        // The JSON object
        public JObject json { get; set; }

        /// <summary>The commander</summary>
        public FrontierApiCommander Cmdr { get; set; }

        /// <summary>The current starsystem</summary>
        public StarSystem CurrentStarSystem { get; set; }

        /// <summary>The last station the commander docked at</summary>
        public Station LastStation { get; set; }

        /// <summary>Whether this profile describes a docked commander</summary>
        public bool docked { get; set; }

        /// <summary>Whether this profile describes a currently living commander</summary>
        public bool alive { get; set; }
    }
}
