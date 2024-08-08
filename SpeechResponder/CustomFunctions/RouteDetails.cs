using Cottle;
using EddiCore;
using EddiNavigationService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Reflection;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class RouteDetails : ICustomFunction
    {
        public string name => "RouteDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.RouteDetails;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            try
            {
                Logging.Debug("RouteDetails() invoked, arguments: ", values);

                var query = values?.FirstOrDefault().AsString;

                if (string.IsNullOrEmpty(query))
                {
                    return Value.EmptyMap;
                }
                if (!Enum.TryParse(query, true, out QueryType queryType))
                {
                    Logging.Warn($"The search query '{query}' is unrecognized.");
                    return Value.EmptyMap;
                }

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
                        if (values.Count >= 2)
                        {
                            stringArg0 = values[1].AsString;
                        }

                        break;
                    }
                    case QueryType.encoded:
                    case QueryType.facilitator:
                    case QueryType.guardian:
                    case QueryType.human:
                    case QueryType.manufactured:
                    case QueryType.raw:
                    case QueryType.scoop:
                    case QueryType.scorpion:
                    {
                        if (values.Count >= 2 && decimal.TryParse(values[1].AsString, out var decimalDistance))
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

                        if (values.Count >= 3)
                        {
                            stringArg1 = values[2].AsString;
                        }

                        break;
                    }
                    case QueryType.carrier:
                    {
                        if (values.Count == 1)
                        {
                            return "Insufficient information to calculate carrier route details. At minimum, please specify a destination star system.";
                        }
                        if (values.Count >= 2)
                        {
                            stringArg0 = values[1].AsString; // Destination system
                        }
                        if (values.Count >= 3)
                        {
                            if (decimal.TryParse(values[2].AsString, out var load) && load > 0)
                            {
                                numericArg = load; // Used capacity
                            }
                            else
                            {
                                stringArg1 = values[2].AsString; // Starting system
                            }
                        }
                        if (values.Count >= 4)
                        {
                            if (!string.IsNullOrEmpty(stringArg1)
                                && decimal.TryParse(values[3].AsString, out var load) && load > 0)
                            {
                                numericArg = load; // Used capacity
                            }
                        }
                        break;
                    }
                }

                // Execute 
                var @event = NavigationService.Instance?.NavQuery(queryType, stringArg0, stringArg1, numericArg);
                string result = null;
                if ( @event != null)
                {
                    EDDI.Instance?.enqueueEvent(@event);
                    result = @event.system;
                }
                return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            }
            catch (Exception e)
            {
                Logging.Error("Unable to resolve RouteDetails() request", e);
                return Value.EmptyMap;
            }
        }, 1, 4);
    }
}
