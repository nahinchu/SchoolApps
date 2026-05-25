namespace SchoolApp.Services.ExcelExportService
{
    public class ImportRowError
    {
        /// <summary>0 = lỗi cấp file (thiếu cột), > 0 = số dòng trong Excel</summary>
        public int RowNumber { get; set; }
        public string Message { get; set; } = "";
    }

    public class ExcelImportResult<T>
    {
        public List<T> ValidRows { get; set; } = new();
        public List<ImportRowError> Errors { get; set; } = new();
    }
}
    