using System;
using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiEvents;
using EddiNavigationService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
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
                if (Enum.TryParse(value, true, out QueryTypes queryType))
                {
                    // Special case any queries which allow optional arguments
                    dynamic[] args = null;
                    switch (queryType)
                    {
                        case QueryTypes.route:
                        {
                            dynamic querySystem = null;
                            if (values.Count == 2)
                            {
                                querySystem = values[1].AsString;
                            }
                            args = new[] {querySystem};
                            break;
                        }
                        case QueryTypes.scoop:
                        {
                            dynamic searchDistance = null;
                            if (values.Count == 2 && decimal.TryParse(values[1].AsString, out var decimalDistance))
                            {
                                searchDistance = decimalDistance;
                            }
                            args = new[] { searchDistance };
                            break;
                        }
                        case QueryTypes.source:
                        {
                            dynamic querySystem = null;
                            if (values.Count == 2)
                            {
                                querySystem = values[1].AsString;
                            }
                            args = new[] { querySystem };
                            break;
                        }
                        case QueryTypes.update:
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
                            args = new[] { arg };
                            break;
                        }
                    }
                    @event = NavigationService.Instance.NavQuery(queryType, args);
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
