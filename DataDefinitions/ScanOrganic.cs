namespace EddiDataDefinitions
{
    public class ScanOrganic
    {
        /// <summary>
        /// 
        /// </summary>
        public static string NormalizedGenus ( string rawName )
        {
            return rawName
                ?.Replace( "Codex_Ent_", "" )
                ?.Replace( "$", "" )
                ?.Replace( "_Name;", "" )
                ?.Replace( "_name;", "" )
                ?.Replace( ";", "" );
        }

        public static string NormalizedSpecies ( string rawName )
        {
            return rawName
                ?.Replace( "Codex_Ent_", "" )
                ?.Replace( "$", "" )
                ?.Replace( "_Name;", "" )
                ?.Replace( "_name;", "" )
                ?.Replace( ";", "" );
        }

        public static string NormalizedVariant ( string rawName )
        {
            return rawName
                ?.Replace( "Codex_Ent_", "" )
                ?.Replace( "$", "" )
                ?.Replace( "_Name;", "" )
                ?.Replace( "_name;", "" )
                ?.Replace( ";", "" );
        }
    }
}
