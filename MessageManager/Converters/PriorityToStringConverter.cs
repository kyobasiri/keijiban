// Converters/PriorityToStringConverter.cs
using Avalonia.Data.Converters;
using MessageManager.Models;
using System;
using System.Globalization;

namespace MessageManager.Converters
{
    public class PriorityToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Priority priority)
            {
                return priority switch
                {
                    Priority.Low => "参考",
                    Priority.Normal => "通常",
                    Priority.High => "重要",
                    Priority.Urgent => "緊急",
                    _ => value.ToString()
                };
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
