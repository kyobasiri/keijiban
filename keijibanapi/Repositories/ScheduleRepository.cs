// =======================================
// Repositories/ScheduleRepository.cs
// =======================================
using Dapper;
using keijibanapi.Models;
using MySqlConnector;
using System.Data;

namespace keijibanapi.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly string _connectionString;

        public ScheduleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDb")
                ?? throw new InvalidOperationException("MariaDb connection string not found");
        }

        private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

        public async Task<IEnumerable<ScheduleItem>> GetScheduleGroupDataAsync(DateTime startDate, DateTime endDate, int? departmentId = null)
        {
            var sqlBuilder = new SqlBuilder();
            var template = sqlBuilder.AddTemplate(@"
                SELECT sgc.title, sgc.start_date as StartDate, sgc.end_date as EndDate, sgc.start_time, sgc.end_time,
                       d.department_name as Department, sgc.department_id as DepartmentId
                FROM schedule_group_cache sgc
                INNER JOIN departments d ON sgc.department_id = d.department_id
                /**where**/
                ORDER BY sgc.start_date, sgc.start_time, sgc.title");

            sqlBuilder.Where("d.is_active = 1");
            sqlBuilder.Where("(d.displaycase = 1 OR d.displaycase = 3)");
            sqlBuilder.Where(@"(sgc.start_date BETWEEN @StartDate AND @EndDate OR sgc.end_date BETWEEN @StartDate AND @EndDate OR (sgc.start_date <= @StartDate AND sgc.end_date >= @StartDate))", new { StartDate = startDate, EndDate = endDate });

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                sqlBuilder.Where("sgc.department_id = @DepartmentId", new { DepartmentId = departmentId.Value });
            }

            using var connection = CreateConnection();
            return await connection.QueryAsync<ScheduleItem>(template.RawSql, template.Parameters);
        }

        public async Task<IEnumerable<ScheduleItem>> GetDepartmentScheduleDataAsync(DateTime startDate, DateTime endDate, int? departmentId = null)
        {
            var sqlBuilder = new SqlBuilder();
            var template = sqlBuilder.AddTemplate(@"
                SELECT dsc.title, dsc.start_date as StartDate, dsc.end_date as EndDate, dsc.start_time, dsc.end_time,
                       d.department_name as Department, dsc.department_id as DepartmentId
                FROM department_schedule_cache dsc
                INNER JOIN departments d ON dsc.department_id = d.department_id
                /**where**/
                ORDER BY dsc.start_date, dsc.start_time, dsc.title");

            sqlBuilder.Where("d.is_active = 1");
            sqlBuilder.Where("(d.displaycase = 2 OR d.displaycase = 3)");
            sqlBuilder.Where(@"(dsc.start_date BETWEEN @StartDate AND @EndDate OR dsc.end_date BETWEEN @StartDate AND @EndDate OR (dsc.start_date <= @StartDate AND dsc.end_date >= @StartDate))", new { StartDate = startDate, EndDate = endDate });

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                sqlBuilder.Where("dsc.department_id = @DepartmentId", new { DepartmentId = departmentId.Value });
            }

            using var connection = CreateConnection();
            return await connection.QueryAsync<ScheduleItem>(template.RawSql, template.Parameters);
        }

        public async Task<IEnumerable<InformationApiItem>> GetInformationAsync(int? departmentId = null)
        {
            var sqlBuilder = new SqlBuilder();
            var template = sqlBuilder.AddTemplate(@"
                SELECT title, post_date as PostDate, content, author
                FROM information_cache
                /**where**/
                ORDER BY post_date DESC, timestamp DESC LIMIT 10");

            if (departmentId.HasValue)
            {
                sqlBuilder.Where("(department_id IS NULL OR department_id = @DepartmentId)", new { DepartmentId = departmentId.Value });
            }
            else
            {
                sqlBuilder.Where("department_id IS NULL");
            }

            using var connection = CreateConnection();
            return await connection.QueryAsync<InformationApiItem>(template.RawSql, template.Parameters);
        }
    }
}
