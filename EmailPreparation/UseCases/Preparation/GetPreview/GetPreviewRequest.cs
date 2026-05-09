using Microsoft.AspNetCore.Http;

namespace UseCases.Preparation.GetPreview;

public record GetPreviewRequest(
    IFormFile template,
    IFormFile table,
    int? from,
    int count,
    string mappingJson
        );