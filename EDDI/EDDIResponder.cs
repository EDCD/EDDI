using EliteDangerousEvents;
using EliteDangerousJournalMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDDI
{
    /// <summary>
    /// The methods required for an EDDI responder.
    /// </summary>
    public interface EDDIResponder
    {
        void Start();

        void Stop();

        void Handle(Event theEvent);
    }
}
