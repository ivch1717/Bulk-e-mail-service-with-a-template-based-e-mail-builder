using Microsoft.AspNetCore.Http;

namespace UseCases.Preparation.UploadTemplate;

/// <summary>
/// Запрос для получения переменных шаблона.
/// </summary>
/// <param name="template">Шаблон.</param>
public sealed record UploadTemplateRequest(IFormFile template);