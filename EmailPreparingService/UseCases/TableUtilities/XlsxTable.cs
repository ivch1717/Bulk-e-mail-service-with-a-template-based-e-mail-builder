using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace UseCases;

/// <summary>
/// Обертка для работы с xlsx таблицами.
/// </summary>
public class XlsxTable : ITable 
{
    /// <summary>
    /// Первый лист таблицы, предполагается что данные на нем.
    /// </summary>
    private ISheet _sheet;
    
    /// <summary>
    /// Общее число строк первого листа.
    /// </summary>
    public int totalRows => _sheet.LastRowNum;

    public XlsxTable(IFormFile file)
    {
        var stream = file.OpenReadStream();
        IWorkbook workbook = new XSSFWorkbook(stream);
        _sheet = workbook.GetSheetAt(0);
    }
    
    /// <summary>
    /// Возвращает все значения из строки.
    /// </summary>
    /// <param name="rowNumber">Номер строки.</param>
    /// <param name="skipEmpty">Добавлять ли пустые значения.</param>
    /// <returns>Список значений таблице в формате строк.</returns>
    public List<string> GetRow(int rowNumber, bool skipEmpty = true)
    {
        List<string> result = [];
        IRow row = _sheet.GetRow(rowNumber);
        for (int i = 0; i < row.LastCellNum; ++i)
        {
            var cell = row.GetCell(i);
            if (skipEmpty && (cell == null || string.IsNullOrEmpty(cell.StringCellValue)))
            {
                break;
            }

            result.Add(cell?.StringCellValue ?? "");
        }
        return result;
    }
}