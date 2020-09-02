using System;
using System.Collections.Generic;
using System.Linq;
using Eddi;
using EddiCore;
using EddiEvents;
using EddiJournalMonitor;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class MainWindowResponderTests
    {
        [TestMethod]
        public void WhenFindingResponders_MainWindowResponderSHOULDBePresent()
        {
            var responders = EDDI.Instance.findResponders();

            responders.Should()
                .ContainSingle(r => r.GetType() == typeof(MainWindowResponder));
        }

        //string line = @"{ ""timestamp"":""2018-10-17T16:17:55Z"", ""event"":""SquadronDemotion"", ""SquadronName"":""TestSquadron"", ""OldRank"":3, ""NewRank"":2 }";
        //List<Event> events = JournalMonitor.ParseJournalEntry(line);
    }
}
