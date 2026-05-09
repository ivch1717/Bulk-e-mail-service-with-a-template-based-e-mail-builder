using Microsoft.AspNetCore.Http;

namespace UseCases.Preparation.Send;

public record SendRequest(
    IFormFile template,
    IFormFile table,
    string mappingJson,
    string subject,
    Guid smtpId,
    bool tracking,
    string? campaignName
        );