using Avalonia.Animation;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace keijiban.ViewModels
{
    /// <summary>
    /// bool値(週末かどうか)を特定の色に変換します。
    /// True: 赤色 (週末), False: 通常色 (平日)
    /// </summary>
    public class BoolToWeekendColorConverter : IValueConverter
    {
        public static readonly BoolToWeekendColorConverter Instance = new();
        private static readonly IBrush WeekendBrush = new SolidColorBrush(Color.Parse("#FF6347"));
        private static readonly IBrush WeekdayBrush = new SolidColorBrush(Color.Parse("#333333"));

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is true ? WeekendBrush : WeekdayBrush;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// bool値(今日かどうか)を特定の背景色に変換します。
    /// True: オレンジ (今日), False: 青 (通常)
    /// </summary>
    public class BoolToTodayBackgroundConverter : IValueConverter
    {
        public static readonly BoolToTodayBackgroundConverter Instance = new();
        private static readonly IBrush TodayBrush = new SolidColorBrush(Color.Parse("#FF8C00"));
        private static readonly IBrush NormalBrush = new SolidColorBrush(Color.Parse("#4682B4"));

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is true ? TodayBrush : NormalBrush;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 文字列が空またはnullでない場合にTrueを返し、UI要素の表示/非表示を切り替えます。
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public static readonly StringToVisibilityConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isVisible = !string.IsNullOrEmpty(value as string);
            return isVisible; // true または false を返す
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// double値を反転させます (-1を掛ける)。
    /// </summary>
    public class NegateConverter : IValueConverter
    {
        public static readonly NegateConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double val) return -val;
            return 0.0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double val) return -val;
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// bool値をAnimation.PlayStateに変換します。
    /// True: Run, False: Pause
    /// </summary>
    public class BoolToPlayStateConverter : IValueConverter
    {
        public static readonly BoolToPlayStateConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is true ? PlayState.Run : PlayState.Pause;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
