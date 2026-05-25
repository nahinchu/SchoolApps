using OfficeOpenXml;

namespace SchoolApp.Services.ExcelExportService
{
    public class ExcelImportService : IExcelImportService
    {
        public ExcelImportService()
        {
            ExcelPackage.License.SetNonCommercialOrganization("SchoolApp");
        }

        public ExcelImportResult<T> Import<T>(
            Stream fileStream,
            IEnumerable<ExcelColumnMap<T>> columns,
            Func<T> factory)
        {
            var cols = columns.ToList();
            var result = new ExcelImportResult<T>();

            using var package = new ExcelPackage(fileStream);
            var ws = package.Workbook.Worksheets.FirstOrDefault();

            if (ws == null || ws.Dimension == null)
            {
                result.Errors.Add(new ImportRowError
                {
                    RowNumber = 0,
                    Message = "File Excel trống hoặc không đọc được."
                });
                return result;
            }

            int totalRows = ws.Dimension.Rows;
            int totalCols = ws.Dimension.Columns;

            // ── Đọc dòng header (dòng 1) → map tên cột → chỉ số cột ──────
            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int c = 1; c <= totalCols; c++)
            {
                var header = ws.Cells[1, c].Text.Trim();
                if (!string.IsNullOrEmpty(header))
                    headerMap[header] = c;
            }

            // ── Kiểm tra các cột bắt buộc có tồn tại trong file không ────
            foreach (var col in cols.Where(x => x.Required))
            {
                if (!headerMap.ContainsKey(col.Header))
                {
                    result.Errors.Add(new ImportRowError
                    {
                        RowNumber = 0,
                        Message = $"File thiếu cột bắt buộc: \"{col.Header}\""
                    });
                }
            }

            if (result.Errors.Any()) return result;

            // ── Xử lý từng dòng dữ liệu (bắt đầu từ dòng 2) ────────────
            for (int r = 2; r <= totalRows; r++)
            {
                // Bỏ qua dòng trống hoàn toàn
                bool isEmpty = true;
                for (int c = 1; c <= totalCols; c++)
                {
                    if (!string.IsNullOrWhiteSpace(ws.Cells[r, c].Text))
                    {
                        isEmpty = false;
                        break;
                    }
                }
                if (isEmpty) continue;

                // Tạo object mới bằng factory
                var obj = factory();
                var rowErrors = new List<string>();

                foreach (var col in cols)
                {
                    // Nếu cột không có trong file → đã báo lỗi cấp file ở trên
                    if (!headerMap.TryGetValue(col.Header, out int colIdx))
                        continue;

                    var cellValue = ws.Cells[r, colIdx].Text.Trim();

                    // Validate Required
                    if (col.Required && string.IsNullOrWhiteSpace(cellValue))
                    {
                        rowErrors.Add($"\"{col.Header}\" không được để trống");
                        continue;
                    }

                    // Gọi Setter — nếu throw (parse lỗi) thì ghi nhận lỗi dòng
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        try
                        {
                            col.Setter(obj, cellValue);
                        }
                        catch (Exception ex)
                        {
                            rowErrors.Add($"\"{col.Header}\": {ex.Message}");
                        }
                    }
                }

                if (rowErrors.Count > 0)
                {
                    result.Errors.Add(new ImportRowError
                    {
                        RowNumber = r,
                        Message = string.Join("; ", rowErrors)
                    });
                }
                else
                {
                    result.ValidRows.Add(obj);
                }
            }

            return result;
        }
    }
}
