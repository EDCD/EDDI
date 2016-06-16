using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataDefinitions
{
    public class RecipeDefinitions
    {
        private static Dictionary<long, Recipe> RecipesByEliteID = new Dictionary<long, Recipe>()
        {
            { 128673790, new Recipe(128673790 , "Resistance Augmented Shield Booster") },
            { 128673692, new Recipe(128673692 , "Increased Range FSD") },
            { 128673751, new Recipe(128673751 , "Weapons Focused Power Distributor") },
        };

        /// <summary>Obtain details of a recipe given its Elite ID</summary>
        public static Recipe RecipeFromEliteID(long id)
        {
            Recipe Recipe = new Recipe();
            Recipe Template;
            if (RecipesByEliteID.TryGetValue(id, out Template))
            {
                Recipe.EDID = Template.EDID;
                Recipe.Name = Template.Name;
            }

            return Recipe;
        }
    }
}
