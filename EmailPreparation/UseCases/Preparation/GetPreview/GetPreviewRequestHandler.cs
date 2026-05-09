using System.Net.Mail;
using System.Text.Json;
using UseCases.TableUtilities;
using UseCases.TemplateUtilities;

namespace UseCases.Preparation.GetPreview;

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
        ITemplate template = _templateFactory.Create(request.template, false);
        List<EmailPreview> result = [];
        foreach (var rowData in allRowData)
        {
            try
            {
                string html = template.CreateEmail(rowData, mapping);
                string email = rowData.data[mapping["email"]];
                var validation = new MailAddress(email);
                result.Add(new(email, html));
            }
            catch (Exception )
            {
                total -= 1;
            }
            
        }
        return new(result, table.CurrentRow, total);
    }
}