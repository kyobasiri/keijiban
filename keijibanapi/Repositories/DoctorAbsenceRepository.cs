// ==========================================
// Repositories/DoctorAbsenceRepository.cs
// ==========================================
using Dapper;
using keijibanapi.Models;
using MySqlConnector;
using System.Data;

namespace keijibanapi.Repositories
{
    public class DoctorAbsenceRepository : IDoctorAbsenceRepository
    {
        private readonly string _connectionString;

        public DoctorAbsenceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MariaDb")
                ?? throw new InvalidOperationException("MariaDb connection string not found");
        }

        private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

        public async Task<IEnumerable<DoctorAbsence>> GetDoctorAbsencesAsync(DateTime startDate)
        {
            const string sql = @"
                SELECT doctor_name as DoctorName, absence_date as Date, start_time as StartTime, end_time as EndTime,
                       reason, detail, minidetail as MiniDetail
                FROM doctor_absences
                WHERE absence_date BETWEEN @StartDate AND DATE_ADD(@StartDate, INTERVAL 7 DAY)
                ORDER BY absence_date, start_time";

            using var connection = CreateConnection();
            return await connection.QueryAsync<DoctorAbsence>(sql, new { StartDate = startDate });
        }
    }
}
