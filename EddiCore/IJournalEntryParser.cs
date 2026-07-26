using EddiEvents;
using System.Collections.Generic;

namespace EddiCore
{
    public interface IJournalEntryParser
    {
        List<Event> ParseJournalEntry ( string line, bool fromLogLoad = false, bool deferSyntheticEvents = true );
    }
}
