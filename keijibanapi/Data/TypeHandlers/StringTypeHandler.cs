// ===================================
// Data/TypeHandlers/StringTypeHandler.cs
// ===================================
using Dapper;
using System.Data;

namespace keijibanapi.Data.TypeHandlers
{
    /// <summary>
    /// Dapperがデータベースの文字列とC#のEnumを相互に変換するためのハンドラ
    /// </summary>
    /// <typeparam name="T">対象となるEnumの型</typeparam>
    public class StringTypeHandler<T> : SqlMapper.TypeHandler<T> where T : Enum
    {
        // DBから読み取った値をEnumに変換する方法を定義
        public override T Parse(object value)
        {
            // value.ToString() が null でなければ、それをEnumに変換する
            // true を指定することで、大文字・小文字を区別しない
            return (T)Enum.Parse(typeof(T), value.ToString()!, true);
        }

        // C#のEnumの値をDBに書き込む際の変換方法を定義
        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.Value = value.ToString();
        }
    }
}
