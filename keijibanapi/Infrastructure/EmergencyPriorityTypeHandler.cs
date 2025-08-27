using Dapper;
using keijibanapi.Models; // EmergencyPriority enum がある名前空間を指定
using System.Data;
// ファイルパス: Infrastructure/EmergencyPriorityTypeHandler.cs

// ネームスペースをフォルダ構造に合わせる
namespace keijibanapi.Infrastructure
{
    using Dapper;
    using keijibanapi.Models; // EmergencyPriority enum が定義されている名前空間
    using System.Data;

    public class EmergencyPriorityTypeHandler : SqlMapper.TypeHandler<EmergencyPriority>
    {
        public override EmergencyPriority Parse(object value)
        {
            if (value == null || value is DBNull)
            {
                // Enumのデフォルト値を返すなど、NULLの場合の適切な処理を記述
                return default(EmergencyPriority);
            }
            return Enum.Parse<EmergencyPriority>(value.ToString(), true);
        }

        public override void SetValue(IDbDataParameter parameter, EmergencyPriority value)
        {
            parameter.Value = value.ToString();
        }
    }
}
