namespace EddiDataDefinitions
{
    public class CodexEntry
    {
        public Organic organic;
        public Geology geology;
        public Astronomical astronomical;
        public Guardian guardian;
        public Thargoid thargoid;

        public long entryId;
        public string edname;

        public string subCategory;
        public string category;
        public string region;
        public string system;

        public CodexEntry ( long entryId, string edname, string subCategory, string category, string region, string system )
        {
            organic = new Organic();
            astronomical = new Astronomical();
            geology = new Geology();
            guardian = new Guardian();
            thargoid = new Thargoid();

            this.entryId = entryId;
            this.edname = edname;
            this.subCategory = subCategory;
            this.category = category;
            this.region = region;
            this.system = system;


            if ( category == "Biology" ) {
                if ( subCategory == "Organic_Structures" )
                {
                    organic = Organic.Lookup( entryId, edname );
                }
                else if ( subCategory == "Geology_and_Anomalies" ) {
                    geology = Geology.Lookup( entryId, edname );
                }
            }
            else if ( category == "StellarBodies" )
            {
                astronomical = Astronomical.Lookup( entryId, edname );
            }
            else if ( category == "Civilisations" ) {
                if ( subCategory == "Guardian" )
                {
                    guardian = Guardian.Lookup( entryId, edname );
                }
                else if ( subCategory == "Thargoid" )
                {
                    thargoid = Thargoid.Lookup( entryId, edname );
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string NormalizedName ( string rawName )
        {
            return rawName
                ?.Replace( "Codex_Ent_", "" )
                ?.Replace( "$", "" )
                ?.Replace( "_Name;", "" )
                ?.Replace( "_name;", "" )
                ?.Replace( ";", "" );
        }

        public static string NormalizedSubCategory ( string rawName )
        {
            return rawName
                ?.Replace( "Codex_SubCategory_", "" )
                ?.Replace( "$", "" )
                ?.Replace( ";", "" );
        }

        public static string NormalizedCategory ( string rawName )
        {
            return rawName
                ?.Replace( "Codex_Category_", "" )
                ?.Replace( "$", "" )
                ?.Replace( ";", "" );
        }
    }
}
