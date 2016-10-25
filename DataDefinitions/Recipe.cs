using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousDataDefinitions
{
    /// <summary>A recipe</summary>
    public class Recipe
    {
        public string Name { get; set; }

        // Admin
        // The ID in Elite: Dangerous' database
        public long EDID { get; set; }
        // The ID in eddb.io
        public long EDDBID { get; set; }

        public Recipe()
        {
        }

        public Recipe(long EDID, string Name)
        {
            this.EDID = EDID;
            this.Name = Name;
        }
    }
}
