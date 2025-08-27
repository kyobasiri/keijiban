// ==========================================
// Repositories/ExtraScheduleRepository.cs
// ==========================================
using Dapper;
using keijibanapi.Models;
using MySqlConnector;
using System.Data;

namespace keijibanapi.Repositories
{
    public class ExtraScheduleRepository : IExtraScheduleRepository
    {
        private readonly string _connectionString;

        public ExtraScheduleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDb")
                ?? throw new InvalidOperationException("MariaDb connection string not found");
        }

        private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

        public async Task<IEnumerable<ExtraScheduleData>> GetExtraScheduleDataAsync(DateTime startDate, DateTime endDate, int? departmentId = null)
        {
            var sqlBuilder = new SqlBuilder();
            var template = sqlBuilder.AddTemplate(@"
                SELECT id, department_id as DepartmentId, schedule_date as ScheduleDate, start_time as StartTime,
                       end_time as EndTime, title, category, created_at as CreatedAt, updated_at as UpdatedAt
                FROM extrascheduledata
                /**where**/
                ORDER BY schedule_date, category, start_time, title");

            sqlBuilder.Where("schedule_date BETWEEN @StartDate AND @EndDate", new { StartDate = startDate, EndDate = endDate });

            if (departmentId.HasValue)
            {
                sqlBuilder.Where("department_id = @DepartmentId", new { DepartmentId = departmentId.Value });
            }

            using var connection = CreateConnection();
            return await connection.QueryAsync<ExtraScheduleData>(template.RawSql, template.Parameters);
        }
    }
}
