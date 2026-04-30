namespace UseCases.UploadTemplate;

public interface IUploadTemplateRequestHandler
{
    /// <summary>
    /// Обрабатывает запрос по получению множества переменных.
    /// </summary>
    /// <param name="request">Файл шаблона.</param>
    /// <returns>Список уникальных переменных.</returns>
    public List<string> Handle(UploadTemplateRequest request);
}