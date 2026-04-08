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

    public int CurrentRow { get; private set; }

    private List<string> _headers;

    public XlsxTable(IFormFile file)
    {
        var stream = file.OpenReadStream();
        IWorkbook workbook = new XSSFWorkbook(stream);
        _sheet = workbook.GetSheetAt(0);
        _headers = [];
        for (int i = 0; i < totalRows; i++)
        {
            _headers = GetRow(i);
            if (_headers.Count > 0)
            {
                CurrentRow = i + 1;
                break;
            }
        }
    }
    
    public XlsxTable(IFormFile file, int currentRow)
    {
        var stream = file.OpenReadStream();
        IWorkbook workbook = new XSSFWorkbook(stream);
        _sheet = workbook.GetSheetAt(0);
        _headers = [];
        for (int i = 0; i < totalRows; i++)
        {
            _headers = GetRow(i);
            if (_headers.Count > 0)
            {
                break;
            }
        }
        CurrentRow = currentRow;
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

    public List<RowData> GetData(HashSet<string> columns, int count)
    {
        List<RowData> result = [];
        while (CurrentRow < totalRows && result.Count < count)
        {
            RowData cur = new RowData(new Dictionary<string, string>());
            IRow row = _sheet.GetRow(CurrentRow);
            for (int i = 0; i < row.LastCellNum; ++i)
            {
                var cell = row.GetCell(i);
                if (cell == null || string.IsNullOrEmpty(cell.StringCellValue))
                {
                    break;
                }
                cur.data[_headers[i]] = cell.StringCellValue;
            }
            if (isRowValid(columns, cur))
            {
                result.Add(cur);
            }
            CurrentRow++;
        }
        
        return result;
    }

    public int GetTotal(HashSet<string> columns)
    {
        int total = 0;
        for (int i = CurrentRow; i < totalRows; i++)
        {
            RowData cur = new RowData(new Dictionary<string, string>());
            IRow row = _sheet.GetRow(i);
            if (row == null)
            {
                continue;
            }
            for (int j = 0; j < row.LastCellNum && j < _headers.Count; j++)
            {
                var cell = row.GetCell(j);
                if (cell == null || string.IsNullOrEmpty(cell.StringCellValue))
                {
                    break;
                }
                cur.data[_headers[j]] = cell.StringCellValue;
            }

            if (isRowValid(columns, cur))
            {
                total++;
            }
        }
        return total;
    }

    private bool isRowValid(HashSet<string> columns, RowData cur)
    {
        return columns.All(c => cur.data.ContainsKey(c) && !string.IsNullOrEmpty(cur.data[c]));
    }
}