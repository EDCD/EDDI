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
        public string description => @"
This function will play an audio file as supplied in the argument. This function uses SSML tags.

Play() takes one argument: the path to the file to play.  This file must be a '.wav' file.  Any backslashes for path separators must be escaped, so '\\' must be written as '\\\\'

Common usage of this is to provide a pre-recorded custom audio file rather than use EDDI's text-to-speech, for example:

    {Play('C:\\Users\\CmdrMcDonald\\Desktop\\Warning.wav')}";
        public NativeFunction function => new NativeFunction((values) =>
        {
            return @"<audio src=""" + values[0].AsString + @""" />";
        }, 1);
    }
}
