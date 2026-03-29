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
        static readonly Dictionary<string, object> SpanshQueryFilter = [ ];
        Task<RouteDetailsEvent> ResolveAsync ( [NotNull] Query query, [NotNull] StarSystem startSystem );
    }
}
