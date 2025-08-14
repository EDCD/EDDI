using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EddiNavigationService
{
    public interface IQueryResolver
    {
        QueryType Type { get; }
        Dictionary<string, object> SpanshQueryFilter { get; }
        Task<RouteDetailsEvent> ResolveAsync ( [NotNull] Query query, [NotNull] StarSystem startSystem );
    }
}
