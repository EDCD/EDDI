using Cottle.Functions;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Humanise : ICustomFunction
    {
        public string name => "Humanise";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => @"
This function will turn its argument into a more human number, for example turning 31245 in to \""just over thirty thousand\"".

Humanise() takes one argument: the number to humanise.

Common usage of this is to provide human-sounding numbers when speaking rather than saying every digit, for example:

    You have {Humanise(cmdr.credits)} credits.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            return Translations.Humanize(values[0].AsNumber);
        }, 1);
    }
}
