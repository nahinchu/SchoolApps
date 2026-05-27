namespace SchoolApp.Services.CompletionService
{
    public interface ICourseCompletionService
    {
        /// <summary>
        /// Kiểm tra tất cả lesson (và quiz bắt buộc) đã hoàn thành chưa.
        /// Nếu đủ điều kiện và chưa được đánh dấu → mark IsCompleted = true và lưu DB.
        /// Returns true nếu VỪA hoàn thành lần này (để hiện thông báo chúc mừng).
        /// Returns false nếu chưa đủ điều kiện, hoặc đã hoàn thành từ trước.
        /// </summary>
        bool TryComplete(int studentId, int courseId);
    }
}
