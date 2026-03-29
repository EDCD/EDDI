using System;
using System.Collections.Generic;

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

            NotTerraformable = new TerraformState("NotTerraformable");
            Terraformable = new TerraformState("Terraformable");
            Terraforming = new TerraformState("Terraforming");
            Terraformed = new TerraformState("Terraformed");
        }

        public static readonly TerraformState NotTerraformable;
        public static readonly TerraformState Terraformable;
        public static readonly TerraformState Terraforming;
        public static readonly TerraformState Terraformed;

        // dummy used to ensure that the static constructor has run
        public TerraformState () : this( "" )
        { }

        private TerraformState(string edname) : base(edname, edname)
        { }

        public static new TerraformState FromName ( string from )
        {
            if ( !string.IsNullOrEmpty(from) && equivalencyMap.TryGetValue(from.ToLowerInvariant(), out var terraformState) )
            {
                return terraformState;
            }

            return ResourceBasedLocalizedEDName<TerraformState>.FromName( from );
        }
        public static new TerraformState FromEDName ( string from )
        {
            if ( !string.IsNullOrEmpty( from ) && equivalencyMap.TryGetValue( from.ToLowerInvariant(), out var terraformState ) )
            {
                return terraformState;
            }

            return ResourceBasedLocalizedEDName<TerraformState>.FromEDName( from );
        }  
        
        private static Dictionary<string, TerraformState> equivalencyMap => new( StringComparer.OrdinalIgnoreCase )
        {
            { "not terraformable", NotTerraformable },
            { "terraformable", Terraformable }
        };
    }
}
