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

            var Terraformable = new TerraformState("Terraformable");
            var Terraforming = new TerraformState("Terraforming");
            var Terraformed = new TerraformState("Terraformed");
        }

        public static readonly TerraformState NotTerraformable = new TerraformState("NotTerraformable");

        // dummy used to ensure that the static constructor has run
        public TerraformState() : this("")
        {}

        private TerraformState(string edname) : base(edname, edname)
        {}
    }
}
