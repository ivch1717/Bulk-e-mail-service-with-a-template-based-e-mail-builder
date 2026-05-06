using Microsoft.AspNetCore.Mvc;
using UseCases.ExtractTableHeaders;
using UseCases.GetPreview;
using UseCases.UploadTemplate;

namespace Presentation.Endpoints;

/// <summary>
/// Эндпоинты, отвечающие за рассылку и ее подготовку.
/// </summary>
/// <param name="uploadTemplateRequestHandler">Обработчик шаблона.</param>
/// <param name="extractTableHeadersRequestHandler">Обработчик таблицы.</param>
/// <param name="getPreviewRequestHandler">Обработчик генерации предпросмотра.</param>
/// <param name="sendRequestHandler">Обработчик рассылки.</param>
[ApiController]
[Route("api/preparation")]
public class EmailPreparationEndpoints(
    IUploadTemplateRequestHandler uploadTemplateRequestHandler,
    IExtractTableHeadersRequestHandler extractTableHeadersRequestHandler,
    IGetPreviewRequestHandler getPreviewRequestHandler,
    ISendRequestHandler sendRequestHandler)
    : ControllerBase
{
    /// <summary>
    /// Распознает переменные в шаблоне.
    /// </summary>
    /// <param name="request">Шаблон.</param>
    /// <returns>Переменные.</returns>
    [HttpPost("upload-template")]
    public IActionResult UploadTemplate([FromForm] UploadTemplateRequest request)
    {
        return Ok(uploadTemplateRequestHandler.Handle(request));
    }

    /// <summary>
    /// Распознает заголовки столбцов в таблице.
    /// </summary>
    /// <param name="request">Таблица.</param>
    /// <returns>Заголовки.</returns>
    [HttpPost("extract-table-headers")]
    public IActionResult ExtractTableHeaders([FromForm] ExtractTableHeadersRequest request)
    {
        return Ok(extractTableHeadersRequestHandler.Handle(request));
    }
    
    /// <summary>
    /// Получение определенного числа писем для предпросмотра на сайте.
    /// </summary>
    /// <param name="request">.xlsx таблица,
    /// .html шаблон.
    /// Начальная строка таблицы.
    /// Количество писем которые нужно сгенерировать.
    /// Переменные шаблона со столбцами таблицы.</param>
    /// <returns>Список писем с адресатами и номер строки следующей за той, что была обработана последней.</returns>
    [HttpPost("get-preview")]
    public IActionResult GetPreview([FromForm] GetPreviewRequest request)
    {
        return Ok(getPreviewRequestHandler.Handle(request));
    }
    
    /// <summary>
    /// Запускает рассылку.
    /// </summary>
    /// <param name="request">Параметры рассылки.</param>
    /// <returns>200 Ok, если рассылка успешно передана в сервис рассылки.
    /// 400 Bad request, если произошла ошибка.</returns>
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromForm] SendRequest request)
    {
        try
        {
            return Ok(await sendRequestHandler.Handle(request));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
