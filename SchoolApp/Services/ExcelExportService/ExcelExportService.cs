using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace SchoolApp.Services.ExcelExportService
{
    public class ExcelExportService : IExcelExportService
    {
        public ExcelExportService()
        {
            ExcelPackage.License.SetNonCommercialOrganization("SchoolApp");
        }

        public byte[] Export<T>(
            IEnumerable<T> data,
            IEnumerable<ExcelColumnDef<T>> columns,
            string sheetName = "Sheet1",
            string? title = null)
        {
            var cols = columns.ToList();
            var rows = data.ToList();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add(sheetName);

            int startRow = 1;

            if (!string.IsNullOrWhiteSpace(title))
            {
                ws.Cells[1, 1, 1, cols.Count].Merge = true;
                var titleCell = ws.Cells[1, 1];
                titleCell.Value = title;
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Font.Size = 14;
                titleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                titleCell.Style.Font.Color.SetColor(Color.FromArgb(0x1a, 0x2a, 0x4a)); // navy
                startRow = 2;
            }

            // Header row
            int headerRow = startRow;
            for (int c = 0; c < cols.Count; c++)
            {
                var cell = ws.Cells[headerRow, c + 1];
                cell.Value = cols[c].Header;
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0x1a, 0x2a, 0x4a)); // navy
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(0xcc, 0xaa, 0x00));
                ws.Column(c + 1).Width = cols[c].Width;
            }

            // Data rows
            for (int r = 0; r < rows.Count; r++)
            {
                int excelRow = headerRow + 1 + r;
                bool isEven = r % 2 == 1;

                for (int c = 0; c < cols.Count; c++)
                {
                    var cell = ws.Cells[excelRow, c + 1];
                    var value = cols[c].ValueSelector(rows[r]);
                    cell.Value = value;

                    if (cols[c].Format != null)
                        cell.Style.Numberformat.Format = cols[c].Format;

                    if (isEven)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0xf5, 0xf7, 0xfa));
                    }

                    cell.Style.Border.BorderAround(ExcelBorderStyle.Hair, Color.LightGray);
                }
            }

            // Auto-filter trên header
            if (rows.Count > 0)
                ws.Cells[headerRow, 1, headerRow + rows.Count, cols.Count].AutoFilter = true;

            ws.View.FreezePanes(headerRow + 1, 1);

            return package.GetAsByteArray();
        }
    }
}
