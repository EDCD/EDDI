using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiEvents;
using EddiNavigationService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using Utilities;

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
                if (Enum.TryParse(value, true, out QueryType queryType))
                {
                    // Special case any queries which allow optional arguments
                    string systemArg = null;
                    string stationArg = null;
                    decimal? distanceArg = null;

                    switch (queryType)
                    {
                        case QueryType.most:
                        case QueryType.neutron:
                        case QueryType.route:
                        case QueryType.source:
                        case QueryType.update:
                        {
                            if (values.Count == 2)
                            {
                                systemArg = values[1].AsString;
                            }
                            break;
                        }
                        case QueryType.scoop:
                        {
                            if (values.Count == 2 && decimal.TryParse(values[1].AsString, out var decimalDistance))
                            {
                                distanceArg = decimalDistance;
                            }
                            break;
                        }
                        case QueryType.set:
                        {
                            if (values.Count >= 2)
                            {
                                systemArg = values[1].AsString;
                            }
                            if (values.Count == 3)
                            {
                                stationArg = values[2].AsString;
                            }
                            break;
                        }
                    }
                    @event = NavigationService.Instance.NavQuery(queryType, systemArg, stationArg, distanceArg);
                }
                else
                {
                    Logging.Warn($"The search query '{value}' is unrecognized.");
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
