namespace EddiDataDefinitions
{
    public class CodexEntry
    {
        public long entryId { get; }
        public string edname { get; }
        public string entryName { get; }
        public string subCategoryName { get; }
        public string categoryName { get; }
        public string localizedRegion { get; }
        public string systemName { get; }
        public Organic organic { get; private set; }
        public Geology geology { get; private set; }
        public Astronomical astronomical { get; private set; }
        public Guardian guardian { get; private set; }
        public Thargoid thargoid { get; private set; }

        public CodexEntry ( long entryId, string edname, string subCategoryEDName, string categoryEDName, string localizedRegion, string systemName )
        {
            this.entryId = entryId;
            this.edname = edname;
            this.entryName = NormalizedName( edname );
            this.subCategoryName = NormalizedSubCategory( subCategoryEDName );
            this.categoryName = NormalizedCategory( categoryEDName );
            this.localizedRegion = localizedRegion;
            this.systemName = systemName;
            
            if ( categoryEDName == "Biology" ) 
            {
                if ( subCategoryEDName == "Organic_Structures" )
                {
                    organic = Organic.Lookup( entryId, edname );
                }
                else if ( subCategoryEDName == "Geology_and_Anomalies" ) 
                {
                    geology = Geology.Lookup( entryId, edname );
                }
            }
            else if ( categoryEDName == "StellarBodies" )
            {
                astronomical = Astronomical.Lookup( entryId, edname );
            }
            else if ( categoryEDName == "Civilisations" ) 
            {
                if ( subCategoryEDName == "Guardian" )
                {
                    guardian = Guardian.Lookup( entryId, edname );
                }
                else if ( subCategoryEDName == "Thargoid" )
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
                .Replace( ";", "" )
                .Replace("_", " " );
        }

        private static string NormalizedSubCategory ( string subcategoryEDName )
        {
            return subcategoryEDName?
                .Replace( "Codex_SubCategory_", "" )
                .Replace( "$", "" )
                .Replace( ";", "" )
                .Replace( "_", " " );
        }

        private static string NormalizedCategory ( string categoryEDName )
        {
            return categoryEDName?
                .Replace( "Codex_Category_", "" )
                .Replace( "$", "" )
                .Replace( ";", "" )
                .Replace( "_", " " );
        }
    }
}
