namespace SchoolApp.Services.ExcelExportService
{
    /// <summary>
    /// Ánh xạ 1 cột trong file Excel → 1 property của object T.
    /// Header phải khớp với tên cột trong dòng đầu tiên của file.
    /// </summary>
    public class ExcelColumnMap<T>
    {
        public string Header { get; set; } = "";
        public Action<T, string> Setter { get; set; } = (_, _) => { };
        public bool Required { get; set; } = false;
    }
}
