namespace EddiDataDefinitions
{
    public class CodexEntry
    {
        public OrganicItem organic;             // TODO:#2212........[Change to CodexOrganicItem?]
        public AstrometricItem astrology;       // TODO:#2212........[Change to CodexAstrometricItem?]
        public GeologyItem geology;             // TODO:#2212........[Change to CodexGeologyItem?]
        //public GuardianItem guardian;         // TODO:#2212........[Add Guardian codex entries]
        //public ThargoidItem thargoid;         // TODO:#2212........[Add Thargoid codex entries]

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
                    // Intended primary source (EntryIds have changed?)
                    //OrganicItem organicItem = OrganicInfo.LookupByEntryId (entryId);

                    // Fallback
                    //organic = OrganicInfo.LookupByVariant( edname );
                    organic = OrganicInfo.Lookup( entryId, edname );
                }
                else if ( subCategory == "Geology_and_Anomalies" ) {
                    //geology = GeologyInfo.LookupByName( edname );
                    geology = GeologyInfo.Lookup( entryId, edname );
                }
            }
            else if ( category == "StellarBodies" )
            {
                //astrology = AstrometricInfo.LookupByName( edname );
                astrology = AstrometricInfo.Lookup( entryId, edname );
            }
            else if ( category == "Civilisations" ) {
                // TODO:#2212........[Possibly combine Thargoid and Guardian?]
                if ( subCategory == "Guardian" )
                {
                    // TODO:#2212........[Add Guardian codex entries]
                }
                else if ( subCategory == "Thargoid" )
                {
                    // TODO:#2212........[Add Thargoid codex entries]
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
