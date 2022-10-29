using System;
using System.Linq;

namespace EddiNavigationService
{
    public enum QueryType
    {
        route,
        encoded,
        expiring,
        facilitator,
        farthest,
        guardian,
        human,
        manufactured,
        most,
        nearest,
        neutron,
        carrier,
        raw,
        recalculating,
        scoop,
        scorpion,
        source,
        set,
        update,
        cancel
    }

    public static class QueryTypeExtensions
    {
        public static string LocalizedName(this QueryType queryType)
        {
            return Properties.NavigationService.ResourceManager
                .GetString($"query_type_{queryType}".ToLowerInvariant());
        }

        public static QueryGroup? Group(this QueryType queryType)
        {
            foreach (var queryGroup in Enum.GetValues(typeof(QueryGroup)).Cast<QueryGroup>())
            {
                if (queryGroup.QueryTypes().Contains(queryType)) { return queryGroup; }
            }
            return null;
        }
    }

    public class QueryTypeConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is null || value is "") { return value; }
            return ((QueryType)value).LocalizedName();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
