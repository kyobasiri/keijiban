// =======================================
// Repositories/DepartmentRepository.cs
// =======================================
using Dapper;
using keijibanapi.Models;
using MySqlConnector;
using System.Data;

namespace keijibanapi.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly string _connectionString;

        public DepartmentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDb")
                ?? throw new InvalidOperationException("MariaDb connection string not found");
        }

        private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

        public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync(int? displaycase = null)
        {
            string sql;
            var parameters = new DynamicParameters();
            if (displaycase.HasValue)
            {
                sql = @"SELECT department_id as Id, department_name as Name FROM departments
                        WHERE is_active = true AND (displaycase = @displaycase OR displaycase = 3)
                        ORDER BY department_id";
                parameters.Add("displaycase", displaycase.Value);
            }
            else
            {
                sql = @"SELECT department_id as Id, department_name as Name FROM departments
                        WHERE is_active = true ORDER BY department_id";
            }
            using var connection = CreateConnection();
            return await connection.QueryAsync<Department>(sql, parameters);
        }

        public async Task<Dictionary<int, string>> GetDepartmentNamesDictionaryAsync()
        {
            const string sql = "SELECT department_id, department_name FROM departments WHERE is_active = true AND department_id IS NOT NULL";
            using var connection = CreateConnection();
            var result = await connection.QueryAsync(sql);
            return result.ToDictionary(row => (int)row.department_id, row => (string)row.department_name);
        }

        public async Task<IEnumerable<DepartmentMaster>> GetAllDepartmentMastersAsync()
        {
            const string sql = @"
                SELECT id, department_id, department_name, external_system_name, external_department_id,
                       external_department_name, displaycase, is_active, created_at, updated_at
                FROM departments ORDER BY is_active DESC, external_system_name, department_id";
            using var connection = CreateConnection();
            return await connection.QueryAsync<DepartmentMaster>(sql);
        }

        public async Task<bool> IsDepartmentIdDuplicateAsync(int id, int? departmentId)
        {
            const string sql = @"
                SELECT COUNT(*) FROM departments 
                WHERE id != @Id AND department_id = @DepartmentId
                  AND external_system_name = (SELECT external_system_name FROM departments WHERE id = @Id)";
            using var connection = CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id, DepartmentId = departmentId });
            return count > 0;
        }

        public async Task<bool> UpdateDepartmentMasterAsync(UpdateDepartmentMasterRequest request)
        {
            const string sql = @"
                UPDATE departments SET department_id = @DepartmentId, department_name = @DepartmentName,
                       displaycase = @DisplayCase, is_active = @IsActive, updated_at = NOW()
                WHERE id = @Id";
            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(sql, request);
            return affectedRows > 0;
        }
    }
}
