using System;

namespace EddiNavigationService
{
    public enum QueryGroup
    {
        galaxy,
        missions,
        services
    }

    public static class QueryGroupExtensions
    {
        public static QueryType[] QueryTypes(this QueryGroup queryGroup)
        {
            switch (queryGroup)
            {
                case QueryGroup.galaxy:
                {
                    return new[]
                    {
                        QueryType.neutron, 
                        QueryType.scoop
                    };
                }
                case QueryGroup.missions:
                {
                    return new[]
                    {
                        QueryType.expiring,
                        QueryType.farthest,
                        QueryType.most,
                        QueryType.nearest,
                        QueryType.route,
                        QueryType.source
                    };
                }
                case QueryGroup.services:
                {
                    return new[]
                    {
                        QueryType.encoded,
                        QueryType.facilitator,
                        QueryType.guardian,
                        QueryType.human,
                        QueryType.manufactured,
                        QueryType.raw
                    };
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static QueryType DefaultQueryType(this QueryGroup queryGroup)
        {
            switch (queryGroup)
            {
                case QueryGroup.galaxy:
                {
                    return QueryType.neutron;
                }
                case QueryGroup.missions:
                {
                    return QueryType.route;
                }
                case QueryGroup.services:
                {
                    return QueryType.encoded;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static string LocalizedName(this QueryGroup queryGroup)
        {
            return Properties.NavigationService.ResourceManager
                .GetString($"query_group_{queryGroup}".ToLowerInvariant());
        }
    }

    public class QueryGroupConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is null || value is "") { return value; }
            return ((QueryGroup)value).LocalizedName();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
