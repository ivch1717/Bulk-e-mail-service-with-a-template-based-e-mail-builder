using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace UseCases;

public class UploadDataRequestHandler : IUploadDataRequestHandler
{
    IAddressParser _addressParser;
    ITemplateParser _templateParser;
    IDataParser _dataParser;

    public UploadDataRequestHandler(IAddressParser addressParser, ITemplateParser templateParser, IDataParser dataParser)
    {
        _addressParser = addressParser;
        _templateParser = templateParser;
        _dataParser = dataParser;
    }
    
    public UploadDataResponse Handle(UploadDataRequest request)
    {
        List<string> adresses = _addressParser.Parse(request.data);
        List<string> emails = [];
        for (int i = 0; i < adresses.Count; i++)
        {
            emails.Add(_templateParser.Paste(ReadText(request.template), _dataParser.Parse(request.data, i)));
        }
        return new UploadDataResponse(adresses, emails);
    }
    
    public string ReadText(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        return reader.ReadToEnd();
    }
}

public interface ITemplateParser
{
    public string Paste(string text, List<string> data);
}

public class TemplateParser : ITemplateParser
{
    public string Paste(string text, List<string> data)
    {
        int c = 0;
        return Regex.Replace(text, @"\[\[(.*?)\]\]", match =>
        {
            return data[c++];
        });
    }
}

public interface IDataParser
{
    public List<string> Parse(IFormFile file, int column);
}

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
