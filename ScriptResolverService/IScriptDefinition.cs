namespace EddiScriptResolverService
{
    public interface IScriptDefinition
    {
        string Name { get; }
        string Value { get; }
        bool Enabled { get; }
        int? Priority { get; }
        string includes { get; }
    }
}
