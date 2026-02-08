using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiInaraResponder;
using EddiInaraService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class InaraResponderTests : TestBase
    {
        private InaraResponder responder;
        private FakeInaraService fakeInaraService;

        /// <summary>
        /// Fake implementation of IInaraService for testing purposes.
        /// </summary>
        private class FakeInaraService : IInaraService
        {
            public List<InaraAPIEvent> EnqueuedEvents { get; } = new List<InaraAPIEvent>();

            public void Start ( bool eddiIsBeta = false )
            {
                // No-op for testing
            }

            public void Stop ()
            {
                // No-op for testing
            }

            public void EnqueueAPIEvent ( InaraAPIEvent inaraAPIEvent )
            {
                EnqueuedEvents.Add( inaraAPIEvent );
            }

            public Task<List<InaraResponse>> SendEventBatchAsync ( List<InaraAPIEvent> events, EddiConfigService.Configurations.InaraConfiguration inaraConfiguration = null )
            {
                return Task.FromResult( new List<InaraResponse>() );
            }

            public Task<InaraCmdr> GetCommanderProfileAsync ( string cmdrName = null )
            {
                return Task.FromResult<InaraCmdr>( null );
            }

            public Task<List<InaraCmdr>> GetCommanderProfilesAsync ( IList<string> cmdrNames )
            {
                return Task.FromResult( new List<InaraCmdr>() );
            }

            public bool checkAPIcredentialsOk ( EddiConfigService.Configurations.InaraConfiguration inaraConfiguration )
            {
                return true;
            }
        }

        [TestInitialize]
        public void Setup()
        {
            MakeSafe();
            fakeInaraService = new FakeInaraService();
            responder = new InaraResponder { inaraService = fakeInaraService };
        }

        #region ResponderMetadata Tests

        [TestMethod]
        public void ResponderName_ReturnsValidName()
        {
            // Act
            var name = responder.ResponderName();

            // Assert
            Assert.IsNotNull(name);
            Assert.IsFalse(string.IsNullOrWhiteSpace(name));
        }

        [TestMethod]
        public void LocalizedResponderName_ReturnsValidName()
        {
            // Act
            var name = responder.LocalizedResponderName();

            // Assert
            Assert.IsNotNull(name);
            Assert.IsFalse(string.IsNullOrWhiteSpace(name));
        }

        [TestMethod]
        public void ResponderDescription_ReturnsValidDescription()
        {
            // Act
            var description = responder.ResponderDescription();

            // Assert
            Assert.IsNotNull(description);
            Assert.IsFalse(string.IsNullOrWhiteSpace(description));
        }

        #endregion

        #region HandleAsync Tests - Event Filtering

        [TestMethod]
        public void HandleAsync_WithNullEvent_ReturnsCompletedTask()
        {
            // Act
            var task = responder.HandleAsync(null);

            // Assert
            Assert.IsTrue(task.IsCompleted && !task.IsFaulted );
        }

        [TestMethod]
        public void HandleAsync_WhenInTelepresence_ReturnsCompletedTask()
        {
            // Arrange
            EDDI.Instance.inTelepresence = true;
            var @event = new DiedEvent(DateTime.UtcNow, new List<Killer>());

            // Act
            var task = responder.HandleAsync(@event);

            // Assert
            Assert.IsTrue(task.IsCompleted && !task.IsFaulted );

            // Cleanup
            EDDI.Instance.inTelepresence = false;
        }

        [TestMethod]
        public void HandleAsync_WhenGameIsBeta_ReturnsCompletedTask()
        {
            // Arrange
            EDDI.Instance.gameIsBeta = true;
            var @event = new DiedEvent(DateTime.UtcNow, new List<Killer>());

            // Act
            var task = responder.HandleAsync(@event);

            // Assert
            Assert.IsTrue(task.IsCompleted && !task.IsFaulted);

            // Cleanup
            EDDI.Instance.gameIsBeta = false;
        }

        [TestMethod]
        public void HandleAsync_WhenGameVersionNull_ReturnsCompletedTask()
        {
            // Arrange
            var currentVersion = EDDI.Instance.GameVersion;
            EDDI.Instance.GameVersion = null;
            var @event = new DiedEvent(DateTime.UtcNow, new List<Killer>());

            // Act
            var task = responder.HandleAsync(@event);

            // Assert
            Assert.IsTrue(task.IsCompleted && !task.IsFaulted );

            // Cleanup
            EDDI.Instance.GameVersion = currentVersion;
        }

        [TestMethod]
        public void HandleAsync_WhenGameVersionBelowMinimum_ReturnsCompletedTask()
        {
            // Arrange
            var currentVersion = EDDI.Instance.GameVersion;
            EDDI.Instance.GameVersion = new Version(3, 9);
            EDDI.Instance.inTelepresence = false;
            EDDI.Instance.gameIsBeta = false;
            var @event = new DiedEvent(DateTime.UtcNow, new List<Killer>());

            // Act
            var task = responder.HandleAsync(@event);

            // Assert
            Assert.IsTrue(task.IsCompleted && !task.IsFaulted );

            // Cleanup
            EDDI.Instance.GameVersion = currentVersion;
        }

        [TestMethod]
        public void HandleAsync_WhenEventTimestampOlderThan30Days_ReturnsCompletedTask()
        {
            // Arrange
            var oldTimestamp = DateTime.UtcNow.AddDays(-31);
            EDDI.Instance.inTelepresence = false;
            EDDI.Instance.gameIsBeta = false;
            EDDI.Instance.GameVersion = new Version(4, 0);
            var @event = new DiedEvent(oldTimestamp, new List<Killer>());

            // Act
            var task = responder.HandleAsync(@event);

            // Assert
            Assert.IsTrue(task.IsCompleted && !task.IsFaulted );
        }

        #endregion

        #region GetModuleData Tests

        [TestMethod]
        public void GetModuleData_WithNullModule_ReturnsSlotNameOnly()
        {
            // Act
            var result = InaraResponder.GetModuleData("Slot01", null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Slot01", result["slotName"]);
        }

        [TestMethod]
        public void GetModuleData_WithValidModule_ReturnsCompleteModuleData()
        {
            // Arrange
            var module = new Module("TestModule", "TestModule", 5, "A", 1000, ModuleMount.Fixed, 30, 100 )
            {
                price = 1000,
                health = 100,
                enabled = true,
                hot = false,
                priority = 1,
                modified = false
            };

            // Act
            var result = InaraResponder.GetModuleData("Slot01", module);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey("slotName"));
            Assert.IsTrue(result.ContainsKey("itemName"));
            Assert.IsTrue(result.ContainsKey("itemValue"));
            Assert.IsTrue(result.ContainsKey("itemHealth"));
            Assert.AreEqual("Slot01", result["slotName"]);
            Assert.AreEqual("TestModule", result["itemName"]);
            Assert.AreEqual(1000L, result["itemValue"]);
            Assert.AreEqual(1.0M, result["itemHealth"]);
        }

        [TestMethod]
        public void GetModuleData_WithModifiedModule_IncludesEngineeringData()
        {
            // Arrange
            var modifier = new EngineeringModifier
            {
                EDName = "TestModifier",
                currentValue = 1.5M,
                originalValue = 1.0M,
                lessIsGood = false,
                valueStr = null
            };

            var module = new Module("ModifiedModule", "ModifiedModule", 5, "A", 5000, ModuleMount.Fixed, 30, 100)
            {
                health = 100,
                enabled = true,
                hot = false,
                price = 5000,
                priority = 1,
                modified = true,
                modificationEDName = "Blueprint",
                engineerlevel = 3,
                engineerquality = 80,
                engineerExperimentalEffectEDName = "ExperimentalEffect",
                modifiers = new List<EngineeringModifier> { modifier }
            };

            // Act
            var result = InaraResponder.GetModuleData("Slot01", module);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey("engineering"));
            var engineering = result["engineering"] as Dictionary<string, object>;
            Assert.IsNotNull(engineering);
            Assert.AreEqual("Blueprint", engineering["blueprintName"]);
            Assert.AreEqual(3, engineering["blueprintLevel"]);
            Assert.AreEqual(80M, engineering["blueprintQuality"]);
            Assert.AreEqual("ExperimentalEffect", engineering["experimentalEffect"]);
        }

        #endregion

        #region Minor Faction Reputation Tests

        [TestMethod]
        public void MinorFactionReputations_WithNeutralReputation_ExcludesFaction()
        {
            // Arrange
            var factions = new List<Faction>
            {
                new Faction { name = "NeutralFaction", myreputation = 0 }
            };

            // Act
            var method = typeof(InaraResponder).GetMethod(
                "minorFactionReputations",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (List<Dictionary<string, object>>)method?.Invoke(null, new object[] { factions });

            // Assert
            Assert.AreEqual(0, result?.Count);
        }

        [TestMethod]
        public void MinorFactionReputations_WithAlliedReputation_IncludesFaction()
        {
            // Arrange
            var factions = new List<Faction>
            {
                new Faction { name = "AlliedFaction", myreputation = 50 }
            };

            // Act
            var method = typeof(InaraResponder).GetMethod(
                "minorFactionReputations",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (List<Dictionary<string, object>>)method?.Invoke(null, new object[] { factions });

            // Assert
            Assert.AreEqual(1, result?.Count);
            Assert.AreEqual("AlliedFaction", result?[0]["minorfactionName"]);
            Assert.AreEqual(0.5M, result?[0]["minorfactionReputation"]);
        }

        [TestMethod]
        public void MinorFactionReputations_WithHostileReputation_IncludesFaction()
        {
            // Arrange
            var factions = new List<Faction>
            {
                new Faction { name = "HostileFaction", myreputation = -100 }
            };

            // Act
            var method = typeof(InaraResponder).GetMethod(
                "minorFactionReputations",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (List<Dictionary<string, object>>)method?.Invoke(null, new object[] { factions });

            // Assert
            Assert.AreEqual(1, result?.Count);
            Assert.AreEqual("HostileFaction", result?[0]["minorfactionName"]);
            Assert.AreEqual(-1.0M, result?[0]["minorfactionReputation"]);
        }

        #endregion

        #region Specific Event Handler Tests

        [TestMethod]
        public void HandleDiedEvent_EnqueuesEvent()
        {
            // Arrange
            var killers = new List<Killer>
            {
                new Killer ("Killer1", "Adder", CombatRating.Competent)
            };
            var @event = new DiedEvent(DateTime.UtcNow, killers);
            EDDI.Instance.inTelepresence = false;
            EDDI.Instance.gameIsBeta = false;
            EDDI.Instance.GameVersion = new Version(4, 0);

            // Act
            var task = responder.HandleAsync(@event);

            // Assert
            Assert.IsTrue(task.IsCompleted && !task.IsFaulted );
            Assert.IsTrue(fakeInaraService.EnqueuedEvents.Count > 0);
        }

        [TestMethod]
        public void HandleCargoEvent_WithInventory_EnqueuesEventData()
        {
            // Arrange
            var inventory = new List<CargoInfoItem>
            {
                new CargoInfoItem { name = "Commodity1", count = 10 },
                new CargoInfoItem { name = "Commodity2", count = 20 }
            };
            var @event = new CargoEvent(DateTime.UtcNow, true, "Ship", inventory, 30 );
            EDDI.Instance.inTelepresence = false;
            EDDI.Instance.gameIsBeta = false;
            EDDI.Instance.GameVersion = new Version(4, 0);

            // Act
            var task = responder.HandleAsync(@event);

            // Assert
            Assert.IsTrue(task.IsCompleted && !task.IsFaulted );
            Assert.IsTrue(fakeInaraService.EnqueuedEvents.Count > 0);
        }
        
        #endregion

        #region Configuration Tests

        [TestMethod]
        public void ConfigurationTabItem_ReturnsNonNull()
        {
            // Act
            var tabItem = responder.ConfigurationTabItem();

            // Assert
            Assert.IsNotNull(tabItem);
        }

        #endregion

        #region HandleStatusAsync Tests

        [TestMethod]
        public void HandleStatusAsync_AlwaysReturnsCompletedTask()
        {
            // Arrange
            var status = new Status();

            // Act
            var task = responder.HandleStatusAsync(status);

            // Assert
            Assert.IsTrue(task.IsCompleted && !task.IsFaulted );
        }

        #endregion
    }
}