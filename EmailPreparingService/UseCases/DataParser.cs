using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace UseCases;

public class DataParser : IDataParser
{
    public List<string> Parse(IFormFile data, int column)
    {
        List<string> result = [];
        using var stream = data.OpenReadStream();
        IWorkbook workbook = new XSSFWorkbook(stream);
        ISheet sheet = workbook.GetSheetAt(0);
        int c = 2;
        while (true)
        {
            try
            {
                var cell = sheet.GetRow(c).GetCell(column);
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