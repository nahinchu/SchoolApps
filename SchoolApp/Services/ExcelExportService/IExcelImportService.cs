namespace SchoolApp.Services.ExcelExportService
{
    public interface IExcelImportService
    {
        ExcelImportResult<T> Import<T>(
            Stream fileStream,
            IEnumerable<ExcelColumnMap<T>> columns,
            Func<T> factory);
    }
}
