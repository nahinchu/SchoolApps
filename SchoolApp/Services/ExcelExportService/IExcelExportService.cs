namespace SchoolApp.Services.ExcelExportService
{
    public interface IExcelExportService
    {
        byte[] Export<T>(
            IEnumerable<T> data,
            IEnumerable<ExcelColumnDef<T>> columns,
            string sheetName = "Sheet1",
            string? title = null);
    }
}
