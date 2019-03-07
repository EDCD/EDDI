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
        private bool speechQueueHandlerRunning;

        public event EventHandler SpeechReadyEvent;
        protected virtual void SendToSpeechServiceThread(EddiSpeech speech, EventArgs @eventArgs)
        {
            SpeechReadyEvent?.Invoke(speech, @eventArgs);
        }

        public SpeechQueue()
        {
            PrepareSpeechQueues();
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
            enqueueSpeech(speech);
        }

        public void StartOrContinue()
        {
            if (!speechQueueHandlerRunning)
            {
                speechQueueHandlerRunning = true;
                while (speechQueueHandlerRunning && priorityQueues.Any(q => q.Count > 0))
                {
                    EddiSpeech speech = dequeueSpeech();
                    if (speech != null)
                    {
                        SendToSpeechServiceThread(speech, EventArgs.Empty);
                    }
                }
                speechQueueHandlerRunning = false;
            }
        }

        public void Stop()
        {
            speechQueueHandlerRunning = false;
        }

        private void enqueueSpeech(EddiSpeech speech)
        {
            if (speech == null) { return; }
            if (priorityQueues.ElementAtOrDefault(speech.priority) != null)
            {
                dequeueStaleSpeech(speech);
                priorityQueues[speech.priority].Enqueue(speech);
            }
        }

        private EddiSpeech dequeueSpeech()
        {
            for (int i = 0; i < priorityQueues.Count; i++)
            {
                if (priorityQueues[i].TryDequeue(out EddiSpeech speech))
                {
                    return speech;
                }
            }
            return null;
        }

        public EddiSpeech peekSpeech()
        {
            for (int i = 0; i < priorityQueues.Count; i++)
            {
                if (priorityQueues[i].TryPeek(out EddiSpeech speech))
                {
                    return speech;
                }
            }
            return null;
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
