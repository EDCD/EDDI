using EddiSpanshService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System.Collections.Generic;
using System;
using System.Net;
using Tests.Properties;

namespace UnitTests
{
    // A mock rest client for the Spansh Service
    internal class FakeSpanshRestClient : ISpanshRestClient
    {
        public Dictionary<string, string> CannedContent = new Dictionary<string, string>();

        public Uri BuildUri(IRestRequest request)
        {
            return new Uri("fakeSpansh://" + request.Resource);
        }

        IRestResponse<T> ISpanshRestClient.Execute<T>(IRestRequest request)
        {
            // this will throw if given a resource not in the canned dictionaries: that's OK
            string content = CannedContent[request.Resource];
            IRestResponse<T> restResponse = new RestResponse<T>
            {
                Content = content,
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = HttpStatusCode.OK,
            };
            return restResponse;
        }

        IRestResponse ISpanshRestClient.Get(IRestRequest request)
        {
            // this will throw if given a resource not in the canned dictionaries: that's OK
            string content = CannedContent[request.Resource];
            IRestResponse restResponse = new RestResponse
            {
                Content = content,
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = HttpStatusCode.OK,
            };
            return restResponse;
        }

        public void Expect(string resource, string content)
        {
            CannedContent[resource] = content;
        }
    }

    [TestClass]
    public class SpanshServiceTests : TestBase
    {
        FakeSpanshRestClient fakeSpanshRestClient;
        private SpanshService fakeSpanshService;

        [TestInitialize]
        public void start()
        {
            fakeSpanshRestClient = new FakeSpanshRestClient();
            fakeSpanshService = new SpanshService(fakeSpanshRestClient);
            MakeSafe();
        }

        [TestMethod]
        public void TestSpanshCarrierRoute()
        {
            // Arrange
            fakeSpanshRestClient.Expect("fleetcarrier/route", "{\"job\":\"F2B5B476-4458-11ED-9B9F-5DE194EB4526\",\"status\":\"queued\"}");
            fakeSpanshRestClient.Expect("results/F2B5B476-4458-11ED-9B9F-5DE194EB4526", DeserializeJsonResource<string>(Resources.SpanshCarrierResult));

            // Act
            var result = fakeSpanshService.GetCarrierRoute("NLTT 13249", new[] { "Sagittarius A*" }, 25000);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.FillVisitedGaps);
            Assert.IsFalse(result.GuidanceEnabled);
            Assert.AreEqual(26021.94M, result.RouteDistance);
            Assert.AreEqual(6845, result.RouteFuelTotal);
            Assert.AreEqual(54, result.Waypoints.Count);

            Assert.AreEqual(0M, result.Waypoints[0].distance);
            Assert.AreEqual(26021.94M, result.Waypoints[0].distanceRemaining);
            Assert.AreEqual(0M, result.Waypoints[0].distanceTraveled);
            Assert.AreEqual(6845, result.Waypoints[0].fuelNeeded);
            Assert.AreEqual(0, result.Waypoints[0].fuelUsed);
            Assert.AreEqual(0, result.Waypoints[0].fuelUsedTotal);
            Assert.IsFalse(result.Waypoints[0].hasIcyRing);
            Assert.IsFalse(result.Waypoints[0].hasNeutronStar);
            Assert.IsFalse(result.Waypoints[0].hasPristineMining);
            Assert.AreEqual(0, result.Waypoints[0].index);
            Assert.IsTrue(result.Waypoints[0].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[0].isMissionSystem);
            Assert.AreEqual(0, result.Waypoints[0].missionids.Count);
            Assert.IsFalse(result.Waypoints[0].refuelRecommended);
            Assert.AreEqual((ulong)2869440882065, result.Waypoints[0].systemAddress);
            Assert.AreEqual("NLTT 13249", result.Waypoints[0].systemName);
            Assert.IsFalse(result.Waypoints[0].visited);
            Assert.AreEqual(-38.8125M, result.Waypoints[0].x);
            Assert.AreEqual(9.3125M, result.Waypoints[0].y);
            Assert.AreEqual(-60.53125M, result.Waypoints[0].z);

            Assert.AreEqual(499.52M, result.Waypoints[3].distance);
            Assert.AreEqual(24522.68M, result.Waypoints[3].distanceRemaining);
            Assert.AreEqual(1499.26M, result.Waypoints[3].distanceTraveled);
            Assert.AreEqual(6449, result.Waypoints[3].fuelNeeded);
            Assert.AreEqual(132, result.Waypoints[3].fuelUsed);
            Assert.AreEqual(396, result.Waypoints[3].fuelUsedTotal);
            Assert.IsTrue(result.Waypoints[3].hasIcyRing);
            Assert.IsFalse(result.Waypoints[3].hasNeutronStar);
            Assert.IsTrue(result.Waypoints[3].hasPristineMining);
            Assert.AreEqual(3, result.Waypoints[3].index);
            Assert.IsFalse(result.Waypoints[3].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[3].isMissionSystem);
            Assert.AreEqual(0, result.Waypoints[3].missionids.Count);
            Assert.IsFalse(result.Waypoints[3].refuelRecommended);
            Assert.AreEqual((ulong)18262335236073, result.Waypoints[3].systemAddress);
            Assert.AreEqual("Praea Euq IQ-W b56-8", result.Waypoints[3].systemName);
            Assert.IsFalse(result.Waypoints[3].visited);
            Assert.AreEqual(-51.15625M, result.Waypoints[3].x);
            Assert.AreEqual(-2.96875M, result.Waypoints[3].y);
            Assert.AreEqual(1437.34375M, result.Waypoints[3].z);

            Assert.AreEqual(34.82M, result.Waypoints[53].distance);
            Assert.AreEqual(0M, result.Waypoints[53].distanceRemaining);
            Assert.AreEqual(26021.94M, result.Waypoints[53].distanceTraveled);
            Assert.AreEqual(0, result.Waypoints[53].fuelNeeded);
            Assert.AreEqual(14, result.Waypoints[53].fuelUsed);
            Assert.AreEqual(6845, result.Waypoints[53].fuelUsedTotal);
            Assert.IsFalse(result.Waypoints[53].hasIcyRing);
            Assert.IsFalse(result.Waypoints[53].hasNeutronStar);
            Assert.IsFalse(result.Waypoints[53].hasPristineMining);
            Assert.AreEqual(53, result.Waypoints[53].index);
            Assert.IsTrue(result.Waypoints[53].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[53].isMissionSystem);
            Assert.AreEqual(0, result.Waypoints[53].missionids.Count);
            Assert.IsFalse(result.Waypoints[53].refuelRecommended);
            Assert.AreEqual((ulong)20578934, result.Waypoints[53].systemAddress);
            Assert.AreEqual("Sagittarius A*", result.Waypoints[53].systemName);
            Assert.IsFalse(result.Waypoints[53].visited);
            Assert.AreEqual(25.21875M, result.Waypoints[53].x);
            Assert.AreEqual(-20.90625M, result.Waypoints[53].y);
            Assert.AreEqual(25899.96875M, result.Waypoints[53].z);
        }

        [TestMethod]
        public void TestSpanshGalaxyRoute()
        {
            // Arrange
            fakeSpanshRestClient.Expect("generic/route", "{\"job\":\"F2B5B476-4458-11ED-9B9F-5DE194EB4527\",\"status\":\"queued\"}");
            fakeSpanshRestClient.Expect("results/F2B5B476-4458-11ED-9B9F-5DE194EB4527", DeserializeJsonResource<string>(Resources.SpanshGalaxyResult));

            // Act
            var ship = EddiDataDefinitions.ShipDefinitions.FromEDModel("Anaconda");
            ship.frameshiftdrive = EddiDataDefinitions.Module.FromEDName("Int_Hyperdrive_Size6_Class5");
            var result = fakeSpanshService.GetGalaxyRoute("NLTT 13249", "Soul Sector EL-Y d7", ship);

            // Assert[9]
            Assert.IsNotNull(result);
            Assert.IsTrue(result.FillVisitedGaps);
            Assert.IsFalse(result.GuidanceEnabled);
            Assert.AreEqual(8178.36M, result.RouteDistance);
            Assert.AreEqual(259, result.Waypoints.Count);

            Assert.AreEqual(0M, result.Waypoints[0].distance);
            Assert.AreEqual(8178.36M, result.Waypoints[0].distanceRemaining);
            Assert.AreEqual(0M, result.Waypoints[0].distanceTraveled);
            Assert.IsFalse(result.Waypoints[0].hasNeutronStar);
            Assert.AreEqual(0, result.Waypoints[0].index);
            Assert.IsFalse(result.Waypoints[0].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[0].isMissionSystem);
            Assert.IsFalse(result.Waypoints[0].isScoopable);
            Assert.AreEqual(0, result.Waypoints[0].missionids.Count);
            Assert.IsFalse(result.Waypoints[0].refuelRecommended);
            Assert.AreEqual((ulong)2869440882065, result.Waypoints[0].systemAddress);
            Assert.AreEqual("NLTT 13249", result.Waypoints[0].systemName);
            Assert.IsFalse(result.Waypoints[0].visited);
            Assert.AreEqual(-38.8125M, result.Waypoints[0].x);
            Assert.AreEqual(9.3125M, result.Waypoints[0].y);
            Assert.AreEqual(-60.53125M, result.Waypoints[0].z);

            Assert.AreEqual(30.06M, result.Waypoints[63].distance);
            Assert.AreEqual(6311.29M, result.Waypoints[63].distanceRemaining);
            Assert.AreEqual(1867.07M, result.Waypoints[63].distanceTraveled);
            Assert.IsTrue(result.Waypoints[63].hasNeutronStar);
            Assert.AreEqual(63, result.Waypoints[63].index);
            Assert.IsFalse(result.Waypoints[63].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[63].isMissionSystem);
            Assert.IsFalse(result.Waypoints[63].isScoopable);
            Assert.AreEqual(0, result.Waypoints[63].missionids.Count);
            Assert.IsFalse(result.Waypoints[63].refuelRecommended);
            Assert.AreEqual((ulong)147647924467, result.Waypoints[63].systemAddress);
            Assert.AreEqual("Outopps AS-B d13-4", result.Waypoints[63].systemName);
            Assert.IsFalse(result.Waypoints[63].visited);
            Assert.AreEqual(-1276.5M, result.Waypoints[63].x);
            Assert.AreEqual(182.53125M, result.Waypoints[63].y);
            Assert.AreEqual(-1182.375M, result.Waypoints[63].z);

            Assert.AreEqual(30.74M, result.Waypoints[254].distance);
            Assert.AreEqual(201.48M, result.Waypoints[254].distanceRemaining);
            Assert.AreEqual(7976.88M, result.Waypoints[254].distanceTraveled);
            Assert.IsFalse(result.Waypoints[254].hasNeutronStar);
            Assert.AreEqual(254, result.Waypoints[254].index);
            Assert.IsFalse(result.Waypoints[254].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[254].isMissionSystem);
            Assert.IsTrue(result.Waypoints[254].isScoopable);
            Assert.AreEqual(0, result.Waypoints[254].missionids.Count);
            Assert.IsTrue(result.Waypoints[254].refuelRecommended);
            Assert.AreEqual((ulong)937166915403, result.Waypoints[254].systemAddress);
            Assert.AreEqual("Hypoae Ain LI-K d8-27", result.Waypoints[254].systemName);
            Assert.IsFalse(result.Waypoints[254].visited);
            Assert.AreEqual(-4917.28125M, result.Waypoints[254].x);
            Assert.AreEqual(74.8125M, result.Waypoints[254].y);
            Assert.AreEqual(-5385.25M, result.Waypoints[254].z);

            Assert.AreEqual(118.13M, result.Waypoints[258].distance);
            Assert.AreEqual(0M, result.Waypoints[258].distanceRemaining);
            Assert.AreEqual(8178.36M, result.Waypoints[258].distanceTraveled);
            Assert.IsFalse(result.Waypoints[258].hasNeutronStar);
            Assert.AreEqual(258, result.Waypoints[258].index);
            Assert.IsFalse(result.Waypoints[258].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[258].isMissionSystem);
            Assert.IsFalse(result.Waypoints[258].isScoopable);
            Assert.AreEqual(0, result.Waypoints[258].missionids.Count);
            Assert.IsFalse(result.Waypoints[258].refuelRecommended);
            Assert.AreEqual((ulong)249938593603, result.Waypoints[258].systemAddress);
            Assert.AreEqual("Soul Sector EL-Y d7", result.Waypoints[258].systemName);
            Assert.IsFalse(result.Waypoints[258].visited);
            Assert.AreEqual(-5043.15625M, result.Waypoints[258].x);
            Assert.AreEqual(85.03125M, result.Waypoints[258].y);
            Assert.AreEqual(-5513.09375M, result.Waypoints[258].z);
        }
    }
}
