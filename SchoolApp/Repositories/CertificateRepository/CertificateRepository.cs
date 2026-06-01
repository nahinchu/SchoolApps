using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories.CertificateRepository
{
    public class CertificateRepository : Repository<Certificate>, ICertificateRepository
    {
        public CertificateRepository(AppDbContext context) : base(context) { }

        public Certificate? GetByEnrollment(int enrollmentId)
        {
            return _dbSet
                .Include(c => c.Student)
                .Include(c => c.Course)
                .FirstOrDefault(c => c.EnrollmentId == enrollmentId);
        }

        public Certificate? GetByCode(string code)
        {
            return _dbSet
                .Include(c => c.Student)
                .Include(c => c.Course)
                .FirstOrDefault(c => c.CertificateCode == code);
        }

        public IQueryable<Certificate> GetByStudentWithDetails(int studentId)
        {
            return _dbSet
                .Include(c => c.Course)
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.IssuedDate);
        }
    }
}
