using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using SnowbreakGachaExport.Models;

namespace SnowbreakGachaExport.Tools;

public class LogListConvert : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not List<HistoryItem> list) return null;
        
        var res = new List<FiveStarItem>();
        var lastFive = 0;
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Star != 5) continue;
            
            res.Add(new FiveStarItem(list[i].Name, i - lastFive + 1));
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