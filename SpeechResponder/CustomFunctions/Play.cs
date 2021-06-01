using Cottle.Functions;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Play : ICustomFunction
    {
        public string name => "Play";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Play;
        public NativeFunction function => new NativeFunction((values) =>
        {
            // Whether the audio should be played asynchronously
            bool async = false;
            if (values.Count > 1)
            {
                async = values[1].AsBoolean;
            }

            // The volume override (where 100 is normal volume)
            decimal? volumeOverride = null;
            if (values.Count > 2)
            {
                volumeOverride = values[2].AsNumber;
            }

            try
            {
                ISoundOut GetSoundOut()
                {
                    if (WasapiOut.IsSupportedOnCurrentPlatform)
                    {
                        return new WasapiOut();
                    }
                    else
                    {
                        return new DirectSoundOut();
                    }
                }

                // Play the audio, waiting for the audio to complete unless we're in async mode
                void playAudio()
                {
                    using (EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset))
                    using (var soundOut = GetSoundOut())
                    {
                        var source = CodecFactory.Instance.GetCodec(values[0].AsString);
                        var waitTime = source.GetTime(source.Length);
                        soundOut.Initialize(source);
                        if (volumeOverride != null)
                        {
                            soundOut.Volume = (float)volumeOverride / 100;
                        }
                        soundOut.Play();
                        waitHandle.WaitOne(waitTime);
                        soundOut.Stop();
                    }
                }
                if (async) 
                { 
                    Task.Run(playAudio); 
                }
                else
                {
                    playAudio();
                }
            }
            catch (Exception e)
            {
                Logging.Warn(e.Message, e);
                return e.Message;
            }

            return "";
        }, 1, 3);
    }
}
