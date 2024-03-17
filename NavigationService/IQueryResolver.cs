using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;

namespace EddiNavigationService
{
    public interface IQueryResolver
    {
        QueryType Type { get; }
        RouteDetailsEvent Resolve ( [NotNull] Query query, [NotNull] StarSystem startSystem );
    }
}
