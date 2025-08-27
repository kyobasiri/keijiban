// ==========================================
// Repositories/IDoctorAbsenceRepository.cs
// ==========================================
using keijibanapi.Models;

namespace keijibanapi.Repositories
{
    /// <summary>
    /// 医師不在情報へのアクセスを定義します。
    /// </summary>
    public interface IDoctorAbsenceRepository
    {
        Task<IEnumerable<DoctorAbsence>> GetDoctorAbsencesAsync(DateTime startDate);
    }
}
