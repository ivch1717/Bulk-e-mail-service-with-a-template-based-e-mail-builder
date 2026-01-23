using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace UseCases;

public class AddressParser : IAddressParser
{
    ITableExtracter _tableExtracter;

    public AddressParser(ITableExtracter tableExtracter)
    {
        _tableExtracter = tableExtracter;
    }
    
    public List<string> Parse(IFormFile data, TableInfo tableInfo)
    {
        List<string> candidates = _tableExtracter.Extract(data, tableInfo);
        foreach (var candidate in candidates)
        {
            if (!isEmail(candidate))
            {
                throw new InvalidDataException($"Invalid email address {candidate}");
            }
        }
        return candidates;
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