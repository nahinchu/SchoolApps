using SchoolApp.Models;

namespace SchoolApp.Services.CertificateService
{
    public interface ICertificateService
    {
        Certificate GetOrCreate(int enrollmentId);
        byte[] GeneratePdf(Certificate cert);
    }
}
