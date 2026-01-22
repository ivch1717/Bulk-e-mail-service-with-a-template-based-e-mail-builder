using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace UseCases;

public class AddressParser : IAddressParser
{
    public List<string> Parse(IFormFile data)
    {
        List<string> result = [];
        using var stream = data.OpenReadStream();
        IWorkbook workbook = new XSSFWorkbook(stream);
        ISheet sheet = workbook.GetSheetAt(0);
        IRow emails = sheet.GetRow(1);
        for (int i = 0; i < emails.LastCellNum; i++)
        {
            ICell? cell = emails.GetCell(i);
            if (cell == null)
            {
                break;
            }
            string value = cell.StringCellValue;
            if (string.IsNullOrEmpty(value) || !isEmail(value))
            {
                break;
            }
            result.Add(value);
        }
        return result;
    }

    private bool isEmail(string value)
    {
        try
        {
            var email = new MailAddress(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}