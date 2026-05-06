using Microsoft.AspNetCore.Http;

namespace UseCases.UploadTemplate;

/// <summary>
/// Запрос для получения переменных шаблона.
/// </summary>
/// <param name="template">Шаблон.</param>
public sealed record UploadTemplateRequest(IFormFile template);