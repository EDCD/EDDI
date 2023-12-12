using Utilities;

namespace EddiDataDefinitions
{
    public class CodexEntry
    {
        public long entryId { get; }

        public string edname { get; }

        [PublicAPI]
        public string entryName { get; }

        [PublicAPI]
        public string subCategoryName { get; }

        [PublicAPI]
        public string categoryName { get; }

        [PublicAPI]
        public string localizedRegion { get; }

        [PublicAPI]
        public string systemName { get; }
        
        public ulong systemAddress { get; }

        [PublicAPI]
        public Organic organic { get; private set; }

        [PublicAPI]
        public Geology geology { get; private set; }

        [PublicAPI]
        public Astronomical astronomical { get; private set; }

        [PublicAPI]
        public Guardian guardian { get; private set; }

        [PublicAPI]
        public Thargoid thargoid { get; private set; }

        public CodexEntry ( long entryId, string edname, string subCategoryEDName, string categoryEDName, string localizedRegion, string systemName, ulong systemAddress )
        {
            this.entryId = entryId;
            this.edname = edname;
            this.entryName = NormalizedName( edname );
            this.subCategoryName = NormalizedSubCategory( subCategoryEDName );
            this.categoryName = NormalizedCategory( categoryEDName );
            this.localizedRegion = localizedRegion;
            this.systemName = systemName;
            this.systemAddress = systemAddress;
            
            if ( categoryName == "Biology" ) 
            {
                if ( subCategoryName == "Organic_Structures" )
                {
                    organic = Organic.Lookup( entryId, edname );
                }
                else if ( subCategoryName == "Geology_and_Anomalies" ) 
                {
                    geology = Geology.Lookup( entryId, edname );
                }
            }
            else if ( categoryName == "StellarBodies" )
            {
                astronomical = Astronomical.Lookup( entryId, edname );
            }
            else if ( categoryName == "Civilisations" ) 
            {
                if ( subCategoryName == "Guardian" )
                {
                    guardian = Guardian.Lookup( entryId, edname );
                }
                else if ( subCategoryName == "Thargoid" )
                {
                    thargoid = Thargoid.Lookup( entryId, edname );
                }
            }
        }

        private static string NormalizedName ( string edname )
        {
            return edname?
                .Replace( "Codex_Ent_", "" )
                .Replace( "$", "" )
                .Replace( "_Name;", "" )
                .Replace( "_name;", "" )
                .Replace( ";", "" );
        }

        private static string NormalizedSubCategory ( string subcategoryEDName )
        {
            return subcategoryEDName?
                .Replace( "Codex_SubCategory_", "" )
                .Replace( "$", "" )
                .Replace( ";", "" );
        }

        private static string NormalizedCategory ( string categoryEDName )
        {
            return categoryEDName?
                .Replace( "Codex_Category_", "" )
                .Replace( "$", "" )
                .Replace( ";", "" );
        }
    }
}
