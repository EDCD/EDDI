using EddiCore;
using EddiConfigService;
using EddiDataDefinitions;
using EddiEvents;
using EddiStatusMonitor;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiNavigationService
{
    public class PlanetaryGuidance
    {
        // Guidance variables
        private const int guidanceIntervalMilliseconds = 3000;
        private static Task guidanceTask;
        private static CancellationTokenSource guidanceCancellationTS = new CancellationTokenSource();
        private static GuidanceStatus guidanceStatus;

        private static readonly object statusLock = new object();
        private Status currentStatus { get; set; }
        private Status lastStatus { get; set; }

        private readonly NavigationMonitorConfiguration navConfig = ConfigService.Instance.navigationMonitorConfiguration;
        private ObservableCollection<NavBookmark> bookmarks { get; set; }

        public PlanetaryGuidance(ref ObservableCollection<NavBookmark> bookmarks)
        {
            this.bookmarks = bookmarks;

            StatusMonitor.StatusUpdatedEvent += (s, e) =>
            {
                if (s is Status status)
                {
                    lock (statusLock)
                    {
                        OnStatusUpdated(status);
                    }
                }
            };
        }

        public void StopGuidance()
        {
            guidanceCancellationTS.Cancel();
        }

        private void OnStatusUpdated(Status status)
        {
            lastStatus = currentStatus;
            currentStatus = status;

            if (currentStatus != null && lastStatus != null)
            {
                if (!string.IsNullOrEmpty(currentStatus.bodyname) && !string.IsNullOrEmpty(EDDI.Instance.CurrentStarSystem?.systemname))
                {
                    TryEngageGuidanceSystem(EDDI.Instance.CurrentStarSystem.systemname, currentStatus.bodyname);
                }
                else
                {
                    DisengageGuidanceSystem();
                }
            }
        }

        public void DisengageGuidanceSystem(bool completed = false)
        {
            if (guidanceStatus != GuidanceStatus.complete && guidanceStatus != GuidanceStatus.disengaged)
            {
                StopGuidance();
                guidanceStatus = completed ? GuidanceStatus.complete : GuidanceStatus.disengaged;
                EDDI.Instance.enqueueEvent(new GuidanceSystemEvent(DateTime.UtcNow, guidanceStatus));
            }
        }

        public bool TryEngageGuidanceSystem(string systemname, string bodyname)
        {
            // Find a guidance-eligible bookmark that matches a given systemname and bodyname, if any
            var navBookmark = bookmarks.FirstOrDefault(b =>
                b.isset &&
                string.Equals(b.systemname, systemname, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(b.bodyname, bodyname, StringComparison.InvariantCultureIgnoreCase) &&
                b.latitude != null &&
                b.longitude != null);
            if (navBookmark != null)
            {
                // Activate guidance system when near an eligible bookmark
                EngageGuidanceSystem(navBookmark);
            }
            return navBookmark != null;
        }

        public void EngageGuidanceSystem(NavBookmark navBookmark)
        {
            if (guidanceStatus != GuidanceStatus.engaged && guidanceStatus != GuidanceStatus.update)
            {
                guidanceTask = Task.Run(() => GuidanceSystemLoop(navBookmark), guidanceCancellationTS.Token);
                guidanceTask.GetAwaiter().OnCompleted(() =>
                {
                    guidanceCancellationTS.Dispose();
                    guidanceCancellationTS = new CancellationTokenSource();
                });
            }
        }

        private void GuidanceSystemLoop(NavBookmark navBookmark)
        {
            if (navBookmark is null || !navConfig.guidanceSystemEnabled) { return; }

            do
            {
                // Don't update guidance while we're not near the surface of our bookmarked body, while we're ascending
                var ascendingInShip = EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP && currentStatus?.altitude > lastStatus?.altitude;
                if (currentStatus == null || !currentStatus.near_surface
                    || navBookmark.bodyname != currentStatus.bodyname
                    || ascendingInShip)
                {
                    Thread.Sleep(guidanceIntervalMilliseconds);
                    continue;
                }

                // Determine our distance and altitude in kilometers
                var surfaceDistanceKm = SurfaceDistanceKm(currentStatus, navBookmark.latitude, navBookmark.longitude);
                var altitudeKm = currentStatus.altitude / 1000;

                if (surfaceDistanceKm != null && altitudeKm != null)
                {
                    var atDestination = surfaceDistanceKm < 3.0M;
                    if (!atDestination)
                    {
                        if (guidanceStatus != GuidanceStatus.engaged && guidanceStatus != GuidanceStatus.update)
                        {
                            // Guidance system is active and tracking a bookmark where it wasn't before.
                            guidanceStatus = GuidanceStatus.engaged;
                            EDDI.Instance.enqueueEvent(new GuidanceSystemEvent(DateTime.UtcNow, guidanceStatus, navBookmark));
                        }

                        // Update guidance
                        guidanceStatus = GuidanceStatus.update;
                        UpdateGuidanceSystem(navBookmark, surfaceDistanceKm, altitudeKm);
                    }
                    else if (guidanceStatus != GuidanceStatus.complete)
                    {
                        // We've arrived - guidance system deactivated
                        DisengageGuidanceSystem(true);
                    }
                }
                Thread.Sleep(guidanceIntervalMilliseconds);
            }
            while (navConfig.guidanceSystemEnabled && navBookmark.isset && guidanceStatus != GuidanceStatus.complete);
        }

        private void UpdateGuidanceSystem(NavBookmark navBookmark, decimal? surfaceDistanceKm, decimal? altitudeKm)
        {
            if (navBookmark is null || surfaceDistanceKm is null || altitudeKm is null) { return; }

            // Determine our heading
            decimal? heading = SurfaceHeadingDegrees(currentStatus, navBookmark.latitude, navBookmark.longitude);
            decimal? headingError = HeadingError(heading, currentStatus.heading);

            // Determine our slope
            decimal? slope = null;
            decimal? slopeError = null;
            if (currentStatus != null && currentStatus.near_surface && currentStatus?.slope != null && currentStatus.altitude != null)
            {
                slope = (decimal)Math.Round(Math.Atan2((double)altitudeKm, (double)surfaceDistanceKm) * 180 / Math.PI, 4) * -1;
                slopeError = slope - currentStatus.slope;
            }

            // If we are moving in a straight line and our heading error is greater than our slope error,
            // we'll assume we know where we are going even if we're off course.
            var headingVariation = Math.Abs(HeadingError(currentStatus.heading, lastStatus.heading) ?? 0);
            if (headingVariation < 2 && headingError > slopeError) { return; }

            var trueDistanceKm = (decimal?)Math.Sqrt((double)(surfaceDistanceKm * surfaceDistanceKm + altitudeKm * altitudeKm));
            EDDI.Instance.enqueueEvent(new GuidanceSystemEvent(DateTime.UtcNow, guidanceStatus, navBookmark, heading, headingError, slope, slopeError, trueDistanceKm));
        }

        private static decimal? HeadingError(decimal? heading1, decimal? heading2)
        {
            return ((heading2 - heading1 + 540) % 360) - 180;
        }

        public static void SurfaceCoordinates(Status curr, out decimal? destinationLatitude, out decimal? destinationLongitude)
        {
            Functions.SurfaceCoordinates(curr.altitude, curr.planetradius, curr.slope, curr.heading, curr.latitude, curr.longitude, out destinationLatitude, out destinationLongitude);
        }

        public static decimal? SurfaceDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            return Functions.SurfaceDistanceKm(curr.planetradius, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude);
        }

        public static decimal? SurfaceHeadingDegrees(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            return Functions.SurfaceHeadingDegrees(curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude);
        }
    }
}
