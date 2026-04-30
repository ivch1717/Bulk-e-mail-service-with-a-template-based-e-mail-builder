using Microsoft.AspNetCore.Http;

namespace UseCases.UploadTemplate;

/// <summary>
/// Список уникальных переменных обнаруженных в шаблоне.
/// </summary>
/// <param name="template">Файл шаблона.</param>
public sealed record UploadTemplateRequest(
    IFormFile template
    );