using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Utilities;

namespace EddiSpeechService
{
    public class SpeechQueue
    {
        public List<ConcurrentQueue<EddiSpeech>> priorityQueues { get; private set; }
        public bool hasSpeech => priorityQueues.Any(q => q.Count > 0);

        public SpeechQueue()
        {
            PrepareSpeechQueues();
        }

        private static SpeechQueue instance;
        private static readonly object instanceLock = new object();
        public static SpeechQueue Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No Speech queue instance: creating one");
                            instance = new SpeechQueue();
                        }
                    }
                }
                return instance;
            }
        }

        private void PrepareSpeechQueues()
        {
            priorityQueues = new List<ConcurrentQueue<EddiSpeech>>();

            // Priority 0: System messages (always top priority)
            // Priority 1: Highest user settable priority, interrupts lower priorities
            // Priority 2: High priority
            // Priority 3: Standard priority
            // Priority 4: Low priority
            // Priority 5: Lowest priority, interrupted by any higher priority

            for (int i = 0; i <= 5; i++)
            {
                priorityQueues.Add(new ConcurrentQueue<EddiSpeech>());
            }
        }

        public void Enqueue(EddiSpeech speech)
        {
            if (speech == null) { return; }
            if (priorityQueues.ElementAtOrDefault(speech.priority) != null)
            {
                dequeueStaleSpeech(speech);
                priorityQueues[speech.priority].Enqueue(speech);
            }
        }

        public bool TryDequeue(out EddiSpeech speech)
        {
            speech = null;
            for (int i = 0; i < priorityQueues.Count; i++)
            {
                if (priorityQueues[i].TryDequeue(out EddiSpeech selectedSpeech))
                {
                    speech = selectedSpeech;
                    return true;
                }
            }
            return false;
        }

        public bool TryPeek(out EddiSpeech speech)
        {
            speech = null;
            for (int i = 0; i < priorityQueues.Count; i++)
            {
                if (priorityQueues[i].TryPeek(out EddiSpeech selectedSpeech))
                {
                    speech = selectedSpeech;
                    return true;
                }
            }
            return false;
        }

        public void DequeueAllSpeech()
        {
            // Don't clear system messages (priority 0)
            for (int i = 1; i < priorityQueues.Count; i++)
            {
                while (priorityQueues[i].TryDequeue(out EddiSpeech speech)) { }
            }
        }

        public void DequeueSpeechOfType(string type)
        {
            // Don't clear system messages (priority 0)
            for (int i = 1; i < priorityQueues.Count; i++)
            {
                ConcurrentQueue<EddiSpeech> priorityHolder = new ConcurrentQueue<EddiSpeech>();
                while (priorityQueues[i].TryDequeue(out EddiSpeech eddiSpeech)) { filterSpeechQueue(type, ref priorityHolder, eddiSpeech); };
                while (priorityHolder.TryDequeue(out EddiSpeech eddiSpeech)) { priorityQueues[i].Enqueue(eddiSpeech); };
            }
        }

        private void filterSpeechQueue(string type, ref ConcurrentQueue<EddiSpeech> speechQueue, EddiSpeech eddiSpeech)
        {
            if (!(bool)eddiSpeech?.eventType?.Contains(type))
            {
                speechQueue.Enqueue(eddiSpeech);
            }
        }

        private void dequeueStaleSpeech(EddiSpeech eddiSpeech)
        {
            // List EDDI event types of where stale event data should be removed in favor of more recent data
            string[] eventTypes = new string[]
                {
                    "Next jump",
                    "Heat damage",
                    "Heat warning",
                    "Hull damaged",
                    "Under attack"
                };

            foreach (string eventType in eventTypes)
            {
                if (eddiSpeech.eventType == eventType)
                {
                    DequeueSpeechOfType(eventType);
                }
            }
        }
    }
}
