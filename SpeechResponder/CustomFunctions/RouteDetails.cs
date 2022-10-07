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
                    string stringArg0 = null;
                    string stringArg1 = null;
                    decimal? numericArg = null;

                    // Set arguments as required
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
                                stringArg0 = values[1].AsString;
                            }

                            break;
                        }
                        case QueryType.scoop:
                        {
                            if (values.Count == 2 && decimal.TryParse(values[1].AsString, out var decimalDistance))
                            {
                                numericArg = decimalDistance;
                            }

                            break;
                        }
                        case QueryType.set:
                        {
                            if (values.Count >= 2)
                            {
                                stringArg0 = values[1].AsString;
                            }

                            if (values.Count == 3)
                            {
                                stringArg1 = values[2].AsString;
                            }

                            break;
                        }
                        case QueryType.carrier:
                        {
                            if (values.Count == 2)
                            {
                                stringArg0 = EDDI.Instance.CurrentStarSystem.systemname;
                                stringArg1 = values[1].AsString; // Destination system
                                numericArg = EDDI.Instance.FleetCarrier.usedCapacity;
                            }
                            if (values.Count == 3)
                            {
                                if (decimal.TryParse(values[2].AsString, out var load) && load > 0)
                                {
                                    stringArg0 = EDDI.Instance.CurrentStarSystem.systemname;
                                    stringArg1 = values[1].AsString; // Destination system
                                    numericArg = load; // Used capacity
                                }
                                else
                                {
                                    stringArg0 = values[1].AsString; // Starting system
                                    stringArg1 = values[2].AsString; // Destination system
                                    numericArg = EDDI.Instance.FleetCarrier.usedCapacity;
                                }
                            }
                            if (values.Count == 4)
                            {
                                stringArg0 = values[1].AsString; // Starting system
                                stringArg1 = values[2].AsString; // Destination system
                                numericArg = decimal.TryParse(values[3].AsString, out var load2) ? (decimal?)load2 : EDDI.Instance.FleetCarrier.usedCapacity;
                            }
                            break;
                        }
                    }

                    // Execute 
                    @event = NavigationService.Instance.NavQuery(queryType, stringArg0, stringArg1, numericArg);
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
