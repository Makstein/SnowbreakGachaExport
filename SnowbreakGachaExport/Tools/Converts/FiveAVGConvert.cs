using Avalonia.Data.Converters;
using SnowbreakGachaExport.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SnowbreakGachaExport.Tools.Converts;

public class FiveAVGConvert : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not List<HistoryItem> list) return null;

        var avg = 0.0f;
        var lastFive = list.Count;
        var fiveCount = 0;
        for (var i = list.Count - 1; i >= 0; --i)
        {
            if (list[i].Star != 5) continue;

            avg += lastFive - i;
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