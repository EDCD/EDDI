using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiCrimeMonitor;
using EddiMaterialMonitor;
using EddiNavigationService;
using EddiShipMonitor;
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
        public string description => @"
This function will produce a destination/route for valid mission destinations.

RouteDetails takes one mandatory argument, the `routetype`, and up to two optional arguments.

The following `routetype` values are valid:

* `cancel` Cancel the currently stored route.
* `encoded` Nearest encoded materials trader.
* `expiring` Destination of your next expiring mission.
* `facilitator` Nearest 'Legal Facilities' contact.
* `farthest` Mission destination farthest from your current location.
* `guardian` Nearest guardian technology broker.
* `human` Nearest human technology broker.
* `manufactured` Nearest manufactured materials trader.
* `most` Nearest system with the most missions. Takes a system name as an optional secondary argument. If set, the resulting route shall be plotted relative to the specified star system rather than relative to the current star system.
* `nearest` Mission destination nearest to your current location.
* `next` Next destination in the currently stored route.
* `raw` Nearest raw materials trader.
* `route` 'Traveling Salesman' (RNNA) route for all active missions. Takes a system name as an optional secondary argument. If set, the resulting route shall be plotted relative to the specified star system rather than relative to the current star system.
* `scoop` Nearest scoopable star system.
* `set` Set destination route to a single system. Sets the route destination to the last star system name returned from a `Route details` event. An optional second argument sets the route destination to the star system name specified instead. An optional third argument sets the destination station.
* `source` Destination to nearest mission 'cargo source'. Takes a system name as an optional secondary argument. If set, the resulting route shall be plotted relative to the specified star system rather than relative to the current star system. 
* `update` Update to the next mission route destination, once all missions in current system are completed. Takes a system name as an optional secondary argument. If set, the resulting route shall be plotted relative to the specified star system rather than relative to the current star system.

Common usage of this is to provide destination/route details, dependent on the 'routetype', for example:

    {RouteDetails(""cancel"")}
    {RouteDetails(""set"", ""Achenar"", ""Macmillan Terminal"")}
    {set system to RouteDetails(""nearest"")}
    {set system to RouteDetails(""most"", ""Sol"")}

Upon success of the query, a 'Route details' event is triggered, providing a following event data:

* `routetype` Type of route query (see above).
* `destination` Destination system.
* `distance` Destination distance
* `route` '_' Delimited system(s) list, depending on route type:
  * `most` Other systems with most number of missions.
  * `route` Missions route list
  * `source` Other systems designated as a source for missions.
* `count` Count of missions, systems, or expiry seconds, etc, depending on route type:
  * `expiring` Expiry seconds.
  * `farthest` Missions in the system.
  * `most` Number of most missions.
  * `nearest` Missions in the system.
  * `route` Systems in the route.
  * `scoop` Number if scoopable stars found, within search radius.
  * `source` Number of source systems.
  * `update` Remaining systems in the route.
* `routedistance` Remaining distance of the route (multiple or single), with the following exceptions:
  * `scoop` Distance of the search radius.
* `missionids` Mission ID(s) associated with the destination system.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            CrimeMonitor crimeMonitor = (CrimeMonitor)EDDI.Instance.ObtainMonitor("Crime monitor");
            MaterialMonitor materialMonitor = (MaterialMonitor)EDDI.Instance.ObtainMonitor("Material monitor");
            int materialMonitorDistance = materialMonitor.maxStationDistanceFromStarLs ?? Constants.maxStationDistanceDefault;
            string result = null;
            string value = values[0].AsString;
            if (!string.IsNullOrEmpty(value))
            {
                switch (value)
                {
                    case "cancel":
                        {
                            NavigationService.Instance.CancelDestination();
                        }
                        break;
                    case "encoded":
                        {
                            result = NavigationService.Instance.GetServiceRoute("encoded", materialMonitorDistance);
                        }
                        break;
                    case "expiring":
                        {
                            result = NavigationService.Instance.GetExpiringRoute();
                        }
                        break;
                    case "facilitator":
                        {
                            int distance = crimeMonitor.maxStationDistanceFromStarLs ?? 10000;
                            bool isChecked = crimeMonitor.prioritizeOrbitalStations;
                            result = NavigationService.Instance.GetServiceRoute("facilitator", distance, isChecked);
                        }
                        break;
                    case "farthest":
                        {
                            result = NavigationService.Instance.GetFarthestRoute();
                        }
                        break;
                    case "guardian":
                        {
                            result = NavigationService.Instance.GetServiceRoute("guardian", materialMonitorDistance);
                        }
                        break;
                    case "human":
                        {
                            result = NavigationService.Instance.GetServiceRoute("human", materialMonitorDistance);
                        }
                        break;
                    case "manufactured":
                        {
                            result = NavigationService.Instance.GetServiceRoute("manufactured", materialMonitorDistance);
                        }
                        break;
                    case "most":
                        {
                            if (values.Count == 2)
                            {
                                result = NavigationService.Instance.GetMostRoute(values[1].AsString);
                            }
                            else
                            {
                                result = NavigationService.Instance.GetMostRoute();
                            }
                        }
                        break;
                    case "nearest":
                        {
                            result = NavigationService.Instance.GetNearestRoute();
                        }
                        break;
                    case "next":
                        {
                            result = NavigationService.Instance.GetNextInRoute();
                        }
                        break;
                    case "raw":
                        {
                            result = NavigationService.Instance.GetServiceRoute("raw", materialMonitorDistance);
                        }
                        break;
                    case "route":
                        {
                            if (values.Count == 2)
                            {
                                result = NavigationService.Instance.GetMissionsRoute(values[1].AsString);
                            }
                            else
                            {
                                result = NavigationService.Instance.GetMissionsRoute();
                            }
                        }
                        break;
                    case "scoop":
                        {
                            if (values.Count == 2)
                            {
                                result = NavigationService.Instance.GetScoopRoute((decimal)values[1].AsNumber);
                            }
                            else
                            {
                                ShipMonitor.JumpDetail detail = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).JumpDetails("total");
                                result = NavigationService.Instance.GetScoopRoute(detail.distance);
                            }
                        }
                        break;
                    case "set":
                        {
                            if (values.Count == 3)
                            {
                                result = NavigationService.Instance.SetDestination(values[1].AsString, values[2].AsString);
                            }
                            else if (values.Count == 2)
                            {
                                result = NavigationService.Instance.SetDestination(values[1].AsString);
                            }
                            else
                            {
                                result = NavigationService.Instance.SetDestination();
                            }
                        }
                        break;
                    case "source":
                        {
                            if (values.Count == 2)
                            {
                                result = NavigationService.Instance.GetSourceRoute(values[1].AsString);
                            }
                            else
                            {
                                result = NavigationService.Instance.GetSourceRoute();
                            }
                        }
                        break;
                    case "update":
                        {
                            if (values.Count == 2)
                            {
                                result = NavigationService.Instance.UpdateRoute(values[1].AsString);
                            }
                            else
                            {
                                result = NavigationService.Instance.UpdateRoute();
                            }
                        }
                        break;
                }
            }
            return new ReflectionValue(result ?? new object());
        }, 1, 3);
    }
}
