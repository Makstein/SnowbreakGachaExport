using Avalonia.Data.Converters;
using SnowbreakGachaExport.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SnowbreakGachaExport.Tools;

public class LogListConvert : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not List<HistoryItem> list) return null;

        var res = new List<FiveStarItem>();
        var lastFive = list.Count;
        for (var i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].Star != 5) continue;

            res.Add(new FiveStarItem(list[i].Name, lastFive - i));
            lastFive = i;
        }

        res.Reverse();
        return res;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}