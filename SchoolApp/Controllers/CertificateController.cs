using Microsoft.AspNetCore.Mvc;
using SchoolApp.Filters;
using SchoolApp.Services.CertificateService;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Controllers
{
    public class CertificateController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly ICertificateService _certificateService;

        public CertificateController(IUnitOfWork uow, ICertificateService certificateService)
        {
            _uow = uow;
            _certificateService = certificateService;
        }

        [AuthorizeUser]
        public IActionResult My()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId")!.Value;
            var certs = _uow.Certificates.GetByStudentWithDetails(studentId).ToList();
            return View(certs);
        }

        [AuthorizeUser]
        public IActionResult Download(int id)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId")!.Value;
            var cert = _uow.Certificates.GetById(id);

            if (cert == null || cert.StudentId != studentId)
                return NotFound();

            // Reload with navigation props for PDF
            var fullCert = _uow.Certificates.GetByEnrollment(cert.EnrollmentId)!;
            var pdf = _certificateService.GeneratePdf(fullCert);
            var fileName = $"ChungChi_{fullCert.Course?.CourseName?.Replace(" ", "_")}_{fullCert.IssuedDate:yyyyMMdd}.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        // Public — không cần đăng nhập
        public IActionResult Verify(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                ViewBag.Valid = false;
                return View();
            }

            var cert = _uow.Certificates.GetByCode(code);
            ViewBag.Valid = cert != null;
            return View(cert);
        }
    }
}
