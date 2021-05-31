using System.Web.Security;
using System.Windows.Input;
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
            // We use prosody rather than emphasis so that we can tune the output.
            // Prosody also seems to be better supported by languages than emphasis.
            
            string strong (string inputString)
            { 
                return $@"<prosody rate=""75%"" volume=""+30%"">{inputString}</prosody>";
            }
            string moderate(string inputString)
            {
                return $@"<prosody rate=""90%"" volume=""+15%"">{inputString}</prosody>";
            }
            string none(string inputString)
            {
                // The "none" level is used to prevent the synthesis processor from emphasizing words that it might typically emphasize. 
                return $@"<emphasis level =""none"">{inputString}</emphasis>";
            }
            string reduced(string inputString)
            {
                return $@"<prosody rate=""125%"" volume=""-15%"">{inputString}</prosody>";
            }

            if (values.Count == 1)
            {
                return strong(values[0].AsString);
            }
            if (values.Count == 2)
            {
                switch (values[1].AsString.ToLowerInvariant())
                {
                    default:
                        return strong(values[0].AsString);
                    case "moderate":
                        return moderate(values[0].AsString);
                    case "none":
                        return none(values[0].AsString);
                    case "reduced":
                        return reduced(values[0].AsString);
                }
            }
            return "The Emphasize function is used improperly. Please review the documentation for correct usage.";
        }, 1, 2);
    }
}
