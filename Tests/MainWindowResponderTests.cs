using Eddi;
using EddiCore;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class MainWindowResponderTests
    {
        [TestMethod]
        public void WhenEDDIIsInitialized_MainWindowResponderSHOULDBePresent()
        {
            var responders = EDDI.Instance.responders;

            responders.Should()
                .ContainSingle(r => r.GetType() == typeof(MainWindowResponder));
        }

        [TestMethod]
        public void WhenAppIsLaunched_SquadronNameSHOULDBeEmpty()
        {
            //var line = @"{ ""timestamp"":""2018-10-17T16:17:55Z"", ""event"":""SquadronDemotion"", ""SquadronName"":""TestSquadron"", ""OldRank"":3, ""NewRank"":2 }";
            //var events = JournalMonitor.ParseJournalEntry(line);
            //EDDI.Instance.enqueueEvent(events.First());

            //if (Application.Current?.MainWindow != null)
            //{
            //    ((MainWindow)Application.Current.MainWindow).squadronPowerDropDown.SelectedItem = power.localizedName;

            //}
        }
    }
}
