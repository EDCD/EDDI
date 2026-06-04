using NAudio.Wave;
using NAudio.CoreAudioApi;
using EddiConfigService;
using System;
using System.Runtime.InteropServices;
using Utilities;

namespace EddiSpeechService
{
    public static class SoundManager
    {
        internal static IWavePlayer GetSoundOut ( IWaveProvider provider )
        {
            // Try WASAPI first
            try
            {
                WasapiOut wasapiOut;
                var deviceId = ConfigService.Instance.speechServiceConfiguration?.AudioDevice;
                if ( !string.IsNullOrEmpty( deviceId ) )
                {
                    try
                    {
                        var enumerator = new MMDeviceEnumerator();
                        var device = enumerator.GetDevice( deviceId );
                        wasapiOut = new WasapiOut( device, AudioClientShareMode.Shared, true, 200 );
                    }
                    catch ( Exception ex )
                    {
                        Logging.Warn( $"Failed to initialize WASAPI with selected device {deviceId}, falling back to default device.", ex );
                        wasapiOut = new WasapiOut();
                    }
                }
                else
                {
                    wasapiOut = new WasapiOut();
                }

                if ( TryInitializeSoundOut( wasapiOut, provider ) )
                {
                    return wasapiOut;
                }
                Logging.Warn( "Falling back to legacy DirectSoundOut." );
            }
            catch ( Exception ex )
            {
                Logging.Warn( "WASAPI output initialization failed, falling back to DirectSoundOut.", ex );
            }

            // Fallback: DirectSoundOut
            try
            {
                var directSoundOut = new DirectSoundOut();
                if ( TryInitializeSoundOut( directSoundOut, provider ) )
                {
                    return directSoundOut;
                }
            }
            catch ( Exception ex )
            {
                Logging.Warn( "DirectSoundOut output initialization failed.", ex );
            }

            Logging.Warn( "Unable to initialize any playback device." );
            return null;
        }

        private static bool TryInitializeSoundOut ( IWavePlayer soundOut, IWaveProvider provider )
        {
            try
            {
                soundOut.Init( provider );
            }
            catch ( COMException ce )
            {
                Logging.Warn( $"Failed to initialize. {ce.Source} not registered. Installation may be corrupt or Windows version may be incompatible. ", ce );
                return false;
            }
            catch ( InvalidCastException ice )
            {
                Logging.Warn( $"Failed to initialize. {ice.Message} ", ice );
                return false;
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"Failed to initialize sound output: {ex.Message}", ex );
                return false;
            }
            return true;
        }
    }
}