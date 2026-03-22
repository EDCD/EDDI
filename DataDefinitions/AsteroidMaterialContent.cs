namespace EddiDataDefinitions
{
    /// <summary> Asteroid material content </summary>
    public class AsteroidMaterialContent ( string edName )
        : ResourceBasedLocalizedEDName<AsteroidMaterialContent>( edName,
            edName.Replace( "$AsteroidMaterialContent_", "" ).Replace( ";", "" ) )
    {
        static AsteroidMaterialContent()
        {
            resourceManager = Properties.AsteroidMaterialContent.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new AsteroidMaterialContent(edname);

            var High = new AsteroidMaterialContent("$AsteroidMaterialContent_High;");
            var Medium = new AsteroidMaterialContent("$AsteroidMaterialContent_Medium;");
            var Low = new AsteroidMaterialContent("$AsteroidMaterialContent_Low;");
        }

        // dummy used to ensure that the static constructor has run
        public AsteroidMaterialContent() : this("")
        { }
    }
}
