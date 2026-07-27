namespace EddiNavigationService
{
    public class Query
    {
        public Query (
            QueryType queryType,
            string stringArg0 = null,
            string stringArg1 = null,
            decimal? numericArg = null,
            bool? booleanArg = null,
            bool fromUi = false )
            : this( queryType, stringArg0, stringArg1, numericArg, booleanArg, fromUi, new EddiNavigationRuntimeContext() )
        { }

        internal Query (
            QueryType queryType,
            string stringArg0,
            string stringArg1,
            decimal? numericArg,
            bool? booleanArg,
            bool fromUi,
            INavigationRuntimeContext runtimeContext )
        {
            QueryType = queryType;
            StringArg0 = stringArg0;
            StringArg1 = stringArg1;
            NumericArg = numericArg;
            BooleanArg = booleanArg;
            FromUI = fromUi;
            RuntimeContext = runtimeContext;
        }

        public QueryType QueryType { get; }
        public string StringArg0 { get; }
        public string StringArg1 { get; }
        public decimal? NumericArg { get; }
        public bool? BooleanArg { get; }
        public bool FromUI { get; }
        internal INavigationRuntimeContext RuntimeContext { get; }
    }
}
