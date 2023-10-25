using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using SnowbreakGachaExport.Models;

namespace SnowbreakGachaExport.Tools.Converts;

public class FiveAVGConvert : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not List<HistoryItem> list) return null;

        list.Reverse();
        var avg = 0.0f;
        var lastFive = 0;
        var fiveCount = 0;
        for (var i = 0; i < list.Count; ++i)
        {
            if (list[i].Star != 5) continue;
            
            avg += i - lastFive + 1;
            lastFive = i;
            ++fiveCount;
        }

        return avg / fiveCount;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}