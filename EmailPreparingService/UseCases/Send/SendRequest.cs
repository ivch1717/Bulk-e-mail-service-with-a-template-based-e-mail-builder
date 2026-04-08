using System.Text.Json;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Models;
using UseCases.TemplateUtilities;

namespace UseCases.GetPreview;

public record SendRequest(
    IFormFile template,
    IFormFile table,
    string mappingJson
        );
        
public record SendResponse(
    
);

public interface ISendRequestHandler
{
    public Task<SendResponse> Handle(SendRequest request);
}

public class SendRequestHandler : ISendRequestHandler
{
    /// <summary>
    /// Фабрика таблиц для поддержки разных форматов таблиц.
    /// </summary>
    private ITableFactory _tableFactory;
    
    /// <summary>
    /// Фабрика шаблонов (возможно лишнее).
    /// </summary>
    private ITemplateFactory _templateFactory;
    
    private readonly AppDbContext _db;

    public SendRequestHandler(ITableFactory tableFactory, ITemplateFactory templateFactory, AppDbContext db)
    {
        _tableFactory = tableFactory;
        _templateFactory = templateFactory;
        _db = db;
    }
    
    
    public async Task<SendResponse> Handle(SendRequest request)
    {
        Dictionary<string, string> mapping = JsonSerializer.Deserialize<Dictionary<string, string>>(request.mappingJson);
        ITable table;
        HashSet<string> columns = mapping.Values.ToHashSet();
        table = _tableFactory.Create(request.table);
        List<RowData> allRowData = table.GetData(columns, table.totalRows);
        ITemplate template = _templateFactory.Create(request.template);
        foreach (var rowData in allRowData)
        {
            string html = template.CreateEmail(rowData);
            string email = rowData.data[mapping["email"]];
            _db.OutboxEmails.Add(new OutboxEmail
            {
                Id = Guid.NewGuid(),
                To = email,
                Html = html,
                CreatedAt = DateTime.UtcNow,
                Sent = false
            });
        }

        await _db.SaveChangesAsync();
        return new SendResponse();
    }
}