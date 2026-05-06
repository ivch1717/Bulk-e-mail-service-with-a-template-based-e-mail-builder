namespace UseCases.Preparation.UploadTemplate;

/// <summary>
/// Ответ для получения переменных шаблона.
/// </summary>
/// <param name="variables">Переменные.</param>
public record UploadTemplateResponse(List<string> variables);