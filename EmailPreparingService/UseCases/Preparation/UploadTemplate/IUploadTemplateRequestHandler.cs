namespace UseCases.Preparation.UploadTemplate;

/// <summary>
/// Интерфейс обработчика шаблонов.
/// </summary>
public interface IUploadTemplateRequestHandler
{
    /// <summary>
    /// Распознает переменные в шаблоне.
    /// </summary>
    /// <param name="request">Шаблон.</param>
    /// <returns>Переменные.</returns>
    public UploadTemplateResponse Handle(UploadTemplateRequest request);
}