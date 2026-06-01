using SchoolApp.Models;

namespace SchoolApp.Repositories.CertificateRepository
{
    public interface ICertificateRepository : IRepository<Certificate>
    {
        Certificate? GetByEnrollment(int enrollmentId);
        Certificate? GetByCode(string code);
        IQueryable<Certificate> GetByStudentWithDetails(int studentId);
    }
}
