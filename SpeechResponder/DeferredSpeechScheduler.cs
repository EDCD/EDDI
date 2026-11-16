using EddiEvents;
using EddiScriptResolverService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpeechResponder
{
    // Owns only speech requests. It never re-enqueues journal events or changes core event delivery.
    internal sealed class DeferredSpeechScheduler
    {
        private readonly SemaphoreSlim rendering = new( 1, 1 );
        private readonly object sync = new();
        private readonly List<PendingSpeech> pending = new();
        private readonly Func<TimeSpan, Func<Task>, IDisposable> schedule;
        private long generation;
        private long sequence;

        private sealed class PendingSpeech
        {
            internal DeferredSpeechRequest Request;
            internal Event Origin;
            internal Func<bool, Task> Render;
            internal IDisposable Timer;
            internal long Generation;
            internal long Sequence;
        }

        internal DeferredSpeechScheduler ( Func<TimeSpan, Func<Task>, IDisposable> schedule = null )
        {
            this.schedule = schedule ?? ( ( delay, callback ) => new Timer(
                _ => callback().SafeFireAndForget( ex => Logging.Error( "Deferred speech failed", ex ) ),
                null, delay, Timeout.InfiniteTimeSpan ) );
        }

        internal long Generation { get { lock ( sync ) { return generation; } } }

        internal long LastSequence { get { lock ( sync ) { return sequence; } } }

        // Both ordinary and deferred renders use this gate. Waiting for a timer never holds it.
        internal async Task RunAsync ( Func<Task> render )
        {
            await rendering.WaitAsync().ConfigureAwait( false );
            try { await render().ConfigureAwait( false ); }
            finally { rendering.Release(); }
        }

        internal void Schedule ( DeferredSpeechRequest request, Event origin, long expectedGeneration,
            Func<bool, Task> render )
        {
            lock ( sync )
            {
                if ( generation != expectedGeneration || origin?.fromLoad == true ) { return; }
                if ( pending.Count >= 32 )
                {
                    throw new InvalidOperationException( "At most 32 SpeakAfter reports may be pending." );
                }
                var entry = new PendingSpeech
                {
                    Request = request, Origin = origin, Render = render, Generation = generation, Sequence = ++sequence
                };
                pending.Add( entry );
                try
                {
                    entry.Timer = schedule( TimeSpan.FromMilliseconds( request.MaximumDelayMs ),
                        () => RunAsync( () => ReleaseAsync( entry, false ) ) );
                }
                catch
                {
                    pending.Remove( entry );
                    throw;
                }
            }
        }

        // Called under the render gate, after the awaited top-level speech script has finished.
        internal async Task ScriptCompletedAsync ( string scriptName, Event completedEvent, long maximumSequence = long.MaxValue )
        {
            PendingSpeech[] ready;
            lock ( sync )
            {
                ready = pending.Where( p => p.Sequence <= maximumSequence && p.Request.WaitForScript == scriptName &&
                    Matches( p.Origin, completedEvent, scriptName ) ).ToArray();
            }
            foreach ( var entry in ready )
            {
                await ReleaseAsync( entry, true ).ConfigureAwait( false );
            }
        }

        private static bool Matches ( Event origin, Event completed, string scriptName )
        {
            if ( completed?.fromLoad == true || ( origin != null && ReferenceEquals( origin, completed ) ) ||
                 ( origin != null && completed != null && completed.timestamp < origin.timestamp ) ) { return false; }

            // The mapping use case must never be released by FSS, replay, another body, or another system.
            if ( origin is BodyMappedEvent mapping && scriptName == SurfaceSignalsEvent.NAME )
            {
                return completed is SurfaceSignalsEvent signals && mapping.bodyId != null && signals.detectionType == "SAA" &&
                       signals.systemAddress == mapping.systemAddress && signals.bodyId == mapping.bodyId;
            }
            return true;
        }

        private async Task ReleaseAsync ( PendingSpeech entry, bool matched )
        {
            lock ( sync )
            {
                if ( !pending.Remove( entry ) || entry.Generation != generation ) { return; }
                entry.Timer?.Dispose();
            }
            await entry.Render( matched ).ConfigureAwait( false );
        }

        internal void Clear ()
        {
            lock ( sync )
            {
                generation++;
                foreach ( var entry in pending ) { entry.Timer?.Dispose(); }
                pending.Clear();
            }
        }
    }
}
