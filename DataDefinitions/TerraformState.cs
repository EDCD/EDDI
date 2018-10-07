using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Terraform States
    /// </summary>
    public class TerraformState : ResourceBasedLocalizedEDName<TerraformState>
    {
        static TerraformState()
        {
            resourceManager = Properties.TerraformState.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new TerraformState(edname);

            None = new TerraformState("None");
            var Terraformable = new TerraformState("Terraformable");
            var Terraforming = new TerraformState("Terraforming");
            var Terraformed = new TerraformState("Terraformed");
        }

        public static readonly TerraformState None;

        // dummy used to ensure that the static constructor has run, defaulting to "None" if not otherwise specified
        public TerraformState() : this("None")
        {}

        private TerraformState(string edname) : base(edname, edname)
        {}

        new public static TerraformState FromName(string from)
        {
            return ResourceBasedLocalizedEDName<TerraformState>.FromName(eddbTerrformState2journalTerraformState(from));
        }

        private static string eddbTerrformState2journalTerraformState(string eddbTerraformState)
        {
            if (eddbTerraformState == null)
            {
                return null;
            }

            eddbTerraformState = eddbTerraformState.ToLowerInvariant();

            if (eddbTerraformState == "candidate for terraforming")
            {
                return "terraformable";
            }

            if (eddbTerraformState == "terraforming completed")
            {
                return "terraformed";
            }

            if (eddbTerraformState == "being terraformed")
            {
                return "terraforming";
            }

            return eddbTerraformState;
        }
    }
}
