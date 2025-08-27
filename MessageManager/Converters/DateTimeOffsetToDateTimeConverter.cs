using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MessageManager.Converters
{
    public class DateTimeOffsetToDateTimeConverter : IValueConverter
    {
        // ViewModel (DateTimeOffset?) から UI (DateTime?) へ値を渡すときに呼ばれる
        // 今回のエラー「cannot convert DateTimeOffset to DateTime?」はここで発生している
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTimeOffset dto)
            {
                // ViewModelのDateTimeOffsetをUIが期待するDateTimeに変換して返す
                return dto.DateTime;
            }
            return null; // valueがnullの場合もnullを返す
        }

        // UI (DateTime?) から ViewModel (DateTimeOffset?) へ値を戻すときに呼ばれる
        // 前回のエラー「cannot convert DateTime to DateTimeOffset?」はここで発生していた
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                // UIのDateTimeをViewModelが期待するDateTimeOffsetに変換して返す
                // Unspecifiedカインドの場合、ローカルタイムとして解釈させる
                if (dt.Kind == DateTimeKind.Unspecified)
                {
                    return new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Local));
                }
                return new DateTimeOffset(dt);
            }
            return null; // valueがnullの場合もnullを返す
        }
    }
}
