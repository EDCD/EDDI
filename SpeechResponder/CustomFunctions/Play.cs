using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

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
            return @"<audio src=""" + values[0].AsString + @""" />";
        }, 1);
    }
}
