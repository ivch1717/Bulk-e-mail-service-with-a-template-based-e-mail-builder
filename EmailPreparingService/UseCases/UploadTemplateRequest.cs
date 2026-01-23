using Microsoft.AspNetCore.Http;

namespace UseCases;

public sealed record UploadTemplateRequest(
    IFormFile template
    );