using Cottle;

namespace EddiScriptResolverService
{
    public sealed record DeferredSpeechRequest (
        string ScriptName, int MaximumDelayMs, string WaitForScript, Value Args );
}
