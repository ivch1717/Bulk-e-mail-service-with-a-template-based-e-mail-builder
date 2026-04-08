using System.Text.Json;
using Microsoft.AspNetCore.Http;
using UseCases.TemplateUtilities;

namespace UseCases.GetPreview;

public record GetPreviewRequest(
    IFormFile template,
    IFormFile table,
    int? from,
    int count,
    string mappingJson
        );
        
public record GetPreviewResponse(
    List<EmailPreview> emailPreviews,
    int nextRow,
    int? total
);

public record EmailPreview(
    string to,
    string html
);

public interface IGetPreviewRequestHandler
{
    public GetPreviewResponse Handle(GetPreviewRequest request);
}

public class GetPreviewRequestHandler : IGetPreviewRequestHandler
{
    /// <summary>
    /// Фабрика таблиц для поддержки разных форматов таблиц.
    /// </summary>
    private ITableFactory _tableFactory;
    
    /// <summary>
    /// Фабрика шаблонов (возможно лишнее).
    /// </summary>
    private ITemplateFactory _templateFactory;
    
    public GetPreviewRequestHandler(ITableFactory tableFactory, ITemplateFactory templateFactory)
    {
        _tableFactory = tableFactory;
        _templateFactory = templateFactory;
    }
    
    
    public GetPreviewResponse Handle(GetPreviewRequest request)
    {
        Dictionary<string, string> mapping = JsonSerializer.Deserialize<Dictionary<string, string>>(request.mappingJson);
        ITable table;
        int? total;
        HashSet<string> columns = mapping.Values.ToHashSet();
        if (request.from == null)
        {
            table = _tableFactory.Create(request.table);
            total = table.GetTotal(columns);
        }
        else
        {
            table = _tableFactory.Create(request.table, (int)request.from);
            total = null;
        }
        List<RowData> allRowData = table.GetData(columns, request.count);
        ITemplate template = _templateFactory.Create(request.template);
        List<EmailPreview> result = [];
        foreach (var rowData in allRowData)
        {
            string html = template.CreateEmail(rowData);
            string email = rowData.data[mapping["email"]];
            result.Add(new (email, html));
        }
        return new(result, table.CurrentRow, total);
    }
}