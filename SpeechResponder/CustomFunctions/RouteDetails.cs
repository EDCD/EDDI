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
        public string description => Properties.CustomFunctions_Untranslated.RouteDetails;
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
