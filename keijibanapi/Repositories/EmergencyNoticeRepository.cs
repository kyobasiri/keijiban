// =============================================
// Repositories/EmergencyNoticeRepository.cs
// =============================================
using Dapper;
using keijibanapi.Models;
using MySqlConnector;
using System.Data;

namespace keijibanapi.Repositories
{
    public class EmergencyNoticeRepository : IEmergencyNoticeRepository
    {
        private readonly string _connectionString;

        public EmergencyNoticeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDb")
                ?? throw new InvalidOperationException("MariaDb connection string not found");
        }

        private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

        // N+1問題を解決する実装
        public async Task<IEnumerable<EmergencyNotice>> GetAllWithDepartmentsAsync()
        {
            const string noticesSql = @"
                SELECT id, priority, notice_type, notice_content, is_all_departments,
                       is_active, created_at, updated_at, created_by_department_id,
                       target_department_names
                FROM v_emergency_notices_with_departments
                ORDER BY created_at DESC, is_active DESC LIMIT 30";

            using var connection = CreateConnection();
            var notices = (await connection.QueryAsync<EmergencyNotice>(noticesSql)).ToList();

            if (!notices.Any()) return notices;

            var noticeIds = notices.Where(n => !n.TargetDepartmentNames.Equals("全部署")).Select(n => n.Id);
            if (!noticeIds.Any()) return notices;

            const string deptsSql = "SELECT notice_id, department_id FROM emergency_notice_departments WHERE notice_id IN @noticeIds";
            var departmentMappings = await connection.QueryAsync<(int notice_id, int department_id)>(deptsSql, new { noticeIds });

            var noticeToDepts = departmentMappings.ToLookup(m => m.notice_id, m => m.department_id);

            foreach (var notice in notices)
            {
                if (noticeToDepts.Contains(notice.Id))
                {
                    notice.TargetDepartments = noticeToDepts[notice.Id].ToList();
                }
            }
            return notices;
        }

        public async Task<IEnumerable<EmergencyNotice>> GetActiveNoticesForDepartmentAsync(int departmentId)
        {
            const string sql = @"
                SELECT DISTINCT
                        CAST(id AS SIGNED) AS id,
                        priority,
                        notice_type,
                        notice_content,
                        CAST(is_active AS SIGNED) AS is_active,
                        created_at,
                        updated_at,
                        CAST(created_by_department_id AS SIGNED) AS created_by_department_id
                    FROM v_department_active_notices
                    WHERE department_id = @DepartmentId
                    AND is_active = 1
                    ORDER BY priority DESC, created_at DESC";
            using var connection = CreateConnection();

            // 接続文字列はIConfigurationから取得
            var connectionString = _connectionString;


            return await connection.QueryAsync<EmergencyNotice>(sql, new { DepartmentId = departmentId });            
        }

        public async Task<EmergencyNotice?> GetByIdAsync(int id)
        {
            // GetAllWithDepartmentsAsyncはビューを使っているため、単体取得は個別に実装
            // この実装はGetAllWithDepartmentsAsyncを流用
            var allNotices = await GetAllWithDepartmentsAsync();
            return allNotices.FirstOrDefault(n => n.Id == id);
        }

        public async Task<int> CreateAsync(CreateEmergencyNoticeRequest request)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                const string sql = @"
                    INSERT INTO emergency_notices (priority, notice_type, notice_content, is_all_departments, created_by_department_id) 
                    VALUES (@Priority, @NoticeType, @NoticeContent, @IsAllDepartments, @CreatedByDepartmentId);
                    SELECT LAST_INSERT_ID();";

                var newId = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    request.Priority,
                    request.NoticeType,
                    request.NoticeContent,
                    IsAllDepartments = request.TargetDepartments == null || !request.TargetDepartments.Any(),
                    request.CreatedByDepartmentId
                }, transaction);

                if (request.TargetDepartments?.Any() == true)
                {
                    const string deptSql = "INSERT INTO emergency_notice_departments (notice_id, department_id) VALUES (@NoticeId, @DepartmentId)";
                    foreach (var deptId in request.TargetDepartments)
                    {
                        await connection.ExecuteAsync(deptSql, new { NoticeId = newId, DepartmentId = deptId }, transaction);
                    }
                }
                await transaction.CommitAsync();
                return newId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(UpdateEmergencyNoticeRequest request)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                const string sql = @"
                    UPDATE emergency_notices SET priority = @Priority, notice_type = @NoticeType, notice_content = @NoticeContent,
                           is_all_departments = @IsAllDepartments, is_active = @IsActive, updated_at = CURRENT_TIMESTAMP
                    WHERE id = @Id";

                var rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    request.Priority,
                    request.NoticeType,
                    request.NoticeContent,
                    IsAllDepartments = request.TargetDepartments == null || !request.TargetDepartments.Any(),
                    request.IsActive,
                    request.Id
                }, transaction);

                if (rowsAffected == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                const string deleteSql = "DELETE FROM emergency_notice_departments WHERE notice_id = @NoticeId";
                await connection.ExecuteAsync(deleteSql, new { NoticeId = request.Id }, transaction);

                if (request.TargetDepartments?.Any() == true)
                {
                    const string deptSql = "INSERT INTO emergency_notice_departments (notice_id, department_id) VALUES (@NoticeId, @DepartmentId)";
                    foreach (var deptId in request.TargetDepartments)
                    {
                        await connection.ExecuteAsync(deptSql, new { NoticeId = request.Id, DepartmentId = deptId }, transaction);
                    }
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ToggleAsync(int id, bool isActive)
        {
            const string sql = "UPDATE emergency_notices SET is_active = @IsActive, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IsActive = isActive });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM emergency_notices WHERE id = @Id";
            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }
    }
}
