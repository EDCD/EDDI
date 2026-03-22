namespace EddiNavigationService
{
    public class Query (
        QueryType queryType,
        string stringArg0 = null,
        string stringArg1 = null,
        decimal? numericArg = null,
        bool? booleanArg = null,
        bool fromUi = false )
    {
        public QueryType QueryType { get; } = queryType;

        public string StringArg0 { get; } = stringArg0;

        public string StringArg1 { get; } = stringArg1;

        public decimal? NumericArg { get; } = numericArg;

        public bool? BooleanArg { get; } = booleanArg;

        public bool FromUI { get; } = fromUi;
    }
}
