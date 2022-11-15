using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Utilities;

namespace EddiEddnResponder.Toolkit
{
    public class GameVersionAugmenter
    {
        // We keep track of game version and active expansions locally
        // Ref. https://github.com/EDCD/EDDN/blob/master/docs/Developers.md#horizons-and-odyssey-flags

        public string gameVersion { get; private set; }

        public string gameBuild { get; private set; }

        public bool? inHorizons { get; private set; }

        public bool? inOdyssey { get; private set; }

        internal void GetVersionInfo(string eventType, IDictionary<string, object> data)
        {
            try
            {
                if (string.Equals("FileHeader", eventType, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Do not attempt to use the 'Horizons' and 'Odyssey' value(s) from a Fileheader event as the semantics are different.
                    // In the Fileheader event the Odyssey flag is indicating whether it's a 4.0 game client.
                    // In the LoadGame event the Horizons and Odyssey flags indicate if those features are active,
                    // but in the 3.8 game client case you only get the Horizons boolean.

                    gameVersion = JsonParsing.getString(data, "gameversion") ?? gameVersion;
                    gameBuild = JsonParsing.getString(data, "build") ?? gameBuild;
                }
                else if (string.Equals("LoadGame", eventType, StringComparison.InvariantCultureIgnoreCase))
                {
                    gameVersion = JsonParsing.getString(data, "gameversion") ?? gameVersion;
                    gameBuild = JsonParsing.getString(data, "build") ?? gameBuild;
                    inHorizons = JsonParsing.getOptionalBool(data, "Horizons") ?? inHorizons;
                    inOdyssey = JsonParsing.getOptionalBool(data, "Odyssey") ?? inOdyssey;
                }
                else if (string.Equals("Outfitting", eventType, StringComparison.InvariantCultureIgnoreCase)
                         || string.Equals("Shipyard", eventType, StringComparison.InvariantCultureIgnoreCase))
                {
                    inHorizons = JsonParsing.getOptionalBool(data, "Horizons") ?? inHorizons;
                    inOdyssey = JsonParsing.getOptionalBool(data, "Odyssey") ?? inOdyssey;
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to parse Elite Dangerous version data for EDDN", ex);
            }
        }

        internal IDictionary<string, object> AugmentVersion(IDictionary<string, object> data, string gameBuildOverrideCAPI = null)
        {
            // Apply game version augment (only if the game is running)
            if (Process.GetProcessesByName("Elite - Dangerous (Client)").Any())
            {
                // Only include flags that are present in the source files.
                // If the source value is null, do not add anything.
                if (!data.ContainsKey("horizons") && inHorizons != null)
                {
                    data.Add("horizons", inHorizons);
                }
                if (!data.ContainsKey("odyssey") && inOdyssey != null)
                {
                    data.Add("odyssey", inOdyssey);
                }
            }
            
            //if (!data.ContainsKey("gameversion") && !string.IsNullOrEmpty(gameVersion))
            //{
            //    data.Add("gameversion", gameVersion);
            //}
            //if (!data.ContainsKey("gamebuild") && 
            //    (!string.IsNullOrEmpty(gameBuild) || !string.IsNullOrEmpty(gameBuild)))
            //{
            //    data.Add("gamebuild", !string.IsNullOrEmpty(gameBuildOverrideCAPI) 
            //        ? gameBuildOverrideCAPI 
            //        : gameBuild);
            //}
            return data;
        }
    }
}