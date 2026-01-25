using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace UseCases;

public class TableExtracter : ITableExtracter
{
    public List<string> Extract(IFormFile data, TableInfo tableInfo)
    {
        using var stream = data.OpenReadStream();
        IWorkbook workbook = new XSSFWorkbook(stream);
        ISheet sheet = workbook.GetSheetAt(0);
        if (tableInfo.row)
        {
            return ExtractRow(sheet, tableInfo);
        }
        return ExtractColumn(sheet, tableInfo);
    }

    public List<string> ExtractRow(ISheet sheet, TableInfo tableInfo)
    {
        List<string> result = [];
        IRow row = sheet.GetRow(tableInfo.offsetY);
        for (int i = tableInfo.offsetX; i < row.LastCellNum; i++)
        {
            ICell? cell = row.GetCell(i);
            if (cell == null)
            {
                break;
            }
            string value = cell.StringCellValue;
            if (string.IsNullOrEmpty(value))
            {
                break;
            }
            result.Add(value);
        }
        return result;
    }

    public List<string> ExtractColumn(ISheet sheet, TableInfo tableInfo)
    {
        List<string> result = [];
        int c = tableInfo.offsetY;
        while (true)
        {
            try
            {
                var cell = sheet.GetRow(c).GetCell(tableInfo.offsetX);
                if (cell == null)
                {
                    break;
                }
                result.Add(cell.StringCellValue);
                c++;
            }
            catch
            {
                break;
            }
            
        }
        return result;
    }
}