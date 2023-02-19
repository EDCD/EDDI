using EddiEvents;

namespace EddiNavigationService
{
    public interface IQueryResolver
    {
        QueryType Type { get; }
        RouteDetailsEvent Resolve ( Query query );
    }
}
