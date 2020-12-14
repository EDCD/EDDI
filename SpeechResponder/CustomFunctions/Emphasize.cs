using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Emphasize : ICustomFunction
    {
        public string name => "Emphasize";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.Emphasize;
        public NativeFunction function => new NativeFunction((values) =>
        {
            if (values.Count == 1)
            {
                return @"<emphasis level =""strong"">" + values[0].AsString + @"</emphasis>";
            }
            if (values.Count == 2)
            {
                return @"<emphasis level =""" + values[1].AsString + @""">" + values[0].AsString + @"</emphasis>";
            }
            return "The Emphasize function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
