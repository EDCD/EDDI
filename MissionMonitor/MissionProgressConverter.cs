using System;
using System.Globalization;
using System.Windows.Data;

namespace EddiMissionMonitor
{
    public sealed class MissionProgressConverter : IMultiValueConverter
    {
        public object Convert ( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            if ( values.Length != 5 ) { return string.Empty; }
            if ( values[ 1 ] is not true ) { return values[ 0 ] as string ?? string.Empty; }
            if ( values[ 2 ] is not true || values[ 3 ] is not long contribution || values[ 4 ] is not int percentile )
            {
                return string.Empty;
            }
            return string.Format( CultureInfo.CurrentCulture, Properties.MissionMonitor.progress_cg_format,
                contribution, percentile );
        }

        public object[] ConvertBack ( object value, Type[] targetTypes, object parameter, CultureInfo culture ) =>
            throw new NotSupportedException();
    }
}
