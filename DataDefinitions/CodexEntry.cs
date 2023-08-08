namespace EddiDataDefinitions
{
    public class CodexEntry
    {
        public OrganicItem organic;
        public AstrometricItem astrology;
        public GeologyItem geology;
        public GuardianItem guardian;
        public ThargoidItem thargoid;

        public long entryId;
        public string edname;

        public string subCategory;
        public string category;
        public string region;
        public string system;

        public CodexEntry ( long entryId, string edname, string subCategory, string category, string region, string system )
        {
            organic = new OrganicItem();
            astrology = new AstrometricItem();
            geology = new GeologyItem();

            this.entryId = entryId;
            this.edname = edname;
            this.subCategory = subCategory;
            this.category = category;
            this.region = region;
            this.system = system;


            if ( category == "Biology" ) {
                if ( subCategory == "Organic_Structures" )
                {
                    organic = OrganicInfo.Lookup( entryId, edname );
                }
                else if ( subCategory == "Geology_and_Anomalies" ) {
                    geology = GeologyInfo.Lookup( entryId, edname );
                }
            }
            else if ( category == "StellarBodies" )
            {
                astrology = AstrometricInfo.Lookup( entryId, edname );
            }
            else if ( category == "Civilisations" ) {
                if ( subCategory == "Guardian" )
                {
                    guardian = GuardianInfo.Lookup( entryId, edname );
                }
                else if ( subCategory == "Thargoid" )
                {
                    thargoid = ThargoidInfo.Lookup( entryId, edname );
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
