namespace SchoolApp.Services.ExcelExportService
{
    public class ExcelColumnDef<T>
    {
        public string Header { get; set; } = "";
        public Func<T, object?> ValueSelector { get; set; } = _ => null;
        public string? Format { get; set; }
        public double Width { get; set; } = 20;
    }
}
