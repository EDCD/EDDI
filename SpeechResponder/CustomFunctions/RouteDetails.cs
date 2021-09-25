using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiEvents;
using EddiNavigationService;
using EddiShipMonitor;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class RouteDetails : ICustomFunction
    {
        public string name => "RouteDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.RouteDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            string result = null;
            string value = values[0].AsString;
            RouteDetailsEvent @event = null;
            if (!string.IsNullOrEmpty(value))
            {
                switch (value.ToLowerInvariant())
                {
                    case "encoded":
                    {
                        @event = NavigationService.Instance.NavQuery(QueryTypes.encoded);
                        break;
                    }
                    case "expiring":
                    {
                        @event = NavigationService.Instance.NavQuery(QueryTypes.expiring);
                        break;
                    }
                    case "facilitator":
                    {
                        @event = NavigationService.Instance.NavQuery(QueryTypes.facilitator);
                        break;
                    }
                    case "farthest":
                    {
                        @event = NavigationService.Instance.NavQuery(QueryTypes.farthest);
                        break;
                    }
                    case "guardian":
                    {
                        @event = NavigationService.Instance.NavQuery(QueryTypes.guardian);
                        break;
                    }
                    case "human":
                    {
                        @event = NavigationService.Instance.NavQuery(QueryTypes.human);
                        break;
                    }
                    case "manufactured":
                    {
                        @event = NavigationService.Instance.NavQuery(QueryTypes.manufactured);
                        break;
                    }
                    case "most":
                    {
                        @event = NavigationService.Instance.NavQuery(QueryTypes.most);
                        break;
                    }
                    case "nearest":
                    {
                        @event = NavigationService.Instance.NavQuery(QueryTypes.nearest);
                        break;
                    }
                    case "raw":
                    {
                        @event = NavigationService.Instance.NavQuery(QueryTypes.raw);
                        break;
                    }
                    case "route":
                    {
                        dynamic querySystem = null;
                        if (values.Count == 2)
                        {
                            querySystem = values[1].AsString;
                        }
                        @event = NavigationService.Instance.NavQuery(QueryTypes.route, new [] { querySystem });
                        break;
                    }
                    case "scoop":
                    {
                        dynamic searchDistance = null;
                        if (values.Count == 2 && decimal.TryParse(values[1].AsString, out var decimalDistance))
                        {
                            searchDistance = decimalDistance;
                        }
                        @event = NavigationService.Instance.NavQuery(QueryTypes.scoop, new [] {searchDistance} );
                        break;
                    }
                    case "source":
                    {
                        dynamic querySystem = null;
                        if (values.Count == 2)
                        {
                            querySystem = values[1].AsString;
                        }
                        @event = NavigationService.Instance.NavQuery(QueryTypes.source, new [] { querySystem });
                        break;
                    }
                    case "update":
                    {
                        dynamic arg = null;
                        if (values.Count == 2)
                        {
                            if (bool.TryParse(values[1].AsString, out var boolArg))
                            {
                                arg = boolArg;
                            }
                            else if (decimal.TryParse(values[1].AsString, out var decimalArg))
                            {
                                arg = decimalArg;
                            }
                            else
                            {
                                arg = values[1].AsString;
                            }
                        }
                        @event = NavigationService.Instance.NavQuery(QueryTypes.source, new[] { arg });
                        break;
                    }
                }
            }
            if (@event != null)
            {
                EDDI.Instance?.enqueueEvent(@event);
                result = @event.system;
            }
            return new ReflectionValue(result ?? new object());
        }, 1, 3);
    }
}
