using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SchoolApp.Models;
using SchoolApp.UnitOfWork;

namespace SchoolApp.Services.CertificateService
{
    public class CertificateService : ICertificateService
    {
        private readonly IUnitOfWork _uow;

        public CertificateService(IUnitOfWork uow) => _uow = uow;

        public Certificate GetOrCreate(int enrollmentId)
        {
            var existing = _uow.Certificates.GetByEnrollment(enrollmentId);
            if (existing != null) return existing;

            var enrollment = _uow.Enrollments.GetWithDetails(enrollmentId);
            if (enrollment == null || !enrollment.IsCompleted)
                throw new InvalidOperationException("Enrollment chưa hoàn thành.");

            var cert = new Certificate
            {
                EnrollmentId = enrollmentId,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                CertificateCode = Guid.NewGuid().ToString(),
                IssuedDate = enrollment.CompletedAt ?? DateTime.UtcNow
            };

            _uow.Certificates.Add(cert);
            _uow.SaveChanges();

            return _uow.Certificates.GetByEnrollment(enrollmentId)!;
        }

        public byte[] GeneratePdf(Certificate cert)
        {
            // Dùng Segoe UI (Windows) hoặc fallback về DejaVu Sans (Linux/Docker)
            // QuestPDF tự resolve font theo OS
            var fontFamily = "Segoe UI";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0);
                    // Không đặt DefaultTextStyle toàn trang để tránh override từng element
                    page.DefaultTextStyle(x => x.FontFamily(fontFamily));

                    page.Content().Background("#1b2d42").Column(col =>
                    {
                        // Gold top border
                        col.Item().Height(8).Background("#c9a84c");

                        col.Item().PaddingHorizontal(60).PaddingVertical(40).Column(content =>
                        {
                            // Header row: school name (Latin — LetterSpacing OK) + tagline
                            content.Item().Row(row =>
                            {
                                row.RelativeItem()
                                    .Text("SCHOOL APP")
                                    .FontFamily(fontFamily)
                                    .FontSize(13).FontColor("#c9a84c").Bold()
                                    .LetterSpacing(0.25f); // nhỏ, chỉ cho Latin text

                                row.ConstantItem(140).AlignRight()
                                    .Text("HỌC TRỰC TUYẾN")
                                    .FontFamily(fontFamily)
                                    .FontSize(10).FontColor("#8090a0");
                            });

                            // Tiêu đề — KHÔNG dùng LetterSpacing cho tiếng Việt
                            content.Item().PaddingTop(20)
                                .BorderBottom(1).BorderColor("#c9a84c").PaddingBottom(20)
                                .Text("CHỨNG CHỈ HOÀN THÀNH")
                                .FontFamily(fontFamily)
                                .FontSize(26).Bold().FontColor("#c9a84c").AlignCenter();

                            content.Item().PaddingTop(16)
                                .Text("Chứng nhận rằng")
                                .FontFamily(fontFamily)
                                .FontSize(14).FontColor("#a0b0c0").AlignCenter().Italic();

                            content.Item().PaddingTop(12)
                                .Text(cert.Student?.FullName ?? "")
                                .FontFamily(fontFamily)
                                .FontSize(34).Bold().FontColor(Colors.White).AlignCenter();

                            content.Item().PaddingTop(8)
                                .Text("đã hoàn thành xuất sắc khóa học")
                                .FontFamily(fontFamily)
                                .FontSize(14).FontColor("#a0b0c0").AlignCenter().Italic();

                            content.Item().PaddingTop(10)
                                .Text(cert.Course?.CourseName ?? "")
                                .FontFamily(fontFamily)
                                .FontSize(22).Bold().FontColor("#c9a84c").AlignCenter();

                            content.Item().PaddingTop(30).Row(row =>
                            {
                                // Ngày cấp
                                row.RelativeItem().Column(col2 =>
                                {
                                    col2.Item().Text("Ngày cấp")
                                        .FontFamily(fontFamily)
                                        .FontSize(11).FontColor("#8090a0").AlignCenter();
                                    col2.Item().PaddingTop(4)
                                        .Text(cert.IssuedDate.ToLocalTime().ToString("dd/MM/yyyy"))
                                        .FontFamily(fontFamily)
                                        .FontSize(14).Bold().FontColor(Colors.White).AlignCenter();
                                });

                                row.ConstantItem(1).Background("#3a4d60");

                                // Mã chứng chỉ
                                row.RelativeItem().Column(col2 =>
                                {
                                    col2.Item().Text("Mã chứng chỉ")
                                        .FontFamily(fontFamily)
                                        .FontSize(11).FontColor("#8090a0").AlignCenter();
                                    col2.Item().PaddingTop(4)
                                        .Text(cert.CertificateCode)
                                        .FontFamily("Courier New") // monospace cho UUID
                                        .FontSize(8).FontColor("#c9a84c").AlignCenter();
                                });

                                row.ConstantItem(1).Background("#3a4d60");

                                // Xác nhận
                                row.RelativeItem().Column(col2 =>
                                {
                                    col2.Item().Text("Xác nhận bởi")
                                        .FontFamily(fontFamily)
                                        .FontSize(11).FontColor("#8090a0").AlignCenter();
                                    col2.Item().PaddingTop(4)
                                        .Text("Ban Giám Đốc")
                                        .FontFamily(fontFamily)
                                        .FontSize(14).Bold().FontColor(Colors.White).AlignCenter();
                                });
                            });

                            content.Item().PaddingTop(20)
                                .Text($"Xác thực tại: /Certificate/Verify/{cert.CertificateCode}")
                                .FontFamily("Courier New")
                                .FontSize(8).FontColor("#506070").AlignCenter().Italic();
                        });

                        // Gold bottom border
                        col.Item().Height(8).Background("#c9a84c");
                    });
                });
            }).GeneratePdf();
        }
    }
}
