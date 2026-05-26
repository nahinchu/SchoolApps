namespace SchoolApp.DTOs
{
    public class AdminDashboardViewModel
    {
        // Stats cards
        public int TotalStudents { get; set; }
        public int TotalManagers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public decimal TotalRevenue { get; set; }

        // Chart 1: Học viên đăng ký mới theo tháng (12 tháng gần nhất)
        public string[] MonthLabels { get; set; } = [];
        public int[] NewStudentsByMonth { get; set; } = [];

        // Chart 2: Doanh thu theo tháng (12 tháng gần nhất)
        public decimal[] RevenueByMonth { get; set; } = [];

        // Chart 3: Top khóa học phổ biến
        public string[] TopCourseNames { get; set; } = [];
        public int[] TopCourseEnrollments { get; set; } = [];

        // Chart 4: Trạng thái thanh toán
        public int PaidCount { get; set; }
        public int PendingCount { get; set; }
        public int CancelledCount { get; set; }

        // Bảng dữ liệu gần nhất
        public List<RecentEnrollmentRow> RecentEnrollments { get; set; } = [];
        public List<RecentPaymentRow> RecentPayments { get; set; } = [];
    }

    public class RecentEnrollmentRow
    {
        public string StudentName { get; set; } = "";
        public string CourseName { get; set; } = "";
        public DateTime EnrollDate { get; set; }
        public decimal? Grade { get; set; }
    }

    public class RecentPaymentRow
    {
        public string StudentName { get; set; } = "";
        public string CourseName { get; set; } = "";
        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}