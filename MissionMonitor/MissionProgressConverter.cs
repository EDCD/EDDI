using System;
using System.Globalization;
using System.Windows.Data;

namespace EddiMissionMonitor
{
    public sealed class MissionProgressConverter : IMultiValueConverter
    {
        public object Convert ( object[] values, Type targetType, object parameter, CultureInfo culture )
        {
            if ( values.Length is not ( 5 or 7 or 8 ) ) { return string.Empty; }
            if ( values[ 1 ] is not true )
            {
                var confirmed = values[ 0 ] as string ?? string.Empty;
                if ( !string.IsNullOrEmpty( confirmed ) ) { return confirmed; }
                if ( values.Length >= 7 && values[ 5 ] is long && values[ 6 ] is int && parameter as string == "tooltip" )
                {
                    return Properties.MissionMonitor.progress_combat_estimate_tooltip;
                }
                return values.Length >= 7 && values[ 5 ] is long kills && values[ 6 ] is int required
                    ? string.Format( CultureInfo.CurrentCulture, Properties.MissionMonitor.progress_combat_estimate_format, kills, required )
                    : string.Empty;
            }
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
