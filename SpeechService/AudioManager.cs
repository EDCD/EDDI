using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpeechService
{
    public class AudioManager
    {
        private static readonly object activeAudioLock = new();
        private readonly ConcurrentDictionary<IWavePlayer, CancellationTokenSource> activeAudioTS = new();
        
        public bool eddiAudioPlaying
        {
            get
            {
                lock ( activeAudioLock )
                {
                    return !activeAudioTS.IsEmpty;
                }
            }
        }

        public async Task PlayAudioAsync ( string fileName, decimal? volumeOverride )
        {
            var absolutePath = Files.GetAbsoluteFilePath( Constants.DATA_DIR, fileName );
            await using ( var audioSource = new AudioFileReader( absolutePath ) )
            using ( var soundOut = SoundManager.GetSoundOut( audioSource ) )
            {
                if ( soundOut == null )
                {
                    return;
                }

                Logging.Debug( $"Beginning audio playback for {fileName}." );

                if ( volumeOverride != null )
                {
                    audioSource.Volume = Math.Max( Math.Min( (float)volumeOverride / 100, 1 ), 0 );
                }

                soundOut.Play();

                var cancellationTokenSource = new CancellationTokenSource();
                lock ( activeAudioLock )
                {
                    activeAudioTS.TryAdd( soundOut, cancellationTokenSource );
                }

                try
                {
                    var waitTime = audioSource.TotalTime;
                    Logging.Debug( $"Waiting for audio - {waitTime.TotalMilliseconds} ms (unless ended early)." );
                    await Task.Delay( waitTime, cancellationTokenSource.Token ).ConfigureAwait(false);
                }
                catch ( OperationCanceledException )
                {
                    // Graceful exit on cancellation
                }

                Logging.Debug( $"Ending audio playback for {fileName}." );
                lock ( activeAudioLock )
                {
                    if ( activeAudioTS.TryRemove( soundOut, out var ts ) )
                    {
                        ts.Dispose();
                    }
                }
            }
        }

        public void StopAudio ()
        {
            Logging.Debug( "Ending all audio playback." );
            lock ( activeAudioLock )
            {
                foreach ( var soundOut in activeAudioTS.Keys )
                {
                    if ( activeAudioTS.TryRemove( soundOut, out var ts ) )
                    {
                        ts.Cancel();
                        ts.Token.WaitHandle.WaitOne( TimeSpan.FromMilliseconds( 100 ) );
                        ts.Dispose();
                    }
                }
            }
        }
    }
}
