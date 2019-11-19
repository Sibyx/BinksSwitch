using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using BinksSwitch.Network.Entities;

namespace BinksSwitch.UI
{
    public class StatisticsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var record = (Dictionary<Direction, ulong>) value;
            return record != null ? $"{record[Direction.In]} / {record[Direction.Out]}" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}