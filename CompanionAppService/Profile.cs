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
        public Commander Cmdr { get; set; }

        /// <summary>The current starsystem</summary>
        public StarSystem CurrentStarSystem{ get; set; }

        /// <summary>The last station the commander docked at</summary>
        public Station LastStation { get; set; }
    }
}
