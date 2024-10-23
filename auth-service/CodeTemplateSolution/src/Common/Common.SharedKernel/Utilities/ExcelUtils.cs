using System.Reflection;
using ClosedXML.Excel;
using Common.Domain.Excel;
using Common.SharedKernel.Extensions;
using Common.SharedKernel.LogProvider;
using NLog;

namespace Common.SharedKernel.Utilities
{
    public class ExcelUtils
    {
        private static readonly Logger logger = LoggerHelper.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string RequiredMark = "*";
        public static List<T> ReadExcel<T>(Stream fileStream, string sheetName = "") where T : new()
        {
            var dataList = new List<T>();

            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = string.IsNullOrEmpty(sheetName) ? workbook.Worksheet(1) : workbook.Worksheet(sheetName);

                if (worksheet == null)
                    throw new ArgumentException("Excel file is empty or does not contain any worksheets.");

                // Read column names from the first row
                var columnNames = ReadColumnNames<T>();

                // Map column names to properties of the target class
                var propertyMap = GetPropertyMap<T>(columnNames);

                // Read data from Excel and bind to the target class
                dataList = ReadData<T>(worksheet, propertyMap);
            }

            return dataList;
        }
        public static void CreateExcelFromList<T>(IXLWorkbook workbook, string sheetName, List<T> data) where T : new()
        {
            try
            {
                var columnNames = ReadColumnNames<T>();
                var propertyMap = GetPropertyMap<T>(columnNames);
                var worksheet = workbook.Worksheets.Add(sheetName);
                for (int i = 0; i < propertyMap.Count; i++)
                {
                    var key = propertyMap.ElementAt(i).Key;
                    worksheet.Cell(1, i + 1).Value = key;
                    var headerCell = worksheet.Cell(1, i + 1);
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerCell.Style.Border.OutsideBorderColor = XLColor.Black;
                }

                for (int row = 0; row < data.Count; row++)
                {
                    for (int col = 0; col < propertyMap.Count; col++)
                    {
                        var key = propertyMap.ElementAt(col).Key;
                        var value = propertyMap[key].GetValue(data[row], null)?.ToString();
                        worksheet.Cell(row + 2, col + 1).Value = value;
                        var cell = worksheet.Cell(row + 2, col + 1);
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Border.OutsideBorderColor = XLColor.Black;
                    }
                }

                worksheet.Columns().AdjustToContents();
                worksheet.Rows().AdjustToContents();
                worksheet.SetAutoFilter();

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error creating excel. Message: {ex.Message}");
                throw;
            }

        }
        public static void UpdateWorksheet<T>(IXLWorksheet worksheet, List<T> data, int startRow = 1, bool hasReport = false) where T : new()
        {
            int totalCount = data.Count;
            int start = 0; // start from the second row because first row is header
            int currentRowIndex = startRow == 0 ? 1 : startRow;
            while (start < totalCount)
            {
                var currentData = data[start];
                var columnNames = ReadColumnNames<T>();
                if (hasReport)
                {
                    columnNames.Add("Kết quả");
                    columnNames.Add("Ghi chú");
                }
                var propertyMap = GetPropertyMap<T>(columnNames);
                if (currentData is not null)
                {
                    var properties = currentData.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        var value = currentData?.GetType()?.GetProperty(prop.Name)?.GetValue(currentData, null);
                        var columnKey = propertyMap.FirstOrDefault(x => x.Value == prop).Key;
                        var column = worksheet.CellsUsed().AsEnumerable().FirstOrDefault(a => string.Equals(a.Value.ToString(), columnKey, StringComparison.OrdinalIgnoreCase));
                        if (column != null)
                        {
                            var cell = worksheet.Cell(currentRowIndex + 1, column.Address.ColumnNumber);
                            if (value != null && value.ToString().Contains(';'))
                            {
                                cell.Style.Alignment.WrapText = true;
                                var richText = value.ToString().Split("; ").ToList();
                                cell.Value = string.Join("\n", richText);
                            }
                            else
                            {
                                cell.Value = value?.ToString();
                            }

                            // Set alignment to top left
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                            // Set all borders
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        }
                    }
                    currentRowIndex++;
                }
                start++;
            }
        }

        private static List<string> ReadColumnNames<T>() where T : new()
        {
            var columnNames = new List<string>();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (property.Name.EqualsIgnoreCase("ImportStatusStr") || property.Name.EqualsIgnoreCase("ReportMessage")) continue;
                var excelColumnAttribute = property.GetCustomAttribute<ExcelColumnAttribute>();
                if (excelColumnAttribute != null)
                {
                    if (excelColumnAttribute.IsRequired)
                    {
                        columnNames.Add(excelColumnAttribute.Name + RequiredMark);
                    }
                    else
                    {
                        columnNames.Add(excelColumnAttribute.Name);
                    }
                }
                else
                {
                    columnNames.Add(property.Name); // Default to property name if no DisplayName attribute is specified
                }
            }

            return columnNames;
        }

        private static Dictionary<string, PropertyInfo> GetPropertyMap<T>(List<string> columnNames) where T : new()
        {
            var propertyMap = new Dictionary<string, PropertyInfo>();
            var properties = typeof(T).GetProperties();

            foreach (var columnName in columnNames)
            {
                var property = properties.FirstOrDefault(p => GetColumnName(p) == columnName);
                if (property != null)
                    propertyMap.Add(columnName, property);
            }

            return propertyMap;
        }

        private static string GetColumnName(PropertyInfo property)
        {
            var result = "";
            var excelColumnAttribute = property.GetCustomAttribute<ExcelColumnAttribute>();
            if (excelColumnAttribute != null)
            {
                if (excelColumnAttribute.IsRequired)
                {
                    result = excelColumnAttribute.Name + RequiredMark;
                }
                else
                {
                    result = excelColumnAttribute.Name;
                }
            }
            else
            {
                result = property.Name;
            }

            return result;
        }

        private static List<T> ReadData<T>(IXLWorksheet worksheet, Dictionary<string, PropertyInfo> propertyMap) where T : new()
        {
            var dataList = new List<T>();

            foreach (var row in worksheet.RowsUsed().Skip(1)) // Skip the first row since it contains column headers
            {
                var rowData = new T();
                foreach (var kvp in propertyMap)
                {
                    var columnName = kvp.Key;
                    var property = kvp.Value;
                    var columnIndex = GetColumnIndex<T>(worksheet, columnName); // Get column index based on data annotations
                    if (columnIndex != -1)
                    {
                        var cell = row.Cell(columnIndex); // ClosedXML uses 1-based indexing
                        string cellValue;
                        if (cell.IsMerged())
                        {
                            var mergedRange = cell.MergedRange();
                            var firstCellInMergedRange = mergedRange.FirstCell();
                            cellValue = firstCellInMergedRange.Value.ToString();
                        }
                        else
                        {
                            cellValue = cell.Value.ToString();
                        }
                        property.SetValue(rowData, Convert.ChangeType(cellValue, property.PropertyType));
                    }
                }
                dataList.Add(rowData);
            }

            return dataList;
        }

        private static int GetColumnIndex<T>(IXLWorksheet worksheet, string columnName) where T : new()
        {
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var nameToCheck = GetColumnName(property);
                if (string.Equals(nameToCheck, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    var firstRow = worksheet.Row(1);
                    foreach (var cell in firstRow.CellsUsed())
                    {
                        if (string.Equals(cell.Value.ToString(), nameToCheck, StringComparison.OrdinalIgnoreCase))
                            return cell.Address.ColumnNumber; // ClosedXML uses 1-based indexing
                    }
                }
            }

            return -1;
        }
    }
}
