using Cottle.Functions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class Pause : ICustomFunction
    {
        public string name => "Pause";
        public FunctionCategory Category => FunctionCategory.Tempo;
        public string description => @"
This function will pause the speech for a given amount of time. This function uses SSML tags.

Pause() takes one argument: the number of milliseconds to pause.

Common usage of this is to allow speech to sync up with in-game sounds, for example to wait for a known response to a phrase before continuing, for example:

    Hello.  {Pause(2000)} Yes.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            return @"<break time=""" + values[0].AsNumber + @"ms"" />";
        }, 1);
    }
}
