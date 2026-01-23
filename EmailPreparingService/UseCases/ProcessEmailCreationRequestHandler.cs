using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace UseCases;

public class ProcessEmailCreationRequestHandler : IProcessEmailCreationRequestHandler
{
    IAddressParser _addressParser;
    ITemplateParser _templateParser;
    IDataParser _dataParser;
    ITableExtracter _tableExtracter;

    public ProcessEmailCreationRequestHandler(IAddressParser addressParser, ITemplateParser templateParser, IDataParser dataParser, ITableExtracter tableExtracter)
    {
        _addressParser = addressParser;
        _templateParser = templateParser;
        _dataParser = dataParser;
        _tableExtracter = tableExtracter;
    }
    
    public ProcessEmailCreationResponse Handle(ProcessEmailCreationRequest request)
    {
        List<TableInfo> tableInfos = JsonSerializer.Deserialize<List<TableInfo>>(request.tableInfosJson);
        List<string> adresses = _addressParser.Parse(request.data, tableInfos[0]);
        List<string> emails = [];
        List<List<string>> data = [];
        for (int i = 1; i < tableInfos.Count; i++)
        {
            data.Add(_tableExtracter.Extract(request.data, tableInfos[i]));
        }

        for (int i = 0; i < adresses.Count; i++)
        {
            emails.Add(_templateParser.Paste(ReadText(request.template), data, i));
        }
        return new ProcessEmailCreationResponse(adresses, emails);
    }
    
    public string ReadText(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        return reader.ReadToEnd();
    }
}