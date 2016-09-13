using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    /// <summary>
    /// A map of sample events for each event type, keyed on the event name
    /// </summary>
    public class SampleEvents
    {
        public static Dictionary<string, Event> SAMPLES = new Dictionary<string, Event>();

        static SampleEvents()
        {
            SAMPLES.Add(BondAwardedEvent.NAME, BondAwardedEvent.SAMPLE);
            SAMPLES.Add(BountyAwardedEvent.NAME, BountyAwardedEvent.SAMPLE);
            SAMPLES.Add(BountyIncurredEvent.NAME, BountyIncurredEvent.SAMPLE);
            SAMPLES.Add(CargoCollectedEvent.NAME, CargoCollectedEvent.SAMPLE);
            SAMPLES.Add(CockpitBreachedEvent.NAME, CockpitBreachedEvent.SAMPLE);
            SAMPLES.Add(CombatPromotionEvent.NAME, CombatPromotionEvent.SAMPLE);
            SAMPLES.Add(DockedEvent.NAME, DockedEvent.SAMPLE);
            SAMPLES.Add(EnteredNormalSpaceEvent.NAME, EnteredNormalSpaceEvent.SAMPLE);
            SAMPLES.Add(EnteredSupercruiseEvent.NAME, EnteredSupercruiseEvent.SAMPLE);
            SAMPLES.Add(ExplorationPromotionEvent.NAME, ExplorationPromotionEvent.SAMPLE);
            SAMPLES.Add(FineIncurredEvent.NAME, FineIncurredEvent.SAMPLE);
            SAMPLES.Add(JumpedEvent.NAME, JumpedEvent.SAMPLE);
            SAMPLES.Add(LiftoffEvent.NAME, LiftoffEvent.SAMPLE);
            SAMPLES.Add(StarScannedEvent.NAME, StarScannedEvent.SAMPLE);
            SAMPLES.Add(StartedEvent.NAME, StartedEvent.SAMPLE);
            SAMPLES.Add(TouchdownEvent.NAME, TouchdownEvent.SAMPLE);
            SAMPLES.Add(TradePromotionEvent.NAME, TradePromotionEvent.SAMPLE);
            SAMPLES.Add(UndockedEvent.NAME, UndockedEvent.SAMPLE);
        }
    }
}
