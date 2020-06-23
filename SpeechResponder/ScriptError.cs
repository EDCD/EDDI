namespace EddiSpeechResponder
{
    public class ScriptError
    {
        public string errorMsg { get; private set; }
        public int line { get; private set; }
        public int startPosition { get; private set; }
        public int length { get; private set; }

        public ScriptError(string errorMsg, string script, int startPosition, int length)
        {
            this.errorMsg = errorMsg;
            this.line = script.Substring(0, startPosition).Split('\n').Length;
            this.startPosition = startPosition;
            this.length = length;
        }
    }
}
