using keijiban.Configuration;
using keijiban.Models;

namespace keijiban.Helpers
{
    /// <summary>
    /// アプリケーション内の異なる種類の「優先度」を相互に変換したり、
    /// 関連するUI表現（色、表示名）を取得したりするためのヘルパークラスです。
    /// </summary>
    public static class PriorityConverterHelper
    {
        /// <summary>
        /// EmergencyPriorityをMessagePriorityに変換します。
        /// </summary>
        public static MessagePriority ToMessagePriority(this EmergencyPriority emergencyPriority)
        {
            return emergencyPriority switch
            {
                // 現在は値が一致しているため直接キャストも可能だが、
                // 将来的に片方のenumだけ変更される可能性を考慮し、switch文でのマッピングを維持します。
                EmergencyPriority.Low => MessagePriority.Low,
                EmergencyPriority.Normal => MessagePriority.Normal,
                EmergencyPriority.High => MessagePriority.High,
                EmergencyPriority.Urgent => MessagePriority.Urgent,
                _ => MessagePriority.Normal
            };
        }

        /// <summary>
        /// MessagePriorityをEmergencyPriorityに変換します。
        /// </summary>
        public static EmergencyPriority ToEmergencyPriority(this MessagePriority messagePriority)
        {
            return messagePriority switch
            {
                MessagePriority.Low => EmergencyPriority.Low,
                MessagePriority.Normal => EmergencyPriority.Normal,
                MessagePriority.High => EmergencyPriority.High,
                MessagePriority.Urgent => EmergencyPriority.Urgent,
                _ => EmergencyPriority.Normal
            };
        }

        /// <summary>
        /// 緊急連絡の優先度に応じた色コードを取得します。
        /// </summary>
        public static string GetEmergencyPriorityColor(EmergencyPriority priority)
        {
            return priority switch
            {
                EmergencyPriority.Urgent => AppConstants.Colors.UrgentPriorityText,
                EmergencyPriority.High => AppConstants.Colors.HighPriorityText,
                EmergencyPriority.Normal => AppConstants.Colors.DefaultText,
                _ => AppConstants.Colors.DefaultText
            };
        }

        // (ご提示のコードにはMessagePriority用の色や表示名取得もありましたが、
        //  もしViewModelなどで直接使わない場合は削除しても構いません。
        //  ここでは、将来的な利用を想定して残しておきます。)

        /// <summary>
        /// 部署間連絡の優先度に応じた色コードを取得します。
        /// </summary>
        public static string GetMessagePriorityColor(MessagePriority priority)
        {
            return priority switch
            {
                // 必要に応じてAppConstantsに色を追加
                MessagePriority.Urgent => "#DC143C",
                MessagePriority.High => "#FF8C00",
                MessagePriority.Normal => "#000000",
                MessagePriority.Low => "#808080",
                _ => "#000000"
            };
        }

        /// <summary>
        /// 部署間連絡の優先度表示名を取得します。
        /// </summary>
        public static string GetMessagePriorityDisplayName(MessagePriority priority)
        {
            return priority switch
            {
                MessagePriority.Urgent => "至急",
                MessagePriority.High => "高",
                MessagePriority.Normal => "通常",
                MessagePriority.Low => "低",
                _ => "通常"
            };
        }
    }
}
