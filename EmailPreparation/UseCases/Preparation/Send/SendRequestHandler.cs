using System.Net.Mail;
using System.Text.Json;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models;
using UseCases.TableUtilities;
using UseCases.TemplateUtilities;

namespace UseCases.Preparation.Send;

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
        if (request.subject.Length >= 256)
        {
            throw new ArgumentException($"Длина темы должна быть меньше 255 символов.");
        }
        if (request.smtpId == Guid.Empty ||
            !await _db.SmtpProfiles.AnyAsync(profile => profile.Id == request.smtpId))
        {
            throw new ArgumentException("SMTP profile is required.");
        }

        Dictionary<string, string> mapping = JsonSerializer.Deserialize<Dictionary<string, string>>(request.mappingJson);
        ITable table;
        HashSet<string> columns = mapping.Values.ToHashSet();
        table = _tableFactory.Create(request.table);
        List<RowData> allRowData = table.GetData(columns, table.totalRows);
        ITemplate template = _templateFactory.Create(request.template, request.tracking);
        Guid campaignId = Guid.NewGuid();
        _db.Campaigns.Add(new Campaign
        {
            CampaignId = campaignId,
            CampaignName = string.IsNullOrWhiteSpace(request.campaignName) ? campaignId.ToString() : request.campaignName,
        });
        foreach (var rowData in allRowData)
        {
            try
            {
                string html = template.CreateEmail(rowData, mapping);
                if (request.tracking)
                {
                    html = html.Replace("[[campaignId]]", campaignId.ToString());
                    html = html.Replace("[[email]]", rowData.data[mapping["email"]]);
                }
                string email = rowData.data[mapping["email"]];
                var validation = new MailAddress(email);
                _db.OutboxEmails.Add(new OutboxEmail
                {
                    Id = Guid.NewGuid(),
                    To = email,
                    Html = html,
                    CreatedAt = DateTime.UtcNow,
                    Sent = false,
                    CampaignId = campaignId,
                    Subject = request.subject,
                    SmtpId = request.smtpId
                });
            } catch (Exception) {}
            
        }

        await _db.SaveChangesAsync();
        return new SendResponse();
    }
}