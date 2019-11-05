using EddiDataDefinitions;

namespace EddiBgsService
{
    public interface IBgsService
    {
        Faction GetFactionByName(string factionName, string systemName = null);
    }
}